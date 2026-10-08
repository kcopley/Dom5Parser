"""What the game reads from a .dm file, and whether two files read the same in game.

The referee for the editor's saves: Dominions 6's own way of reading a mod (dmread.py, from the
exe's code), not another parser's. Each of the game's 15 passes (one per entity type) is replayed
on both files; each pass's blocks and the commands in them are compared with their arguments as
the game takes them:
- a number argument as sscanf reads it with the command's format from the exe ("%d %d" for
  #magicskill, one number for most): "5", "+5", "5.0" and "5 -- note" are all 5, and values past
  the ones the game reads don't count;
- a name argument (#weapon "Spear") as the first quoted string on the line, ignoring case (the
  game's lookups do);
- a text (#name, #descr, #msg, ...) exactly as the game keeps it: up to the next quote across
  lines, tabs as spaces, "--" to the end of a line and every "$" dropped, cut at its limit.
The order of blocks and of commands in a block counts (later lines override earlier ones, clears
and copies act on what's above them).

  python3 tools/dom6exe/gameread.py A.dm B.dm            differences, by pass, with lines
  python3 tools/dom6exe/gameread.py A.dm B.dm --json OUT  all of them as JSON
  python3 tools/dom6exe/gameread.py A.dm --dump           what the game reads, pass by pass
  python3 tools/dom6exe/gameread.py --merge M.dm --map M.map.json
      a merged mod (Dom5Tests merge, docs/MERGING.md) against its parts read one after another, as
      the game reads enabled mods: per pass, the parts' blocks in order must be the merged file's,
      numbers mapped through what moved and names taken as the game finds them (the lowest number
      with that name, among the game's, the separate needed mods' and the parts read so far); and
      units that turn into the next or previous number (#shrinkhp, #growhp, #xpshape) keep that
      neighbour.

Exit status 0 when the game reads both the same, 1 when not. The rules come from
tools/dom6exe/data/dmread-<version>.json (written by `dom6exe.py dmread --out ...` from the exe),
so this runs without the game installed.
"""
import argparse, difflib, glob, json, os, re, sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import dmread

HERE = os.path.dirname(os.path.abspath(__file__))
NUM = re.compile(rb'[+-]?\d+')
FLOAT = re.compile(rb'[+-]?(\d+\.?\d*|\.\d+)([eE][+-]?\d+)?')


def default_rules():
    found = sorted(glob.glob(os.path.join(HERE, 'data', 'dmread-*.json')))
    if not found:
        sys.exit('no tools/dom6exe/data/dmread-*.json: write one with dom6exe.py dmread --out')
    return found[-1]


def scan(arg, fmt):
    """sscanf(arg, fmt): the values read, stopping at the first that doesn't parse."""
    out, i = [], 0
    for conv in fmt.split():
        while i < len(arg) and arg[i:i + 1] in (b' ', b'\t'):
            i += 1
        if conv == 's':
            m = re.compile(rb'\S+').match(arg, i)
        elif conv == 'f':
            m = FLOAT.match(arg, i)
        else:
            m = NUM.match(arg, i)
        if not m:
            break
        tok = m.group()
        out.append(float(tok) if conv == 'f' else int(tok) if conv != 's' else tok.decode('utf-8', 'replace'))
        i = m.end()
    return tuple(out)


def argument(arg, fmt):
    """A rest-of-line argument as the game takes it: a quoted name (lookups ignore case and read
    '%' as '_'), or the numbers its format reads."""
    a = arg.lstrip(b' \t')
    if a.startswith(b'"'):
        end = a.find(b'"', 1)
        if end > 0:
            return ('name', a[1:end].decode('utf-8', 'replace').lower().replace('%', '_'))
        return ()   # not closed on its line: the game reads nothing (no name, no number)
    return scan(a, fmt or 'd')


