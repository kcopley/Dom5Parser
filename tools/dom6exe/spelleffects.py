"""What a spell's #damage means for each #effect, read from Dominions6.exe (dom6exe.py spelleffects).

A spell's #damage is a monster for a summoning effect, an enchantment for a global one, an event
#id for 10042, a bitmask for buffs and afflictions, and a plain number (damage, years, gems) for
the rest. A mod merger renumbers monsters, enchantments and event ids, so it must know exactly
which effects take which; the inspector's tables guess some of them wrong. This reads the
effect code itself.

Where the code is (found by the strings the functions print, not by address):
  castlabspell     "castlabspell: bad spellnr (%s, %d)"; the ritual effects (#effect 10000 and
                   up): the effect number, minus 10000, compared one value at a time
  spellblastsquare "spellblastsquare: %s (nof%d, ..." (a combat spell landing): which effects'
                   damage is scaled by the caster's level (>= 1000) and which is an identifier
  blastsquare      "blastsquare: bad unr" (one square): the summoning effects
  hitunit          "hitunit: bad vic" (one unit hit): everything else in battle
  evalspell        "evalspell: bad cnr" (the AI's spell scoring)
and the functions a #damage can end up in (the sinks):
  resolve_summon   "*** Bad summondmg %d": a monster number, -2..-28 special picks (random
                   undead, horror, ...), -1000 and below a monster tag (#montag), -1 nothing
  unique_pick      the function the ritual passes its #damage to that calls resolve_summon for
                   a negative one and compares the others with small keys: 1-99 a hard-coded
                   list of uniques (Bind Ice Devil = 1), 100 and up the monster number itself
  newtempunit      "newtempunit: bad mnr" (battle summons), and every other function that hands
                   one of its arguments to resolve_summon (new units): a monster or tag
  newcom           "newcom: mnr %d, ..." (a monster number, no tags)
  transform        what hitunit's polymorph passes resolve_summon's monster to (a monster)
  newench          "newench %d by %s" (an enchantment number; global and battlefield ones share
                   the numbers: #req_ench sees both)
  spell event      the queue "  unknown spell event id %d" reads (an event's #id, effect code 40)
  addfeatnr        "addfeatnr: bad featnr" (a site number)
  addunitfx        "addunitfx: unr%d fx%d fxattr%d" / "setunitfx: ..." (a unit ability: as the fx
                   number it's an ability, as its value a count)
  age              "Age %s %d years ..." (years)
A branch's #damage is followed back from each call (and each OR/AND/TEST into memory) to the
spell record's damage field (+0x38), the ritual's level-scaled copy, or the battle functions'
damage argument. What the automatic reading can't name (years, a fort type, a gem path) comes
from NOTES below; a note that disagrees with the code is reported.
"""
import bisect, collections, re

from dom6exe import BASE, TABLES, find_table, reg64

S_EFFECT, S_DAMAGE = 0x2e, 0x38          # spell record fields (vanilla_dm.py S_FIELDS)
LIMIT = 400                               # instructions an origin search walks back
VOLATILE = ('rax', 'rcx', 'rdx', 'r8', 'r9', 'r10', 'r11')
ARGS = ('rcx', 'rdx', 'r8', 'r9')
MEMOP = re.compile(r'(?:(BYTE|WORD|DWORD|QWORD|XMMWORD) PTR )?\[([^\]]+)\]')
REFERENCES = ('monster', 'monster_or_tag', 'unique_or_monster_or_tag', 'enchantment', 'event', 'site')

