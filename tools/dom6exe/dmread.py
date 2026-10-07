"""How the game reads a .dm file, as found in Dominions6.exe's code (dom6exe.py dmread).

The README's "Reading .dm files" has the evidence. In short, the game reads each mod 15 times,
once per entity type (a pass). Each pass prepares the file text (prepare() below) and then looks
at every '#' in the whole text, wherever it is (start of a line or not, inside quotes or not).
At a '#' it compares the text after it with its commands: outside a block only its #new.../
#select..., inside one #end, the other types' #new.../#select... (a fatal error for most types)
and its own commands. A command reads its argument from the rest of the line, or a quoted string:
the opening quote on the same line, then everything up to the next quote, across lines, up to a
maximum length. The scan then goes on right after the command name; commands like #name and #msg
skip their string's length too, the description texts (#descr, #details, #summary, ...) don't, so
a '#' in them is read as a command.

This module derives what it needs from the exe (the string commands per type, their maximum
length and whether they skip their text; the #new/#select list; which types refuse a #new/#select
inside a block or a missing #end at the end of the file) and replays the passes over a mod:

  python3 tools/dom6exe/dom6exe.py dmread --mod FILE              where the game reads the file
                                                                  differently from a line-by-line
                                                                  reading (summary + examples)
  python3 tools/dom6exe/dom6exe.py dmread --mod FILE --lines 2540-2550   the commands each pass
                                                                  reads there

Approximations (the README says what the game does exactly): a command name matches when the
text after the '#' starts with it and the next character isn't a letter or digit (the game's
matcher; its generic handlers want a space after a command that takes an argument, and accept
some suffixes after one that doesn't); the longest such name wins (the game takes the first it
checks; no two of its names collide that way). Arguments are shown, not parsed.
"""
import bisect, collections, re, struct

from dom6exe import BASE, Parser, num, reg64

# The commands that open a block in each pass (read outside a block). The general pass has no
# blocks: its commands are read anywhere.
STARTS = {
    'sound': ('selectsound',), 'weapon': ('newweapon', 'selectweapon'), 'armor': ('newarmor', 'selectarmor'),
    'monster': ('newmonster', 'selectmonster'), 'nametype': ('selectnametype',), 'bless': ('selectbless',),
    'site': ('newsite', 'selectsite'), 'nation': ('newnation', 'selectnation'), 'spell': ('newspell', 'selectspell'),
    'item': ('newitem', 'selectitem'), 'global': (), 'poptype': ('selectpoptype',), 'merc': ('newmerc',),
    'event': ('newevent', 'selectevent'), 'template': ('newtemplate',),
}
LINE_MAX = 2499         # the rest-of-line reader copies at most 2499 characters (0x9c3)
# string commands whose argument is always text (a missing opening quote reads nothing; for #descr
# and #name the game stops with "bad descr ..." / "#name, empty name")
TEXT_COMMANDS = {'descr', 'details', 'portent', 'cure', 'summary', 'brief', 'msg', 'name', 'epithet',
                 'addname', 'bossname'}
IDENT = re.compile(rb'[A-Za-z0-9_]+')


def prepare(data):
    """The game's preparemodtext (0x140227150 in 6.37), run on the loaded file before each pass:
    the file's last byte is overwritten by the terminator, a CR before an LF is dropped, tabs
    become spaces, '--' and everything after it on the line is dropped (inside quotes too), and
    every '$' is dropped (the marker it uses for what it removes)."""
    b = bytearray(data)
    if not b:
        return b''
    b[-1] = 0
    end = b.find(0)
    b = b[:end]
    b = b.replace(b'\r\n', b'$\n').replace(b'\t', b' ')
    i = b.find(b'--')
    while i >= 0:
        j = b.find(b'\n', i)
        j = len(b) if j < 0 else j
        b[i:j] = b'$' * (j - i)
        i = b.find(b'--', j)
    return bytes(b.replace(b'$', b''))


