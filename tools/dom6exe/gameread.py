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


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('a')
    ap.add_argument('b', nargs='?')
    ap.add_argument('--rules', help='the rules JSON (default: the newest tools/dom6exe/data/dmread-*.json)')
    ap.add_argument('--json', help='write the differences here')
    ap.add_argument('--show', type=int, default=12, help='differences to print (default 12)')
    ap.add_argument('--dump', action='store_true', help='print what the game reads from A')
    args = ap.parse_args()
    rules = dmread.SavedRules(args.rules or default_rules())
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
