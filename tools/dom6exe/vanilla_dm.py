"""Vanilla monsters (and weapons, armor, spells, items, sites and nations) as .dm commands,
written straight from Dominions6.exe. Blesses, poptypes, nametypes and mercenaries are in
vanilla_other.py; write() puts them all in one file.

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

from dom6exe import TABLES, Parser, monsters, num, reg64, table_records, text

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


def number_scanner(exe, p):
    """The function commands call to read a number argument: called with a "%d"-style format."""
    fmt = collections.Counter()
    for k, (a, o, r, t) in enumerate(p.ins):
        if o == 'call':
            prev = [x for x in p.ins[k - 4:k] if x[1] == 'lea' and x[2].startswith('rdx,') and x[3]]
            if prev and (exe.cstr(prev[-1][3]) or '').startswith('%d'):
                fmt[r.split()[0]] += 1
    return fmt.most_common(1)[0][0]


def generic_lines(calls, val):
    """The generic-handler commands that store this value, and what's left over: one command,
    or for an ability commands OR into (kind 2x: item restrictions, each a fixed bit) one per
    bit, leaving bits no command sets."""
    ors = [c for c in calls if c['repeat'] == 2 and c['max'] is not None]
    if ors and len(ors) == len(calls):
        lines, left = [], val
        # (min is often a register the analysis doesn't follow; these take no argument)
        for c in sorted(ors, key=lambda c: -c['max']):
            if c['max'] and left & c['max'] == c['max']:
                lines.append('#' + c['command'])
                left &= ~c['max']
        return lines, left
    line = generic_line(calls, val)
    return ([line], 0) if line else ([], val)


def generic_line(calls, val):
    """The generic-handler command (of those storing one ability) that stores this value."""
    for c in calls:
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
    return None


def generic_label(calls, key):
    why = []
    for c in calls:
        if c['min'] is not None and c['min'] == c['max']:
            why.append('#%s stores %d' % (c['command'], c['min'] + (0 if c['kind'] == 5 else c['offset'] or 0)))
        else:
            why.append('#%s %s..%s' % (c['command'], c['min'], c['max']))
    return 'ability %d' % key + (' (%s)' % ', '.join(why) if why else '')


class MonsterModel:
    """What each monster command stores, from the parser's code."""

    def __init__(self, exe):
        p = self.p = Parser(exe)
        p._setter = p.ability_setter()
        br = p.branches('monster')
        self.base = int(p.record_layout('monster')['record_base'], 16)
        scanner = number_scanner(exe, p)
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
            if isinstance(e['stores'].get(BODY), int) and e['stores'][BODY] != HUMANOID:
                self.body_cmd.setdefault(e['stores'][BODY], cmd)

    def ability_line(self, key, val):
        """The command that stores this value under this ability, or None."""
        line = generic_line(self.generic.get(key, []), val)
        if line:
            return line
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
    if mons[max(mons)]['name'] == 'end':     # the table's end marker
        del mons[max(mons)]
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
    import vanilla_other
    sections = [('weapon', weapon_commands), ('armor', armor_commands), ('monster', monster_commands),
                ('spell', spell_commands), ('item', item_commands), ('site', site_commands),
                ('nation', nation_commands)]
    text = ['-- Dominions %s vanilla data, written from Dominions6.exe by tools/dom6exe (exe %s).' % (exe.version, exe.sha),
            '-- "-- ro:" lines are stored values no command can set (shown read-only).', '']
    res = {'game_version': exe.version}

    def block(header, e):
        text.append(header)
        text.extend(e['lines'])
        text.extend('-- ro: %s = %s' % (what, val) for what, val in e['readonly'])
        text.append('#end')
        text.append('')

    def summary(kind, entries):
        ro = collections.Counter(w for e in entries for w, v in e['readonly'])
        res[kind] = {'count': len(entries), 'readonly_values': sum(ro.values()), 'readonly_kinds': dict(ro.most_common())}

    for kind, fn in sections:
        cmds = fn(exe)
        # record 0 is a placeholder ("no one", "Nothing") except for nations (0 = Independents)
        if kind != 'nation':
            cmds.pop(0, None)
        for i, e in cmds.items():
            block('#select%s %d' % (kind, i), e)
        summary(kind, list(cmds.values()))
    # blesses, poptypes, nametypes and mercenaries (vanilla_other.py); bless effects are monster
    # abilities, named by the monster commands
    model = MonsterModel(exe)
    p = model.p
    p._setter = None
    nations = {i: name_of(r) for i, r in records(exe, 'nation').items()}
    for kind, (cmds, t) in (('bless', vanilla_other.bless_commands(exe, p, model)),
                            ('poptype', vanilla_other.poptype_commands(exe, p)),
                            ('nametype', vanilla_other.nametype_commands(exe, p))):
        for i, e in cmds.items():
            block('#select%s %d' % (kind, i), e)
        summary(kind, list(cmds.values()))
    bands, t = vanilla_other.merc_commands(exe, p, nations, set(monsters(exe)))
    text.extend(["-- Mercenaries: the game's own bands, in its order. A mod can't select or change them (there",
                 '-- is no #selectmerc): #clearmercs removes them all, and #newmerc adds a band after them. Each',
                 '-- is written as the #newmerc block that would make it.', ''])
    for e in bands:
        block('#newmerc', e)
    summary('merc', bands)
    if path:
        open(path, 'w', encoding='utf-8').write('\n'.join(text))
    return res


