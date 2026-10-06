"""The other vanilla tables the editor lists, written from Dominions6.exe: blesses, poptypes,
nametypes and mercenaries (dom6exe.py vanilla writes them into vanilla.dm after the nations).

As for the other types, each table and its layout come from the parser branches that write it,
and each stored value is written as the command that stores it; values no command stores are
"-- ro:" lines. Checked on every run: each table's records against the end marker or count the
parser uses, and a few vanilla values that only one layout gives.

What the 6.37 code does (addresses are 6.37's):
- Blesses: #selectbless N picks record N (0-99) of a static table, or a bless by name. A record
  holds the name (32 bytes), #path0/#cost0/#path1/#cost1 (int16), two battle-buff words and up
  to 7 (monster ability, value) pairs (the effects), and 4 (scale, value) byte pairs that end at
  0xff (the scale requirements). No command adds effects: #clearfx only empties them, so they are
  read-only. Scale commands share one setter (0x140228ff0) called with the scale's number
  (#chaosscale 0, #slothscale 1, ... #orderscale 6, ...); a scale already listed is replaced.
  #cost1 also raises #path1 to at least 0 (fire), so a bless without a second path gets neither.
  The record after the last vanilla bless is named "end".
- Poptypes: #selectpoptype N (0-249) picks entry N of two tables: the recruitment list (int32
  units, -2, commanders, -1: #addrecunit inserts before -2, #addreccom before -1; #clearrec
  leaves -2, -1) and the defenders, (key, value) int32 pairs ended by key 0 that the generic
  handler writes (#defunit1 215, #defmult1 216, ...). The game reads the first pair with a key
  (0x1402e9760). Poptypes 25-106 have data; no poptype has a name in the game.
- Nametypes: #selectnametype N (100-499) picks a list of up to 1500 name pointers that ends at
  a pointer to "end". #addname appends, #clear empties. The first #selectnametype a game reads
  empties the lists from 169 on, so vanilla lists are 100-168.
- Mercenaries: a table of 300 bands; the vanilla ones are the records before the first whose
  #level byte is 99 (named "end"). There is no #selectmerc: a mod can't change a vanilla band;
  #newmerc takes the first free record after them and #clearmercs marks record 0 as the end.
  So the vanilla bands are written as the #newmerc blocks that would make them, and the editor
  shows them read-only. #newmerc starts a band with #eramask 7, #minpay 100; #unit also sets
  #nrunits 10 when it is 0, so #nrunits comes after it. Up to 7 (nation, percent) pairs no
  command writes set the band's minimum pay for that nation (the hire price function
  0x140224a00 uses #minpay x percent / 100, 100 for other nations): read-only.
"""
import collections
import re
import struct

from dom6exe import BASE, Parser, num, text


# ---------------------------------------------------------------------------------------------
# helpers: a command's branch in its parser, and what it stores

def branch(p, ctx, cmd):
    """Instruction indices of a command's branch in a parser: from its name check to the next
    command's. Parsers that compare inline (cmp BYTE PTR [name],0) load some names ahead, so the
    compares bound the branches when the parser has them."""
    fs = p.contexts[ctx]
    order = sorted(k for k, (f, s) in p.refs.items() if f in fs)
    cmps = [k for k in order if p.ins[k][1] == 'cmp']
    if any(p.refs[k][1] == cmd for k in cmps):
        order = cmps
    for n, i in enumerate(order):
        if p.refs[i][1] == cmd:
            later = [j for j in order[n + 1:] if p.refs[j][1] != cmd and p.refs[j][0] == p.refs[i][0]]
            if later:
                return range(i, later[0])
            # the parser's last command: to the end of its function
            end = next(e for b, e in p.exe.functions() if b == p.refs[i][0])
            return range(i, next(k for k in range(i, i + 2000) if p.ins[k][0] >= end))
    raise SystemExit('no #%s branch in the %s parser' % (cmd, ctx))