class Reading:
    """One file as the game reads it: per pass, its blocks (header, commands, line numbers)."""

    def __init__(self, path, rules):
        self.path = path
        data = open(path, 'rb').read()
        self.text = dmread.Text(data)
        self.passes = {}
        for ctx in dmread.STARTS:
            fmts = rules.formats.get(ctx, {})
            blocks, cur = [], None
            if ctx == 'global':
                cur = {'head': ('global',), 'line': 0, 'items': []}
                blocks.append(cur)
            for pos, kind, c, v in dmread.read_pass(self.text.t, ctx, rules):
                line = self.text.line(pos)
                if kind == 'start':
                    cur = {'head': (c, argument(v, fmts.get(c, 'd'))), 'line': line, 'items': []}
                    blocks.append(cur)
                    continue
                if kind == 'command':
                    item = (c, argument(v, fmts.get(c)))
                elif kind == 'string':
                    s, closed = v
                    item = (c, ('text', None if s is None else s.decode('utf-8', 'replace'), closed))
                elif kind == 'end':
                    item = ('end',)
                else:   # fatal, ignored
                    item = (kind, c, v if isinstance(v, str) else '')
                target = cur if cur is not None else {'head': ('outside a block',), 'line': line, 'items': []}
                if cur is None:
                    blocks.append(target)
                    cur = target
                target['items'].append((item, line))
                if kind == 'end' and ctx != 'global':
                    cur = None
            self.passes[ctx] = blocks


def show(item):
    c = item[0]
    if c in ('fatal', 'ignored'):
        return '%s #%s %s' % (c, item[1], item[2])
    if c == 'end':
        return '#end'
    a = item[1]
    if isinstance(a, tuple) and a and a[0] == 'text':
        t = a[1]
        return '#%s %s' % (c, 'no text' if t is None else '"%s%s' % (t[:70].replace('\n', '\\n'), '…' if len(t) > 70 else '"' if a[2] else ' (not closed)'))
    if isinstance(a, tuple) and a and a[0] in ('name', 'name?'):
        return '#%s "%s"%s' % (c, a[1], '' if a[0] == 'name' else ' (not closed: nothing found)')
    return '#%s %s' % (c, ' '.join(str(x) for x in a)) if a else '#' + c


def closed_text(x, y):
    """Whether x and y are the same text whose closing quote is missing on its line in x (the
    game reads on to the next quote, whatever follows): y closed at the end of that line, or
    running on into what follows differently."""
    if len(x) < 2 or len(y) < 2 or x[0] != y[0] or not isinstance(x[1], tuple) or not isinstance(y[1], tuple):
        return False
    if x[1][:1] != ('text',) or y[1][:1] != ('text',) or x[1][1] is None or y[1][1] is None:
        return False
    first = x[1][1].split('\n', 1)
    return len(first) > 1 and first[0].rstrip() == y[1][1].split('\n', 1)[0].rstrip()


def same_final_text(ia, ib, changed):
    """Whether the changed items are all one text command whose last value in the block (the one
    the game keeps: a later #descr replaces an earlier one) is the same in both."""
    names = {it[0] for it in changed}
    if len(names) != 1 or not all(len(it) > 1 and isinstance(it[1], tuple) and it[1][:1] == ('text',) for it in changed):
        return False
    c = names.pop()
    last = lambda items: next((it for it in reversed(items) if it[0] == c), None)
    x, y = last(ia), last(ib)
    return x is not None and y is not None and (x == y or closed_text(x, y))