# ---------------------------------------------------------------------------------------------
# Weapons and armor: commands store their argument in a record field, set or clear flag bits,
# or add an ability through the type's own setter.

def mask(size):
    return (1 << 8 * size) - 1


def bits_of(v):
    return [1 << b for b in range(64) if v >> b & 1]


class ContextModel:
    """What each command of one entity parser does to the record: fields stored (a constant or
    the parsed argument), flag bits set or cleared, abilities added through the type's setter."""

    def __init__(self, exe, ctx, setter=None):
        self.p = p = Parser(exe)
        p._setter = setter
        br = p.branches(ctx)
        self.base = collections.Counter(e[1] for effs in br.values() for e in effs if e[1] is not None).most_common(1)[0][0]
        scanner = number_scanner(exe, p)
        self.effects = {}
        for cmd, effs in br.items():
            e = {'bits': collections.Counter(), 'clears': collections.Counter(), 'stores': {}, 'abilities': [],
                 'numeric': any(k == 'call' and off == scanner for k, b, off, size, val in effs)}
            for kind, b, off, size, val in effs:
                if kind == 'ability':
                    if (off, val) not in e['abilities']:
                        e['abilities'].append((off, val))
                elif b != self.base:
                    continue
                elif kind == 'set_bits' and val:
                    e['bits'][off] |= val & mask(size)
                elif kind == 'clear_bits' and val is not None:
                    e['clears'][off] |= ~val & mask(size)
                elif kind == 'store':
                    e['stores'].setdefault((off, size), val)
            self.effects[cmd] = e
        self.by_ability = collections.defaultdict(list)
        for cmd, e in sorted(self.effects.items()):
            for key, spec in e['abilities']:
                self.by_ability[key].append((cmd, spec))

    def check(self, cmd, off, size, spec=('arg', 0)):
        """Fail loudly if a command no longer stores what this module assumes."""
        got = self.effects.get(cmd, {}).get('stores', {}).get((off, size))
        if got != spec:
            raise SystemExit('#%s: expected it to store %s at 0x%x/%d, the parser stores %s' % (cmd, spec, off, size, got))

    def flag_lines(self, off, value, default, abilities):
        """Commands that turn the default flag word into this value: first the ones clearing
        default bits the record lacks, then the ones setting its other bits. A command may also
        add abilities if the record has them (#ironweapon: a bit and ability 266); those are
        returned as covered. Bits no command produces come back as unmatched."""
        lines, covered, unmatched = [], set(), 0

        def abilities_ok(e):
            return all(key in abilities and (isinstance(spec, tuple) or abilities[key] == spec) for key, spec in e['abilities'])

        done = 0
        for b in bits_of(default & ~value):
            if any(self.effects[l[1:]]['clears'][off] & b for l in lines):
                continue
            cands = [(bin(e['clears'][off]).count('1'), c) for c, e in self.effects.items()
                     if e['clears'][off] & b and not e['bits'][off] & ~value and not e['stores'] and not e['abilities']]
            if cands:
                lines.append('#' + min(cands)[1])
                done |= self.effects[min(cands)[1]]['bits'][off]
            else:
                unmatched |= b
        need = value & ~default & ~done
        while need:
            b = need & -need
            cands = []
            for c, e in self.effects.items():
                sets = e['bits'][off]
                if sets & b and not sets & ~value and not e['clears'][off] & value & ~sets and not e['stores'] and abilities_ok(e):
                    cands.append((-bin(sets & need).count('1'), len(e['abilities']), c))
            if not cands:
                unmatched |= b
                need &= ~b
                continue
            c = min(cands)[2]
            lines.append('#' + c)
            need &= ~self.effects[c]['bits'][off]
            covered |= {key for key, spec in self.effects[c]['abilities']}
        return lines, covered, unmatched

    def ability_lines(self, abilities, covered):
        lines, ro = [], []
        for key, val in abilities.items():
            if key in covered:
                continue
            line = None
            for cmd, spec in self.by_ability.get(key, []):
                e = self.effects[cmd]
                if e['bits'] or e['stores'] or {k for k, sp in e['abilities']} != {key}:
                    continue
                if isinstance(spec, tuple) and e['numeric']:
                    line = '#%s %d' % (cmd, val - spec[1])
                elif isinstance(spec, tuple) and val == 1:
                    line = '#' + cmd        # no argument; the value 1 comes from a register
                elif spec == val:
                    line = '#' + cmd
                if line:
                    break
            if line:
                lines.append(line)
            else:
                ro.append(('ability %d' % key, val))
        return lines, ro