# What the code alone can't name: (argument, note). The argument of a reference kind is checked
# against the automatic reading; numbers and codes are labels.
NOTES = {
    # battle
    0: ('unused', 'no effect'),
    2: ('damage', 'damage (#dt_normal)'), 3: ('damage', 'fatigue damage'), 4: ('damage', 'fear: morale penalty'),
    7: ('damage', 'poison damage'), 8: ('damage', 'removes this much fatigue (negated into the fatigue change)'),
    10: ('bitmask', 'battle buffs: ORed into the unit\'s first buff word (eff_ben)'),
    11: ('bitmask', 'afflictions (web, false fetters, slime, sleep, disease, ...): the affliction bits'),
    13: ('damage', 'healing'), 15: ('unused', 'return home (any value above 0)'),
    17: ('damage', 'morale change, added to the unit\'s morale'), 20: ('unused', 'blink (any value above 0)'),
    23: ('bitmask', 'battle buffs: ORed into the unit\'s second buff word'),
    24: ('damage', 'holy damage'), 25: ('damage', 'holy damage'), 27: ('damage', 'magic duel strength (999)'),
    28: ('other', 'enslave (999 by convention)'), 29: ('other', 'charm (999 by convention)'),
    32: ('damage', 'damage'), 33: ('damage', 'damage'), 36: ('damage', 'disintegrate (999)'),
    46: ('damage', 'damage'), 56: ('damage', 'damage'), 66: ('damage', 'paralysis'), 67: ('damage', 'weakness: subtracted from ability 1058'),
    72: ('damage', 'damage'), 73: ('damage', 'damage'), 74: ('damage', 'unlife damage (the risen form is the victim\'s #raiseshape, not the damage)'),
    75: ('damage', 'unlife damage'), 96: ('damage', 'damage'), 97: ('damage', 'fear'), 99: ('damage', 'petrify (999)'),
    101: ('damage', 'years of age'), 103: ('damage', 'drain life'), 104: ('damage', 'partial drain'),
    105: ('damage', 'disbelieve (999)'), 107: ('damage', 'damage'),
    108: ('other', 'where the victim is banished: a location code (-12 Inferno, -13 Kokytos) stored as the unit\'s location'),
    109: ('damage', 'capped damage'), 116: ('damage', 'damage'), 123: ('damage', 'damage'), 128: ('damage', 'stun'),
    129: ('damage', 'damage'), 134: ('damage', 'bouncing damage'), 138: ('damage', 'armor damage'),
    139: ('damage', 'capped poison'), 142: ('damage', 'salt damage'),
    144: ('bitmask', 'cloud types (3 bits each) laid on the squares'), 145: ('bitmask', 'cloud types'),
    146: ('bitmask', 'cloud types'), 147: ('bitmask', 'cloud types'), 148: ('bitmask', 'cloud types'),
    149: ('bitmask', 'cloud types'), 150: ('bitmask', 'cloud types'),
    159: ('other', 'a mode (-1, 1, 2, 3, ...)'), 162: ('count', 'images: stored as the value of ability 1096'),
    166: ('unused', 'animate: the new form is the target\'s #animated, not the damage'),
    (500, 599): ('ability', 'a monster ability number, set to (effect - 499) unless higher'),
    (600, 699): ('ability', 'a monster ability number, increased by (effect - 599)'),
    # rituals
    10010: ('bitmask', 'ORed into the caster\'s first buff word'), 10019: ('unused', 'teleport'),
    10022: ('unused', ''), 10023: ('bitmask', 'ORed into the caster\'s second buff word'),
    10030: ('unused', 'dispel'), 10034: ('unused', 'wish'), 10035: ('unused', 'cross breeding'),
    10039: ('unused', 'gift of reason'), 10040: ('damage', 'seeking arrow: damage (level-scaled)'),
    10041: ('damage', 'damage (level-scaled, then by temperature)'), 10044: ('unused', 'transformation'),
    10045: ('unused', 'force transformation (a random monster with an ability)'),
    10048: ('other', 'which gems the site search looks for: a path 0-9, 51 (elements), 55 (all), or a mask'),
    10049: ('unused', 'wind ride'), 10052: ('unused', ''), 10053: ('unused', 'vengeance of the dead'),
    10057: ('unused', 'mind hunt'), 10063: ('other', 'the fort built: a fort type (level-scaled)'),
    10064: ('bitmask', 'afflictions given to the army (only the low 32 bits)'),
    10068: ('unused', 'summon animals: the animals depend on the terrain'),
    10070: ('damage', 'wall damage (level-scaled; negative)'), 10076: ('unused', 'Tartarian gate: hard-coded units'),
    10077: ('unused', 'gateway'), 10079: ('unused', 'faery trod'), 10090: ('unused', 'stygian paths'),
    10091: ('damage', 'damage of the remote battle effect'), 10092: ('count', 'souls imprinted (level-scaled)'),
    10094: ('damage', 'damage'), 10095: ('unused', 'cloud trapeze'), 10098: ('unused', 'winged monkeys'),
    10100: ('other', 'a terrain list key 1-3: the units are hard-coded'),
    10101: ('count', 'years of age'), 10102: ('unused', 'horror seed'), 10110: ('unused', 'dreams of R\'lyeh'),
    10111: ('count', 'years of age, subtracted'), 10112: ('damage', 'damage to the caster'),
    10113: ('unused', 'astral harpoon'), 10115: ('unused', 'acashic record'),
    10116: ('unused', 'unleash imprisoned ones: hard-coded units'), 10117: ('count', 'gold per gem (level-scaled)'),
    10118: ('count', 'blood feast'), 10125: ('unused', 'mind vessel'), 10127: ('unused', 'infernal breeding'),
    10131: ('bitmask', 'afflictions cured (cleared from the affliction word)'),
    10132: ('bitmask', 'afflictions cured (cleared from the affliction word)'), 10135: ('unused', 'raven feast'),
    10136: ('bitmask', 'the curse (affliction bits, level-scaled)'), 10152: ('unused', 'disenchantment'),
    10086: ('unused', 'no branch in castlabspell: nothing happens (Dom5 had a remote single-turn enchantment)'),
    10153: ('other', 'an element compared with the target'), 10155: ('other', 'a region index (table of 564-byte records)'),
    10156: ('unused', 'arcane analysis'), 10157: ('unused', 'astral disruption'),
    10160: ('count', 'gems transported (level-scaled)'), 10161: ('unused', 'transport item'),
    10163: ('unused', 'nexus gate'), 10164: ('count', 'gold'), 10167: ('unused', 'lichcraft'),
    10168: ('other', 'project self: passed to the province search'), 10169: ('unused', 'army teleport'),
    (10500, 10599): ('ability', 'a monster ability number on the caster, set to (effect - 10499) unless higher'),
}


def split_args(args):
    depth, parts, cur = 0, [], ''
    for ch in args:
        if ch == '[':
            depth += 1
        elif ch == ']':
            depth -= 1
        if ch == ',' and depth == 0:
            parts.append(cur)
            cur = ''
        else:
            cur += ch
    parts.append(cur)
    return [p.strip() for p in parts]


def mem_parts(op):
    """(base, index, disp) of a memory operand, or None."""
    m = MEMOP.search(op)
    if not m:
        return None
    base = index = None
    disp = 0
    for t in re.findall(r'[+-]?[^+-]+', m.group(2)):
        sign = -1 if t.startswith('-') else 1
        t = t.lstrip('+-')
        if '*' in t:
            index = reg64(t.split('*')[0])
        elif t.startswith('0x') or t.isdigit():
            disp += sign * int(t, 16 if t.startswith('0x') else 10)
        elif reg64(t):
            if base is None:
                base = reg64(t)
            else:
                index = reg64(t)
        elif t == 'rip':
            base = 'rip'
    return base, index, disp


def slot_name(op):
    p = mem_parts(op)
    if p and p[0] in ('rsp', 'rbp') and p[1] is None:
        return '%s%+#x' % (p[0], p[2])
    return None


