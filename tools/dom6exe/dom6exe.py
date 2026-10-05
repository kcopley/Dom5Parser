#!/usr/bin/env python3
"""Reads Dominions 6 game facts straight from Dominions6.exe.

The dom6inspector's CSV game data is extracted from the exe by a tool that is rarely
maintained. This reads the exe directly instead:

  commands   which mod commands the game's .dm parser actually reads, per entity type
             (#selectmonster, #newweapon, ...), and for monster and item commands the
             numbered ability each one sets
  monsters   the vanilla monster table: name, base stats, numbered abilities
  readonly   abilities vanilla monsters have that no mod command can set; an editor shows
             them read-only

Usage:
  python3 tools/dom6exe/dom6exe.py [--exe PATH] [--inspector DIR] commands|monsters|readonly [--out FILE]

Nothing is hard-coded to one game build: the parser functions are found by the command
names they compare against, the generic command handlers by how often they are called with
a command name, and the monster table by locating known vanilla names. Layout facts that
were worked out by reading the code (record size, where the abilities sit) are checked
against known vanilla values on every run.

Needs Python 3.8+ and GNU objdump (binutils) for the disassembly.
"""
import argparse, bisect, collections, csv, hashlib, json, os, re, struct, subprocess, sys, tempfile

DEFAULT_EXE = '/mnt/c/Games/Steam/steamapps/common/Dominions6/Dominions6.exe'
BASE = 0x140000000

# A command each parser function checks for, and only that function (besides the top-level
# dispatcher, which reads #new* / #select*).
CONTEXT_ANCHORS = {
    'monster': 'copystats', 'item': 'copyitem', 'weapon': 'copyweapon', 'armor': 'copyarmor',
    'spell': 'copyspell', 'site': 'copysite', 'nation': 'clearnation', 'merc': 'bossname',
    'poptype': 'cleardef', 'nametype': 'addname', 'event': 'req_lab', 'global': 'gemlongevity',
    'bless': 'researchgoal', 'sound': 'selectsound', 'top': 'newmonster',
}


class Exe:
    def __init__(self, path):
        self.path = path
        self.data = open(path, 'rb').read()
        d = self.data
        pe = struct.unpack_from('<I', d, 0x3c)[0]
        nsec = struct.unpack_from('<H', d, pe + 6)[0]
        opt = struct.unpack_from('<H', d, pe + 20)[0]
        self.sections = {}
        for i in range(nsec):
            o = pe + 24 + opt + 40 * i
            name = d[o:o + 8].rstrip(b'\0').decode()
            vsz, va, rsz, raw = struct.unpack_from('<IIII', d, o + 8)
            self.sections[name] = (BASE + va, raw, rsz, vsz)
        self.sha = hashlib.sha256(d).hexdigest()[:16]
        m = re.search(rb'\0version (\d+\.\d+)\0', d)
        self.version = m.group(1).decode() if m else None

    def offset(self, addr):
        for va, raw, rsz, vsz in self.sections.values():
            if va <= addr < va + rsz:
                return raw + addr - va
        return None

    def address(self, off):
        for va, raw, rsz, vsz in self.sections.values():
            if raw <= off < raw + rsz:
                return va + off - raw
        return None

    def cstr(self, addr, limit=200):
        o = self.offset(addr)
        if o is None:
            return None
        e = self.data.find(b'\0', o, o + limit)
        if e < 0:
            return None
        try:
            return self.data[o:e].decode('ascii')
        except UnicodeDecodeError:
            return None

    def functions(self):
        """(start, end) of every function, from the exception table (.pdata)."""
        va, raw, rsz, vsz = self.sections['.pdata']
        out = []
        for k in range(0, rsz, 12):
            b, e, u = struct.unpack_from('<III', self.data, raw + k)
            if b:
                out.append((BASE + b, BASE + e))
        return sorted(out)

    def disassembly(self):
        """Parsed objdump output, cached per exe (it takes a few seconds)."""
        cache = os.path.join(tempfile.gettempdir(), 'dom6exe-%s.asm' % self.sha)
        if not os.path.exists(cache):
            with open(cache + '.tmp', 'w') as f:
                subprocess.run(['objdump', '-d', '--no-show-raw-insn', '-M', 'intel', self.path], stdout=f, check=True)
            os.replace(cache + '.tmp', cache)
        pat = re.compile(r'^\s+([0-9a-f]+):\s+(\S+)\s*(.*?)(?:\s+# 0x([0-9a-f]+))?$')
        ins = []
        for line in open(cache):
            m = pat.match(line)
            if m:
                ins.append((int(m.group(1), 16), m.group(2), m.group(3), int(m.group(4), 16) if m.group(4) else None))
        return ins


