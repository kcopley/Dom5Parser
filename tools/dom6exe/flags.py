#!/usr/bin/env python3
"""How the game draws a nation's flag, read from Dominions6.exe (dom6exe.py sprites writes it
into sprites-6.37.json as "flag"). Numbers only: the pictures are the player's own
data/flag.trs.

What the code does (6.37; the function the game's log calls "createflags", 0x140122b10):
  A nation's flag is image nation + 1 of flag.trs. Images 1-5 (nations 0-4: Independents and
  the special monster slots; image 0 is the unowned flag) are drawn as they are in the file
  (#indepflag replaces images 0-5). For every nation from 5 to 499 the game builds the image
  itself, at start and again after reading the mods (so a mod's #color and #secondarycolor
  recolor a vanilla flag, and a mod nation gets a plain one):
    1. image 501 (the pole),
    2. image 502 (the cloth) with each channel multiplied by the nation's #color
       (record +0x94, three floats 0-1; byte = channel * int(c * 255) / 255, truncated),
    3. image 503 (the cloth's border) multiplied the same way by #secondarycolor (+0xa0),
    4. for nations up to 135, image 500 + nation (its emblem: Arcoscephale's caduceus,
       Ermor's eagle, ...) as it is,
  each blended over the last by its alpha, then stored back into flag.trs image nation + 1 as
  RGB565 (a pixel nothing covers is transparent; one that comes out 0x0000 is too). A mod's
  #flag file replaces that image afterwards: it wins. The flag code reads #secondarycolor as
  it is: a nation without one (a mod's new nation: its colors start at 0) gets a black border
  (the animated background, not the flag, falls back to #color).

Found from the code, not fixed addresses: the function is the one that opens the flag archive
(its place in the game's archive list) and steps through the nation table by its record size;
from it the three part images it loads, the record fields it multiplies by 255.0, the first
and last nation of its loop, the last nation with an emblem and the emblem's image (lea
edx,[nation + N]), and the image it stores the flag into (nation + 1).

  python3 tools/dom6exe/flags.py sheet OUT.png [--nations 5,8,9] [--data DIR] [--exe PATH]
renders the composed flags of the vanilla nations (or the ones listed) to a contact sheet,
outside the repository, to check them by eye; `check` prints a checksum of their pixels, the one
`Dom5Editor --snapshot --flags OUT.png` logs for the editor's (they agree when the editor builds
the flags the same way); `rule` prints what `dom6exe.py sprites` writes as "flag".
"""
import argparse, bisect, os, re, struct, sys

from dom6exe import DEFAULT_EXE, TABLES, Exe, find_table, num


def _function_of(exe, ins, index, i):
    funcs = exe.functions()
    starts = [f[0] for f in funcs]
    lo, hi = funcs[bisect.bisect_right(starts, ins[i][0]) - 1]
    k = index[lo]
    out = []
    while k < len(ins) and ins[k][0] < hi:
        out.append(ins[k])
        k += 1
    return out