def setter_of(p, ctx, key_cmd):
    """The ability setter of an entity type: the function a command's branch (or the shared
    tail it jumps to) calls with the ability number in edx."""
    for i, (f, s) in sorted(p.refs.items()):
        if s == key_cmd and f in p.contexts[ctx]:
            for k in range(i, i + 150):
                a, op, args, tgt = p.ins[k]
                if op == 'mov' and re.match(r'edx,0x[0-9a-f]+$', args):
                    for a2, op2, args2, t2 in p.ins[k + 1:k + 9]:
                        if op2 == 'call':
                            return args2.split()[0]
                        if op2 == 'jmp':
                            t = p.index[num(args2.split()[0])]
                            return next(x[2].split()[0] for x in p.ins[t:t + 6] if x[1] == 'call')
    return None


def records(exe, kind):
    start, size, recs = table_records(exe, TABLES[kind])
    out = {i: r for i, r in recs.items() if r[:1] != b'\0'}
    # the table's end marker: a record named "end" after the last vanilla one
    if out and name_of(out[max(out)]) == 'end':
        del out[max(out)]
    return out


def name_of(r):
    return text(r[:36].split(b'\0')[0])


# weapon record (6.37): defaults from #clear (0x140227f90)
W_FIELDS = [('dmg', 0x28, 8, 2), ('att', 0x30, 2, 0), ('def', 0x32, 2, 0), ('len', 0x36, 2, 1),
            ('nratt', 0x3a, 2, 1), ('ammo', 0x3c, 2, 0), ('aoe', 0x5a, 2, 0), ('sound', 0x5c, 2, 7),
            ('rcost', 0x5e, 2, 0)]
W_TYPE, W_RANGE, W_FLAGS, W_SECONDARY = 0x34, 0x38, 0x40, 0x50
W_FLYSPR, W_EXPLSPR, W_ABILITIES = 0x52, 0x56, 0x60
W_DEFAULT_FLAGS, W_DEFAULT_TYPE = 0x200001, 2


def weapon_commands(exe):
    p = Parser(exe)
    model = ContextModel(exe, 'weapon', setter_of(p, 'weapon', 'fireifhit'))
    for cmd, off, size, default in W_FIELDS:
        model.check(cmd, off, size)
    model.check('range', W_RANGE, 2)
    model.check('secondaryeffect', W_SECONDARY, 2) if False else None
    dt = {e['stores'][(W_TYPE, 2)]: c for c, e in sorted(model.effects.items()) if isinstance(e['stores'].get((W_TYPE, 2)), int)}
    out = {}
    for i, r in sorted(records(exe, 'weapon').items()):
        u16 = lambda o: struct.unpack_from('<h', r, o)[0]
        lines, ro = ['#name "%s"' % name_of(r).replace('"', "'")], []
        rng = u16(W_RANGE)
        if rng:
            lines.append('#range %d' % rng)      # also sets len 0 and ammo 12 if unset: written below
        for cmd, off, size, default in W_FIELDS:
            v = struct.unpack_from('<q' if size == 8 else '<h', r, off)[0]
            if v != default or cmd in ('dmg', 'att', 'def', 'len', 'nratt', 'rcost') or (rng and cmd in ('len', 'ammo')):
                lines.append('#%s %d' % (cmd, v))
        t = u16(W_TYPE)
        if t != W_DEFAULT_TYPE:
            lines.append('#' + dt[t]) if t in dt else ro.append(('damage type', t))
        ab = {}
        for k in range(7):
            key, val = struct.unpack_from('<ii', r, W_ABILITIES + 8 * k)
            if not key:
                break
            ab.setdefault(key, val)
        flags = struct.unpack_from('<Q', r, W_FLAGS)[0]
        fl, covered, unmatched = model.flag_lines(W_FLAGS, flags, W_DEFAULT_FLAGS, ab)
        lines += fl
        ro += [('flag bit', hex(b)) for b in bits_of(unmatched)]
        sec = u16(W_SECONDARY)
        if sec:
            lines.append('#secondaryeffect %d' % sec if sec > 0 else '#secondaryeffectalways %d' % -sec)
        if u16(W_FLYSPR) != -1 or u16(W_FLYSPR + 2):
            lines.append('#flyspr %d %d' % (u16(W_FLYSPR), u16(W_FLYSPR + 2)))
        if u16(W_EXPLSPR) != -1:
            lines.append('#explspr %d' % u16(W_EXPLSPR))
            if u16(W_EXPLSPR + 2) != 9:      # #explspr always stores 9 frames
                ro.append(('explosion sprite frames', u16(W_EXPLSPR + 2)))
        al, aro = model.ability_lines(ab, covered)
        lines += al
        ro += aro
        out[i] = {'lines': lines, 'readonly': ro}
    return out