def fields_of(p, ctx):
    """(table, {command: (offset, size)}) for the commands that store their parsed argument in
    the table's records (Parser.branches)."""
    br = p.branches(ctx)
    base = collections.Counter(e[1] for effs in br.values() for e in effs if e[1] is not None).most_common(1)[0][0]
    fields = {}
    for cmd, effs in br.items():
        for kind, b, off, size, val in effs:
            if kind == 'store' and b == base and val == ('arg', 0):
                fields.setdefault(cmd, (off, size))
    return base, fields


def record_size(p, idx, base):
    """The record size a branch multiplies the record number by before addressing the table
    (imul r,r,size; ... lea r,[base]), or `lea r,[a+a*k]; shl r,s` for (k + 1) << s."""
    ins = p.ins
    for k in idx:
        a, o, args, t = ins[k]
        m = re.match(r'\w+,\w+,(0x[0-9a-f]+)$', args)
        if o == 'imul' and m and any(x[3] == base for x in ins[k - 3:k + 4]):
            return num(m.group(1))
        m = re.match(r'(\w+),\[(\w+)\+\2\*(\d)\]$', args)
        if o == 'lea' and m and ins[k + 1][1] == 'shl':
            return (int(m.group(3)) + 1) << num(ins[k + 1][2].split(',')[1])
    return None


def table_target(p, idx, lo, hi):
    """The first address in [lo, hi) a branch loads."""
    return next((p.ins[k][3] for k in idx if p.ins[k][1] == 'lea' and p.ins[k][3] is not None and lo <= p.ins[k][3] < hi), None)


def cmp_limit(p, idx, regs=r'e\w\w|r\d+d'):
    """The constant a branch compares a register with (cmp reg,N)."""
    for k in idx:
        m = re.match(r'(%s),(0x[0-9a-f]+)$' % regs, p.ins[k][2])
        if p.ins[k][1] == 'cmp' and m:
            return num(m.group(2))
    return None


def cstr(exe, b):
    return text(b.split(b'\0')[0])


def q(s):
    return '"%s"' % s.replace('"', "'")


# ---------------------------------------------------------------------------------------------
# Blesses

SCALE_CMDS = ['chaosscale', 'slothscale', 'coldscale', 'deathscale', 'misfortscale', 'drainscale',
              'orderscale', 'prodscale', 'heatscale', 'growthscale', 'luckscale', 'magicscale']


