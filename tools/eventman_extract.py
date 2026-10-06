#!/usr/bin/env python3
"""
Text of the event modding manual (dom6eventman.pdf) without a PDF library: its pages are
Haru-made, with UTF-16 text in Tj operators; columns are put back in reading order.

Usage: python3 tools/eventman_extract.py dom6eventman.pdf > docs/pdf_extracted/eventman.txt
"""
import re, zlib, sys
data = open(sys.argv[1], 'rb').read()
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
print('\n'.join(out))