# armor record (6.37): protection by body part (part, value) from 0x24, 4 ability numbers at
# 0x48 with their values at 0x58. #prot sets the part(s) its type covers (shield 5, helmet 1,
# else torso 2, arms 3, legs 4 to the same value); #protparts h b sets head 1 and parts 2-4.
# Body armor is written with its torso value: the game uses body and head protection only.
# Part 6 (a few necklaces, bracers, a barding) has no command and stays read-only.
A_PARTS, A_DEF, A_ENC, A_TYPE, A_RCOST, A_AB_KEYS, A_AB_VALS = 0x24, 0x3e, 0x40, 0x42, 0x44, 0x48, 0x58


def armor_commands(exe):
    p = Parser(exe)
    model = ContextModel(exe, 'armor', setter_of(p, 'armor', 'ironarmor'))
    for cmd, off in (('def', A_DEF), ('enc', A_ENC), ('type', A_TYPE), ('rcost', A_RCOST)):
        model.check(cmd, off, 2)
    out = {}
    for i, r in sorted(records(exe, 'armor').items()):
        h = lambda o: struct.unpack_from('<h', r, o)[0]
        lines, ro = ['#name "%s"' % name_of(r).replace('"', "'")], []
        typ = h(A_TYPE)
        lines += ['#type %d' % typ, '#def %d' % h(A_DEF), '#rcost %d' % h(A_RCOST), '#enc %d' % h(A_ENC)]
        parts = []
        for k in range(4):
            part, val = h(A_PARTS + 4 * k), h(A_PARTS + 4 * k + 2)
            if not part:
                break
            parts.append((part, val))
        pd = dict(parts)
        kinds = [x for x, v in parts]
        if not parts:
            pass
        elif kinds == [5] and typ == 4 or kinds == [1] and typ == 6:
            lines.append('#prot %d' % parts[0][1])
        elif 2 in pd and set(kinds) <= {1, 2, 3, 4}:
            # the game uses body and head protection only (the user, 2026-10-05): arms and legs
            # (parts 3, 4), which vanilla often sets lower than the torso, are left out
            lines.append('#protparts %d %d' % (pd[1], pd[2]) if 1 in pd else '#prot %d' % pd[2])
        else:
            ro.append(('protection by part', ' '.join('%d:%d' % x for x in parts)))
        ab = {}
        for k in range(4):
            key, val = struct.unpack_from('<i', r, A_AB_KEYS + 4 * k)[0], struct.unpack_from('<i', r, A_AB_VALS + 4 * k)[0]
            if not key:
                break
            ab.setdefault(key, val)
        al, aro = model.ability_lines(ab, set())
        lines += al
        ro += aro
        out[i] = {'lines': lines, 'readonly': ro}
    return out


# item record (6.37): defaults from #clear (0x1402290d0): main path 0 level 1, no second path,
# type 8. #constlevel N stores N / 2 (the manual's levels are 1, 3, 5, ...), #mainpath also
# raises the main level to 1, #restricted N appends ability 278 = N. Abilities share the
# monster numbering and the generic handler.
I_CONST, I_MAINPATH, I_SECPATH, I_MAINLEVEL, I_SECLEVEL, I_TYPE, I_SPR = 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a
I_WEAPON, I_ARMOR, I_SPELL, I_AUTOSPELL, I_ABILITIES = 0x2c, 0x2e, 0x30, 0x54, 0x78
I_FLAGS = (0x1f8, 0x200, 0x208)
RESTRICTED, TYPE9 = 278, 1423