class BlessTable:
    def __init__(self, exe, p):
        self.exe, self.p = exe, p
        self.base, f = fields_of(p, 'bless')
        for cmd in ('path0', 'cost0', 'path1', 'cost1'):
            if f.get(cmd, (None, 0))[1] != 2:
                raise SystemExit('bless #%s: no int16 store found' % cmd)
        self.fields = {c: f[c][0] for c in ('path0', 'cost0', 'path1', 'cost1')}
        self.record = record_size(p, branch(p, 'bless', 'path0'), self.base)
        sel = branch(p, 'bless', 'selectbless')
        self.count = cmp_limit(p, sel)
        # #clearfx zeroes the two buff words and the first effect's key; #clearscales ends the
        # scale list
        br = p.branches('bless')
        clears = sorted(off for kind, b, off, size, val in br['clearfx'] if kind == 'store' and b == self.base and size == 8)
        self.buffs, self.effects = clears[0], clears[-1]
        self.scales, self.scale_end = next((off, val) for kind, b, off, size, val in br['clearscales']
                                           if kind == 'store' and b == self.base and size == 1)
        self.n_effects = (self.scales - self.effects) // 16
        self.n_scales = None
        # the scale commands: the number each passes (edx) to the shared setter call, which
        # they jump to (or the last one runs into)
        jumps = collections.Counter()
        for cmd in SCALE_CMDS:
            jumps.update(p.ins[k][2].split()[0] for k in branch(p, 'bless', cmd) if p.ins[k][1] == 'jmp')
        tail = num(jumps.most_common(1)[0][0])
        setter = next(x[2].split()[0] for x in p.ins[p.index[tail]:p.index[tail] + 6] if x[1] == 'call')
        i0 = p.index[num(setter)]
        # it scans the (scale, value) bytes: cmp counter,2 * slots
        self.n_scales = next(num(x[2].split(',')[1]) for x in p.ins[i0:i0 + 40]
                             if x[1] == 'cmp' and re.match(r'r\d+d,0x[0-9a-f]+$', x[2])) // 2
        self.scale_cmd = {}
        for cmd in SCALE_CMDS:
            edx = None
            for k in branch(p, 'bless', cmd):
                a, o, args, t = p.ins[k]
                if o == 'mov' and re.match(r'edx,0x[0-9a-f]+$', args):
                    edx = num(args.split(',')[1])
                elif o == 'xor' and args == 'edx,edx':
                    edx = 0
                if edx is not None and (o == 'jmp' and args.split()[0] == hex(tail) or a == tail):
                    break
            else:
                edx = None
            if edx is None:
                raise SystemExit('bless #%s: no scale number' % cmd)
            self.scale_cmd.setdefault(edx, cmd)
        if len(self.scale_cmd) != 12:
            raise SystemExit('bless scales: expected 12, found %s' % self.scale_cmd)
        if None in (self.record, self.count, self.n_scales):
            raise SystemExit('bless table: record size, count or scale slots not found')

    def records(self):
        o = self.exe.offset(self.base)
        out = {}
        for i in range(self.count):
            r = self.exe.data[o + i * self.record:o + (i + 1) * self.record]
            name = cstr(self.exe, r[:32])
            if name in ('end', ''):
                break
            out[i] = r
        else:
            raise SystemExit('bless table: no "end" record')
        return out


def bless_commands(exe, p, monster_model):
    t = BlessTable(exe, p)
    out = {}
    for i, r in t.records().items():
        h = lambda c: struct.unpack_from('<h', r, t.fields[c])[0]
        lines, ro = ['#name ' + q(cstr(exe, r[:32]))], []
        lines += ['#path0 %d' % h('path0'), '#cost0 %d' % h('cost0')]
        if h('path1') >= 0:
            lines += ['#path1 %d' % h('path1'), '#cost1 %d' % h('cost1')]
        elif h('cost1'):
            ro.append(('#cost1 without a second path', h('cost1')))
        for k in range(t.n_scales):
            s, v = r[t.scales + 2 * k], struct.unpack_from('<b', r, t.scales + 2 * k + 1)[0]
            if s == t.scale_end:
                break
            if s in t.scale_cmd:
                lines.append('#%s %d' % (t.scale_cmd[s], v))
            else:
                ro.append(('scale %d' % s, v))
        b1, b2 = struct.unpack_from('<QQ', r, t.buffs)
        if b1:
            ro.append(('battle buffs', hex(b1)))
        if b2:
            ro.append(('battle buffs 2', hex(b2)))
        for k in range(t.n_effects):
            key, val = struct.unpack_from('<qq', r, t.effects + 16 * k)
            if not key:
                break
            # (monster ability numbers: named by the monster command that stores the value)
            line = monster_model.ability_line(key, val)
            ro.append(('effect', line if line else '%s: %d' % (monster_model.ability_label(key), val)))
        out[i] = {'lines': lines, 'readonly': ro}
    return out, t


# ---------------------------------------------------------------------------------------------
# Poptypes