def compare(a, b):
    """The differences between two readings: per pass, blocks only in one, and commands that
    differ inside blocks both have (by header, in order). A text whose missing closing quote a
    rewrite added is its own kind, 'text closed'."""
    out = []
    for ctx in dmread.STARTS:
        ba, bb = a.passes[ctx], b.passes[ctx]
        heads = difflib.SequenceMatcher(None, [x['head'] for x in ba], [x['head'] for x in bb], autojunk=False)
        for op, i1, i2, j1, j2 in heads.get_opcodes():
            if op == 'equal':
                for x, y in zip(ba[i1:i2], bb[j1:j2]):
                    ia, ib = [it for it, _ in x['items']], [it for it, _ in y['items']]
                    if ia == ib:
                        continue
                    sm = difflib.SequenceMatcher(None, ia, ib, autojunk=False)
                    for o, k1, k2, l1, l2 in sm.get_opcodes():
                        if o == 'replace' and k2 - k1 == l2 - l1 and all(closed_text(p, q) for p, q in zip(ia[k1:k2], ib[l1:l2])):
                            o = 'text closed'
                        elif o != 'equal' and same_final_text(ia, ib, ia[k1:k2] + ib[l1:l2]):
                            o = 'same final text'
                        if o != 'equal':
                            out.append({'pass': ctx, 'block': show(x['head'] if len(x['head']) > 1 else x['head'] + ((),)) if x['head'][0] != 'global' else 'global',
                                        'lines': [x['line'], y['line']], 'change': o,
                                        'a': [(show(it), ln) for it, ln in x['items'][k1:k2]],
                                        'b': [(show(it), ln) for it, ln in y['items'][l1:l2]]})
            else:
                for x in ba[i1:i2]:
                    out.append({'pass': ctx, 'block': show(x['head'] + ((),) if len(x['head']) == 1 else x['head']), 'lines': [x['line'], None],
                                'change': 'only in A', 'a': [(show(it), ln) for it, ln in x['items'][:8]], 'b': []})
                for y in bb[j1:j2]:
                    out.append({'pass': ctx, 'block': show(y['head'] + ((),) if len(y['head']) == 1 else y['head']), 'lines': [None, y['line']],
                                'change': 'only in B', 'a': [], 'b': [(show(it), ln) for it, ln in y['items'][:8]]})
    return out


# what the merge map calls each kind of number that has names, and its pass
NAMED = {'monster': 'monster', 'weapon': 'weapon', 'armor': 'armor', 'spell': 'spell', 'item': 'item',
         'site': 'site', 'nation': 'nation', 'poptype': 'poptype', 'nametype': 'nametype'}


# what a block's header number is, per pass (a template is numbered by its nation)
HEAD_KIND = dict(NAMED, template='nation')


def names(readings):
    """Per pass: name -> (the lowest number a #new/#select block of that number gives it with
    #name, the reading it's in: its moves map it)."""
    out = {}
    for i, r in enumerate(readings):
        for ctx in NAMED.values():
            table = out.setdefault(ctx, {})
            for blk in r.passes.get(ctx, []):
                head = blk['head']
                if len(head) < 2 or not head[1] or not isinstance(head[1][0], int):
                    continue
                for it, _ in blk['items']:
                    if it[0] == 'name' and len(it) > 1 and isinstance(it[1], tuple) and it[1][:1] == ('text',) and it[1][1]:
                        n = it[1][1].lower().replace('%', '_')
                        if n not in table or head[1][0] < table[n][0]:
                            table[n] = (head[1][0], i)
    return out


FORM_NUMBER = re.compile(r'\((\d+)\)\s*$')

# commands that name a file (relative to the .dm): a merge copies it and rewrites the path, so the
# file is compared, not the path
PATHS = {'spr1', 'spr2', 'xspr1', 'xspr2', 'spr', 'flag', 'indepflag', 'mountedspr1', 'mountedspr2',
         'unmountedspr1', 'unmountedspr2', 'sample', 'icon'}
_files = {}


def file_of(reading, text):
    """The file a path names, from the reading's .dm: its bytes' hash, or None if it isn't there."""
    rel = text.split('"')[0].strip().replace('\\', '/')
    while rel.startswith('./'):
        rel = rel[2:]
    full = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(reading.path)), rel.lstrip('/')))
    if full not in _files:
        try:
            import hashlib
            _files[full] = hashlib.sha1(open(full, 'rb').read()).hexdigest()
        except OSError:
            _files[full] = None
    return _files[full]