def num(x):
    try:
        return int(x, 16) if x.startswith('0x') else int(x)
    except (ValueError, AttributeError):
        return None


IDENT = re.compile(r'[a-z_][a-z0-9_]*$')


class Parser:
    """The game's .dm parser, as found in the code."""

    def __init__(self, exe):
        self.exe = exe
        self.ins = exe.disassembly()
        funcs = exe.functions()
        starts = [f[0] for f in funcs]

        def func_of(a):
            i = bisect.bisect_right(starts, a) - 1
            return funcs[i][0] if i >= 0 and a < funcs[i][1] else None

        # every instruction that loads the address of an identifier-like string
        self.refs = {}
        for idx, (a, op, args, tgt) in enumerate(self.ins):
            if tgt is None or op not in ('lea', 'mov', 'cmp'):
                continue
            s = exe.cstr(tgt)
            if s and IDENT.match(s):
                self.refs[idx] = (func_of(a), s)
        # parser functions per context, found by their anchor commands
        by_func = collections.defaultdict(set)
        for f, s in self.refs.values():
            by_func[f].add(s)
        # the top-level dispatcher compares against every #new* / #select*; each entity parser
        # against its own anchor. A function belongs to one context.
        top = {f for f, names in by_func.items() if {'newmonster', 'newweapon', 'newspell', 'selectsite'} <= names}
        self.contexts = {'top': top}
        for ctx, anchor in CONTEXT_ANCHORS.items():
            if ctx != 'top':
                self.contexts[ctx] = {f for f, names in by_func.items() if anchor in names and f not in top}
        self.by_func = by_func
        # generic handlers: the callees most often called with a command name in rdx
        callee = collections.Counter()
        for idx, (a, op, args, tgt) in enumerate(self.ins):
            if op == 'call':
                for j in range(idx - 1, max(idx - 12, 0), -1):
                    if self.ins[j][1] in ('call',) or self.ins[j][1].startswith('j'):
                        break
                    if j in self.refs and self.ins[j][2].startswith('rdx,'):
                        callee[args.split()[0]] += 1
                        break
        self.generic = [c for c, n in callee.most_common(2)]

    def context_commands(self):
        out = {}
        for ctx, fs in self.contexts.items():
            out[ctx] = sorted({s for f in fs for s in self.by_func[f]})
        return out

    def generic_calls(self):
        """(context, command, ability key) for every call to a generic handler."""
        ctx_of = {f: c for c, fs in self.contexts.items() for f in fs}
        res = []
        for i, (a, op, args, tgt) in enumerate(self.ins):
            if op != 'call' or args.split()[0] not in self.generic:
                continue
            j = i - 1
            while j > 0 and self.ins[j][1] != 'call' and i - j < 40:
                j -= 1
            reg, cmd, fn = {}, None, None
            for k in range(j + 1, i):
                aa, oo, rr, tt = self.ins[k]
                if k in self.refs and rr.startswith('rdx,'):
                    fn, cmd = self.refs[k]
                if oo == 'xor' and rr in ('r9d,r9d', 'r8d,r8d'):
                    reg[rr[:2]] = 0
                m = re.match(r'(r8d|r9d),(0x[0-9a-f]+|\d+)$', rr)
                if oo == 'mov' and m:
                    reg[m.group(1)[:2]] = num(m.group(2))
                m = re.match(r'(r8d|r9d),\[(r8|r9)\+(0x[0-9a-f]+)\]$', rr)
                if oo == 'lea' and m and m.group(2) in reg:
                    reg[m.group(1)[:2]] = reg[m.group(2)] + num(m.group(3))
            if cmd:
                res.append((ctx_of.get(fn), cmd, reg.get('r8')))
        return res

    def direct_ability_sets(self, setter):
        """(command, key) for direct calls to the monster ability setter (#magicboost, #sailing, ...)."""
        res = []
        for i, (a, op, args, tgt) in enumerate(self.ins):
            if op != 'call' or args.split()[0] != setter:
                continue
            key = cmd = None
            for j in range(i - 1, max(i - 60, 0), -1):
                aa, oo, rr, tt = self.ins[j]
                m = re.match(r'edx,(0x[0-9a-f]+|\d+)$', rr)
                if key is None and oo == 'mov' and m:
                    key = num(m.group(1))
                if j in self.refs and rr.startswith('rdx,'):
                    cmd = self.refs[j][1]
                    break
            if cmd:
                res.append((cmd, key))
        return res

    def immediates_in(self, ctx, values):
        """For each value, the commands whose branch in the given parser uses it as a constant:
        the nearest command name compared before the instruction."""
        fs = self.contexts[ctx]
        found = collections.defaultdict(set)
        last = None
        for i, (a, op, args, tgt) in enumerate(self.ins):
            if i in self.refs:
                f, s = self.refs[i]
                last = s if f in fs else None
            if last is None:
                continue
            for m in re.finditer(r'(?:,|\+)(0x[0-9a-f]+)\]?$', args):
                v = int(m.group(1), 16)
                if v in values:
                    found[v].add(last)
        return found

    def ability_setter(self):
        """The function both generic handlers end in to add an ability to a monster: the callee
        of the #magicboost branch (the game's own name for the call is unknown)."""
        # #magicboost 51 <n> adds ability 20 (all paths): the call right after loading edx = 20
        idx = [i for i, (f, s) in self.refs.items() if s == 'magicboost' and f in self.contexts['monster']]
        for i in idx:
            window = self.ins[i:i + 80]
            for k, (a, op, args, tgt) in enumerate(window):
                if op == 'mov' and args == 'edx,0x14':
                    for a2, op2, args2, t2 in window[k + 1:k + 6]:
                        if op2 == 'call':
                            return args2.split()[0]
        return None