def item_commands(exe):
    model = ContextModel(exe, 'item')
    gen = collections.defaultdict(list)
    for c in model.p.generic_call_args():
        if c['context'] == 'item' and c['key'] is not None:
            gen[c['key']].append(c)
    repeatable = {k for k, cs in gen.items() if any(c['repeat'] == 1 for c in cs)} | {RESTRICTED}
    out = {}
    for i, r in sorted(records(exe, 'item').items()):
        b = lambda o: r[o]
        lines, ro = ['#name "%s"' % name_of(r).replace('"', "'")], []
        lines.append('#constlevel %d' % (2 * b(I_CONST) + 1))
        lines.append('#mainpath %d' % b(I_MAINPATH))
        lines.append('#mainlevel %d' % b(I_MAINLEVEL))
        if b(I_SECPATH) != 255:
            lines += ['#secondarypath %d' % b(I_SECPATH), '#secondarylevel %d' % b(I_SECLEVEL)]
        ab0 = {}
        for k in range(24):
            key, val = struct.unpack_from('<qq', r, I_ABILITIES + 16 * k)
            if not key:
                break
            ab0.setdefault(key, val)
        typ, skip = b(I_TYPE), set()
        if typ == 6 and ab0.get(TYPE9) == 1:
            lines.append('#type 9')         # #type 9 stores type 6 and ability 1423 = 1
            skip.add(TYPE9)
        else:
            lines.append('#type %d' % (10 if typ == 9 else typ))     # #type 10 stores 9
        w, a = struct.unpack_from('<hh', r, I_WEAPON)
        if w:
            lines.append('#weapon %d' % w)
        if a:
            lines.append('#armor %d' % a)
        for cmd, off in (('spell', I_SPELL), ('autospell', I_AUTOSPELL)):
            sp = text(r[off:off + 36].split(b'\0')[0])
            if sp:
                lines.append('#%s "%s"' % (cmd, sp))
        spr = struct.unpack_from('<h', r, I_SPR)[0]
        if spr:
            ro.append(('sprite', spr))
        for off in I_FLAGS:
            fl, covered, unmatched = model.flag_lines(off, struct.unpack_from('<Q', r, off)[0], 0, {})
            lines += fl
            ro += [('flag bit 0x%x' % off, hex(x)) for x in bits_of(unmatched)]
        seen = set()
        for k in range(24):
            key, val = struct.unpack_from('<qq', r, I_ABILITIES + 16 * k)
            if not key:
                break
            if key in seen and key not in repeatable:
                ro.append(('ability %d repeated (the game reads the first)' % key, val))
                continue
            seen.add(key)
            if key in skip:
                continue
            if key in MAGICBOOST:
                lines.append('#magicboost %d %d' % (key - 10 if key < 20 else 51 + key - 20, val))
            elif key in GEMPROD:
                lines.append('#gemprod %d %d' % (key - 30, val))
            elif key == RESTRICTED:
                lines.append('#restricted %d' % val)
            else:
                more, left = generic_lines(gen.get(key, []), val)
                lines += more
                if left:
                    ro.append((generic_label(gen.get(key, []), key) if not more else
                               'ability %d bits no command sets' % key, hex(left) if more else left))
        out[i] = {'lines': lines, 'readonly': ro}
    return out


# spell record (6.37), defaults from #clear (0x140258af0): school -1, path 0 (fire) level 1, no
# second path, fatigue 20, effect 2, damage 10, nreff 1. #path n p / #pathlevel n l store at
# +0x26 + n / +0x28 + n. #flightspr and #explspr also store 1 and 9 frames. Abilities: 15
# int32 numbers at +0x64, int64 values at +0xa0, through the generic handler; #restricted N
# appends ability 278 = N.
S_SCHOOL, S_RESEARCH, S_PATH, S_PATHLEVEL = 0x24, 0x25, 0x26, 0x28
S_FIELDS = [('fatiguecost', 0x2a, 2), ('aoe', 0x2c, 2), ('effect', 0x2e, 2), ('range', 0x30, 2),
            ('precision', 0x32, 2), ('damage', 0x38, 8), ('nreff', 0x40, 2)]
S_FLIGHTSPR, S_EXPLSPR, S_SPEC, S_SPEC2, S_NEXT, S_SOUND = 0x42, 0x46, 0x50, 0x58, 0x60, 0x62
S_AB_KEYS, S_AB_VALS, S_AB_COUNT = 0x64, 0xa0, 15