class Code:
    def __init__(self, exe):
        self.exe = exe
        self.ins = exe.disassembly()
        self.addr = [x[0] for x in self.ins]
        self.funcs = exe.functions()
        self.starts = [f[0] for f in self.funcs]
        self.by_target = collections.defaultdict(list)
        self.calls = collections.defaultdict(list)
        for i, (a, op, args, tgt) in enumerate(self.ins):
            if tgt is not None:
                self.by_target[tgt].append(i)
            if op == 'call' and args.startswith('0x'):
                self.calls[int(args, 16)].append(i)
        start, size = find_table(exe, TABLES['spell'])
        self.spell_table = exe.address(start)
        self.spell_size = size

    def idx(self, a):
        return bisect.bisect_left(self.addr, a)

    def string_refs(self, s):
        """Instruction indices that load the address of string s."""
        b = b'\0' + s.encode() + b'\0'
        out, i = [], 0
        while True:
            i = self.exe.data.find(b, i)
            if i < 0:
                return out
            va = self.exe.address(i + 1)
            out += self.by_target.get(va, [])
            i += 1

    def chunk(self, a):
        k = bisect.bisect_right(self.starts, a) - 1
        return self.funcs[k] if k >= 0 and a < self.funcs[k][1] else None

    def entry(self, a):
        """The function a is in: back through contiguous unwind chunks to one that is called."""
        c = self.chunk(a)
        while c and not self.calls.get(c[0]):
            k = self.starts.index(c[0])
            if k == 0 or self.funcs[k - 1][1] != c[0]:
                break
            c = self.funcs[k - 1]
        return c[0]

    def extent(self, f):
        """(start, end) of function f: its chunk and the contiguous ones after it nobody calls."""
        k = self.starts.index(f)
        end = self.funcs[k][1]
        while k + 1 < len(self.funcs) and self.funcs[k + 1][0] == end and not self.calls.get(self.funcs[k + 1][0]):
            k += 1
            end = self.funcs[k][1]
        return f, end

    def anchored(self, s, what, many=False):
        refs = self.string_refs(s)
        fs = sorted({self.entry(self.ins[i][0]) for i in refs})
        if many and fs:
            return fs
        if len(fs) != 1:
            raise SystemExit('%s: %d functions load %r: %s' % (what, len(fs), s, [hex(f) for f in fs]))
        return fs[0]

    # -- data flow, backwards along the instruction stream ------------------------------------

    def origin(self, i, reg, lo, seen=None, slots=(), alternatives=None, path=None):
        """Where the value of register reg at instruction i came from, walking back to lo (or,
        with path, a list of instruction indices in execution order, back along it):
        ('mem', operand, index, position), ('slot', name, index), ('lea', target, index),
        ('const', v), ('call', target), ('entry', reg) or None. A store into a stack slot is
        followed back to the register stored, unless the slot is one of slots. With
        alternatives (a list), a constant assignment is taken as another path's value
        (recorded there) and the walk goes on: the instruction stream interleaves a branch's
        alternatives."""
        seen = seen or set()
        if path is None:
            seq = range(max(lo, i - LIMIT), i + 1)
            q = len(seq) - 1
        else:
            seq = path
            q = len(path) - 1 - path[::-1].index(i) if i in path else len(path)
        q -= 1
        stop = max(0, q - LIMIT)
        while q >= stop:
            j = seq[q]
            a, op, args, tgt = self.ins[j]
            if op == 'call':
                if reg in VOLATILE:
                    return ('call', args, j)
                q -= 1
                continue
            parts = split_args(args)
            d = parts[0] if parts else ''
            s = parts[1] if len(parts) > 1 else ''
            if op == 'cdqe' and reg == 'rax' or op in ('cmp', 'test', 'bt', 'push'):
                q -= 1
                continue
            if reg64(d) == reg and not d.startswith('['):
                if op in ('mov', 'movsxd', 'movsx', 'movzx'):
                    if reg64(s) and '[' not in s:
                        reg = reg64(s)
                    elif '[' in s:
                        sl = slot_name(s)
                        if sl and sl in slots:
                            return ('slot', sl, j)
                        if sl and (sl, j) not in seen:
                            seen.add((sl, j))
                            st = self._store_to(seq, q, sl)
                            if st is None:
                                return ('slot', sl, j)
                            q, reg = st
                            continue
                        return ('mem', s, j, (seq, q))
                    elif s.startswith('0x') or s.lstrip('-').isdigit():
                        const = ('const', int(s, 16) if s.startswith('0x') else int(s))
                        if alternatives is None:
                            return const
                        alternatives.append(const)
                    else:
                        return None
                elif op == 'lea':
                    return ('lea', tgt if tgt is not None else s, j)
                elif op == 'xor' and reg64(s) == reg:
                    if alternatives is None:
                        return ('const', 0)
                    alternatives.append(('const', 0))
                elif op in ('neg', 'not', 'add', 'sub', 'imul', 'and', 'or', 'shl', 'sar', 'shr', 'inc', 'dec') or op.startswith('cmov'):
                    pass            # derived from its own earlier value: keep walking
                else:
                    return None
            q -= 1
        return ('entry', reg) if path is None and seq and seq[0] == lo and q < 0 else None

    def damage_in(self, i, reg, lo, slots, path=None):
        """Can register reg at instruction i hold the spell's #damage (on some path)?"""
        return self.is_damage(self.origin(i, reg, lo, slots=slots, alternatives=[], path=path), lo, slots)

    def _store_to(self, seq, q, slot):
        """The latest store of a register into stack slot before position q: (position, register)."""
        k = q - 1
        while k >= max(0, q - LIMIT):
            a, op, args, tgt = self.ins[seq[k]]
            if op == 'mov':
                parts = split_args(args)
                if len(parts) == 2 and slot_name(parts[0]) == slot:
                    return (k, reg64(parts[1])) if reg64(parts[1]) else None
            k -= 1
        return None

    def is_damage(self, org, lo, slots):
        """Does this origin read the spell's #damage (the record's +0x38) or a damage slot?"""
        if not org:
            return False
        if org[0] == 'slot':
            return org[1] in slots
        if org[0] != 'mem':
            return False
        p = mem_parts(org[1])
        if not p:
            return False
        base, index, disp = p
        if disp == S_DAMAGE:
            regs = [r for r in (base, index) if r]
            seq, q = org[3] if len(org) > 3 else (None, None)
            path = list(seq[:q + 1]) if seq is not None else None
            return any((self.origin(org[2], r, lo, path=path) or (None, None))[1] == self.spell_table for r in regs)
        if disp == self.spell_table - BASE + S_DAMAGE:
            return True
        return False

    # -- dispatchers -------------------------------------------------------------------------

    def node_at(self, i, regs):
        """A test of the effect starting at instruction i: (values, body kind, target, next
        instruction, address, index after it), or None. Shapes: cmp r,v / je|jne; lea e,[r-v];
        cmp e,n / ja|jbe; either followed by movabs m,mask; bt m,r / jb (a set of values)."""
        ins = self.ins
        a, op, args, tgt = ins[i]
        p = split_args(args)
        base, r, k = 0, None, i
        if op == 'lea' and len(p) == 2 and re.fullmatch(r'\[(\w+)-(0x[0-9a-f]+)\]', p[1]):
            rr, n = re.fullmatch(r'\[(\w+)-(0x[0-9a-f]+)\]', p[1]).groups()
            if reg64(rr) not in regs or ins[i + 1][1] != 'cmp':
                return None
            base, r, k = int(n, 16), reg64(p[0]), i + 1
            p = split_args(ins[k][2])
            if reg64(p[0]) != r:
                return None
        elif op == 'cmp' and len(p) == 2 and reg64(p[0]) in regs and not p[0].startswith('['):
            r = reg64(p[0])
        else:
            return None
        if len(p) != 2 or not re.fullmatch(r'0x[0-9a-f]+', p[1]):
            return None
        v = int(p[1], 16)
        if v >= 0x80000000:
            v -= 1 << 32
        j = ins[k + 1]
        if j[1] in ('je', 'jne') and k == i:
            return ([v], j[1], int(j[2], 16), ins[k + 2][0], a, k + 2)
        if j[1] == 'ja' and ins[k + 2][1] == 'movabs' and ins[k + 3][1] == 'bt' and ins[k + 4][1] in ('jb', 'jae'):
            mask = int(split_args(ins[k + 2][2])[1], 16)
            vals = [base + b for b in range(min(v + 1, 64)) if mask >> b & 1]
            return (vals, 'je' if ins[k + 4][1] == 'jb' else 'jne', int(ins[k + 4][2], 16), ins[k + 5][0], a, k + 5)
        if j[1] in ('ja', 'jbe') and k == i + 1:
            return (list(range(base, base + v + 1)), j[1], int(j[2], 16), ins[k + 2][0], a, k + 2)
        return None

    def nodes(self, heads, regs, first_reach=8, minus=0):
        """The compare chains that start at the head instructions: each test's failing path leads
        to the next one (a few loads of the effect in between are skipped)."""
        out, seen = [], set()
        for h in heads:
            i, reach = h, first_reach
            while True:
                found = None
                for k in range(i, min(i + reach, len(self.ins) - 6)):
                    found = self.node_at(k, regs)
                    if found:
                        break
                if not found or found[4] in seen:
                    break
                seen.add(found[4])
                vals, kind, target, nxt, at, after = found
                out.append(([x + minus for x in vals], kind, target, nxt, at))
                i = self.idx(target) if kind in ('jne', 'ja') else after
                reach = 4
        return out

    @staticmethod
    def tails(ins):
        """Where a dispatcher's branches end: the jump targets many branches share."""
        n = collections.Counter(x[2] for x in ins if x[1] == 'jmp' and x[2].startswith('0x'))
        return {t for t, k in n.items() if k >= 5}

    def body(self, node, lo, hi, tails, tests=()):
        """A test's branch: [(start, end)] pieces of the instruction stream, from where the test
        leads (fall-through for jne/ja, the target for je/jbe) to the next test (jne/ja), a jump
        to a shared tail or a return. A jump elsewhere in the function continues it; a later
        conditional target (not a shared tail, not another test of the chain) is another piece."""
        vals, kind, target, nxt, at = node
        start = nxt if kind in ('jne', 'ja') else target
        first_stop = target if kind in ('jne', 'ja') and target > nxt else None
        pieces, budget, work, done = [], 1200, [(start, first_stop)], set()
        while work and budget > 0 and len(pieces) < 12:
            work.sort(key=lambda w: w[0])
            start, stop = work.pop(0)
            if start in done:
                continue
            i = j = self.idx(start)
            while j < len(self.ins) - 1 and j - i < budget and self.ins[j][0] < hi:
                a, op, args, tgt = self.ins[j]
                if stop is not None and a >= stop or a in done and j > i:
                    break
                done.add(a)
                if op.startswith('j') and op != 'jmp' and args.startswith('0x'):
                    t = int(args, 16)        # a later alternative of this branch
                    if a < t < hi and (stop is None or t < stop) and t not in done and '0x%x' % t not in tails and t not in tests:
                        work.append((t, stop))
                if op == 'ret' or (op == 'jmp' and args in tails):
                    j += 1
                    break
                if op == 'jmp' and args.startswith('0x'):
                    t = int(args, 16)
                    j += 1
                    if lo <= t < hi and t not in done and t not in tests:
                        work.append((t, None))
                    break
                j += 1
            pieces.append((self.ins[i][0], self.ins[j][0]))
            budget -= j - i
        return sorted(pieces)

    def uses(self, pieces, slots):
        """What a branch does with the #damage: [(what, address, detail)]. Values are followed
        back along the branch's pieces in execution order."""
        path = []
        for s, e in pieces:
            path += list(range(self.idx(s), self.idx(e)))
        out = []
        dmg = lambda i, r: self.damage_in(i, r, path[0], slots, path=path)

        def dmg_mem(i, operand):          # an operand that reads the damage itself
            sl = slot_name(operand)
            if sl:
                return sl in slots
            return self.is_damage(('mem', operand, i, (path, path.index(i))), path[0], slots)

        for i in path:
            a, op, args, tgt = self.ins[i]
            parts = split_args(args)
            regs = [reg64(p) for p in parts if reg64(p) and '[' not in p]
            if op == 'call' and args.startswith('0x'):
                t = int(args, 16)
                for n, r in enumerate(ARGS, 1):
                    if dmg(i, r):
                        r9 = self.origin(i, 'r9', path[0], path=path)
                        out.append(('call', a, (t, n, r9[1] if r9 and r9[0] == 'const' else None)))
            elif op in ('or', 'and', 'test') and len(parts) == 2 and '[' in parts[0] and reg64(parts[1]):
                if dmg(i, reg64(parts[1])):
                    neg = any(self.ins[k][1] == 'not' and reg64(split_args(self.ins[k][2])[0]) == reg64(parts[1]) for k in range(max(i - 4, 0), i))
                    out.append(('mask', a, op + (' not' if neg else '')))
            elif op == 'cmp' and len(parts) == 2 and regs and re.fullmatch(r'0x[0-9a-f]+', parts[1]):
                if dmg(i, regs[0]):
                    out.append(('compare', a, parts[1]))
            elif op in ('add', 'sub', 'imul', 'mul', 'idiv', 'cmp') and len(parts) >= 1:
                if any(dmg(i, r) for r in regs) or (len(parts) > 1 and '[' in parts[-1] and dmg_mem(i, parts[-1])) \
                        or (len(parts) == 1 and '[' in parts[0] and dmg_mem(i, parts[0])):
                    out.append(('number', a, op))
            elif op == 'mov' and len(parts) == 2 and '[' in parts[0] and not slot_name(parts[0]) and reg64(parts[1]):
                if dmg(i, reg64(parts[1])):
                    out.append(('number', a, 'stored'))
        return out


