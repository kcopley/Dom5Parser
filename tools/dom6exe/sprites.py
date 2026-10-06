"""Which picture the game draws for each vanilla monster and item, read from Dominions6.exe
(dom6exe.py sprites). Numbers only: the pictures are the player's own data/*.trs files.

What the code does (6.37):
  At start the game converts every monster's sprite number (record +0x24, the table ends with
  -1) and every item's (record +0x2a): a number below 1000 is an image index in the archive; a
  number n of 1000 or more is image start(n / 1000) + n % 1000, where start(g) is where the
  archive's g-th group label begins (the labels after the image index, tools/gameart/README.md;
  start(0) = 0). Monsters use monster.trs, items item.trs. A unit's second (attack) frame is the
  next image. A vanilla unmounted sprite (ability 1017, which #unmountedspr1 removes) is
  converted the same way when the game draws a dismounted rider.
  A site's picture is sites.trs group (path + 1) (path 0-9; fire, air, ...), image #look when
  #look is 0-99, else image #level (0-3). A new site starts with look 0, path 0, level 0.

Found from the code, not fixed addresses:
  archive names   the game's list of .trs names (guistuff.trs, mapstuff.trs, item.trs, ...);
                  a draw call names an archive by its place in the list
  conversions     the functions that subtract 1000 * (n / 1000) (imul by -1000): the table each
                  one reads (a monster or item record field) and the archive it loads the group
                  starts of (mov ecx, archive before the call that opens it)
  unmounted       the converter for a single number (no table): the ability number read just
                  before each call to it
  sites           the function that clamps a site's path to 9 and level to 3 and compares the
                  look with 99
"""
import bisect, collections, re, struct

from dom6exe import TABLES, find_table, monster_table, monsters, num


def archive_names(exe):
    """The game's list of .trs archive names, in order (a draw call's archive number indexes it)."""
    d = exe.data
    o = d.find(b'guistuff.trs\0')
    ptr = d.find(struct.pack('<Q', exe.address(o)))
    if o < 0 or ptr < 0:
        raise SystemExit('archive name list not found')
    names = []
    while True:
        s = exe.cstr(struct.unpack_from('<Q', d, ptr + 8 * len(names))[0])
        if not s or not s.endswith('.trs'):
            return names
        names.append(s)


def _archive_before(ins, i, names, back=160):
    """The archive a converter opens: the last 'mov ecx, N' directly before a call, N an archive."""
    for k in range(i - 1, max(i - back, 0), -1):
        a, op, args, t = ins[k]
        m = re.match(r'ecx,(?:\[r\w+\+)?(0x[0-9a-f]+|\d+)\]?$', args) if op in ('mov', 'lea') else None
        if m and ins[k + 1][1] == 'call' and num(m.group(1)) < len(names):
            return names[num(m.group(1))]
    return None


def conversions(exe, tables):
    """For each function that subtracts 1000 * (n / 1000): the table field it reads and the
    archive it opens ({kind: (field offset, archive)}), and the functions that convert a single
    number (no table field: called with the number)."""
    ins = exe.disassembly()
    names = archive_names(exe)
    funcs = exe.functions()
    starts = [f[0] for f in funcs]
    index = {x[0]: k for k, x in enumerate(ins)}
    called = {x[2].split()[0] for x in ins if x[1] == 'call'}
    found, single = {}, []
    for i, (a, op, args, t) in enumerate(ins):
        if op != 'imul' or not args.endswith(',0xfffffc18'):
            continue
        lo, hi = funcs[bisect.bisect_right(starts, a) - 1]
        body = [x for x in ins[index.get(lo, i):i + 80] if x[0] < hi]
        fields = [(kind, x[3] - va) for x in body if x[1] == 'lea' and x[3]
                  for kind, (va, size) in tables.items() if va <= x[3] < va + size]
        if fields:
            arch = _archive_before(ins, i, names)
            for kind, off in fields:
                if arch and off >= 0x24:      # past the name
                    found[kind] = (off, arch)
        elif hex(lo) in called:
            single.append(lo)
    return found, single, names


