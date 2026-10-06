#!/usr/bin/env python3
"""Reads Dominions 6 .trs image archives (data/misc.trs, res.trs, monster.trs, ...).

Usage:
  python3 tools/gameart/trs.py list ARCHIVE
  python3 tools/gameart/trs.py extract ARCHIVE OUTDIR [--only 3,10-20]
  python3 tools/gameart/trs.py sheet ARCHIVE OUT.png [--only 0-99] [--cols 16] [--cell 64] [--zoom 2] [--bg 404040]

ARCHIVE is a path, or just a name (misc.trs) looked up in the game's data folder
(--data DIR, else $DOM6_DATA, else the default Steam folder).

The game's art is not ours to distribute: extract and sheet refuse to write inside this
repository. Write to a scratch folder instead. Format: tools/gameart/README.md.

Python 3.8+, standard library only.
"""
import argparse, os, struct, sys, zlib

DEFAULT_DATA = [
    '/mnt/c/Games/Steam/steamapps/common/Dominions6/data',
    '/mnt/c/Program Files (x86)/Steam/steamapps/common/Dominions6/data',
    'C:/Games/Steam/steamapps/common/Dominions6/data',
    'C:/Program Files (x86)/Steam/steamapps/common/Dominions6/data',
    os.path.expanduser('~/.steam/steam/steamapps/common/Dominions6/data'),
]
REPO = os.path.realpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))

FLAG_HALF = 0x01    # drawn at half its pixel size (a 64x64 image shown where a 32x32 one is)
FLAG_END = 0x02     # tested by the game when counting a run of images; not set in 6.37's files
FLAG_ALPHA = 0x04   # set on images whose pixels carry an alpha byte (the decoder reads the
                    # image's own mode word instead)


class TrsImage:
    __slots__ = ('index', 'width', 'height', 'flags', 'raw_offset', 'rle_offset', 'length', 'group')

    @property
    def offset(self):
        return self.raw_offset or self.rle_offset


class TrsArchive:
    """A .trs file: header, image index, group labels, and a decoder per image."""

    def __init__(self, path):
        self.path = path
        with open(path, 'rb') as f:
            self.data = f.read()
        d = self.data
        if d[:3] != b'TCS':   # (the game checks these three bytes; the files say TCSF)
            raise ValueError(f'{path}: not a .trs archive (no TCSF magic)')
        self.count, self.version, self.pitch = struct.unpack('>HHH', d[4:10])
        # version 2+: 12-byte records from 0x0C (width, height, u16 flags, u32 raw offset,
        # u32 run-length offset); older: 10-byte records from 0x0A without the flags.
        first, size = (12, 12) if self.version >= 2 else (10, 10)
        self.images = []
        for i in range(self.count):
            rec = d[first + size * i: first + size * (i + 1)]
            img = TrsImage()
            img.index = i
            img.width = rec[0] or 256   # one byte each: 0 means 256
            img.height = rec[1] or 256
            img.flags = struct.unpack('>H', rec[2:4])[0] if size == 12 else 0
            img.raw_offset, img.rle_offset = struct.unpack('>II', rec[size - 8:size])
            img.group = ''
            self.images.append(img)
        # Group labels: (u32 first index, NUL-terminated name) pairs, ended by 0xFFFFFFFF.
        self.groups = []
        p = first + size * self.count
        while p + 4 <= len(d):
            first = struct.unpack('>I', d[p:p + 4])[0]
            p += 4
            if first == 0xFFFFFFFF:
                break
            end = d.index(b'\0', p)
            self.groups.append((first, d[p:end].decode('latin-1')))
            p = end + 1
        self.data_start = p
        for gi, (first, name) in enumerate(self.groups):
            last = self.groups[gi + 1][0] if gi + 1 < len(self.groups) else self.count
            for i in range(first, min(last, self.count)):
                self.images[i].group = name
        # Lengths: images are stored back to back, so each runs to the next one's offset.
        offsets = sorted({im.offset for im in self.images if im.offset} | {len(d)})
        nxt = {o: offsets[k + 1] for k, o in enumerate(offsets[:-1])}
        for im in self.images:
            im.length = nxt.get(im.offset, 0) - im.offset if im.offset else 0

    def encoding(self, im):
        if im.raw_offset:
            return 'raw16'
        if not im.rle_offset:
            return 'empty'
        if self.version >= 4:
            mode = struct.unpack('>H', self.data[im.rle_offset:im.rle_offset + 2])[0]
            return 'rle8a' if mode == 1 else 'rle8'
        return 'rle16'

    def decode(self, i):
        """Returns (width, height, RGBA bytes) for image i, converted the way the game does."""
        im = self.images[i]
        w, h = im.width, im.height
        out = bytearray(w * h * 4)
        d = self.data
        enc = self.encoding(im)
        if enc == 'raw16':
            # w*h big-endian RGB565 pixels, rows top to bottom.
            p = im.raw_offset
            for k in range(w * h):
                _put565(out, k * 4, (d[p] << 8) | d[p + 1])
                p += 2
        elif enc == 'rle16':
            # u16 runs-1, then runs: u16 skip in bytes on a surface `pitch` (800) pixels wide,
            # u16 count-1 (0xFFFF: no pixels, a skip longer than 0xFFFF bytes is split), then
            # count RGB565 pixels. Skipped pixels are transparent.
            p = im.rle_offset
            runs = struct.unpack('>H', d[p:p + 2])[0] + 1
            p += 2
            pos = 0
            for _ in range(runs):
                skip, cnt = struct.unpack('>HH', d[p:p + 4])
                p += 4
                cnt = (cnt + 1) & 0xFFFF
                pos += skip // 2
                for _k in range(cnt):
                    y, x = divmod(pos, self.pitch)
                    if x < w and y < h:
                        _put565(out, (y * w + x) * 4, (d[p] << 8) | d[p + 1])
                    p += 2
                    pos += 1
        elif enc in ('rle8', 'rle8a'):
            # u16 mode (1: pixels carry an alpha byte), u16 runs, then runs: skip, count,
            # pixels. skip and count are one byte, or 0xFF and a 24-bit value; positions run
            # across rows of the image's own width. Pixels are RGB565, plus an alpha byte in
            # mode 1 (then the color keys below don't apply).
            p = im.rle_offset
            alpha = struct.unpack('>H', d[p:p + 2])[0] == 1
            runs = struct.unpack('>H', d[p + 2:p + 4])[0]
            p += 4
            pos = 0
            for _ in range(runs):
                skip = d[p]; p += 1
                if skip == 0xFF:
                    skip = (d[p] << 16) | (d[p + 1] << 8) | d[p + 2]; p += 3
                cnt = d[p]; p += 1
                if cnt == 0xFF:
                    cnt = (d[p] << 16) | (d[p + 1] << 8) | d[p + 2]; p += 3
                pos += skip
                for _k in range(cnt):
                    v = (d[p] << 8) | d[p + 1]
                    if pos < w * h:
                        _put565(out, pos * 4, v, d[p + 2] if alpha else None)
                    p += 3 if alpha else 2
                    pos += 1
        return w, h, bytes(out)