def classify(uses, sinks):
    """The argument kind from a branch's uses of the damage, and the evidence."""
    kinds = []
    for what, a, det in uses:
        if what == 'mask':
            kinds.append(('bitmask', '%x: %s into memory' % (a, det)))
        elif what == 'compare':
            kinds.append(('compared', '%x: compared with %s' % (a, det)))
        elif what == 'number':
            kinds.append(('number', '%x: %s' % (a, det)))
        elif what == 'call':
            t, n, r9 = det
            name = sinks.get(t)
            if name == 'resolve_summon' and n == 4 or name == 'newtempunit' and n == 1:
                kinds.append(('monster_or_tag', '%x: %s 0x%x' % (a, name, t)))
            elif isinstance(name, tuple) and name[1] == n:
                kinds.append(('monster_or_tag', '%x: 0x%x, which passes arg%d to resolve_summon' % (a, t, n)))
            elif name == 'unique_pick' and n == 3:
                kinds.append(('unique_or_monster_or_tag', '%x: unique_pick 0x%x' % (a, t)))
            elif name == 'newcom' and n == 1 or name == 'transform' and n == 2:
                kinds.append(('monster', '%x: %s 0x%x' % (a, name, t)))
            elif name == 'newench' and n == 1:
                kinds.append(('enchantment', '%x: newench 0x%x' % (a, t)))
            elif name == 'spell_event' and n == 2:
                kinds.append(('event', '%x: spell event queue 0x%x' % (a, t)))
            elif name == 'addfeatnr' and n == 2:
                kinds.append(('site', '%x: addfeatnr 0x%x' % (a, t)))
            elif name in ('addunitfx', 'setunitfx') and n == 2:
                kinds.append(('ability', '%x: %s 0x%x' % (a, name, t)))
            elif name in ('addunitfx', 'setunitfx') and n == 3:
                kinds.append(('count', '%x: %s value 0x%x' % (a, name, t)))
            elif name == 'age':
                kinds.append(('count', '%x: age 0x%x' % (a, t)))
            elif name == 'hitunit' and n == 3:
                kinds.append(('bitmask' if r9 == 11 else 'damage', '%x: hitunit(effect %s) 0x%x' % (a, r9, t)))
            elif name is None:
                kinds.append(('number', '%x: passed to 0x%x (arg%d)' % (a, t, n)))
    for k in REFERENCES:
        for kind, ev in kinds:
            if kind == k:
                return k, ev
    for k in ('ability', 'bitmask', 'count', 'damage', 'number', 'compared'):
        for kind, ev in kinds:
            if kind == k:
                return k, ev
    return None, None