def rest_of_line(t, i):
    """The rest-of-line reader (0x1400f42a0): skips spaces, copies to the end of the line (at
    most LINE_MAX characters), trims trailing spaces. Numbers are then read from it with sscanf."""
    while i < len(t) and t[i] in b' \t':
        i += 1
    j = i
    while j < len(t) and t[j] not in b'\r\n\0' and j - i < LINE_MAX:
        j += 1
    return t[i:j].rstrip(b' \t')


def quoted(t, i, maxlen):
    """The quoted-string reader (0x1400f43e0): the opening quote must come before the end of the
    line; then it copies up to the closing quote, across lines, at most maxlen - 1 characters.
    Returns (text, closed) or (None, False) when there's no opening quote on the line."""
    while i < len(t) and t[i] != 0x22:
        if t[i] == 0x0a:
            return None, False
        i += 1
    if i >= len(t):
        return None, False
    i += 1
    j = i
    while j < len(t) and t[j] != 0x22 and j - i < maxlen - 1:
        j += 1
    return t[i:j], j < len(t) and t[j] == 0x22


class Rules:
    """What the passes need, read from the exe."""

    def __init__(self, exe, parser=None):
        p = self.p = parser or Parser(exe)
        self.funcs = exe.functions()
        self.starts = [f[0] for f in self.funcs]
        # a parser's code is split into chunks (.pdata entries chained to the first one); the
        # contexts are found by chunk, so map every chunk to its function's first one
        self.root = chunk_roots(exe)
        self.ctx_roots = {c: {self.root.get(f, f) for f in fs} for c, fs in p.contexts.items()}
        mon = p.contexts['monster']
        # the monster parser's #descr branch: lea rdx,"descr"; call MATCHER; ...; call QUOTED
        self.matcher, self.quoted = self._calls_after(mon, 'descr', 2)
        # #magicboost reads "%d %d" from the rest of the line: call MATCHER; ...; call LINE
        self.line = self._calls_after(mon, 'magicboost', 2)[1]
        top = next(iter(p.contexts['top']))
        self.dispatcher = top
        self.new_select = [s for i, (f, s) in sorted(p.refs.items()) if f == top]
        self.commands = {c: {s for f in fs for s in p.by_func[f]} for c, fs in p.contexts.items() if c != 'top'}
        # contexts whose parser calls the #new/#select check (a fatal error inside a block), and
        # those that refuse a block left open at the end of the file
        self.must_end, self.eof_end = set(), set()
        for a, op, args, tgt in p.ins:
            if op == 'call' and num(args.split()[0]) == top:
                self.must_end.add(self.context_of(a))
            elif op == 'lea' and tgt is not None:
                s = exe.cstr(tgt, 80)
                if s and s.startswith('no #end for modded'):
                    self.eof_end.add(self.context_of(a))
        self.must_end.discard(None)
        self.eof_end.discard(None)
        self.strings = self._string_commands()

    def chunk_of(self, a):
        i = bisect.bisect_right(self.starts, a) - 1
        return self.funcs[i][0] if i >= 0 and a < self.funcs[i][1] else None

    def context_of(self, a):
        f = self.chunk_of(a)
        f = self.root.get(f, f)
        return next((c for c, fs in self.ctx_roots.items() if f in fs and c != 'top'), None)

    def _calls_after(self, fs, name, n):
        p = self.p
        i = next(i for i, (f, s) in sorted(p.refs.items()) if f in fs and s == name)
        out = []
        while len(out) < n:
            i += 1
            if p.ins[i][1] == 'call':
                out.append(num(p.ins[i][2].split()[0]))
        return out

    def _string_commands(self):
        """Per context: the commands whose branch reads a quoted string straight from the file
        text (the scan position plus the file buffer, not a copy of the line), with the maximum
        length passed (r8) and whether the branch adds the string's length to the position."""
        p = self.p
        out = collections.defaultdict(dict)
        for idx, (a, op, args, tgt) in enumerate(p.ins):
            if op != 'call' or num(args.split()[0]) != self.quoted:
                continue
            ctx = self.context_of(a)
            if ctx is None:
                continue
            f = self.chunk_of(a)
            cmd = next((p.refs[j][1] for j in range(idx - 1, max(idx - 400, 0), -1)
                        if j in p.refs and p.refs[j][0] == f), None)
            # registers from the previous call: constants, the function's constant registers,
            # and eax = 0 after the matcher's 'test eax,eax; jne'
            j = idx - 1
            while j > 0 and p.ins[j][1] != 'call' and idx - j < 40:
                j -= 1
            reg = dict(p.const_registers(f)) if f in p.contexts.get(ctx, ()) else {}
            reg['rax'] = 0
            from_text = False
            for k in range(j + 1, idx):
                aa, oo, rr, tt = p.ins[k]
                dst = rr.split(',')[0]
                d = reg64(dst)
                m = re.match(r'(\w+),(0x[0-9a-f]+|\d+)$', rr)
                m2 = re.match(r'(\w+),\[(\w+)\+(0x[0-9a-f]+)\]$', rr)
                if d == 'rcx':
                    # mov(sxd) rcx,POS; add rcx,BUFFER reads from the file text
                    from_text = oo == 'add' or (from_text and oo != 'lea')
                if oo == 'mov' and m and d:
                    reg[d] = num(m.group(2))
                elif oo == 'xor' and d and rr.split(',')[1] == dst:
                    reg[d] = 0
                elif oo == 'lea' and m2 and d and reg64(m2.group(2)) in reg:
                    reg[d] = reg[reg64(m2.group(2))] + num(m2.group(3))
                elif d and oo not in ('cmp', 'test', 'push') and not oo.startswith('j'):
                    reg.pop(d, None)
            if not from_text or cmd is None:
                continue
            adv = False
            for aa, oo, rr, tt in p.ins[idx + 1:idx + 12]:
                if oo == 'call' or oo.startswith('j'):
                    break
                if oo == 'add' and rr.endswith(',eax'):
                    adv = True
            out[ctx][cmd] = {'max_chars': reg.get('r8', 0) - 1 if reg.get('r8') else None, 'skips_text': adv}
        return dict(out)

    def summary(self):
        return {'matcher': hex(self.matcher), 'quoted_string_reader': hex(self.quoted),
                'rest_of_line_reader': hex(self.line), 'new_select_check': hex(self.dispatcher),
                'new_select_names': self.new_select, 'refuse_new_select_in_block': sorted(self.must_end),
                'refuse_missing_end_at_eof': sorted(self.eof_end), 'string_commands': self.strings}


