"""Where the game's own texts are in Dominions6.exe (dom6exe.py texts).

Descriptions aren't in the entity records. The game keeps them in two lists of string pointers
in .data, one for spells and one for everything else (monsters, items, nations): a key entry
(":Heavy Cavalry", ":mon1712", ":era1 Abysia", ":details Encase in Ice"), possibly more keys
sharing the text, then the text; the list ends with ":end". A lookup builds a key and searches
the list from the end: "::" keys first (the ones mods add), then ":" keys, ignoring case; the
text is the first entry after the key's run of ":" entries. A mod's #descr appends to the same
list ("::mon%d" + its text before ":end"), so the latest wins.

Everything is read from the code, not from known texts:
  key format each mod command        the #descr, #summary, #brief, #details, #portent, #cure
  writes, and the function that      branches of the monster, item, nation and spell parsers
  appends it                         (a sprintf into a buffer, then a call with that buffer)
  list, capacity, end key            the lookup function: it loads the list the append function
                                     loads, scans it for the end key up to the capacity, and
                                     checks entries for a second ':'
  key forms the game looks up        the lookup's callers: a key formatted with sprintf or the
                                     lookup's printf wrapper ("mon%d", "era%d nation%d") or a
                                     table record's name (an index times the record size plus
                                     the table's address), with a prefix ("details") joined by
                                     the lookup's own "%s %s"; a call whose result is tested
                                     (found or not) and followed by another is a fallback
Which lookup is which command's: the one in the same entity's table that also tries the mod
command's key without its colons. Checked on every run: each list ends with its end key within
its capacity, every entry points into .data, and every text kind finds texts for vanilla
entities. Only the locations are written (texts-6.37.json); the editor reads the texts from the
player's own exe (Dom5Edit/GameData/VanillaTexts.cs).
"""
import bisect, collections, hashlib, itertools, re, struct

from dom6exe import TABLES, Parser, find_table, num, reg64, text

# The text commands per parser, and the entity table each one's lookup reads names from
TEXT_COMMANDS = {'monster': ['descr'], 'item': ['descr'], 'nation': ['descr', 'summary', 'brief'],
                 'spell': ['descr', 'details', 'portent', 'cure']}
# Record fields a key is formatted with, besides the name at +0 (the README's nation layout)
KEY_FIELDS = {('nation', 0xac): 'era'}
VOLATILE = ('rax', 'rcx', 'rdx', 'r8', 'r9', 'r10', 'r11')