class SpellEffects:
    def __init__(self, exe):
        self.exe = exe
        self.c = c = Code(exe)
        f = {}
        f['castlabspell'] = c.anchored('castlabspell: bad spellnr (%s, %d)', 'ritual effects')
        f['spellblastsquare'] = c.anchored('spellblastsquare cnr%d, spl%d, lvl%d\n', 'combat spell landing')
        f['blastsquare'] = c.anchored('blastsquare: bad unr', 'blastsquare')
        f['hitunit'] = c.anchored('hitunit: bad vic', 'hitunit')
        f['evalspell'] = c.anchored('evalspell: bad cnr', 'evalspell')
        f['resolve_summon'] = c.anchored('*** Bad summondmg %d\n', 'summon resolver')
        f['newtempunit'] = c.anchored('newtempunit: bad mnr', 'battle summon')
        f['newcom'] = c.anchored('newcom: mnr %d, lnr %d, unr %d\n', 'new commander')
        f['newench'] = c.anchored('newench %d by %s\n', 'new enchantment')
        f['addfeatnr'] = c.anchored('addfeatnr: bad featnr', 'add site')
        # (two functions print each of these: the unit's and its commander record's variants)
        fx = {'addunitfx': c.anchored('addunitfx: unr%d fx%d fxattr%d\n', 'add unit ability', many=True),
              'setunitfx': c.anchored('setunitfx: unr%d fx%d fxattr%d\n', 'set unit ability', many=True)}
        f['addunitfx'], f['setunitfx'] = fx['addunitfx'][0], fx['setunitfx'][0]
        f['age'] = c.anchored('Age %s %d years (max %d, rate %d, nice %d, death %d, peek %d)\n', 'age')
        consumer = c.anchored('  unknown spell event id %d\n', 'spell event lookup')
        f['spell_event'] = self._event_queue(consumer)
        f['hitunit_wrapper'] = self._wrapper(f['hitunit'])
        f['transform'] = self._transform(f['hitunit'], f['resolve_summon'])
        f['unique_pick'] = self._unique(f['castlabspell'], f['resolve_summon'])
        self.f = f
        self.sinks = {v: k for k, v in f.items()}
        for k, vs in fx.items():
            for v in vs:
                self.sinks[v] = k
        self.sinks[f['hitunit_wrapper']] = 'hitunit'
        self.sinks[f['hitunit']] = 'hitunit'
        # functions that hand one of their arguments to resolve_summon as the monster (new units,
        # newtempunit, the unique picker's negative keys): that argument is a monster or tag
        self.summon_arg = {}
        for i in c.calls[f['resolve_summon']]:
            fn = c.entry(c.ins[i][0])
            org = c.origin(i, 'r9', c.idx(fn))
            if org and org[0] == 'entry' and org[1] in ARGS:
                self.summon_arg[fn] = ARGS.index(org[1]) + 1
        for fn, n in self.summon_arg.items():
            if fn not in self.sinks:
                self.sinks[fn] = ('summon', n)

    def _event_queue(self, consumer):
        """The function that writes the queue the spell event lookup reads."""
        c = self.c
        lo, hi = c.extent(consumer)
        for i in range(c.idx(lo), c.idx(hi)):
            a, op, args, tgt = c.ins[i]
            p = mem_parts(args) if op == 'movsxd' else None
            if p and p[2] > 0x10000000:
                disp = p[2]
                for j, (b, op2, args2, t2) in enumerate(c.ins):
                    if op2 == 'mov' and args2.startswith('DWORD PTR [') and mem_parts(split_args(args2)[0]) and mem_parts(split_args(args2)[0])[2] == disp:
                        return c.entry(b)
        raise SystemExit('spell event queue not found')

    def _wrapper(self, hitunit):
        """The small function that only passes its arguments on to hitunit."""
        c = self.c
        for i in c.calls[hitunit]:
            f = c.entry(c.ins[i][0])
            lo, hi = c.extent(f)
            n = c.idx(hi) - c.idx(lo)
            calls = [x for x in c.ins[c.idx(lo):c.idx(hi)] if x[1] == 'call']
            if n < 30 and len(calls) == 1:
                return f
        raise SystemExit('hitunit wrapper not found')

    def _transform(self, hitunit, resolve):
        """What hitunit passes resolve_summon's monster to (in edx): the polymorph."""
        c = self.c
        lo, hi = c.extent(hitunit)
        found = collections.Counter()
        for i in c.calls[resolve]:
            if not lo <= c.ins[i][0] < hi:
                continue
            for k in range(i + 1, i + 12):
                a, op, args, tgt = c.ins[k]
                if op == 'call':
                    org = c.origin(k, 'rdx', c.idx(lo))
                    if org and org[0] == 'call' and org[1] == hex(resolve):
                        found[int(args, 16)] += 1
                    break
        if not found:
            raise SystemExit('transform not found')
        return found.most_common(1)[0][0]

    def _unique(self, castlab, resolve):
        """A callee that calls resolve_summon for a negative key and compares the rest with keys."""
        c = self.c
        lo, hi = c.extent(castlab)
        for i in range(c.idx(lo), c.idx(hi)):
            a, op, args, tgt = c.ins[i]
            if op != 'call' or not args.startswith('0x'):
                continue
            t = int(args, 16)
            if t == resolve:
                continue
            tl, th = c.extent(c.entry(t)) if c.chunk(t) else (t, t)
            body = c.ins[c.idx(tl):c.idx(th)]
            if any(x[1] == 'call' and x[2] == hex(resolve) for x in body[:12]) and \
                    sum(1 for x in body if x[1] == 'cmp' and x[2].startswith('r8d,0x')) >= 10:
                return t
        raise SystemExit('unique summon picker not found')

    # -- facts -------------------------------------------------------------------------------

    def summon_codes(self):
        """resolve_summon's special negative numbers and where monster tags start."""
        c = self.c
        lo, hi = c.extent(self.f['resolve_summon'])
        codes, tag = set(), None
        for i in range(c.idx(lo), c.idx(hi)):
            a, op, args, tgt = c.ins[i]
            p = split_args(args)
            if op == 'cmp' and len(p) == 2 and p[0] in ('esi', 'r9d') and p[1].startswith('0xffff'):
                v = int(p[1], 16) - (1 << 32)
                if c.ins[i + 1][1] == 'jg':
                    tag = v
                else:
                    codes.add(v)
        return sorted(codes, reverse=True), tag

    def unique_keys(self):
        c = self.c
        t = self.f['unique_pick']
        lo, hi = c.extent(c.entry(t))
        keys, passthrough = [], None
        for i in range(c.idx(lo), c.idx(hi)):
            a, op, args, tgt = c.ins[i]
            p = split_args(args)
            if op == 'cmp' and p[0] == 'r8d' and p[1].startswith('0x'):
                if c.ins[i + 1][1].startswith('cmovge'):
                    passthrough = int(p[1], 16)
                else:
                    keys.append(int(p[1], 16))
        return sorted(keys), passthrough

    def not_scaled(self):
        """Effects spellblastsquare doesn't scale by the caster's level (#damage 1000 and up is
        otherwise +1 per level): the tests on the effect right after it is loaded, which all
        jump past the scaling loop."""
        c = self.c
        lo, hi = c.extent(self.f['spellblastsquare'])
        ilo = c.idx(lo)
        for i in range(ilo, c.idx(hi)):
            a, op, args, tgt = c.ins[i]
            p = split_args(args)
            mp = mem_parts(p[1]) if op in ('movsx', 'movzx') and len(p) == 2 else None
            if mp and mp[2] == c.spell_table - BASE + S_EFFECT:
                nodes = c.nodes([i + 1], {reg64(p[0])}, first_reach=20)
                if nodes:
                    skip = collections.Counter(n[2] for n in nodes if n[1] in ('je', 'jbe')).most_common(1)[0][0]
                    return sorted(v for n in nodes if n[2] == skip and n[1] in ('je', 'jbe') for v in n[0])
        raise SystemExit('spellblastsquare: the effect tests not found')

    def ai_thousands(self):
        """Where the AI reads a combat effect 1000-9999 as effect % 1000."""
        c = self.c
        lo, hi = c.extent(self.f['evalspell'])
        ins = c.ins[c.idx(lo):c.idx(hi)]
        for k, (a, op, args, tgt) in enumerate(ins):
            if op == 'sub' and args.endswith(',0x3e8') and any(x[1] == 'cmp' and x[2].endswith(',0x2710') for x in ins[k - 3:k]):
                return a
        return None

    def thousands_elsewhere(self):
        """Does any battle function (castspell's landing, blastsquare, hitunit) reduce 1000-9999?"""
        c = self.c
        for name in ('spellblastsquare', 'blastsquare', 'hitunit'):
            lo, hi = c.extent(self.f[name])
            ins = c.ins[c.idx(lo):c.idx(hi)]
            for k, (a, op, args, tgt) in enumerate(ins):
                if op == 'sub' and args.endswith(',0x3e8') and any(x[1] == 'cmp' and x[2].endswith(',0x2710') for x in ins[k - 3:k]):
                    return name, a
        return None

    # -- the dispatchers ---------------------------------------------------------------------

    def ritual(self):
        c = self.c
        lo, hi = c.extent(self.f['castlabspell'])
        ilo = c.idx(lo)
        # the effect: a WORD load of the record's +0x2e, then "sub reg,10000"
        eff_reg, chain_start, scaled = None, None, set()
        for i in range(ilo, c.idx(hi)):
            a, op, args, tgt = c.ins[i]
            p = split_args(args)
            if op == 'movsx' and len(p) == 2 and 'WORD PTR' in p[1]:
                mp = mem_parts(p[1])
                if mp and mp[2] == S_EFFECT and any((c.origin(i, r, ilo) or (0, 0))[1] == c.spell_table for r in mp[:2] if r):
                    eff_reg = reg64(p[0])
            if eff_reg and op == 'sub' and reg64(p[0]) == eff_reg and p[1] == '0x2710':
                chain_start = a
                break
        if not chain_start:
            raise SystemExit('castlabspell: effect - 10000 not found')
        # the level-scaled damage: where the first damage load is stored before the chain
        for i in range(ilo, c.idx(chain_start)):
            a, op, args, tgt = c.ins[i]
            p = split_args(args)
            if op == 'mov' and len(p) == 2 and slot_name(p[0]) and reg64(p[1]):
                if c.is_damage(c.origin(i, reg64(p[1]), ilo), ilo, set()):
                    scaled.add(slot_name(p[0]))
                    break
        tails = c.tails(c.ins[ilo:c.idx(hi)])
        out = {}
        nodes = c.nodes([c.idx(chain_start) + 1], {eff_reg}, first_reach=600, minus=10000)
        tests = {n[4] for n in nodes}
        for node in nodes:
            vals = node[0]
            if any(v < 10000 for v in vals):
                continue
            kind, ev = classify(c.uses(c.body(node, chain_start, hi, tails, tests), scaled), self.sinks)
            for v in vals:
                out.setdefault(v, (kind, ev, node[4]))
        return out, sorted(scaled)

    def battle(self):
        """blastsquare's summoning chain and hitunit's effect chain."""
        c = self.c
        out = {}
        # blastsquare: the effect and damage are stack arguments; which ones, from spellblastsquare's call
        sb_lo, sb_hi = c.extent(self.f['spellblastsquare'])
        bs = self.f['blastsquare']
        eff_arg = dmg_arg = None
        for i in c.calls[bs]:
            if not sb_lo <= c.ins[i][0] < sb_hi:
                continue
            for k in range(i - 1, i - 40, -1):
                a, op, args, tgt = c.ins[k]
                p = split_args(args)
                sl = slot_name(p[0]) if op == 'mov' and len(p) == 2 else None
                if sl and sl.startswith('rsp+') and reg64(p[1]):
                    org = c.origin(k, reg64(p[1]), c.idx(sb_lo))
                    n = (int(sl[4:], 16) - 0x20) // 8 + 5
                    if org and org[0] == 'mem' and (mem_parts(org[1]) or (0, 0, 0))[2] == c.spell_table - BASE + S_EFFECT:
                        eff_arg = n
                    elif c.is_damage(org, c.idx(sb_lo), set()) or (org and org[0] == 'slot'):
                        dmg_arg = dmg_arg or n
            break
        bl, bh = c.extent(bs)
        frame = self._frame(bs)
        eff_slot = 'rsp%+#x' % (frame + 8 * (eff_arg - 5))
        dmg_slot = 'rsp%+#x' % (frame + 8 * (dmg_arg - 5))
        btails = c.tails(c.ins[c.idx(bl):c.idx(bh)])
        nodes = self._chains(bl, bh, {eff_slot}, set())
        tests = {n[4] for n in nodes}
        for node in nodes:
            kind, ev = classify(c.uses(c.body(node, bl, bh, btails, tests), {dmg_slot}), self.sinks)
            if kind:
                for v in node[0]:
                    out.setdefault(v, (kind, ev, node[4], 'blastsquare'))
        # hitunit: effect in r9d (kept in a frame slot), damage in r8 (kept in a slot, and its low half)
        hl, hh = c.extent(self.f['hitunit'])
        ihl = c.idx(hl)
        eff_slots, dmg_slots, eff_regs = set(), set(), {'r9'}
        for i in range(ihl, ihl + 80):
            a, op, args, tgt = c.ins[i]
            p = split_args(args)
            if op == 'mov' and len(p) == 2 and slot_name(p[0]):
                if p[1] in ('r9d', 'r9'):
                    eff_slots.add(slot_name(p[0]))
                if p[1] == 'r8':
                    dmg_slots.add(slot_name(p[0]))
            if op == 'mov' and len(p) == 2 and p[1] == 'r9d' and reg64(p[0]):
                eff_regs.add(reg64(p[0]))
        # the 32-bit copy of the damage: a local written from it (not an outgoing call argument
        # slot, which every call with more than four arguments rewrites)
        writes = collections.Counter(slot_name(split_args(x[2])[0]) for x in c.ins[ihl:c.idx(hh)]
                                     if x[1] == 'mov' and x[2].startswith(('DWORD PTR [rsp', 'QWORD PTR [rsp')))
        for i in range(ihl, c.idx(hh)):
            a, op, args, tgt = c.ins[i]
            p = split_args(args)
            if op == 'mov' and len(p) == 2 and slot_name(p[0]) and p[0].startswith('DWORD') and reg64(p[1]) \
                    and writes[slot_name(p[0])] <= 3:
                org = c.origin(i, reg64(p[1]), ihl)
                if org and org[0] == 'slot' and org[1] in dmg_slots:
                    dmg_slots.add(slot_name(p[0]))
        htails = c.tails(c.ins[ihl:c.idx(hh)])
        nodes = self._chains(hl, hh, eff_slots, eff_regs - {'r9'})
        tests = {n[4] for n in nodes}
        for node in nodes:
            kind, ev = classify(c.uses(c.body(node, hl, hh, htails, tests), dmg_slots), self.sinks)
            if len(node[0]) > 10 and kind in (None, 'number', 'damage', 'compared'):
                continue        # a range test of damage types (96-151, ...): not one effect each
            for v in node[0]:
                if 0 <= v < 1000 and kind and (v not in out or out[v][0] not in REFERENCES):
                    out[v] = (kind, ev, node[4], 'hitunit')
                elif 0 <= v < 1000 and v not in out:
                    out[v] = (None, None, node[4], 'hitunit')
        return out, {'effect_arg': eff_arg, 'damage_arg': dmg_arg, 'hitunit_damage_slots': sorted(dmg_slots)}

    def _chains(self, lo, hi, eff_slots, entry_regs):
        """The effect tests in a function: a chain starts after each load of the effect from
        its slot (and after the prologue's copy of the effect argument into entry_regs)."""
        c = self.c
        heads = []
        for i in range(c.idx(lo), c.idx(hi)):
            a, op, args, tgt = c.ins[i]
            p = split_args(args)
            if op in ('mov', 'movsxd', 'movsx') and len(p) == 2 and reg64(p[0]) and slot_name(p[1]) in eff_slots:
                heads.append((i + 1, {reg64(p[0])}))
            elif op == 'mov' and len(p) == 2 and reg64(p[0]) in entry_regs and p[1] == 'r9d' and i - c.idx(lo) < 80:
                heads.append((i + 1, {reg64(p[0])}))
        out, seen = [], set()
        for h, regs in heads:
            for node in c.nodes([h], regs, first_reach=40):
                if node[4] not in seen:
                    seen.add(node[4])
                    out.append(node)
        return out

    def _frame(self, f):
        """Stack offset of the first stack argument (arg5) in function f, from its prologue."""
        c = self.c
        pushes, sub, eax = 0, 0, 0
        for a, op, args, tgt in c.ins[c.idx(f):c.idx(f) + 20]:
            if op == 'push' or op == 'rex' and args.startswith('push'):
                pushes += 1
            if op == 'mov' and args.startswith('eax,0x'):
                eax = int(args.split(',')[1], 16)       # a big frame: mov eax,N; call __chkstk; sub rsp,rax
            if op == 'sub' and args.startswith('rsp,'):
                n = args.split(',')[1]
                sub = eax if n == 'rax' else int(n, 16)
                break
        return sub + 8 * pushes + 8 + 0x20