def chunk_roots(exe):
    """For every .pdata entry, the first entry of its function (following chained unwind info)."""
    va, raw, rsz, vsz = exe.sections['.pdata']
    parent = {}
    for k in range(0, rsz, 12):
        b, e, u = struct.unpack_from('<III', exe.data, raw + k)
        if not b:
            continue
        o = exe.offset(BASE + u)
        if o is None:
            continue
        up = None
        if exe.data[o] >> 3 & 4:        # UNW_FLAG_CHAININFO: the parent entry follows the codes
            n = exe.data[o + 2]
            up = BASE + struct.unpack_from('<I', exe.data, o + 4 + 2 * ((n + 1) & ~1))[0]
        parent[BASE + b] = up
    out = {}
    for a in parent:
        r, seen = a, set()
        while parent.get(r) and r not in seen:
            seen.add(r)
            r = parent[r]
        out[a] = r
    return out


def match(t, i, names):
    """The longest of names the text at i starts with, followed by a non-alphanumeric."""
    m = IDENT.match(t, i)
    if not m:
        return None
    w = m.group().decode('latin1')
    for k in range(len(w), 0, -1):
        if (k == len(w) or w[k] == '_') and w[:k] in names:
            return w[:k]
    return None


def read_pass(t, ctx, rules):
    """One pass over prepared text t: a list of (position, kind, command, value). Kinds: start,
    end, command, string (value = (text, closed)), fatal (the game stops with an error),
    ignored (a #new/#select the pass doesn't act on inside a block)."""
    starts = STARTS[ctx]
    names = rules.commands[ctx] - set(starts) - {'end'}
    strings = rules.strings.get(ctx, {})
    in_block = ctx == 'global'
    out = []
    i, n = 0, len(t)
    while i < n:
        if t[i] != 0x23:
            i += 1
            continue
        j = i + 1
        if ctx == 'global':
            c = match(t, j, names)
            if c:
                out.append((i, 'command', c, rest_of_line(t, j + len(c))))
                j += len(c)
        elif not in_block:
            c = match(t, j, starts)
            if c:
                out.append((i, 'start', c, rest_of_line(t, j + len(c))))
                in_block = True
                j += len(c)
        elif match(t, j, ('end',)):
            out.append((i, 'end', 'end', b''))
            in_block = False
            j += 3
        elif ctx in rules.must_end and any(t.startswith(s.encode(), j) for s in rules.new_select):
            out.append((i, 'fatal', IDENT.match(t, j).group().decode('latin1'),
                        'You must end modding the %s before using a #new... or #select... command' % ctx))
            return out
        else:
            c = match(t, j, names)
            if c and c in strings:
                j += len(c)
                s, closed = quoted(t, j, strings[c]['max_chars'] + 1)
                out.append((i, 'string', c, (s, closed)))
                if s is not None and strings[c]['skips_text']:
                    j += len(s)
            elif c:
                out.append((i, 'command', c, rest_of_line(t, j + len(c))))
                j += len(c)
            elif match(t, j, starts) or any(match(t, j, (s,)) for s in rules.new_select):
                out.append((i, 'ignored', IDENT.match(t, j).group().decode('latin1'), b''))
        i = j + 1
    if in_block and ctx in rules.eof_end:
        out.append((n, 'fatal', 'end', 'no #end for modded %s' % ctx))
    return out