class PoptypeTables:
    def __init__(self, exe, p):
        self.exe, self.p = exe, p
        br = p.branches('poptype')

        def cleared(cmd):
            return next((b, off, size, val) for kind, b, off, size, val in br[cmd] if kind == 'store')
        self.rec, _, _, empty = cleared('clearrec')
        self.dfn, _, _, _ = cleared('cleardef')
        # #clearrec stores the empty list as one int64: two int32 markers (-2, -1)
        self.unit_end, self.com_end = struct.unpack('<ii', struct.pack('<Q', empty))
        self.rec_size = record_size(p, branch(p, 'poptype', 'clearrec'), self.rec)
        self.def_size = record_size(p, branch(p, 'poptype', 'cleardef'), self.dfn)
        self.count = cmp_limit(p, branch(p, 'poptype', 'selectpoptype')) + 1
        # #addrecunit / #addreccom: the marker each inserts before (r8d of the insert call)
        self.insert_before = {}
        for cmd in ('addrecunit', 'addreccom'):
            r8 = None
            for k in branch(p, 'poptype', cmd):
                a, o, args, t = p.ins[k]
                if o == 'mov' and re.match(r'r8d,0x[0-9a-f]+$', args):
                    r8 = struct.unpack('<i', struct.pack('<I', num(args.split(',')[1])))[0]
                elif o == 'or' and args == 'r8d,0xffffffff':
                    r8 = -1
                elif o == 'call':
                    if r8 is not None and r8 < 0:
                        break
                    r8 = None
            self.insert_before[cmd] = r8
        if self.insert_before != {'addrecunit': self.unit_end, 'addreccom': self.com_end}:
            raise SystemExit('poptype recruitment markers: #clearrec stores %d, %d; the commands insert before %s'
                             % (self.unit_end, self.com_end, self.insert_before))
        self.generic = collections.defaultdict(list)
        for c in p.generic_call_args():
            if c['context'] == 'poptype' and c['key'] is not None:
                self.generic[c['key']].append(c)
        if not (self.rec_size and self.def_size and self.count):
            raise SystemExit('poptype tables: sizes not found')

    def records(self):
        d, out = self.exe.data, {}
        ro, do = self.exe.offset(self.rec), self.exe.offset(self.dfn)
        for i in range(self.count):
            rec = struct.unpack_from('<%di' % (self.rec_size // 4), d, ro + i * self.rec_size)
            dfn = struct.unpack_from('<%di' % (self.def_size // 4), d, do + i * self.def_size)
            units, coms, part = [], [], 0
            for x in rec:
                if x == self.com_end and part == 1:
                    break
                if x == self.unit_end and part == 0:
                    part = 1
                    continue
                if x <= 0:
                    units = coms = None     # not a list (an unused entry is all zeros)
                    break
                (units if part == 0 else coms).append(x)
            pairs = []
            for k in range(self.def_size // 8):
                key, val = dfn[2 * k], dfn[2 * k + 1]
                if not key:
                    break
                pairs.append((key, val))
            if units is not None and (units or coms or pairs):
                out[i] = (units, coms, pairs)
        return out


def poptype_commands(exe, p):
    t = PoptypeTables(exe, p)
    recs = t.records()
    # layout check: Barbarians (25) recruit 139 and 140, commander 141, and defend with 139
    if recs.get(25, ([], [], []))[:2] != ([139, 140], [141]) or recs[25][2][:1] != [(215, 139)]:
        raise SystemExit('poptype tables not where expected (25: %s)' % (recs.get(25),))
    out = {}
    for i, (units, coms, pairs) in recs.items():
        lines, ro = [], []
        lines += ['#addrecunit %d' % u for u in units]
        lines += ['#addreccom %d' % c for c in coms]
        seen = set()
        for key, val in pairs:
            calls = t.generic.get(key, [])
            if key in seen:
                ro.append(('defender %s repeated (the game reads the first)' % (
                    '#' + calls[0]['command'] if calls else 'key %d' % key), val))
                continue
            seen.add(key)
            line = None
            for c in calls:
                if (c['min'] is None or c['min'] <= val) and (c['max'] is None or val <= c['max']):
                    line = '#%s %d' % (c['command'], val)
                    break
            if line:
                lines.append(line)
            else:
                ro.append(('defender key %d' % key, val))
        out[i] = {'lines': lines, 'readonly': ro}
    return out, t


# ---------------------------------------------------------------------------------------------
# Nametypes

class NametypeTable:
    def __init__(self, exe, p):
        self.exe, self.p = exe, p
        sel = branch(p, 'nametype', 'selectnametype')
        # #selectnametype N: N - first in 0..last (add eax,-first; cmp eax,last)
        first = last = None
        for k in sel:
            a, o, args, t = p.ins[k]
            m = re.match(r'eax,(0x[0-9a-f]+)$', args)
            if o == 'add' and m:
                first = -struct.unpack('<i', struct.pack('<I', num(m.group(1))))[0]
            elif o == 'cmp' and m and first is not None:
                last = num(m.group(1))
                break
        self.first, self.count = first, last + 1
        add = branch(p, 'nametype', 'addname')
        # the list table: the address #addname indexes by the list number times the list size
        for k in add:
            a, o, args, t = p.ins[k]
            m = re.match(r'\w+,\w+,(0x[0-9a-f]+)$', args)
            if o == 'imul' and m:
                self.size = num(m.group(1))
                self.base = next(x[3] for x in p.ins[k - 3:k + 3] if x[1] == 'lea' and x[3] is not None)
                break
        self.slots = self.size // 8
        # the sentinel #addname looks for: a pointer to this string
        self.end = next(self.exe.cstr(p.ins[k][3]) for k in add if p.ins[k][1] == 'lea' and p.ins[k][3] is not None
                        and self.exe.cstr(p.ins[k][3]) == 'end')
        # the first #selectnametype empties the lists from here on
        self.mod_first = None
        for k in sel:
            t = p.ins[k][3]
            if p.ins[k][1] == 'lea' and t is not None and self.base < t < self.base + self.size * self.count \
                    and (t - self.base) % self.size == 0:
                self.mod_first = self.first + (t - self.base) // self.size
        if None in (self.first, self.count, self.mod_first):
            raise SystemExit('nametype table not found')

    def lists(self):
        d, o, out = self.exe.data, self.exe.offset(self.base), {}
        for i in range(self.count):
            names = []
            for j in range(self.slots):
                ptr = struct.unpack_from('<Q', d, o + i * self.size + 8 * j)[0]
                if not ptr:
                    break
                po = self.exe.offset(ptr)
                s = text(d[po:d.find(b'\0', po)]) if po is not None else None
                if s is None:
                    raise SystemExit('nametype %d: name %d is not a string' % (self.first + i, j))
                if s == self.end:
                    break
                names.append(s)
            else:
                raise SystemExit('nametype %d: no end' % (self.first + i))
            if names:
                out[self.first + i] = names
        return out


def nametype_commands(exe, p):
    t = NametypeTable(exe, p)
    lists = t.lists()
    if any(n >= t.mod_first for n in lists):
        raise SystemExit('nametypes past %d have names; the game empties those' % t.mod_first)
    out = {n: {'lines': ['#addname ' + q(s) for s in names], 'readonly': []} for n, names in lists.items()}
    return out, t


# ---------------------------------------------------------------------------------------------
# Mercenaries

MERC_FIELDS = ['level', 'com', 'unit', 'nrunits', 'minmen', 'minpay', 'xp', 'randequip', 'recrate', 'eramask']


class MercTable:
    def __init__(self, exe, p):
        self.exe, self.p = exe, p
        self.base, f = fields_of(p, 'merc')
        missing = [c for c in MERC_FIELDS if c not in f]
        if missing:
            raise SystemExit('merc commands without a store: %s' % missing)
        self.fields = {c: f[c] for c in MERC_FIELDS}
        self.record = record_size(p, branch(p, 'merc', 'xp'), self.base)
        hi = self.base + self.record
        # strings: the address each branch copies to
        self.name = table_target(p, branch(p, 'merc', 'name'), self.base, hi) - self.base
        self.boss = table_target(p, branch(p, 'merc', 'bossname'), self.base, hi) - self.base
        item = branch(p, 'merc', 'item')
        self.items = table_target(p, item, self.base, hi) - self.base
        self.n_items = cmp_limit(p, item)
        # #newmerc: how many bands fit ("too many #newmerc"), and the end marker it writes into
        # the record after the new band (the #level byte)
        new = branch(p, 'merc', 'newmerc')
        self.count = cmp_limit(p, new, r'esi|r\d+d')
        level_off = self.fields['level'][0]
        self.end_marker = None
        for k in new:
            m = re.match(r'BYTE PTR \[\w+\+\w+\*1\+(0x[0-9a-f]+)\],(0x[0-9a-f]+)$', p.ins[k][2])
            if p.ins[k][1] == 'mov' and m and num(m.group(1)) == level_off:
                self.end_marker = num(m.group(2))
        # the per-nation pay: the words after #xp that the band setup copies (read, not written
        # by any command): find its reads of the record, 15 int16 from the int32 slots
        lo, hi2 = self.fields['xp'][0] + 4, self.fields['randequip'][0]
        reads = set()
        for a, o, args, t in p.ins:
            m = re.search(r'WORD PTR \[\w+\+\w+\*1\+(0x[0-9a-f]+)\]$', args)
            if o == 'movzx' and m:
                off = BASE + num(m.group(1)) - self.base
                if lo <= off < hi2:
                    reads.add(off)
        self.pay = sorted(reads)
        if None in (self.record, self.count, self.end_marker, self.n_items) or len(self.pay) < 2:
            raise SystemExit('merc table not found')

    def records(self):
        d, o, out = self.exe.data, self.exe.offset(self.base), []
        for i in range(self.count):
            r = d[o + i * self.record:o + (i + 1) * self.record]
            v = {}
            for c, (off, size) in self.fields.items():
                v[c] = struct.unpack_from({1: '<b', 2: '<h', 4: '<i'}[size], r, off)[0]
            v['name'] = cstr(self.exe, r[self.name:self.name + 36])
            if v['level'] == self.end_marker:
                if v['name'] != 'end':
                    raise SystemExit('merc table: the end record is named %r' % v['name'])
                return out
            v['bossname'] = cstr(self.exe, r[self.boss:self.boss + 36])
            v['items'] = [s for s in (cstr(self.exe, r[self.items + 36 * k:self.items + 36 * (k + 1)])
                                      for k in range(self.n_items)) if s]
            words = [struct.unpack_from('<h', r, off)[0] for off in self.pay]
            v['pay'] = [(words[k], words[k + 1]) for k in range(0, len(words) - 1, 2) if words[k]]
            out.append(v)
        raise SystemExit('merc table: no end record')


def merc_commands(exe, p, nations, monsters_known):
    t = MercTable(exe, p)
    bands = t.records()
    # layout check: every band's commander is a vanilla monster and its pay pairs name nations
    for v in bands:
        if v['com'] not in monsters_known or v['unit'] and v['unit'] not in monsters_known:
            raise SystemExit('merc %r: commander/unit %d/%d not monsters' % (v['name'], v['com'], v['unit']))
        if any(n not in nations or not 0 < pct <= 1000 for n, pct in v['pay']):
            raise SystemExit('merc %r: per-nation pay %s does not name nations' % (v['name'], v['pay']))
    out = []
    for v in bands:
        lines = ['#name ' + q(v['name']), '#bossname ' + q(v['bossname']), '#level %d' % v['level'],
                 '#com %d' % v['com']]
        if v['unit']:
            lines.append('#unit %d' % v['unit'])      # (sets #nrunits 10 when it is 0)
        lines += ['#nrunits %d' % v['nrunits'], '#minmen %d' % v['minmen'], '#minpay %d' % v['minpay'],
                  '#xp %d' % v['xp'], '#randequip %d' % v['randequip'], '#recrate %d' % v['recrate'],
                  '#eramask %d' % v['eramask']]
        lines += ['#item ' + q(s) for s in v['items']]
        ro = [('minimum pay for nation %d (%s)' % (n, nations[n]), '%d%%' % pct) for n, pct in v['pay']]
        out.append({'lines': lines, 'readonly': ro, 'values': v})
    return out, t