# ---------------------------------------------------------------------------------------------
# Monster table

MON_RECORD_CHECK = [(1, 'Logrian Slinger'), (2, 'Standard'), (3, 'Serpent Cataphract')]
MON_NAME_LEN = 0x24
MON_STATS = (0x28, 12)          # 12 int16
MON_ABILITIES = (0x40, 48)      # 48 (int64 key, int64 value)
MON_MAX = 20000                 # the parser rejects monster numbers above 19999 (cmp 0x4e1f)


def monster_table(exe):
    d = exe.data
    a = d.find(b'\0' + MON_RECORD_CHECK[0][1].encode() + b'\0') + 1
    b = d.find(b'\0' + MON_RECORD_CHECK[1][1].encode() + b'\0', a) + 1
    size = b - a
    for i, name in MON_RECORD_CHECK:
        o = a + (i - 1) * size
        if d[o:o + len(name)].decode('latin1') != name:
            raise SystemExit('monster table not recognized (record %d is not %r)' % (i, name))
    return a - size, size     # file offset of record 0, record size


def monsters(exe):
    start, size = monster_table(exe)
    va, raw, rsz, vsz = exe.sections['.data']
    out = {}
    i = 0
    while i < MON_MAX and start + (i + 1) * size <= raw + rsz:
        r = exe.data[start + i * size:start + (i + 1) * size]
        name = r[:MON_NAME_LEN].split(b'\0')[0].decode('latin1')
        if name:
            ab = {}
            for k in range(MON_ABILITIES[1]):
                key, val = struct.unpack_from('<qq', r, MON_ABILITIES[0] + 16 * k)
                if key:
                    ab[key] = val
            out[i] = {'name': name, 'stats': list(struct.unpack_from('<%dh' % MON_STATS[1], r, MON_STATS[0])), 'abilities': ab}
        i += 1
    # layout check: Heavy Cavalry rides mount 3515 (ability 1015) in Dominions 6.37
    hc = out.get(20)
    if not hc or hc['name'] != 'Heavy Cavalry' or not hc['abilities']:
        raise SystemExit('monster abilities not where expected; the record layout changed')
    return out


