"""Vanilla monsters as .dm commands, written straight from Dominions6.exe.

Each value in a vanilla monster record is written as the command the game's own .dm parser
stores it with: the parser read backwards. Some stored values no command can produce exactly;
those are written as the value the game uses, with the command a modder would write for it
(intrinsic resistances, item slots), or listed as read-only.

Facts used here that come from reading the 6.37 code (addresses are 6.37's):
- Generic handler (0x1402595f0): (line, name, ability, min, max, kind, extra). min == max: no
  argument, stores min + extra. kind % 10 == 5: optional argument, default extra, no offset.
  Otherwise the argument is clamped to [min, max] and extra is added. kind // 10 == 1 appends
  (the command can repeat), 2 ORs into the current value.
- Ability setter (0x140228350): with overwrite, an existing entry is replaced (value 0 removes
  it); otherwise a non-zero value is appended.
- Ability getter (0x1401c5ae0) adds intrinsic flags: fire/cold/shock/poison resistance +15
  (fire +10 more with the heat-aura flag or any heat aura, cold +10 with the cold-aura flag or
  any cold aura, poison +10 for undead or inanimate or with a poison cloud), heat and cold aura
  +3 with their flags. Spirit sight (607) is 1 for horrors.
- #poisonres/#fireres/#coldres/#shockres 100 set the intrinsic flag instead of a value.
- Item slots (0x1401034a2): ability 182, else 0xc0000 with #noitem's flag, else 0xf2206.
- Leadership (0x1402db422): class + abilities 157 (#command) + 158 + 159 + 160.
- Fixed names: game init copies them into the table #fixedname writes (36 bytes per monster).
"""
import collections
import re
import struct

from dom6exe import Parser, monsters, num, reg64

FLAGS, FLAGS2, BODY = 0x368, 0x370, 0x374
HUMANOID = 0x701
STATS = ['ap', 'mapmove', 'size', 'hp', 'prot', 'str', 'enc', 'prec', 'att', 'def', 'mr', 'mor']
COSTS = ['gcost', 'rcost', 'rpcost']

# ability getter (0x1401c5ae0): intrinsic flags per ability
FIRERES, COLDRES, SHOCKRES, POISONRES, HEAT, COLD = 198, 201, 199, 200, 316, 220
POISONCLOUD, HORROR, SPIRITSIGHT = 106, 391, 607
F_FIRERES, F_COLDRES, F_SHOCKRES, F_POISONRES = 0x200, 0x40, 0x40000, 0x400
F_HEAT, F_COLD, F_UNDEAD_OR_INANIMATE = 0x80, 0x10000, 0x40000020
RESIST_CMDS = [(FIRERES, 'fireres'), (COLDRES, 'coldres'), (SHOCKRES, 'shockres'), (POISONRES, 'poisonres'),
               (HEAT, 'heat'), (COLD, 'cold')]
INTRINSIC_BITS = F_FIRERES | F_COLDRES | F_SHOCKRES | F_POISONRES | F_HEAT | F_COLD
# other abilities written by a command of their own below
STEALTH, F_STEALTHY = 108, 0x8000000
ITEMSLOTS, F_NOITEM, SLOTS_DEFAULT, SLOTS_NOITEM = 182, 0x10000000, 0xf2206, 0xc0000
SAIL_SIZE, SAIL_SHIPS = 112, 410          # #sailing <max unit size> <ship size>
GEMPROD = range(30, 39)                   # #gemprod <gem> <n>: 30 + gem
MAGICBOOST = range(10, 23)                # #magicboost <path> <n>: 10 + path, 20-22 = 51-53
LEADER_BONUS = 160                        # adds to leadership; no command sets it
F2_LEADER = 0x1000000 | 0x2000000 | 0x4000000 | 0x8000000 | 0x10000000
F_TELEPORT, TELEPORT_MOVE = 0x4000000000, 100
F_STORMIMMUNE = 0x2000000000


def getter(m, key):
    """The value the game uses for an ability: the stored one plus intrinsic flags."""
    v, f = m['abilities'].get(key, 0), m['flags']
    if key == FIRERES and f & F_FIRERES:
        v += 15 + (10 if f & F_HEAT or getter(m, HEAT) > 0 else 0)
    elif key == COLDRES and f & F_COLDRES:
        v += 15 + (10 if f & F_COLD or getter(m, COLD) > 0 else 0)
    elif key == SHOCKRES and f & F_SHOCKRES:
        v += 15
    elif key == POISONRES and f & F_POISONRES:
        v += 15 + (10 if f & F_UNDEAD_OR_INANIMATE or getter(m, POISONCLOUD) > 0 else 0)
    elif key == HEAT and f & F_HEAT:
        v += 3
    elif key == COLD and f & F_COLD:
        v += 3
    elif key == SPIRITSIGHT and getter(m, HORROR) > 0:
        v = 1
    return v