def normalized(reading, ctx, kinds, effects, moves, table, reading_moves):
    """A pass's blocks with numbers mapped (moves: kind -> old -> new) and names as numbers (the
    number the name finds, mapped by the moves of the reading it's in: reading_moves)."""
    def named(kind, name):
        num, i = table[NAMED[kind]][name]
        return int(reading_moves[i].get(kind, {}).get(str(num), num))
    out = []
    for blk in reading.passes.get(ctx, []):
        head = blk['head']
        own = HEAD_KIND.get(ctx)
        if len(head) > 1 and head[1]:
            a = head[1]
            if a[0] == 'name' and own in NAMED and a[1] in table.get(NAMED[own], {}):
                a = (named(own, a[1]),)
            elif isinstance(a[0], int) and own:
                a = (int(moves.get(own, {}).get(str(a[0]), a[0])),) + tuple(a[1:])
            head = (head[0], a)
        effect = next((it[1][0] for it, _ in blk['items'] if it[0] == 'effect' and len(it) > 1 and it[1] and isinstance(it[1][0], int)), None)
        items = []
        for it, line in blk['items']:
            c = it[0]
            kind = kinds.get(ctx, {}).get(c)
            if kind == 'spell damage':
                kind = effects.get(str(effect))
            if kind and len(it) > 1 and isinstance(it[1], tuple) and it[1]:
                a = it[1]
                k = kind
                if a[0] == 'name':
                    k = 'monster' if kind == 'monster or tag' else kind
                    if k in NAMED and a[1] in table.get(NAMED[k], {}):
                        a = (named(k, a[1]),)
                elif isinstance(a[0], int):
                    v = a[0]
                    if kind == 'monster or tag':
                        k, v = ('monster tag', -v) if v < 0 else ('monster', v)
                    new = moves.get(k, {}).get(str(v))
                    if new is not None:
                        new = int(new)
                        a = ((-new if kind == 'monster or tag' and a[0] < 0 else new),) + tuple(a[1:])
                it = (c, a)
            elif c in PATHS and len(it) > 1 and isinstance(it[1], tuple) and it[1][:1] == ('text',) and it[1][1]:
                h = file_of(reading, it[1][1])
                if h:
                    it = (c, ('file', h))
            elif ctx == 'template' and c == 'form' and len(it) > 1 and isinstance(it[1], tuple) and it[1][:1] == ('text',) and it[1][1]:
                # "Dragon (265)": the monster's number in the name (the manual)
                t = it[1][1]
                f = FORM_NUMBER.search(t)
                if f:
                    t = t[:f.start(1)] + str(int(moves.get('monster', {}).get(f.group(1), f.group(1)))) + t[f.end(1):]
                    it = (c, ('text', t) + tuple(it[1][2:]))
            items.append(it)
        out.append({'head': head, 'line': blk['line'], 'items': items})
    return out


def local(path):
    """A path the map wrote on Windows (D:\\..., \\\\wsl.localhost\\Ubuntu\\...) as this system sees it."""
    if os.name == 'nt' or not path:
        return path
    w = re.match(r'^\\\\wsl(?:\.localhost|\$)\\[^\\]+(\\.*)$', path)
    if w:
        return w.group(1).replace('\\', '/')
    d = re.match(r'^([A-Za-z]):\\(.*)$', path)
    if d:
        return '/mnt/%s/%s' % (d.group(1).lower(), d.group(2).replace('\\', '/'))
    return path