def rule(exe):
    """The flag recipe: {archive, parts, color fields, nation range, emblem, image offset}."""
    import sprites
    names = sprites.archive_names(exe)
    archive = names.index('flag.trs')
    start, size = find_table(exe, TABLES['nation'])
    table = exe.address(start)
    ins = exe.disassembly()
    index = {x[0]: k for k, x in enumerate(ins)}
    found = None
    for i, (a, op, args, t) in enumerate(ins):
        # the loop's step through the nation table
        if op != 'add' or not re.match(r'r\w+,%s$' % hex(size), args):
            continue
        body = _function_of(exe, ins, index, i)
        opens = any(x[1] == 'mov' and x[2] == 'ecx,%s' % hex(archive) for x in body)
        ptr = [x for x in body if x[1] == 'lea' and x[3] and table <= x[3] < table + TABLES['nation']['count'] * size]
        if opens and len(ptr) == 1 and any(x[1] == 'mulss' for x in body):
            found = (body, ptr[0])
            break
    if not found:
        raise SystemExit('flag function not found')
    body, ptr = found
    reg = ptr[2].split(',')[0]

    # the part images, in the order they are loaded before the loop (the first call preloads them)
    parts = []
    for x in body[:body.index(ptr)]:
        m = re.match(r'edx,(0x[0-9a-f]+)$', x[2])
        if x[1] == 'mov' and m and 0x100 <= num(m.group(1)) < 0x1000 and num(m.group(1)) not in parts:
            parts.append(num(m.group(1)))
    if len(parts) != 3:
        raise SystemExit('flag parts: expected three images, found %s' % parts)

    # the colors: float loads through the table pointer, in two triples (cloth, then border)
    loads = []
    for x in body:
        m = re.match(r'xmm\d+,DWORD PTR \[%s([+-]0x[0-9a-f]+)?\]$' % reg, x[2])
        if x[1] == 'movss' and m:
            off = m.group(1)
            loads.append(0 if not off else (num(off[1:]) if off[0] == '+' else -num(off[1:])))
    if len(loads) != 6:
        raise SystemExit('flag colors: expected six float loads, found %s' % loads)
    first, field = divmod(ptr[3] - table, size)
    colors = [field + min(loads[:3]), field + min(loads[3:])]
    if any(sorted(x - min(t) for x in t) != [0, 4, 8] for t in (loads[:3], loads[3:])):
        raise SystemExit('flag colors: not two runs of three floats: %s' % loads)
    scale = None
    for x in body:
        if x[1] == 'movss' and x[3] and exe.offset(x[3]) is not None:
            v = struct.unpack_from('<f', exe.data, exe.offset(x[3]))[0]
            if v > 1:
                scale = v
                break

    # the loop: first nation (mov ebx,N before the loop), the last (cmp esi,N; jl), the image
    # (lea esi,[rbx+1]), the emblem (cmp ebx,N; jg ... lea edx,[rbx+N])
    nation = None
    at = body.index(ptr)
    for x in body[at:at + 6]:
        m = re.match(r'(e\w\w),(0x[0-9a-f]+)$', x[2])
        if x[1] == 'mov' and m and num(m.group(2)) == first:
            nation = m.group(1)
            break
    if nation is None:
        raise SystemExit('flag loop: no register starts at nation %d' % first)
    image = emblem = emblem_last = last = None
    step = None
    for k, x in enumerate(body):
        m = re.match(r'(e\w\w),\[r%s\+(0x[0-9a-f]+)\]$' % nation[1:], x[2])
        if x[1] == 'lea' and m and m.group(1) == 'edx':
            emblem = num(m.group(2))
        elif x[1] == 'lea' and m and image is None:
            image, step = num(m.group(2)), m.group(1)
        m = re.match(r'%s,(0x[0-9a-f]+)$' % nation, x[2])
        if x[1] == 'cmp' and m and body[k + 1][1] == 'jg':
            emblem_last = num(m.group(1))
    for k, x in enumerate(body):
        m = re.match(r'%s,(0x[0-9a-f]+)$' % (step or '-'), x[2])
        if x[1] == 'cmp' and m and any(y[1] == 'jl' for y in body[k + 1:k + 4]):
            last = num(m.group(1)) - 1
    if None in (image, emblem, emblem_last, last, scale):
        raise SystemExit('flag loop not understood: image %s emblem %s up to %s last %s scale %s' % (image, emblem, emblem_last, last, scale))
    return {
        'archive': names[archive], 'image': 'nation + %d' % image, 'image_offset': image,
        'composed': {'first': first, 'last': last},
        'pole': parts[0], 'cloth': parts[1], 'border': parts[2],
        'cloth_color': {'command': 'color', 'record_offset': colors[0]},
        'border_color': {'command': 'secondarycolor', 'record_offset': colors[1]},
        'tint': 'channel * int(c * %g) / %g, truncated' % (scale, scale),
        'emblem': {'image': 'nation + %d' % emblem, 'offset': emblem, 'last_nation': emblem_last},
    }


def collect(exe):
    """The "flag" entry of sprites-6.37.json: the rule, and the vanilla nations' flags."""
    r = rule(exe)
    start, size = find_table(exe, TABLES['nation'])
    fixed, composed = {}, {}
    for n in range(TABLES['nation']['count']):
        rec = exe.data[start + n * size:start + (n + 1) * size]
        name = rec[:36].split(b'\0')[0]
        if name == b'end':
            break
        if not name:
            continue
        if n < r['composed']['first']:
            fixed[str(n)] = n + r['image_offset']
        elif n <= r['composed']['last']:
            composed[str(n)] = n + r['emblem']['offset'] if n <= r['emblem']['last_nation'] else None
    r['fixed'] = fixed
    r['nations'] = composed
    r['note'] = ('A nation\'s flag is %s image nation + %d. Nations %d-%d: the image as it is in the file. Nations %d-%d: '
                 'composed at start and after the mods are read: image %d (pole), image %d (cloth) times #color, '
                 'image %d (border) times #secondarycolor (no fallback to #color), then for nations up to %d image '
                 'nation + %d (emblem; "nations" lists each vanilla one), each blended over the last by its alpha, '
                 'stored as RGB565 (0 is transparent). A mod\'s #flag file replaces the image.') % (
        r['archive'], r['image_offset'], 0, r['composed']['first'] - 1, r['composed']['first'], r['composed']['last'],
        r['pole'], r['cloth'], r['border'], r['emblem']['last_nation'], r['emblem']['offset'])
    return r


# ---------------------------------------------------------------------------------------------
# composing a flag (to check the rule by eye; the editor does the same, Dom5Editor/Sprites/GameArt.cs)