def ability_names(mons, inspector_dir):
    """Name ability keys after the dom6inspector unit CSV column whose values agree (hints only)."""
    path = os.path.join(inspector_dir, 'gamedata', 'BaseU.csv') if inspector_dir else None
    if not path or not os.path.exists(path):
        return {}
    rows = {int(r['id']): r for r in csv.DictReader(open(path), delimiter='\t')}
    cols = [c for c in next(iter(rows.values())) if c not in ('id', 'name', 'end')]
    nonzero = {c: {i for i, r in rows.items() if (r.get(c) or '') not in ('', '0')} for c in cols}
    holders = collections.defaultdict(dict)
    for i, m in mons.items():
        if i in rows:
            for k, v in m['abilities'].items():
                holders[k][i] = str(v)
    names = {}
    for key, h in holders.items():
        match = collections.Counter(c for i, v in h.items() for c in cols if rows[i].get(c) == v)
        best = None
        for c, agree in match.items():
            ratio = agree / len(set(h) | nonzero[c])
            if best is None or ratio > best[1]:
                best = (c, ratio)
        if best and best[1] >= 0.9:
            names[key] = best[0]
    return names


# ---------------------------------------------------------------------------------------------

def cmd_commands(exe, args):
    p = Parser(exe)
    gen = p.generic_calls()
    keys = collections.defaultdict(dict)
    for ctx, cmd, key in gen:
        if ctx in ('monster', 'item') and key is not None:
            keys[ctx][cmd] = key
    setter = p.ability_setter()
    if setter:
        for cmd, key in p.direct_ability_sets(setter):
            if key is not None:
                keys['monster'].setdefault(cmd, key)
    return {
        'game_version': exe.version, 'exe_sha256_16': exe.sha,
        'generic_handlers': p.generic,
        'ability_setter': setter,
        'contexts': p.context_commands(),
        'ability_keys': {ctx: dict(sorted(v.items())) for ctx, v in keys.items()},
    }


def cmd_monsters(exe, args):
    mons = monsters(exe)
    return {'game_version': exe.version, 'exe_sha256_16': exe.sha, 'count': len(mons), 'monsters': mons}


def cmd_readonly(exe, args):
    c = cmd_commands(exe, args)
    mons = monsters(exe)
    settable = set(c['ability_keys'].get('monster', {}).values())
    # #magicboost writes 10 + path (0-9) and 20-22 for the multi-path forms
    if 'magicboost' in c['contexts']['monster']:
        settable |= set(range(10, 23))
    names = ability_names(mons, args.inspector)
    used = collections.Counter(k for m in mons.values() for k in m['abilities'])
    # commands with their own handler: the ability number appears as a constant in their branch
    p = Parser(exe)
    candidates = p.immediates_in('monster', {k for k in used if k not in settable and k > 30})
    ro = []
    for k, n in sorted(used.items(), key=lambda kv: -kv[1]):
        if k in settable:
            continue
        holders = [i for i, m in mons.items() if k in m['abilities']]
        ro.append({'ability': k, 'name_hint': names.get(k), 'monsters': n,
                   'possibly_set_by': sorted(candidates.get(k, ())),
                   'examples': ['%d %s = %s' % (i, mons[i]['name'], mons[i]['abilities'][k]) for i in holders[:3]]})
    return {'game_version': exe.version, 'exe_sha256_16': exe.sha, 'vanilla_ability_keys': len(used), 'settable_by_monster_commands': len(set(used) & settable),
            'not_settable': ro}


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('what', choices=['commands', 'monsters', 'readonly'])
    ap.add_argument('--exe', default=os.environ.get('DOM6_EXE', DEFAULT_EXE))
    ap.add_argument('--inspector', default=os.environ.get('DOM6INSPECTOR', '/mnt/c/Projects/dom6inspector'),
                    help='dom6inspector checkout, for naming ability numbers (hints only)')
    ap.add_argument('--out', help='write JSON here (default: stdout)')
    args = ap.parse_args()
    exe = Exe(args.exe)
    res = {'commands': cmd_commands, 'monsters': cmd_monsters, 'readonly': cmd_readonly}[args.what](exe, args)
    text = json.dumps(res, indent=1, default=str)
    if args.out:
        open(args.out, 'w').write(text + '\n')
        print('wrote', args.out, file=sys.stderr)
    else:
        print(text)


if __name__ == '__main__':
    main()