def _put565(out, o, v, alpha=None):
    """One pixel as the game converts it: R = v>>8 & 0xF8, G = v>>3 & 0xFC, B = v<<3 & 0xF8.
    Without an alpha byte, 0x0000 is transparent and magenta 0xF81F is a shadow (black, alpha
    0x80)."""
    r, g, b = (v >> 8) & 0xF8, (v >> 3) & 0xFC, (v << 3) & 0xF8
    if alpha is None:
        if v == 0:
            return
        if v == 0xF81F:
            r, g, b, alpha = 0, 0, 0, 0x80
        else:
            alpha = 0xFF
    out[o:o + 4] = bytes((r, g, b, alpha))


def write_png(path, w, h, rgba):
    def chunk(tag, body):
        c = tag + body
        return struct.pack('>I', len(body)) + c + struct.pack('>I', zlib.crc32(c) & 0xFFFFFFFF)
    rows = b''.join(b'\0' + rgba[y * w * 4:(y + 1) * w * 4] for y in range(h))
    png = (b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', w, h, 8, 6, 0, 0, 0))
           + chunk(b'IDAT', zlib.compress(rows, 9)) + chunk(b'IEND', b''))
    with open(path, 'wb') as f:
        f.write(png)


# 3x5 digits for the contact sheet's index labels.
DIGITS = {
    '0': '111101101101111', '1': '010110010010111', '2': '111001111100111', '3': '111001111001111',
    '4': '101101111001001', '5': '111100111001111', '6': '111100111101111', '7': '111001001001001',
    '8': '111101111101111', '9': '111101111001111',
}