def merge_check(merged_path, map_path, rules, vanilla_path, limit):
    m = json.load(open(map_path))
    for p in m['parts']:
        p['file'] = local(p['file'])
    m['separate'] = [local(f) for f in m.get('separate', [])]
    kinds, effects = m['commands'], m['spell_effects']
    vanilla = Reading(vanilla_path, rules) if vanilla_path and os.path.exists(vanilla_path) else None
    separate = [Reading(f, rules) for f in m.get('separate', [])]
    parts = [(Reading(p['file'], rules), p.get('moves', {})) for p in m['parts']]
    merged = Reading(merged_path, rules)
    base = ([vanilla] if vanilla else []) + separate
    diffs, known = [], []
    # a part whose file doesn't end with a line break: read alone, the game drops its last byte
    # (its last #end is #en); inside the merged file it doesn't: an #end more there
    unended = {id(r) for r, _ in parts if not open(r.path, 'rb').read().endswith(b'\n')}
    last_blocks = set()
    for r, _ in parts:
        if id(r) in unended:
            for ctx in dmread.STARTS:
                if r.passes.get(ctx):
                    last_blocks.add((ctx, r.passes[ctx][-1]['line'], r.path))

    def run_on(items):
        """A text without its closing quote on its line: it runs on over the next lines, which a
        merge may have rewritten (a path, a reference): its text changes with them."""
        return items and all(len(t) > 1 and isinstance(t[1], tuple) and t[1][:1] == ('text',) and t[1][1] and '\n' in t[1][1] for t in items)
    for ctx in dmread.STARTS:
        expected = []
        for i, (r, moves) in enumerate(parts):
            # names as the game finds them while reading this part: the game's, the separate mods',
            # the parts before it and its own
            table = names(base + [x for x, _ in parts[:i + 1]])
            blocks = normalized(r, ctx, kinds, effects, moves, table, [{}] * len(base) + [mv for _, mv in parts[:i + 1]])
            for b in blocks:
                b['path'] = r.path
            if ctx == 'global' and expected and blocks:
                expected[0]['items'].extend(blocks[0]['items'])
                blocks = blocks[1:]
            expected.extend(blocks)
        got = normalized(merged, ctx, kinds, effects, {}, names(base + [merged]), [{}] * (len(base) + 1))
        heads = difflib.SequenceMatcher(None, [x['head'] for x in expected], [x['head'] for x in got], autojunk=False)
        for op, i1, i2, j1, j2 in heads.get_opcodes():
            if op == 'equal':
                for x, y in zip(expected[i1:i2], got[j1:j2]):
                    if x['items'] != y['items']:
                        sm = difflib.SequenceMatcher(None, x['items'], y['items'], autojunk=False)
                        for o, k1, k2, l1, l2 in sm.get_opcodes():
                            if o == 'equal':
                                continue
                            d = (ctx, show_head(x['head']), y['line'], [show(t) for t in x['items'][k1:k2]], [show(t) for t in y['items'][l1:l2]])
                            if o == 'insert' and y['items'][l1:l2] == [('end',)] and (ctx, x['line'], x.get('path')) in last_blocks:
                                known.append(('#end restored (the part ends without a line break)',) + d)
                            elif o == 'replace' and run_on(x['items'][k1:k2]) and run_on(y['items'][l1:l2]):
                                known.append(('a text without its closing quote runs over a rewritten line',) + d)
                            else:
                                diffs.append(d)
            else:
                for x in expected[i1:i2]:
                    diffs.append((ctx, show_head(x['head']), None, ['(block only in the parts)'], []))
                for y in got[j1:j2]:
                    diffs.append((ctx, show_head(y['head']), y['line'], [], ['(block only in the merged file)']))
    # units that turn into the next or previous number keep it
    chain = []
    for r, moves in parts:
        mon = {str(k): int(v) for k, v in moves.get('monster', {}).items()}
        at = lambda n: mon.get(str(n), n)
        for blk in r.passes.get('monster', []):
            head = blk['head']
            if len(head) < 2 or not head[1] or not isinstance(head[1][0], int):
                continue
            n, cmds = head[1][0], {it[0] for it, _ in blk['items']}
            for step, has in ((1, bool(cmds & {'shrinkhp', 'xpshape', 'labxpshape'}) and 'xpshapemon' not in cmds), (-1, 'growhp' in cmds)):
                if has and at(n + step) != at(n) + step:
                    chain.append('%s: monster %d -> %d turns into %d, now %d (should be %d)' % (os.path.basename(r.path), n, at(n), n + step, at(n + step), at(n) + step))
    print('%s: the game reads it %s; %d chained unit(s) out of order%s' % (
        os.path.basename(merged_path), 'as its %d parts one after another' % len(parts) if not diffs else 'differently from its parts: %d difference(s)' % len(diffs), len(chain),
        ''.join('; %d %s' % (n, k) for k, n in sorted({k[0]: sum(1 for x in known if x[0] == k[0]) for k in known}.items()))))
    for ctx, head, line, a, b in diffs[:limit]:
        print('  [%s] %s (merged line %s)' % (ctx, head, line))
        for s in a[:3]:
            print('      parts : %s' % s)
        for s in b[:3]:
            print('      merged: %s' % s)
    for c in chain[:limit]:
        print('  ' + c)
    return 1 if diffs or chain else 0