def spell_commands(exe):
    model = ContextModel(exe, 'spell')
    for cmd, off, size in S_FIELDS + [('spec', S_SPEC, 8), ('spec2', S_SPEC2, 8), ('nextspell', S_NEXT, 2),
                                      ('sound', S_SOUND, 2), ('school', S_SCHOOL, 1), ('researchlevel', S_RESEARCH, 1)]:
        model.check(cmd, off, size)
    gen = collections.defaultdict(list)
    for c in model.p.generic_call_args():
        if c['context'] == 'spell' and c['key'] is not None:
            gen[c['key']].append(c)
    repeatable = {k for k, cs in gen.items() if any(c['repeat'] == 1 for c in cs)} | {RESTRICTED}
    out = {}
    for i, r in sorted(records(exe, 'spell').items()):
        sb = lambda o: struct.unpack_from('<b', r, o)[0]
        h = lambda o: struct.unpack_from('<h', r, o)[0]
        lines, ro = ['#name "%s"' % name_of(r).replace('"', "'")], []
        lines += ['#school %d' % sb(S_SCHOOL), '#researchlevel %d' % sb(S_RESEARCH)]
        for n in (0, 1):
            if sb(S_PATH + n) != -1:
                lines += ['#path %d %d' % (n, sb(S_PATH + n)), '#pathlevel %d %d' % (n, sb(S_PATHLEVEL + n))]
        for cmd, off, size in S_FIELDS:
            lines.append('#%s %d' % (cmd, struct.unpack_from('<q' if size == 8 else '<h', r, off)[0]))
        lines.append('#spec %d' % struct.unpack_from('<Q', r, S_SPEC)[0])
        for cmd, off in (('spec2', S_SPEC2),):
            v = struct.unpack_from('<Q', r, off)[0]
            if v:
                lines.append('#%s %d' % (cmd, v))
        if h(S_NEXT):
            lines.append('#nextspell %d' % h(S_NEXT))
        if h(S_SOUND):
            lines.append('#sound %d' % h(S_SOUND))
        for cmd, off, frames in (('flightspr', S_FLIGHTSPR, 1), ('explspr', S_EXPLSPR, 9)):
            if h(off) != -1:
                lines.append('#%s %d' % (cmd, h(off)))
                if h(off) and h(off + 2) != frames:     # #flightspr stores 1 frame, #explspr 9
                    ro.append(('%s frames' % cmd, h(off + 2)))
        seen = set()
        for k in range(S_AB_COUNT):
            key = struct.unpack_from('<i', r, S_AB_KEYS + 4 * k)[0]
            val = struct.unpack_from('<q', r, S_AB_VALS + 8 * k)[0]
            if not key:
                break
            if key in seen and key not in repeatable:
                ro.append(('ability %d repeated (the game reads the first)' % key, val))
                continue
            seen.add(key)
            if key == RESTRICTED:
                lines.append('#restricted %d' % val)
                continue
            more, left = generic_lines(gen.get(key, []), val)
            lines += more
            if left:
                ro.append((generic_label(gen.get(key, []), key), left))
        out[i] = {'lines': lines, 'readonly': ro}
    return out


# site record (6.37): look +0x28, path +0x2a, level +0x2c, rarity +0x2e, 16 (ability, value)
# int64 pairs from +0x30, terrain mask +0x130. #gems p n stores ability p + 1 = n.
T_LOOK, T_PATH, T_LEVEL, T_RARITY, T_ABILITIES, T_LOC, T_AB_COUNT = 0x28, 0x2a, 0x2c, 0x2e, 0x30, 0x130, 16
SITE_GEMS = range(1, 10)


def site_commands(exe):
    model = ContextModel(exe, 'site')
    for cmd, off, size in (('look', T_LOOK, 2), ('path', T_PATH, 2), ('level', T_LEVEL, 2), ('rarity', T_RARITY, 2),
                           ('loc', T_LOC, 4)):
        model.check(cmd, off, size)
    gen = collections.defaultdict(list)
    for c in model.p.generic_call_args():
        if c['context'] == 'site' and c['key'] is not None:
            gen[c['key']].append(c)
    repeatable = {k for k, cs in gen.items() if any(c['repeat'] == 1 for c in cs)}
    out = {}
    for i, r in sorted(records(exe, 'site').items()):
        h = lambda o: struct.unpack_from('<h', r, o)[0]
        lines, ro = ['#name "%s"' % name_of(r).replace('"', "'")], []
        lines += ['#path %d' % h(T_PATH), '#level %d' % h(T_LEVEL), '#rarity %d' % h(T_RARITY),
                  '#loc %d' % struct.unpack_from('<i', r, T_LOC)[0]]
        if h(T_LOOK) != -1:
            lines.append('#look %d' % h(T_LOOK))
        seen = set()
        for k in range(T_AB_COUNT):
            key, val = struct.unpack_from('<qq', r, T_ABILITIES + 16 * k)
            if not key:
                break
            if key in seen and key not in repeatable and key not in SITE_GEMS:
                ro.append(('ability %d repeated (the game reads the first)' % key, val))
                continue
            seen.add(key)
            if key in SITE_GEMS:
                lines.append('#gems %d %d' % (key - 1, val))
                continue
            more, left = generic_lines(gen.get(key, []), val)
            lines += more
            if left:
                ro.append((generic_label(gen.get(key, []), key), left))
        out[i] = {'lines': lines, 'readonly': ro}
    return out


# nation record (6.37): name, epithet +0x24, era +0xac, 200 abilities (int32 numbers from
# +0xb0, int64 values from +0x3d0), the recruitment list +0xa10 (int32; sections end with -2
# commanders, -3 foreign units, -4 foreign commanders, -1 end: #addreccom etc. insert with
# those markers), the god list +0xab8 to the record's end (#addgod N appends N, #delgod N
# appends -N). Found from the parser: #color/#secondarycolor (three floats each, 0-1), the
# status word #name checks (-998: an unused slot, which #name makes a nation); and from the code
# that names pretender files (newlords/<name>_N.2h), the nation's file name (no command).
N_EPITHET, N_ERA, N_AB_KEYS, N_AB_VALS, N_AB_COUNT, N_REC, N_GODS = 0x24, 0xac, 0xb0, 0x3d0, 200, 0xa10, 0xab8
N_REC_CMDS = ['addrecunit', 'addreccom', 'addforeignunit', 'addforeigncom']
SAVE_NAME_FORMAT = '%s/newlords/%s_%d.2h'