def make_sheet(arc, indices, cols, cell, bg, zoom=1):
    label_h = 12
    cw, ch = cell + 4, cell + 4 + label_h
    rows = (len(indices) + cols - 1) // cols
    W, H = cols * cw, max(1, rows) * ch
    img = bytearray(W * H * 4)
    for k in range(W * H):
        img[k * 4:k * 4 + 4] = bytes((0, 0, 0, 255))
    for n, i in enumerate(indices):
        cx, cy = (n % cols) * cw, (n // cols) * ch
        for y in range(cy + 1, cy + ch - 1):          # cell background
            o = (y * W + cx + 1) * 4
            img[o:o + (cw - 2) * 4] = bytes((bg[0], bg[1], bg[2], 255)) * (cw - 2)
        x0 = cx + 2
        for d in str(i):                               # index label, 2x scale, top-left
            bits = DIGITS[d]
            for r in range(5):
                for c in range(3):
                    if bits[r * 3 + c] == '1':
                        for yy in range(2):
                            for xx in range(2):
                                o = ((cy + 1 + r * 2 + yy) * W + x0 + c * 2 + xx) * 4
                                img[o:o + 4] = b'\xff\xff\x40\xff'
            x0 += 8
        try:
            w, h, px = arc.decode(i)
        except Exception:
            continue
        scale = 1
        while w // scale > cell or h // scale > cell:
            scale += 1
        z = zoom if scale == 1 and w * zoom <= cell and h * zoom <= cell else 1
        tw, th = (w // scale) * z, (h // scale) * z
        ox, oy = cx + 2 + (cell - tw) // 2, cy + 2 + label_h + (cell - th) // 2
        for y in range(th):
            for x in range(tw):
                s = ((y // z * scale) * w + x // z * scale) * 4
                a = px[s + 3]
                if not a:
                    continue
                o = ((oy + y) * W + ox + x) * 4
                for c in range(3):
                    img[o + c] = (px[s + c] * a + img[o + c] * (255 - a)) // 255
    return W, H, bytes(img)


def parse_only(spec, count):
    if not spec:
        return list(range(count))
    out = []
    for part in spec.split(','):
        if '-' in part:
            a, b = part.split('-')
            out.extend(range(int(a), min(int(b), count - 1) + 1))
        else:
            out.append(int(part))
    return [i for i in out if 0 <= i < count]


def find_archive(name, data_dir):
    if os.path.exists(name):
        return name
    dirs = [data_dir] if data_dir else []
    if os.environ.get('DOM6_DATA'):
        dirs.append(os.environ['DOM6_DATA'])
    dirs += DEFAULT_DATA
    for d in dirs:
        p = os.path.join(d, name)
        if os.path.exists(p):
            return p
    sys.exit(f'{name}: not found (give a path, or --data with the game\'s data folder)')


def refuse_repo(path):
    real = os.path.realpath(path)
    if real == REPO or real.startswith(REPO + os.sep):
        sys.exit(f'{path} is inside the repository: the game\'s art must not be committed. '
                 'Write to a scratch folder outside it.')


def main():
    ap = argparse.ArgumentParser(description='Dominions 6 .trs image archives')
    ap.add_argument('--data', help="the game's data folder")
    sub = ap.add_subparsers(dest='cmd', required=True)
    p = sub.add_parser('list'); p.add_argument('archive')
    p = sub.add_parser('extract'); p.add_argument('archive'); p.add_argument('outdir')
    p.add_argument('--only')
    p = sub.add_parser('sheet'); p.add_argument('archive'); p.add_argument('out')
    p.add_argument('--only'); p.add_argument('--cols', type=int, default=16)
    p.add_argument('--cell', type=int, default=64); p.add_argument('--bg', default='404040')
    p.add_argument('--zoom', type=int, default=1, help='enlarge small images (nearest pixel)')
    a = ap.parse_args()
    arc = TrsArchive(find_archive(a.archive, a.data))

    if a.cmd == 'list':
        print(f'# {a.archive}: {arc.count} images, version {arc.version}, pitch {arc.pitch}, '
              f'groups {len(arc.groups)}')
        print('index\tsize\tflags\tencoding\toffset\tlength\tgroup')
        for im in arc.images:
            print(f'{im.index}\t{im.width}x{im.height}\t{im.flags}\t{arc.encoding(im)}\t'
                  f'{im.offset:#x}\t{im.length}\t{im.group}')
    elif a.cmd == 'extract':
        refuse_repo(a.outdir)
        os.makedirs(a.outdir, exist_ok=True)
        idx = parse_only(a.only, arc.count)
        for i in idx:
            w, h, px = arc.decode(i)
            write_png(os.path.join(a.outdir, f'{i:04d}.png'), w, h, px)
        print(f'{len(idx)} images -> {a.outdir}')
    elif a.cmd == 'sheet':
        refuse_repo(a.out)
        bg = bytes.fromhex(a.bg)
        W, H, px = make_sheet(arc, parse_only(a.only, arc.count), a.cols, a.cell, bg, a.zoom)
        write_png(a.out, W, H, px)
        print(f'{W}x{H} -> {a.out}')


if __name__ == '__main__':
    main()