def span_text(values):
    """[1, 2, 3, 5] -> '1-3, 5' (negatives: '-28..-2')."""
    out, run = [], []
    for v in sorted(values):
        if run and v == run[-1] + 1:
            run.append(v)
            continue
        if run:
            out.append(run)
        run = [v]
    if run:
        out.append(run)
    sep = lambda r: '..' if r[0] < 0 else '-'
    return ', '.join(str(r[0]) if len(r) == 1 else '%d%s%d' % (r[0], sep(r), r[-1]) for r in out)


def note_for(effect):
    if effect in NOTES:
        return NOTES[effect]
    for k, v in NOTES.items():
        if isinstance(k, tuple) and k[0] <= effect <= k[1]:
            return v
    return None


def collect(exe):
    se = SpellEffects(exe)
    ritual, scaled = se.ritual()
    battle, bmeta = se.battle()
    codes, tag = se.summon_codes()
    keys, passthrough = se.unique_keys()
    not_scaled = se.not_scaled()
    ai = se.ai_thousands()
    elsewhere = se.thousands_elsewhere()
    effects, problems = {}, []

    def put(effect, auto, ev, where, context):
        note = note_for(effect)
        arg = auto
        if note:
            if auto in REFERENCES and note[0] != auto or note[0] in REFERENCES and note[0] != auto:
                problems.append('effect %d: the code says %s (%s), the note %s' % (effect, auto, ev, note[0]))
            if auto not in REFERENCES:
                arg = note[0]
        if arg in (None, 'number'):
            arg = 'damage' if context == 'battle' and effect not in not_scaled else ('unused' if auto is None else 'other')
        e = {'argument': arg, 'context': context, 'branch': hex(where) if where else None}
        if ev:
            e['evidence'] = ev
        if note and note[1]:
            e['note'] = note[1]
        if context == 'battle':
            e['level_scaled'] = effect not in not_scaled
        effects[effect] = e

    for v, (kind, ev, at) in sorted(ritual.items()):
        put(v, kind, ev, at, 'ritual')
    for v, (kind, ev, at, fn) in sorted(battle.items()):
        put(v, kind, ev, at, 'battle')
    for v in not_scaled:
        if v not in effects:
            put(v, None, None, None, 'battle')
    for k in NOTES:
        for v in (range(k[0], k[1] + 1) if isinstance(k, tuple) else [k]):
            if v not in effects:
                put(v, None, None, None, 'ritual' if v >= 10000 else 'battle')
    # compress ranges with the same reading (500-599, 600-699, 10500-10599)
    ranges = []
    for lo_v, hi_v in ((500, 599), (600, 699), (10500, 10599)):
        group = [effects.get(v) for v in range(lo_v, hi_v + 1)]
        if all(g and g['argument'] == group[0]['argument'] for g in group):
            r = dict(group[0])
            r.pop('branch', None)
            r['from'], r['to'] = lo_v, hi_v
            ranges.append(r)
            for v in range(lo_v, hi_v + 1):
                effects.pop(v, None)
    for p in problems:
        print('*** ' + p)
    f = se.f
    return {
        'game_version': exe.version, 'exe_sha256_16': exe.sha,
        'about': 'What #damage means per #effect (tools/dom6exe/spelleffects.py). argument: monster (a monster '
                 'number), monster_or_tag (a monster number; %d and below the monster tag -N; %s special '
                 'picks, -1 nothing), unique_or_monster_or_tag (1-%d a key into the game\'s lists of uniques, '
                 'keys %s used, the others summon nothing; %d and up a monster number; negative as '
                 'monster_or_tag), enchantment, event (an event\'s #id), site, ability (a monster ability '
                 'number), bitmask, count, damage (a number, level-scaled at 1000+ in battle), other (a code or '
                 'index), unused. Combat effects 1000-9999: the game has no case for them (passed on as written; '
                 'clouds are 144-150; only the AI reads effect %% 1000): their #damage is a plain number.'
                 % (tag, span_text([c for c in codes if c != -1]), passthrough - 1, span_text(keys), passthrough),
        'functions': {k: hex(v) for k, v in sorted(f.items())},
        'summon_special_codes': span_text(codes), 'monster_tag_at_or_below': tag,
        'unique_list_keys': span_text(keys), 'unique_monster_from': passthrough,
        'ritual_scaled_damage_slot': scaled,
        'battle_arguments': bmeta,
        'battle_not_level_scaled': span_text(not_scaled),
        'battle_thousands': {'ai_reads_effect_mod_1000_at': hex(ai) if ai else None,
                             'battle_code_reduces_them': list(elsewhere) if elsewhere else None},
        'effects': {str(k): v for k, v in sorted(effects.items())},
        'ranges': ranges,
    }