def magic_table(exe, p):
    """Monster magic skills: the global table #clearmagic empties (12-byte entries: int16
    monster, int32 path or path mask, int16 level or chance; monster -1 ends it)."""
    # the branch is: lea rdx,"clearmagic"; mov rcx,line; call matcher; test; je; ...; call clearmagic
    clear = None
    for i, (f, s) in p.refs.items():
        if s == 'clearmagic' and f in p.contexts['monster']:
            calls = [num(args.split()[0]) for a, op, args, tgt in p.ins[i:i + 12] if op == 'call']
            if len(calls) >= 2:
                clear = calls[1]
                break
    # it walks the table from its first 'lea rsi,[rip+X]'
    start = next(tgt for a, op, args, tgt in p.ins[p.index[clear]:p.index[clear] + 30]
                 if op == 'lea' and args.startswith('rsi,') and tgt)
    o = exe.offset(start)
    out = collections.defaultdict(list)
    for k in range(30000):
        mon, x, y = struct.unpack_from('<h2xih2x', exe.data, o + 12 * k)
        if mon == -1:
            break
        out[mon].append((x, y))
    return out


def fixed_names(exe, p):
    """Vanilla fixed names: game init copies each into the table #fixedname writes."""
    base = copy = None
    for i, (f, s) in p.refs.items():
        if s == 'fixedname' and f in p.contexts['monster']:
            w = p.ins[i:i + 30]
            for n, x in enumerate(w):
                m = re.match(r'rcx,\[rcx\*4\+(0x[0-9a-f]+)\]$', x[2])
                if x[1] == 'lea' and m:
                    base = 0x140000000 + int(m.group(1), 16)
                    copy = next(y[2] for y in w[n:] if y[1] == 'call')
                    break
            break
    names = {}
    for k, (a, o, r, t) in enumerate(p.ins):
        if o == 'lea' and r.startswith('rcx,') and t and base <= t < base + 36 * 20000 and (t - base) % 36 == 0:
            if any(x[1] == 'call' and x[2] == copy for x in p.ins[k + 1:k + 3]):
                src = [x for x in p.ins[k - 3:k] if x[1] == 'lea' and x[2].startswith('rdx,') and x[3]]
                if src:
                    names[(t - base) // 36] = exe.cstr(src[-1][3])
    return names


class MonsterModel:
    """What each monster command stores, from the parser's code."""

    def __init__(self, exe):
        p = self.p = Parser(exe)
        p._setter = p.ability_setter()
        br = p.branches('monster')
        self.base = int(p.record_layout('monster')['record_base'], 16)
        # the scanner a command calls to read a number argument: called with a "%d"-style format
        fmt = collections.Counter()
        for k, (a, o, r, t) in enumerate(p.ins):
            if o == 'call':
                prev = [x for x in p.ins[k - 4:k] if x[1] == 'lea' and x[2].startswith('rdx,') and x[3]]
                if prev and (exe.cstr(prev[-1][3]) or '').startswith('%d'):
                    fmt[r.split()[0]] += 1
        scanner = fmt.most_common(1)[0][0]
        self.effects = {}
        for cmd, effs in br.items():
            e = {'bits': collections.Counter(), 'clears': collections.Counter(), 'stores': {}, 'abilities': [],
                 'numeric': any(k == 'call' and off == scanner for k, b, off, size, val in effs)}
            for kind, b, off, size, val in effs:
                if kind == 'ability':
                    e['abilities'].append((off, val))
                elif b == self.base and kind == 'set_bits' and val:
                    e['bits'][off] |= val
                elif b == self.base and kind == 'clear_bits' and val is not None:
                    e['clears'][off] |= ~val & ((1 << 8 * size) - 1)
                elif b == self.base and kind == 'store':
                    e['stores'][off] = val
            self.effects[cmd] = e
        # commands that only clear one flag bit (#almostliving): used to undo a bit another
        # command sets along with the one wanted (#demon also sets 'almost undead')
        self.clearer = {}
        for cmd, e in sorted(self.effects.items()):
            if not e['bits'] and not e['abilities'] and not e['stores']:
                for w, v in e['clears'].items():
                    if v and v & (v - 1) == 0:
                        self.clearer.setdefault((w, v), cmd)
        self.generic = collections.defaultdict(list)
        for c in p.generic_call_args():
            if c['context'] == 'monster' and c['key'] is not None:
                self.generic[c['key']].append(c)
        self.direct = collections.defaultdict(list)      # ability -> [(command, value spec)]
        for cmd, e in self.effects.items():
            for key, val in e['abilities']:
                self.direct[key].append((cmd, val))
        self.body_cmd = {}
        for cmd, e in sorted(self.effects.items()):
            if BODY in e['stores'] and e['stores'][BODY] != HUMANOID:
                self.body_cmd.setdefault(e['stores'][BODY], cmd)

    def ability_line(self, key, val):
        """The command that stores this value under this ability, or None."""
        for c in self.generic.get(key, []):
            kind = c['kind'] if c['kind'] is not None else 0
            extra = c['offset'] or 0
            if c['min'] is not None and c['min'] == c['max']:
                if val == c['min'] + (0 if kind == 5 else extra):
                    return '#' + c['command']
                continue
            arg = val if kind == 5 else val - extra
            if c['max'] is not None and arg > c['max'] or c['min'] is not None and arg < c['min']:
                continue
            return '#%s %d' % (c['command'], arg)
        for cmd, spec in self.direct.get(key, []):
            e = self.effects[cmd]
            if e['bits'] or e['stores']:
                continue        # it also sets flags or fields: handled with those
            if isinstance(spec, tuple):
                if e['numeric']:
                    return '#%s %d' % (cmd, val - spec[1])
            elif spec == val:
                return '#' + cmd
        return None

    def repeatable(self, key):
        """Whether a command appends this ability (a monster can have it more than once)."""
        return any(c['repeat'] == 1 for c in self.generic.get(key, []))

    def ability_label(self, key):
        """Name a read-only ability by the commands that store it with other values."""
        why = []
        for c in self.generic.get(key, []):
            if c['min'] is not None and c['min'] == c['max']:
                why.append('#%s stores %d' % (c['command'], c['min'] + (0 if c['kind'] == 5 else c['offset'] or 0)))
            else:
                why.append('#%s %s..%s' % (c['command'], c['min'], c['max']))
        for cmd, spec in self.direct.get(key, []):
            if isinstance(spec, tuple):
                why.append('#%s (not a number)' % cmd if not self.effects[cmd]['numeric'] else '#' + cmd)
            else:
                why.append('#%s stores %s' % (cmd, spec) if spec else 'cleared by #' + cmd)
        return 'ability %d' % key + (' (%s)' % ', '.join(why) if why else '')

    def flag_line(self, m, word, bit, covered):
        """The command for a flag bit, and the commands that undo bits it also sets that the
        monster doesn't have. Of the commands that set the bit, one whose other effects the
        monster also has (#teleport also sets map move 100, #blink only the flag); else one
        whose extra bits a clearing command undoes (#demon + #almostliving)."""
        best = None
        for cmd, e in sorted(self.effects.items()):
            if not e['bits'][word] & bit or e['abilities']:
                continue
            if not all(m['raw_fields'].get(off) == v for off, v in e['stores'].items()):
                continue
            extra = [(w, 1 << b) for w, v in e['bits'].items() for b in range(64)
                     if (v & ~m['raw_bits'].get(w, 0)) >> b & 1]
            if all(x in self.clearer for x in extra):
                score = (len(extra), -len(e['stores']))
                if best is None or score < best[0]:
                    best = (score, cmd, [self.clearer[x] for x in extra])
        if best is None:
            return None, []
        for w, v in self.effects[best[1]]['bits'].items():
            covered[w] |= v & m['raw_bits'].get(w, 0)
        return best[1], best[2]


def monster_commands(exe):
    model = MonsterModel(exe)
    mons = monsters(exe)
    magic = magic_table(exe, model.p)
    fixed = fixed_names(exe, model.p)
    out = {}
    for i, m in sorted(mons.items()):
        m['raw_bits'] = {FLAGS: m['flags'], FLAGS2: m['flags2']}
        m['raw_fields'] = {0x2a: m['mapmove'], BODY: m['body']}
        lines, ro = ['#name "%s"' % m['name'].replace('"', "'")], []
        if i in fixed:
            lines.append('#fixedname "%s"' % fixed[i])
        if m['body'] != HUMANOID:
            if m['body'] in model.body_cmd:
                lines.append('#' + model.body_cmd[m['body']])
            else:
                ro.append(('body shape', hex(m['body'])))
        # flags (intrinsic resistances, stealth, storm immunity and no-items are written below)
        covered = collections.Counter()
        special = {FLAGS: INTRINSIC_BITS | F_STEALTHY | F_STORMIMMUNE | F_NOITEM, FLAGS2: 0}
        if m['flags'] & F_NOITEM:
            lines.append('#noitem')
        undo = []
        for word, field in ((FLAGS, 'flags'), (FLAGS2, 'flags2')):
            for bit in range(64):
                b = 1 << bit
                if m[field] & b and not special[word] & b and not covered[word] & b:
                    cmd, clear = model.flag_line(m, word, b, covered)
                    if cmd:
                        lines.append('#' + cmd)
                        undo += ['#' + c for c in clear if '#' + c not in undo]
                    else:
                        ro.append(('%s bit' % field, hex(b)))
        lines += undo       # after every command that sets those bits
        if not m['flags2'] & F2_LEADER:
            lines.append('#okleader')
        for c in STATS + COSTS:
            lines.append('#%s %d' % (c, m[c]))
        # abilities (the game reads the first of a repeated one unless its command appends)
        seen = set()
        for key, val in m['ability_list']:
            if key in seen and not model.repeatable(key):
                ro.append(('ability %d repeated (the game reads the first)' % key, val))
                continue
            seen.add(key)
            if key in (FIRERES, COLDRES, SHOCKRES, POISONRES, HEAT, COLD, STEALTH, ITEMSLOTS, SAIL_SIZE, SAIL_SHIPS):
                continue
            if key in MAGICBOOST:
                lines.append('#magicboost %d %d' % (key - 10 if key < 20 else 51 + key - 20, val))
            elif key in GEMPROD:
                lines.append('#gemprod %d %d' % (key - 30, val))
            elif key == LEADER_BONUS:
                ro.append(('leadership bonus (ability %d)' % key, val))
            else:
                line = model.ability_line(key, val)
                if line:
                    lines.append(line)
                else:
                    ro.append((model.ability_label(key), val))
        # resistances: the value the game uses (a #fireres etc. clears the intrinsic flag, so
        # editing it gives the value written). The aura flags add 3 to #heat/#cold, which don't
        # clear them: read-only.
        for key, cmd in RESIST_CMDS:
            if key in (HEAT, COLD):
                v = m['abilities'].get(key, 0)
                if m['flags'] & (F_HEAT if key == HEAT else F_COLD):
                    ro.append(('%s aura from a flag (adds to #%s)' % (cmd, cmd), 3))
            else:
                v = getter(m, key)
            if v == 100 and key not in (HEAT, COLD):
                ro.append((cmd, v))     # '#%s 100' would set the intrinsic flag instead
            elif v:
                lines.append('#%s %d' % (cmd, v))
        if m['flags'] & F_STEALTHY:
            lines.append('#stealthy %d' % m['abilities'].get(STEALTH, 0))
        elif STEALTH in m['abilities']:
            ro.append(('ability %d (stealth without the flag)' % STEALTH, m['abilities'][STEALTH]))
        if m['flags'] & F_STORMIMMUNE and 'stormimmune' not in ' '.join(lines):
            lines.append('#stormimmune')
        # #sailing a b: 112 = a (999 when below 1), 410 = b (10 when not 1-10)
        if SAIL_SIZE in m['abilities'] or SAIL_SHIPS in m['abilities']:
            a, b = m['abilities'].get(SAIL_SIZE, 999), m['abilities'].get(SAIL_SHIPS, 0)
            if 1 <= b <= 10 and a >= 1:
                lines.append('#sailing %d %d' % (a, b))
            else:
                ro.append(('sailing (abilities 112, 410)', '%d %d' % (a, b)))
        slots = m['abilities'].get(ITEMSLOTS) or (SLOTS_NOITEM if m['flags'] & F_NOITEM else SLOTS_DEFAULT)
        lines.append('#itemslots %d' % slots)
        for x, y in magic.get(i, []):
            lines.append(('#magicskill %d %d' if x <= 9 or 50 <= x <= 52 else '#custommagic %d %d') % (x, y))
        lines += ['#weapon %d' % w for w in m['weapons']]
        lines += ['#armor %d' % a for a in m['armor']]
        out[i] = {'lines': lines, 'readonly': ro}
    return out


def write(exe, path):
    cmds = monster_commands(exe)
    text = ['-- Dominions %s vanilla monsters, written from Dominions6.exe by tools/dom6exe (exe %s).' % (exe.version, exe.sha),
            '-- "-- ro:" lines are stored values no monster command can set (shown read-only).', '']
    for i, e in cmds.items():
        text.append('#selectmonster %d' % i)
        text.extend(e['lines'])
        text.extend('-- ro: %s = %s' % (what, val) for what, val in e['readonly'])
        text.append('#end')
        text.append('')
    if path:
        open(path, 'w', encoding='utf-8').write('\n'.join(text))
    ro = collections.Counter(w for e in cmds.values() for w, v in e['readonly'])
    return {'game_version': exe.version, 'monsters': len(cmds), 'readonly_values': sum(ro.values()),
            'readonly_kinds': dict(ro.most_common())}