class Emu:
    """Register values over a straight run of instructions (jumps aren't taken unless asked):
    ('c', n) a constant, ('m', size) an index times a record size, ('r', size, addr) a record's
    address (table + index * size; addr is the table plus an offset), ('f', size, addr) a value
    read from a record, ('b', expr) a stack buffer, None unknown. A call into a stack buffer
    with a format string in r8 (sprintf) leaves ('k', format, args) in that buffer."""

    def __init__(self, texts):
        self.t = texts
        self.regs = {}
        self.stack = {}
        self.buffers = {}

    def ea(self, inner, target):
        if 'rip' in inner:
            return ('c', target)
        if re.match(r'(rsp|rbp)\+0x[0-9a-f]+$|(rsp|rbp)$', inner):
            return ('b', inner)
        const, mul = 0, None
        for sign, x, scale in re.findall(r'([+-]?)(\w+)(?:\*(\d))?', inner):
            if re.match(r'0x[0-9a-f]+$', x):
                const += -num(x) if sign == '-' else num(x)
                continue
            v = self.regs.get(reg64(x))
            if v is None or (scale and scale != '1'):
                return None
            if v[0] == 'c':
                const += v[1]
            elif v[0] in ('m', 'r') and mul is None:
                mul = v[1]
                const += v[2] if v[0] == 'r' else 0
            else:
                return None
        return ('r', mul, const) if mul else ('c', const)

    @staticmethod
    def add(a, b):
        if a is None or b is None:
            return None
        if a[0] == 'c' and b[0] == 'c':
            return ('c', a[1] + b[1])
        if a[0] == 'c':
            a, b = b, a
        if a[0] == 'm' and b[0] == 'c':
            return ('r', a[1], b[1])
        if a[0] == 'r' and b[0] == 'c':
            return ('r', a[1], a[2] + b[1])
        return None

    def step(self, ins):
        """One instruction. Returns the target of a call or jmp (the caller handles it)."""
        a, o, args, t = ins
        if o in ('call', 'jmp'):
            return args.split()[0]
        dst, _, src = args.partition(',')
        d = reg64(dst)
        mem = re.match(r'(?:\w+ PTR )?\[([^\]]+)\]$', src)
        if dst.startswith(('QWORD PTR [rsp+', 'DWORD PTR [rsp+')):
            slot = re.search(r'\[(rsp\+0x[0-9a-f]+)\]', dst).group(1)
            self.stack[slot] = ('c', num(src)) if re.match(r'0x[0-9a-f]+$|\d+$', src) else self.regs.get(reg64(src))
            return None
        if not d or o in ('cmp', 'test', 'push') or o.startswith('j'):
            return None
        if o == 'lea' and mem:
            self.regs[d] = self.ea(mem.group(1), t)
        elif o in ('mov', 'movsx', 'movzx', 'movsxd', 'movabs'):
            if re.match(r'0x[0-9a-f]+$|\d+$', src):
                self.regs[d] = ('c', num(src))
            elif mem:
                v = self.ea(mem.group(1), t)
                self.regs[d] = ('f', v[1], v[2]) if v and v[0] == 'r' else None
            else:
                self.regs[d] = self.regs.get(reg64(src))
        elif o == 'imul' and src.count(',') == 1:
            self.regs[d] = ('m', num(src.split(',')[1]))
        elif o == 'add':
            b = ('c', num(src)) if re.match(r'0x[0-9a-f]+$', src) else self.regs.get(reg64(src))
            self.regs[d] = self.add(self.regs.get(d), b)
        elif o == 'xor' and reg64(src) == d:
            self.regs[d] = ('c', 0)
        else:
            self.regs[d] = None
        return None

    def call(self, target):
        """A call: a sprintf into a stack buffer is remembered; the volatile registers are lost."""
        r = self.regs
        fmt = self.t.string(r['r8'][1]) if r.get('r8') and r['r8'][0] == 'c' else None
        if r.get('rcx') and r['rcx'][0] == 'b' and fmt and ('%' in fmt or fmt.startswith(':')):
            self.buffers[r['rcx'][1]] = ('k', fmt, [r.get('r9'), self.stack.get('rsp+0x20'), self.stack.get('rsp+0x28')])
        for v in VOLATILE:
            r.pop(v, None)

    def value(self, reg):
        v = self.regs.get(reg)
        return self.buffers.get(v[1], v) if v and v[0] == 'b' else v