def f32(x):
    """The shortest decimal that reads back as this float32 (the parser reads "%f")."""
    for n in range(1, 10):
        t = '%.*g' % (n, x)
        if struct.unpack('<f', struct.pack('<f', float(t)))[0] == x:
            return t
    return repr(x)


def nation_fields(exe, p, base, size):
    """Record offsets the nation parser writes outside the abilities: the colors (movss stores
    in #color / #secondarycolor), the status word #name compares with the unused marker, and
    the save file name the game formats with SAVE_NAME_FORMAT."""
    import vanilla_other
    out = {}
    for cmd in ('color', 'secondarycolor'):
        offs = sorted(num(m.group(1)) for k in vanilla_other.branch(p, 'nation', cmd)
                      for m in [re.match(r'DWORD PTR \[\w+\+\w+\*1\+(0x[0-9a-f]+)\],xmm\d+$', p.ins[k][2])]
                      if p.ins[k][1] == 'movss' and m)
        if len(offs) != 3:
            raise SystemExit('nation #%s: expected three float stores, found %s' % (cmd, offs))
        out[cmd] = offs[0]
    eax = None
    for k in vanilla_other.branch(p, 'nation', 'name'):
        a, o, args, t = p.ins[k]
        m = re.match(r'WORD PTR \[\w+\+\w+\*1\+(0x[0-9a-f]+)\],ax$', args)
        if o == 'mov' and re.match(r'eax,0x[0-9a-f]+$', args):
            eax = num(args.split(',')[1])
        elif o == 'cmp' and m and eax is not None:
            out['status'] = (num(m.group(1)), struct.unpack('<h', struct.pack('<H', eax & 0xffff))[0])
            break
    # the epithet: as long as the parser reads it (it runs past the 36 bytes after the name)
    regs = {}
    for k in vanilla_other.branch(p, 'nation', 'epithet'):
        a, o, args, t = p.ins[k]
        m = re.match(r'(\w+),(0x[0-9a-f]+)$', args)
        m2 = re.match(r'r8d,\[(\w+)\+(0x[0-9a-f]+)\]$', args)
        if o == 'mov' and m and reg64(m.group(1)):
            regs[reg64(m.group(1))] = num(m.group(2))
        if o == 'lea' and m2 and reg64(m2.group(1)) in regs:
            out['epithet'] = regs[reg64(m2.group(1))] + num(m2.group(2))
            break
    found = collections.Counter()
    for k, (a, o, args, t) in enumerate(p.ins):
        if o == 'lea' and t is not None and exe.cstr(t) == SAVE_NAME_FORMAT:
            for x in p.ins[k - 12:k + 12]:
                if x[1] == 'lea' and x[3] is not None and base <= x[3] < base + size:
                    found[x[3] - base] += 1
    if 'status' not in out or 'epithet' not in out or not found:
        raise SystemExit('nation status word, epithet length or file name not found')
    out['file'] = found.most_common(1)[0][0]
    return out


def nation_direct_keys(p, setter, ctx='nation'):
    """Nation commands that call the nation ability setter themselves: the ability number at
    the first setter call reachable from the command's code (both ways at a conditional jump;
    the parsers branch on 'name or number' and on -1 meaning remove)."""
    fs = p.contexts[ctx]
    order = sorted(k for k, (f, s) in p.refs.items() if f in fs)
    jumps = collections.Counter(x[2].split()[0] for k in order for x in p.ins[k:k + 120] if x[1] == 'jmp')
    loop = jumps.most_common(1)[0][0]       # back to the parser's line loop

    def search(k, const, seen, depth):
        while depth < 120:
            depth += 1
            a, op, args, t = p.ins[k]
            if op == 'call':
                if args.split()[0] == setter:
                    return const.get('rdx')
                for r in ('rax', 'rcx', 'rdx', 'r8', 'r9', 'r10', 'r11'):
                    const.pop(r, None)
            elif op == 'ret':
                return None
            elif op.startswith('j'):
                tgt = args.split()[0]
                if tgt == loop or tgt in seen or not tgt.startswith('0x'):
                    if op == 'jmp':
                        return None
                else:
                    seen.add(tgt)
                    if op == 'jmp':
                        k = p.index[num(tgt)]
                        continue
                    found = search(p.index[num(tgt)], dict(const), seen, depth)
                    if found is not None:
                        return found
            else:
                d = reg64(args.split(',')[0])
                m = re.match(r'(\w+),(0x[0-9a-f]+)$', args)
                m2 = re.match(r'\w+,\[(\w+)([+-])(0x[0-9a-f]+)\]$', args)
                if op == 'mov' and m and d:
                    const[d] = num(m.group(2))
                elif op == 'lea' and m2 and d and reg64(m2.group(1)) in const:
                    const[d] = const[reg64(m2.group(1))] + (1 if m2.group(2) == '+' else -1) * num(m2.group(3))
                elif d and op not in ('cmp', 'test', 'push'):
                    const.pop(d, None)
            k += 1
        return None

    keys = {}
    for n, i in enumerate(order):
        cmd = p.refs[i][1]
        if cmd in keys:
            continue
        # the command's own code starts where the parser advances past its name
        k = next((j for j in range(i, i + 60) if p.ins[j][1] == 'add' and p.ins[j][2].split(',')[0] in ('edi', 'ebx', 'r15d', 'esi')), None)
        if k is not None:
            key = search(k + 1, {}, set(), 0)
            if key is not None:
                keys[cmd] = key
    return keys