def show_head(head):
    return show(head + ((),) if len(head) == 1 else head)


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('a', nargs='?')
    ap.add_argument('b', nargs='?')
    ap.add_argument('--merge', help='a merged mod to check against its parts (with --map)')
    ap.add_argument('--map', help='the merge map (Dom5Tests merge writes MERGED.map.json)')
    ap.add_argument('--vanilla', help='the game data the names are found in too (default: vanilla.dm at the repo root)')
    ap.add_argument('--rules', help='the rules JSON (default: the newest tools/dom6exe/data/dmread-*.json)')
    ap.add_argument('--json', help='write the differences here')
    ap.add_argument('--show', type=int, default=12, help='differences to print (default 12)')
    ap.add_argument('--dump', action='store_true', help='print what the game reads from A')
    args = ap.parse_args()
    rules = dmread.SavedRules(args.rules or default_rules())
    if args.merge:
        vanilla = args.vanilla or os.path.join(HERE, '..', '..', 'vanilla.dm')
        return merge_check(args.merge, args.map or os.path.splitext(args.merge)[0] + '.map.json', rules, vanilla, args.show)
    a = Reading(args.a, rules)
    if args.dump or not args.b:
        for ctx, blocks in a.passes.items():
            for blk in blocks:
                print('[%s] line %d: %s' % (ctx, blk['line'], show(blk['head'] + ((),) if len(blk['head']) == 1 else blk['head'])))
                for it, ln in blk['items']:
                    print('    line %d: %s' % (ln, show(it)))
        return 0
    b = Reading(args.b, rules)
    every = compare(a, b)
    closed = [d for d in every if d['change'] == 'text closed']
    final = [d for d in every if d['change'] == 'same final text']
    diffs = [d for d in every if d['change'] not in ('text closed', 'same final text')]
    by_pass = {}
    for d in diffs:
        by_pass[d['pass']] = by_pass.get(d['pass'], 0) + 1
    print('%s -> %s: the game reads them %s%s' % (os.path.basename(args.a), os.path.basename(args.b),
                                                  'the same' if not diffs else 'differently: %d difference(s) (%s)' % (
                                                      len(diffs), ', '.join('%s %d' % kv for kv in sorted(by_pass.items()))),
                                                  ('; %d text(s) without a closing quote on their line read on differently' % len(closed) if closed else '')
                                                  + ('; %d text(s) set more times, to the same final text' % len(final) if final else '')))
    for d in diffs[:args.show]:
        print('  [%s] %s (lines %s / %s): %s' % (d['pass'], d['block'], d['lines'][0], d['lines'][1], d['change']))
        for s, ln in d['a'][:4]:
            print('      A line %s: %s' % (ln, s))
        for s, ln in d['b'][:4]:
            print('      B line %s: %s' % (ln, s))
    if args.json:
        json.dump({'a': args.a, 'b': args.b, 'rules': rules.game_version, 'differences': len(diffs),
                   'texts_closed': len(closed), 'same_final_text': len(final), 'by_pass': by_pass,
                   'items': diffs + closed + final}, open(args.json, 'w'), indent=1)
    return 1 if diffs else 0


if __name__ == '__main__':
    sys.exit(main())