class Text:
    """Prepared text with positions mapped to the file's line numbers (prepare() removes characters
    but no line breaks)."""

    def __init__(self, data):
        self.raw_lines = data.split(b'\n')
        self.t = prepare(data)
        self.nl = [i for i, ch in enumerate(self.t) if ch == 0x0a]

    def line(self, pos):
        return bisect.bisect_left(self.nl, pos) + 1

    def line_text(self, no):
        return self.raw_lines[no - 1].rstrip(b'\r').decode('utf-8', 'replace') if 0 < no <= len(self.raw_lines) else ''


def differences(data, rules, contexts=None, examples=5):
    """Where the game reads the file differently from a line-by-line reading: counts and examples."""
    tx = Text(data)
    t = tx.t
    found = collections.defaultdict(list)

    def note(kind, pos, text):
        found[kind].append('line %d: %s' % (tx.line(pos), text))

    for ctx in contexts or STARTS:
        res = read_pass(t, ctx, rules)
        spans = []
        for pos, kind, c, v in res:
            if kind == 'fatal':
                note('fatal error', pos, '%s (%s pass, #%s)' % (v, ctx, c))
            elif kind == 'ignored':
                note('#new/#select ignored inside an unended block', pos, '#%s inside a %s block (%s pass)' % (c, ctx, ctx))
            elif kind == 'string':
                s, closed = v
                if s is None:
                    # (#sound and the sprite commands also take a number)
                    if c in TEXT_COMMANDS:
                        note('no opening quote on the line', pos, '#%s (%s): the game reads no text' % (c, ctx))
                    continue
                start = t.index(b'"', pos) + 1
                spans.append((start, start + len(s), c))
                lim = rules.strings[ctx][c]['max_chars']
                if not closed and start + len(s) >= len(t):
                    note('string runs to the end of the file', pos, '#%s (%s): no closing quote' % (c, ctx))
                elif not closed:
                    note('string longer than the game reads', pos, '#%s (%s): %d characters kept%s' % (
                        c, ctx, lim, '; reading goes on inside the rest of the text' if rules.strings[ctx][c]['skips_text'] else ''))
                first, last = tx.line(start), tx.line(start + len(s))
                if last > first:
                    inner = [ln for ln in range(first + 1, last + 1) if re.match(r'\s*#[a-z]', tx.line_text(ln))]
                    if inner and rules.strings[ctx][c]['skips_text']:
                        kind_text = 'a string runs over lines starting with # (they are text, not read)'
                    elif inner:
                        kind_text = 'a description runs over lines starting with # (in its text, and read as commands too)'
                    if inner:
                        note(kind_text, pos, '#%s (%s) runs to line %d; inside: %s' % (
                            c, ctx, last, '; '.join(tx.line_text(ln).strip()[:40] for ln in inner[:3])))
                if closed and last > first:
                    eol = t.find(b'\n', start + len(s))
                    after = t[start + len(s) + 1:eol if eol >= 0 else len(t)].strip()
                    if after.startswith(b'#'):
                        note('commands after a multi-line string\'s closing quote', start + len(s),
                             '#%s (%s) ends, then %s' % (c, ctx, after[:40].decode('utf-8', 'replace')))
            # a command read inside a string that opened on the same line (the multi-line ones are
            # counted above)
            owner = next((a, o) for a, b, o in spans if a <= pos < b) if any(a <= pos < b for a, b, _ in spans) else None
            if kind in ('command', 'string', 'end', 'start') and owner and tx.line(owner[0]) == tx.line(pos):
                note('# inside quotes read as a command', pos, '#%s inside the text of #%s (%s pass): %s' % (
                    c, owner[1], ctx, tx.line_text(tx.line(pos)).strip()[:60]))
    for no, line in enumerate(tx.raw_lines, 1):
        k = line.find(b'--')
        if k > 0 and line[:k].count(b'"') % 2 == 1 and b'"' in line[k:]:
            found['-- inside quotes (the game drops the rest of the line, closing quote included)'].append(
                'line %d: %s' % (no, line.decode('utf-8', 'replace').strip()[:80]))
        k = line.find(b'$')
        if 0 <= k < (line.find(b'--') if b'--' in line else len(line)):
            found['$ in the text (the game drops it)'].append('line %d: %s' % (no, line.decode('utf-8', 'replace').strip()[:80]))
    if data and data[-1:] not in b'\r\n \t':
        found['no line break at the end (the game drops the last character)'].append(
            'line %d: %s' % (len(tx.raw_lines), tx.raw_lines[-1].decode('utf-8', 'replace').strip()[:80]))
    return {k: {'count': len(v), 'examples': v[:examples]} for k, v in found.items()}


def lines(data, rules, first, last, contexts=None):
    """What each pass reads between two line numbers."""
    tx = Text(data)
    out = []
    for ctx in contexts or STARTS:
        for pos, kind, c, v in read_pass(tx.t, ctx, rules):
            ln = tx.line(pos)
            if first <= ln <= last:
                if kind == 'string':
                    s, closed = v
                    cut = s is not None and len(s) >= rules.strings[ctx][c]['max_chars']
                    v = (s or b'').decode('utf-8', 'replace')
                    v = '"%s%s' % (v[:60].replace('\n', '\\n'), '"' if closed else ' (cut at the limit)' if cut else ' (no closing quote)')
                else:
                    v = v.decode('utf-8', 'replace') if isinstance(v, bytes) else v
                out.append((ln, pos, ctx, kind, c, v))
    return ['line %d %-8s %-8s #%s %s' % (ln, ctx, kind, c, v) for ln, pos, ctx, kind, c, v in sorted(out)]