class Texts:
    def __init__(self, exe, p=None):
        self.exe = exe
        self.p = p = p or Parser(exe)
        self.funcs = exe.functions()
        self._starts = [f[0] for f in self.funcs]
        self._addrs = [x[0] for x in p.ins]
        va, raw, rsz, vsz = exe.sections['.data']
        self.data = (va, raw, rsz, vsz)
        # the vanilla tables (dom6exe TABLES): table address and record size, by kind
        self.tables = {}
        for kind in TEXT_COMMANDS:
            start, size = find_table(exe, TABLES[kind])
            self.tables[kind] = (exe.address(start), size)
        self._mod_keys()
        self._lists()
        self._lookups()
        self.check()

    # -----------------------------------------------------------------------------------------
    def string(self, addr):
        """A string in .data: '' for the zero-filled part past the file's bytes."""
        va, raw, rsz, vsz = self.data
        if va + rsz <= addr < va + vsz:
            return ''
        return self.exe.cstr(addr, 2000)

    def _func(self, addr):
        i = bisect.bisect_right(self._starts, addr) - 1
        return self.funcs[i] if i >= 0 and addr < self.funcs[i][1] else None

    def _run(self, lo, hi, follow=False):
        """Emulates instructions lo..hi, yielding (index, emu, target) at each call or jmp.
        With follow, unconditional jumps are taken (a branch's shared tail, often above it)."""
        emu, i, steps = Emu(self), lo, 0
        while i < hi and steps < 200:
            steps += 1
            target = emu.step(self.p.ins[i])
            if target:
                yield i, emu, target
                if self.p.ins[i][1] == 'call':
                    emu.call(target)
                elif follow and num(target):
                    j = self.p.index.get(num(target))
                    if j is not None:
                        i, hi = j, j + 40
                        continue
            i += 1

    def kind_of(self, v):
        """The entity table a record value points into, and the offset in the record."""
        if not v or v[0] not in ('r', 'f'):
            return None, None
        for kind, (addr, size) in self.tables.items():
            if size == v[1] and addr <= v[2] < addr + size:
                return kind, v[2] - addr
        return None, None

    # -----------------------------------------------------------------------------------------
    def _mod_keys(self):
        """(context, command) -> (the key format the command writes, the append function)."""
        p = self.p
        self.mod_keys = {}
        for ctx, cmds in TEXT_COMMANDS.items():
            fs = p.contexts[ctx]
            order = sorted(k for k, (f, s) in p.refs.items() if f in fs)
            for cmd in cmds:
                found = None
                for n, i in enumerate(order):
                    if p.refs[i][1] != cmd:
                        continue
                    later = [j for j in order[n + 1:] if p.refs[j][1] != cmd]
                    hi = later[0] if later else i + 200
                    for k, emu, target in self._run(i, hi, follow=True):
                        key = emu.value('rcx')
                        if p.ins[k][1] == 'call' and key and key[0] == 'k' and key[1].startswith('::'):
                            found = (key[1], num(target))
                            break
                    if found:
                        break
                if not found:
                    raise SystemExit('%s parser: no #%s branch that adds a "::" key' % (ctx, cmd))
                self.mod_keys[(ctx, cmd)] = found

    def _lists(self):
        """The list each append function writes, found by its lookup function: one that loads the
        same address and checks entries for a second ':' (cmp BYTE PTR [reg+0x1],0x3a)."""
        p = self.p
        loads = collections.defaultdict(set)      # .data address -> functions loading it
        colon2 = set()
        for a, o, args, t in p.ins:
            if o == 'lea' and t is not None:
                loads[t].add(self._func(a))
            if o == 'cmp' and re.match(r'BYTE PTR \[\w+\+0x1\],0x3a$', args):
                colon2.add(self._func(a))
        self.lists = {}
        self.list_of = {}
        for append in sorted({a for k, a in self.mod_keys.values()}):
            fn = self._func(append)
            lo, hi = p.index[fn[0]], bisect.bisect_left(self._addrs, fn[1])
            bases = {t for a, o, args, t in p.ins[lo:hi] if o == 'lea' and t is not None and self.exe.offset(t) is not None}
            hits = [(b, f) for b in bases for f in loads[b] if f in colon2 and f != fn]
            if len({b for b, f in hits}) != 1 or len(hits) != 1:
                raise SystemExit('append function %s: lists with a lookup: %s' % (hex(append), hits))
            base, look = hits[0]
            llo, lhi = p.index[look[0]], bisect.bisect_left(self._addrs, look[1])
            body = p.ins[llo:lhi]
            end = next(self.string(t) for a, o, args, t in body if o == 'lea' and t and (self.string(t) or '').startswith(':'))
            cap = next(num(args.split(',')[1]) for a, o, args, t in body if o == 'cmp' and re.match(r'r\w+d,0x[0-9a-f]+$', args))
            if not any(o == 'lea' and t and self.string(t) == '%s %s' for a, o, args, t in body):
                raise SystemExit('lookup %s: no "%%s %%s" (prefix and name) format' % hex(look[0]))
            entries = self._entries(base, cap, end)
            self.lists[base] = {'address': base, 'capacity': cap, 'end': end, 'lookup': look[0], 'append': append,
                                'entries': entries}
            self.list_of[append] = base

    def _entries(self, base, cap, end):
        """The list's strings up to (not including) the end key."""
        d = self.exe.data
        o = self.exe.offset(base)
        out = []
        for k in range(cap):
            ptr = struct.unpack_from('<Q', d, o + 8 * k)[0]
            va, raw, rsz, vsz = self.data
            if not va <= ptr < va + vsz:
                raise SystemExit('list %s: entry %d (%s) is not in .data' % (hex(base), k, hex(ptr)))
            if va + rsz <= ptr:
                out.append((ptr, b''))
                continue
            so = self.exe.offset(ptr)
            s = d[so:d.find(b'\0', so)]
            if s == end.encode():
                return out
            out.append((ptr, s))
        raise SystemExit('list %s: no %s within %d entries' % (hex(base), end, cap))

    def _lookups(self):
        """(context, command) -> the keys the game looks up, in order ("mon{id}", "{name}")."""
        p = self.p
        looks = {v['lookup']: base for base, v in self.lists.items()}
        # thin wrappers: name only (mov rdx,rcx; lea rcx,""; jmp lookup) or printf-like
        # (formats its arguments into a buffer, then looks that up)
        wrappers = {}
        for i, (a, o, args, t) in enumerate(p.ins):
            if o in ('call', 'jmp') and num(args.split()[0]) in looks:
                fn = self._func(a)
                if fn is None:
                    # a leaf without an exception table entry: back to the padding before it
                    lo = i
                    while lo > i - 8 and p.ins[lo - 1][1] not in ('int3', 'ret', 'jmp'):
                        lo -= 1
                    fn = (p.ins[lo][0], a + 1)
                if fn[0] in looks or fn[1] - fn[0] > 0x100:
                    continue
                lo = p.index[fn[0]]
                body = p.ins[lo:i]
                if any(o2 == 'mov' and args2 == 'rdx,rcx' for _, o2, args2, _ in body):
                    wrappers[fn[0]] = ('name', looks[num(args.split()[0])])
                elif any(re.match(r'QWORD PTR \[r11\+0x20\],r9$', args2) for _, o2, args2, _ in body):
                    wrappers[fn[0]] = ('printf', looks[num(args.split()[0])])
        sites = collections.defaultdict(list)    # function -> [(index, list, kind of call)]
        for i, (a, o, args, t) in enumerate(p.ins):
            if o not in ('call', 'jmp'):
                continue
            target = num(args.split()[0])
            fn = self._func(a)
            if fn is None or fn[0] in wrappers or fn[0] in looks:
                continue
            if target in looks:
                sites[fn[0]].append((i, looks[target], 'lookup'))
            elif target in wrappers:
                sites[fn[0]].append((i, wrappers[target][1], wrappers[target][0]))
        chains = []
        for fn, ss in sites.items():
            chain = []
            for n, (i, base, how) in enumerate(ss):
                lo = max(p.index[fn], i - 80)
                emu = next(e for k, e, target in self._run(lo, i + 1) if k == i)   # the registers at the call
                if how == 'lookup':
                    prefix_v, key = emu.regs.get('rcx'), emu.value('rdx')
                    prefix = self.string(prefix_v[1]) if prefix_v and prefix_v[0] == 'c' else None
                elif how == 'name':
                    prefix, key = '', emu.value('rcx')
                else:
                    fmt_v = emu.regs.get('rcx')
                    fmt = self.string(fmt_v[1]) if fmt_v and fmt_v[0] == 'c' else None
                    prefix, key = '', ('k', fmt, [emu.regs.get('rdx'), emu.regs.get('r8'), emu.regs.get('r9')]) if fmt else None
                chain.append(self._key(base, prefix, key))
                # a fallback: the result is tested (found or not) and another lookup follows
                after = list(itertools.takewhile(lambda x: x[1] != 'call', p.ins[i + 1:i + 8]))
                tested = any(o2 == 'test' and args2 == 'eax,eax' for _, o2, args2, _ in after)
                if not (tested and n + 1 < len(ss) and ss[n + 1][0] - i < 30):
                    chains.append(chain)
                    chain = []
        self.texts = {}
        for (ctx, cmd), (fmt, append) in self.mod_keys.items():
            want = fmt.lstrip(':')
            hit = [c for c in chains if all(x for x in c) and want in [x['raw'] for x in c]
                   and {x['kind'] for x in c if x['kind']} == {ctx} and c[0]['list'] == self.list_of[append]]
            if len(hit) != 1:
                raise SystemExit('#%s for %s: %d lookups try "%s"' % (cmd, ctx, len(hit), want))
            self.texts[(ctx, cmd)] = hit[0]

    def _key(self, base, prefix, key):
        """A lookup's key: the raw format ("era%d %s"), the template for the editor
        ("era{era} {name}") and the entity table it names."""
        if prefix is None or key is None:
            return None
        kinds = set()

        def arg(v, spec):
            kind, off = self.kind_of(v)
            if kind:
                kinds.add(kind)
            if v and v[0] == 'r' and off == 0:
                return '{name}'
            if v and v[0] == 'f' and (kind, off) in KEY_FIELDS:
                return '{%s}' % KEY_FIELDS[(kind, off)]
            if spec == 'd' and (v is None or v[0] != 'f'):
                return '{id}'       # the entity's number (a parameter the code doesn't name)
            return '{?}'            # not understood (not one of the text commands' lookups)

        if key[0] == 'k':
            raw = key[1]
            args = iter(key[2])
            template = re.sub(r'%([ds])', lambda m: arg(next(args), m.group(1)), key[1])
        elif key[0] == 'r':
            raw, template = '%s', arg(key, 's')
        else:
            return None
        if '{?}' in template:
            return None
        if prefix:
            raw, template = prefix + ' ' + raw, prefix + ' ' + template
        return {'list': base, 'raw': raw, 'template': template, 'kind': next(iter(kinds)) if len(kinds) == 1 else None}

    # -----------------------------------------------------------------------------------------
    def check(self):
        """Every text kind finds texts for vanilla entities."""
        self.found = {}
        for (ctx, cmd) in self.texts:
            n = sum(1 for _ in self.vanilla_texts(ctx, cmd))
            if not n:
                raise SystemExit('no vanilla %s has a %s' % (ctx, cmd))
            self.found[(ctx, cmd)] = n

    def index(self, base):
        """Folded key -> index of its text, as the lookup finds it: '::' keys before ':' keys,
        the last of equal keys, ASCII case ignored."""
        if not hasattr(self, '_index'):
            self._index = {}
        if base not in self._index:
            fold = lambda b: bytes(c + 32 if 65 <= c <= 90 else c for c in b)
            one, two = {}, {}
            entries = self.lists[base]['entries']
            for i, (ptr, s) in enumerate(entries):
                if s.startswith(b'::'):
                    two[fold(s[2:])] = i
                elif s.startswith(b':'):
                    one[fold(s[1:])] = i
            self._index[base] = (one, two, fold)
        return self._index[base]

    def lookup(self, base, key):
        one, two, fold = self.index(base)
        entries = self.lists[base]['entries']
        k = fold(key)
        i = two.get(k, one.get(k))
        if i is None:
            return None
        while i < len(entries) and entries[i][1].startswith(b':'):
            i += 1
        return text(entries[i][1]) if i < len(entries) and entries[i][1] else None

    def vanilla_texts(self, ctx, cmd):
        """(entity number, text) of every vanilla entity of the kind with that text."""
        addr, size = self.tables[ctx]
        o = self.exe.offset(addr)
        spec = TABLES[ctx]
        chain = self.texts[(ctx, cmd)]
        for i in range(spec['count']):
            r = self.exe.data[o + i * size:o + (i + 1) * size]
            name = r[:r.find(b'\0')]
            if not name:
                continue
            values = {'id': str(i).encode(), 'name': name}
            for off, field in ((off, f) for (k, off), f in KEY_FIELDS.items() if k == ctx):
                values[field] = str(struct.unpack_from('<h', r, off)[0]).encode()
            for look in chain:
                key = re.sub(rb'\{(\w+)\}', lambda m: values[m.group(1).decode()], look['template'].encode())
                t = self.lookup(look['list'], key)
                if t:
                    yield i, t
                    break