def nation_commands(exe):
    p = Parser(exe)
    start, size, _ = table_records(exe, TABLES['nation'])
    nf = nation_fields(exe, p, exe.address(start), size)
    setter = setter_of(p, 'nation', 'startunitnbrs1')
    direct = collections.defaultdict(list)
    for cmd, key in nation_direct_keys(p, setter).items():
        direct[key].append(cmd)
    # #startsite's number comes from a register; #clearsites removes it by number (edx = 0x34
    # built as lea edx,[rax+0x34] with eax 0 after the name check)
    for i, (f, s_) in sorted(p.refs.items()):
        if s_ == 'clearsites' and f in p.contexts['nation']:
            for a, op, args, t in p.ins[i:i + 60]:
                m = re.match(r'edx,(?:\[rax\+)?(0x[0-9a-f]+)\]?$', args)
                if op in ('mov', 'lea') and m:
                    direct[num(m.group(1))].append('startsite')
                    break
            break
    gen = collections.defaultdict(list)
    for c in p.generic_call_args():
        if c['context'] == 'nation' and c['key'] is not None:
            gen[c['key']].append(c)
    repeatable = {k for k, cs in gen.items() if any(c['repeat'] == 1 for c in cs)} | \
        {k for k, cmds in direct.items() if 'startsite' in cmds}
    out = {}
    recs = records(exe, 'nation')
    file_name = lambda r: text(r[nf['file']:nf['file'] + 36].split(b'\0')[0])
    if file_name(recs[0]) != 'ind0':
        raise SystemExit('nation save file name not where expected (Independents: %r)' % file_name(recs[0]))
    for i, r in sorted(recs.items()):
        lines, ro = ['#name "%s"' % name_of(r).replace('"', "'")], []
        ep = text(r[N_EPITHET:N_EPITHET + nf['epithet']].split(b'\0')[0])
        if ep:
            lines.append('#epithet "%s"' % ep.replace('"', "'"))
        lines.append('#era %d' % struct.unpack_from('<h', r, N_ERA)[0])
        for cmd in ('color', 'secondarycolor'):
            lines.append('#%s %s' % (cmd, ' '.join(f32(x) for x in struct.unpack_from('<3f', r, nf[cmd]))))
        off, unused = nf['status']
        if struct.unpack_from('<h', r, off)[0] == unused:
            ro.append(('status', 'unused slot (a mod\'s #name makes it a nation)'))
        if file_name(r):
            ro.append(('file name', file_name(r)))
        section = 0
        for k in range(N_GODS - N_REC >> 2):
            m = struct.unpack_from('<i', r, N_REC + 4 * k)[0]
            if m == -1 or m == 0:
                break
            if m < 0:
                section = {-2: 1, -3: 2, -4: 3}.get(m, section)
                continue
            lines.append('#%s %d' % (N_REC_CMDS[section], m))
        for k in range((len(r) - N_GODS) // 4):
            g = struct.unpack_from('<i', r, N_GODS + 4 * k)[0]
            if not g:
                break
            lines.append('#addgod %d' % g if g > 0 else '#delgod %d' % -g)
        seen = set()
        for k in range(N_AB_COUNT):
            key = struct.unpack_from('<i', r, N_AB_KEYS + 4 * k)[0]
            val = struct.unpack_from('<q', r, N_AB_VALS + 8 * k)[0]
            if not key:
                break
            if key in seen and key not in repeatable:
                ro.append(('ability %d repeated (the game reads the first)' % key, val))
                continue
            seen.add(key)
            if key in direct:
                lines.append('#%s %d' % (sorted(direct[key])[0], val))
                continue
            more, left = generic_lines(gen.get(key, []), val)
            lines += more
            if left:
                ro.append((generic_label(gen.get(key, []), key), left))
        out[i] = {'lines': lines, 'readonly': ro}
    return out