def _canvas(arc, i, w=128, h=128):
    """Image i on a w x h RGBA canvas, top left (how the game loads a part into its buffer)."""
    out = bytearray(w * h * 4)
    iw, ih, px = arc.decode(i)
    for y in range(min(ih, h)):
        for x in range(min(iw, w)):
            s, o = (y * iw + x) * 4, (y * w + x) * 4
            out[o:o + 4] = px[s:s + 4]
    return out


def _tint(px, rgb):
    out = bytearray(px)
    for o in range(0, len(out), 4):
        if out[o + 3]:
            for c in range(3):
                out[o + c] = out[o + c] * rgb[c] // 255
    return out


def _over(dst, src):
    for o in range(0, len(dst), 4):
        a = src[o + 3]
        if a:
            for c in range(3):
                dst[o + c] = (src[o + c] * a + dst[o + c] * (255 - a)) // 255
            dst[o + 3] = 255


def _as_stored(px):
    """The flag as the game stores it (RGB565, alpha dropped) and draws it again."""
    out = bytearray(len(px))
    for o in range(0, len(px), 4):
        if not px[o + 3]:
            continue
        r, g, b = px[o] & 0xF8, px[o + 1] & 0xFC, px[o + 2] & 0xF8
        v = r << 8 | g << 3 | b >> 3
        if v == 0:
            continue
        if v == 0xF81F:
            out[o + 3] = 0x80
        else:
            out[o:o + 4] = bytes((r, g, b, 255))
    return bytes(out)


def tint_bytes(floats, scale=255.0):
    """int(c * 255) in single precision, as the game computes it (mulss, cvttss2si)."""
    s = struct.unpack('<f', struct.pack('<f', scale))[0]
    return [int(struct.unpack('<f', struct.pack('<f', f * s))[0]) for f in floats]


def compose(arc, r, nation, color, secondary):
    """Nation's flag as RGBA (128 x 128), from its #color and #secondarycolor (floats 0-1)."""
    if nation < r['composed']['first']:
        return arc.decode(nation + r['image_offset'])[2]
    px = _canvas(arc, r['pole'])
    _over(px, _tint(_canvas(arc, r['cloth']), tint_bytes(color)))
    _over(px, _tint(_canvas(arc, r['border']), tint_bytes(secondary)))
    if nation <= r['emblem']['last_nation']:
        _over(px, _canvas(arc, nation + r['emblem']['offset']))
    return _as_stored(px)


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('what', choices=['rule', 'sheet', 'check'])
    ap.add_argument('out', nargs='?')
    ap.add_argument('--exe', default=os.environ.get('DOM6_EXE', DEFAULT_EXE))
    ap.add_argument('--data', help="the game's data folder (default: next to the exe)")
    ap.add_argument('--nations', help='e.g. 5,8,9-12 (default: every vanilla nation)')
    ap.add_argument('--cols', type=int, default=12)
    a = ap.parse_args()
    exe = Exe(a.exe)
    if a.what == 'rule':
        import json
        print(json.dumps(collect(exe), indent=1))
        return
    sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'gameart'))
    import trs
    r = collect(exe)
    arc = trs.TrsArchive(trs.find_archive(r['archive'], a.data or os.path.join(os.path.dirname(a.exe), 'data')))
    start, size = find_table(exe, TABLES['nation'])
    nations = trs.parse_only(a.nations, TABLES['nation']['count']) if a.nations else \
        sorted(int(n) for n in list(r['fixed']) + list(r['nations']))

    def flag(n):
        rec = exe.data[start + n * size:start + (n + 1) * size]
        c1 = struct.unpack_from('<3f', rec, r['cloth_color']['record_offset'])
        c2 = struct.unpack_from('<3f', rec, r['border_color']['record_offset'])
        return compose(arc, r, n, c1, c2)

    if a.what == 'check':
        # the checksum Dom5Editor --snapshot --flags logs: each flag's BGRA pixels, in nation order
        import hashlib
        h = hashlib.sha256()
        for n in nations:
            px = bytearray(flag(n))
            px[0::4], px[2::4] = px[2::4], px[0::4]
            h.update(px)
        print('flags: %d nations; pixels sha256 %s' % (len(nations), h.hexdigest()[:16]))
        return
    if not a.out:
        raise SystemExit('sheet: give the PNG to write')
    trs.refuse_repo(a.out)

    class Flags:
        def decode(self, n):
            return 128, 128, flag(n)

    W, H, px = trs.make_sheet(Flags(), nations, a.cols, 128, bytes.fromhex('404040'))
    trs.write_png(a.out, W, H, px)
    print('%d flags, %dx%d -> %s' % (len(nations), W, H, a.out))


if __name__ == '__main__':
    main()