# ---------------------------------------------------------------------------------------------

def write(exe, path):
    """The texts' locations (never the texts): the lists, how each kind's key is made, and a
    checksum of the bytes the editor reads (Dom5Edit/GameData/VanillaTexts.cs)."""
    import json
    t = Texts(exe)
    va, raw, rsz, vsz = t.data
    names = {}
    for base, v in t.lists.items():
        users = sorted({ctx for (ctx, cmd), c in t.texts.items() if c[0]['list'] == base})
        names[base] = 'spell' if users == ['spell'] else 'general'
    if len(set(names.values())) != len(names):
        raise SystemExit('lists: cannot name %s' % names)
    # the bytes the editor reads: both lists (with their end entries) and every string they point to
    spans = []
    for base, v in t.lists.items():
        o = exe.offset(base)
        n = len(v['entries']) + 1
        spans.append((o, o + 8 * n))
        for k in range(n):
            ptr = struct.unpack_from('<Q', exe.data, o + 8 * k)[0]
            if ptr < va + rsz:      # (past the file's bytes: zero-filled, an empty string)
                so = exe.offset(ptr)
                spans.append((so, exe.data.index(b'\0', so) + 1))
    lo, hi = min(a for a, b in spans), max(b for a, b in spans)
    out = {
        'game_version': exe.version, 'exe_sha256_16': exe.sha,
        'note': 'Written by tools/dom6exe (texts). Where Dominions6.exe keeps its texts; the texts '
                'themselves stay in the game. The editor reads the "read" span from the player\'s '
                'exe and uses it only if its sha256 matches.',
        'read': {'file_offset': lo, 'length': hi - lo, 'sha256': hashlib.sha256(exe.data[lo:hi]).hexdigest()},
        'data_section': {'address': hex(va), 'file_offset': raw, 'file_size': rsz, 'memory_size': vsz},
        'lists': {names[b]: {'address': hex(b), 'file_offset': exe.offset(b), 'capacity': v['capacity'],
                             'count': len(v['entries']), 'end': v['end'],
                             'lookup_function': hex(v['lookup']), 'append_function': hex(v['append'])}
                  for b, v in sorted(t.lists.items())},
        'match': 'A key is an entry starting with ":"; the text is the first entry after the key\'s '
                 'run of ":" entries (an empty one: no text); the list ends at the end key. '
                 '"::" keys (a mod\'s) are found before ":" keys, the last of equal keys wins, and '
                 'case is ignored (ASCII). Each kind tries its keys in order: {id} is the entity\'s '
                 'number, {name} its name, {era} a nation\'s era.',
        'texts': {ctx: {cmd: {'list': names[c[0]['list']], 'keys': [x['template'] for x in c],
                              'mod_key': t.mod_keys[(ctx, cmd)][0]}
                        for (c2, cmd), c in sorted(t.texts.items()) if c2 == ctx}
                  for ctx in TEXT_COMMANDS},
        'found': {'%s %s' % k: n for k, n in sorted(t.found.items())},
    }
    if path:
        open(path, 'w').write(json.dumps(out, indent=1) + '\n')
    return out
