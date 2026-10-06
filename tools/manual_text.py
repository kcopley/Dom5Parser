#!/usr/bin/env python3
"""
Text of Illwinter's manuals (dom6modman.pdf, dom6eventman.pdf) without a PDF library: their
pages are made by Haru, with UTF-16 text in Tj operators; the two columns are put back in
reading order (left, then right). tools/command_hints.py reads it.

Usage: python3 tools/manual_text.py dom6eventman.pdf > docs/pdf_extracted/eventman.txt
"""
import re, zlib, sys


def pages_of(path):
    """The manual's pages as lists of lines, in reading order."""
    return [p.split('\n')[1:] for p in text_of(path).split('\n=== page ')]


def text_of(path):
    data = open(path, 'rb').read()
    objs = {int(m.group(1)): m.group(2) for m in re.finditer(rb'(\d+) 0 obj(.*?)endobj', data, re.S)}
    def stream(n):
        body = objs[n]
        m = re.search(rb'stream\r?\n(.*?)\r?\nendstream', body, re.S)
        raw = m.group(1)
        return zlib.decompress(raw) if b'FlateDecode' in body.split(b'stream')[0] else raw
    pages = [int(x) for x in re.findall(rb'(\d+) 0 R', re.search(rb'/Kids \[(.*?)\]', objs[2], re.S).group(1))]
    out = []
    for pi, p in enumerate(pages):
        c = int(re.search(rb'/Contents (\d+) 0 R', objs[p]).group(1))
        content = stream(c).decode('latin1')
        runs = []
        for bt in re.findall(r'BT(.*?)ET', content, re.S):
            m = re.search(r'([-\d.]+)\s+([-\d.]+)\s+Td', bt)
            x, y = (float(m.group(1)), float(m.group(2))) if m else (0, 0)
            txt = ''.join(bytes.fromhex(h).decode('utf-16-be', 'replace') for h in re.findall(r'<([0-9A-Fa-f]*)>\s*Tj', bt))
            bold = '/F2' in bt or '/F3' in bt
            runs.append((-round(y, 0), x, txt))
        runs.sort(key=lambda r: (0 if r[1] < 290 or pi == 0 else 1, r[0], r[1]))
        lines = []
        lasty = None
        for y, x, t in runs:
            if lasty is not None and y == lasty and (x < 290) == (lastx < 290):
                gap = '  |  ' if x > lastx + 150 else ' '
                lines[-1] += gap + t
            else:
                lines.append(t)
            lasty = y; lastx = x
        out.append(f'=== page {pi+1} ===\n' + '\n'.join(lines))
    return '\n'.join(out)


if __name__ == '__main__':
    print(text_of(sys.argv[1]))