def site_rule(exe, names):
    """The archive a site's picture is in, from the function comparing the look with 99 after
    clamping the path to 9 and the level to 3."""
    ins = exe.disassembly()
    for i, (a, op, args, t) in enumerate(ins):
        if op == 'cmp' and re.match(r'r\w+,0x63$', args):
            window = ins[max(i - 30, 0):i]
            clamps = {x[2].split(',')[1] for x in window if x[1] == 'mov' and re.match(r'e\w+,0x[39]$', x[2])}
            fields = {m.group(1) for x in window for m in [re.search(r'WORD PTR \[.*\+(0x2[8ac])\]', x[2])] if m}
            if clamps == {'0x3', '0x9'} and fields == {'0x28', '0x2a', '0x2c'}:
                arch = _archive_before(ins, i, names, back=80)
                if arch:
                    return {'archive': arch, 'look_below': 100, 'path_max': 9, 'level_max': 3, 'new_site': {'look': 0, 'path': 0, 'level': 0}}
    raise SystemExit('site picture function not found')


def unmounted_ability(exe, single):
    """The ability whose value the single-number converter is given: 'mov edx, N' before the
    call that reads it, just before the call to the converter."""
    ins = exe.disassembly()
    found = collections.Counter()
    targets = {hex(s) for s in single}
    for i, (a, op, args, t) in enumerate(ins):
        if op == 'call' and args.split()[0] in targets:
            for k in range(i - 1, max(i - 12, 0), -1):
                if ins[k][1] == 'mov' and re.match(r'edx,0x[0-9a-f]+$', ins[k][2]):
                    found[num(ins[k][2].split(',')[1])] += 1
                    break
    if not found:
        raise SystemExit('unmounted sprite ability not found')
    return found.most_common(1)[0][0]


def collect(exe):
    """The sprites-6.37.json contents: per monster and item the sprite number, and the site rule."""
    mon_start, mon_size = monster_table(exe)
    item_start, item_size = find_table(exe, TABLES['item'])
    tables = {'monster': (exe.address(mon_start), mon_size), 'item': (exe.address(item_start), item_size)}
    sprite, single, names = conversions(exe, tables)
    if set(sprite) != {'monster', 'item'} or not single:
        raise SystemExit('sprite conversions not found: %s' % sorted(sprite))
    ability = unmounted_ability(exe, single)

    mons = monsters(exe)
    off, arch = sprite['monster']
    mon, unmounted = {}, {}
    for i in sorted(mons):
        r = exe.data[mon_start + i * mon_size:mon_start + (i + 1) * mon_size]
        v = struct.unpack_from('<i', r, off)[0]
        if v == -1:
            break           # the end marker the game's conversion stops at
        if v > 0:
            mon[str(i)] = v
        u = mons[i]['abilities'].get(ability)
        if u and u > 0:
            unmounted[str(i)] = u
    ioff, iarch = sprite['item']
    items = {}
    for i in range(1, TABLES['item']['count']):
        r = exe.data[item_start + i * item_size:item_start + (i + 1) * item_size]
        if r[0x24] == 99:
            break           # the end marker the conversion stops at (constlevel, record +0x24, 99)
        v = struct.unpack_from('<h', r, ioff)[0]
        if r[:1] != b'\0' and v > 0:
            items[str(i)] = v
    res = {
        'game_version': exe.version, 'exe_sha256_16': exe.sha,
        'note': 'The sprite numbers the game stores for vanilla monsters and items (tools/dom6exe/sprites.py). '
                'A number n below 1000 is image n of the archive; 1000 or more is image start(n / 1000) + n % 1000, '
                "where start(g) is the first image of the archive's g-th group label (start(0) = 0). "
                'A unit\'s second (attack) frame is the next image. Sites: image (look 0-99, else level 0-3) of '
                'sites.trs group path + 1.',
        'monster': {'archive': arch, 'record_offset': off, 'sprites': mon,
                    'unmounted_ability': ability, 'unmounted': unmounted},
        'item': {'archive': iarch, 'record_offset': ioff, 'sprites': items},
        'site': site_rule(exe, names),
    }
    return res
