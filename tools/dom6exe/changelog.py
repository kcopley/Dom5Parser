"""What changed between two versions of Dominions 6 (dom6exe.py snapshot, dom6exe.py changelog).

Patch notes often say only "misc stats and adjustments". This compares everything tools/dom6exe
reads from two Dominions6.exe files, entity by entity, and writes the differences as Markdown:
units (grouped by the nations that recruit them), weapons, armor, items, spells, sites, nations,
mercenaries, blesses, poptypes, nametypes, events, what a spell's #damage means per #effect, and
the modding side (the commands each entity type reads, their argument formats, text limits).

  dom6exe.py snapshot --out DIR [--texts]      everything the change log needs from one exe
  dom6exe.py changelog OLD NEW [--out F.md] [--texts]

OLD and NEW are snapshot folders, Dominions6.exe files (read now), or vanilla .dm files (only
the entities). Keep a snapshot of the current version before Steam updates the game, outside
the repository; after the update, compare it with the new exe.

The game's prose (descriptions, event messages) is never written into a snapshot or the change
log: a snapshot keeps a checksum of each text, and the log says "description changed". With
--texts the snapshot also keeps the texts (texts.json, for local use: never commit it), and the
change log shows old and new text where both snapshots have them.
"""
import collections, datetime, hashlib, json, os, re, shutil, tempfile

FILES = {'vanilla': 'vanilla.dm', 'events': 'events.dm', 'catalog': 'catalog.json', 'dmread': 'dmread.json',
         'spell_effects': 'spell-effects.json', 'text_hashes': 'text-hashes.json', 'texts': 'texts.json',
         'meta': 'meta.json'}

KIND_OF_HEADER = {'selectweapon': 'weapon', 'selectarmor': 'armor', 'selectmonster': 'monster', 'selectspell': 'spell',
                  'selectitem': 'item', 'selectsite': 'site', 'selectnation': 'nation', 'selectbless': 'bless',
                  'selectpoptype': 'poptype', 'selectnametype': 'nametype', 'newmerc': 'merc', 'selectevent': 'event'}
TITLES = [('monster', 'Units'), ('weapon', 'Weapons'), ('armor', 'Armor'), ('item', 'Magic items'), ('spell', 'Spells'),
          ('site', 'Magic sites'), ('nation', 'Nations'), ('merc', 'Mercenaries'), ('bless', 'Blesses'),
          ('poptype', 'Poptypes'), ('nametype', 'Nametypes'), ('event', 'Events')]
ERA = {'1': 'EA', '2': 'MA', '3': 'LA'}


def _sha(text):
    return hashlib.sha1(text.encode('utf-8', 'surrogatepass')).hexdigest()[:16]


# ---- snapshot ------------------------------------------------------------------------------

def snapshot(exe, out, texts=False):
    """Writes into `out` everything the change log compares, read from this exe."""
    import dom6exe, dmread, events, spelleffects, vanilla_dm
    import texts as text_module
    os.makedirs(out, exist_ok=True)
    vanilla_dm.write(exe, os.path.join(out, FILES['vanilla']))
    events.write(exe, os.path.join(out, FILES['events']), messages=False)
    catalog = dom6exe.cmd_catalog(exe, None)
    rules = dict({'game_version': exe.version, 'exe_sha256_16': exe.sha}, **dmread.Rules(exe).summary())
    effects = spelleffects.collect(exe)
    # the game's texts: a checksum of each (and the texts themselves only when asked: local use)
    hashes, kept = collections.defaultdict(dict), collections.defaultdict(dict)
    for e in events.EventTable(exe).events():
        hashes['event message'][str(e['number'])] = _sha(e['message'])
        if texts:
            kept['event message'][str(e['number'])] = e['message']
    tx = text_module.Texts(exe)
    for ctx, cmds in text_module.TEXT_COMMANDS.items():
        for cmd in cmds:
            for i, t in tx.vanilla_texts(ctx, cmd):
                hashes['%s %s' % (ctx, cmd)][str(i)] = _sha(t)
                if texts:
                    kept['%s %s' % (ctx, cmd)][str(i)] = t
    for key, data in (('catalog', catalog), ('dmread', rules), ('spell_effects', effects), ('text_hashes', hashes)):
        with open(os.path.join(out, FILES[key]), 'w') as f:
            json.dump(data, f, indent=1, sort_keys=True)
            f.write('\n')
    if texts:
        with open(os.path.join(out, FILES['texts']), 'w', encoding='utf-8') as f:
            json.dump(kept, f, indent=1, ensure_ascii=False)
    elif os.path.exists(os.path.join(out, FILES['texts'])):
        os.remove(os.path.join(out, FILES['texts']))
    meta = {'game_version': exe.version, 'exe_sha256_16': exe.sha, 'exe': os.path.abspath(exe.path),
            'made': datetime.date.today().isoformat(), 'contains_game_text': bool(texts),
            'note': 'Written by tools/dom6exe snapshot, for dom6exe.py changelog.' +
                    (' texts.json holds the game\'s own texts: keep this folder to yourself.' if texts else '')}
    with open(os.path.join(out, FILES['meta']), 'w') as f:
        json.dump(meta, f, indent=1)
        f.write('\n')
    return meta


# ---- reading -------------------------------------------------------------------------------

class Entity:
    def __init__(self, kind, key, header):
        self.kind, self.key, self.header = kind, key, header
        self.lines = []          # (command, value): '#hp 11' -> ('hp', '11'); '-- ro: x = 1' -> ('ro: x', '1')

    @property
    def name(self):
        for c, v in self.lines:
            if c == 'name':
                return v.strip('"')
        return None

    def values(self):
        out = collections.OrderedDict()
        for c, v in self.lines:
            out.setdefault(c, []).append(v)
        return out


def parse_dm(path):
    """kind -> key -> Entity, for every #select.../#newmerc block of a vanilla .dm or events file."""
    out = collections.defaultdict(collections.OrderedDict)
    cur, merc = None, collections.Counter()
    for raw in open(path, encoding='utf-8', errors='replace'):
        line = raw.rstrip('\n').strip()
        m = re.match(r'^#(\w+)\s*(.*)$', line)
        if cur is None:
            if m and m.group(1) in KIND_OF_HEADER:
                kind = KIND_OF_HEADER[m.group(1)]
                arg = m.group(2).split('--')[0].strip()
                cur = Entity(kind, arg, line)
            continue
        if line.startswith('#end'):
            if cur.kind == 'merc':      # (bands have no number: by name, in order)
                name = cur.name or '?'
                merc[name] += 1
                cur.key = name if merc[name] == 1 else '%s (%d)' % (name, merc[name])
            out[cur.kind][cur.key] = cur
            cur = None
            continue
        ro = re.match(r'^--\s*ro:\s*(.*?)\s*=\s*(.*)$', line)
        if ro:
            cur.lines.append(('ro: ' + ro.group(1), ro.group(2).strip()))
        elif m:
            value = m.group(2)
            if not value.startswith('"'):
                value = value.split('--')[0]
            else:      # a quoted text: up to its closing quote (a comment may follow)
                q = value.find('"', 1)
                value = value[:q + 1] if q > 0 else value
            cur.lines.append((m.group(1), ' '.join(value.split())))
    return out


class Version:
    """One version's data: a snapshot folder, an exe (read into a temporary snapshot), or a .dm."""

    def __init__(self, source, exe_class=None):
        self.source = source
        self._tmp = None
        if os.path.isfile(source) and source.lower().endswith('.exe'):
            import dom6exe
            self._tmp = tempfile.mkdtemp(prefix='dom6-snapshot-')
            snapshot((exe_class or dom6exe.Exe)(source), self._tmp)
            folder = self._tmp
        elif os.path.isdir(source):
            folder = source
        else:
            folder = None
        self.folder = folder

        def load(key):
            if folder is None:
                return None
            p = os.path.join(folder, FILES[key])
            if not os.path.exists(p):
                return None
            return json.load(open(p, encoding='utf-8')) if p.endswith('.json') else p
        self.meta = load('meta') or {}
        vanilla = load('vanilla') if folder else source
        self.entities = parse_dm(vanilla) if vanilla else {}
        ev = load('events')
        if ev:
            self.entities.update(parse_dm(ev))
        self.catalog, self.dmread = load('catalog'), load('dmread')
        self.spell_effects, self.text_hashes, self.texts = load('spell_effects'), load('text_hashes'), load('texts')

    @property
    def label(self):
        v = self.meta.get('game_version')
        sha = self.meta.get('exe_sha256_16')
        if not v:
            return os.path.basename(self.source.rstrip('/\\'))
        return '%s (exe %s)' % (v, sha) if sha else v

    def close(self):
        if self._tmp:
            shutil.rmtree(self._tmp, ignore_errors=True)


# ---- comparing -----------------------------------------------------------------------------

# which commands name another entity, per kind of block (for names in the log only)
def ref_kind(kind, command, effect_argument=None):
    if command in ('weapon', 'secondaryeffect', 'secondaryeffectalways') and kind in ('monster', 'item', 'weapon'):
        return 'weapon'
    if command == 'armor' and kind in ('monster', 'item'):
        return 'armor'
    if command == 'nextspell':
        return 'spell'
    if command == 'damage' and kind == 'spell':
        return 'monster' if effect_argument in ('monster', 'monster_or_tag', 'unique_or_monster_or_tag') else None
    if kind == 'monster' and re.search(r'shape|summon|makemonsters|batstartsum|battlesum|twiceborn|xpshapemon|templetrainer|slaver', command):
        return 'monster'
    if kind in ('nation', 'poptype') and re.search(r'unit|com|hero|scout|god', command) and not re.search(r'nbr|mult', command):
        return 'monster'
    if kind == 'site' and re.search(r'mon$|com$|summon', command):
        return 'monster'
    if kind == 'merc' and command in ('com', 'unit'):
        return 'monster'
    if kind == 'event' and re.search(r'^(com|[1-9]?d6units|[1-9]?units|[1-9]?com|assassin|stealthcom|req_targmnr|req_monster)', command):
        return 'monster'
    return None


class Names:
    def __init__(self, *versions):
        self.names = collections.defaultdict(dict)
        for v in versions:
            for kind, ents in v.entities.items():
                for key, e in ents.items():
                    if e.name:
                        self.names[kind].setdefault(key, e.name)

    def of(self, kind, value):
        if kind is None:
            return value
        first = value.split()[0] if value else ''
        n = self.names.get(kind, {}).get(first)
        return '%s (%s)' % (value, n) if n else value


def effect_argument(version, entity):
    if entity.kind != 'spell' or not version.spell_effects:
        return None
    eff = next((v for c, v in entity.lines if c == 'effect'), None)
    try:
        e = int(eff)
    except (TypeError, ValueError):
        return None
    if 1000 <= e < 10000:
        return None      # (no battle code reads the thousands: a plain number)
    return (version.spell_effects.get('effects', {}).get(str(e)) or {}).get('argument')


def compare_entity(old, new, names, old_arg=None, new_arg=None):
    """The changes of one entity, as short phrases ('#hp 11 -> 12', 'gained #flying')."""
    a, b = old.values(), new.values()
    out = []

    def show(c, v, arg):
        rk = ref_kind(new.kind, c, arg)
        return names.of(rk, v) if v else ''

    def cmd(c):
        return c if c.startswith('ro: ') else '#' + c

    for c in list(a) + [x for x in b if x not in a]:
        va, vb = a.get(c, []), b.get(c, [])
        if va == vb:
            continue
        arg_a, arg_b = (old_arg, new_arg) if c == 'damage' else (None, None)
        if len(va) <= 1 and len(vb) <= 1:
            if not va:
                out.append('gained %s%s' % (cmd(c), (' ' + show(c, vb[0], arg_b)) if vb[0] else ''))
            elif not vb:
                out.append('lost %s%s' % (cmd(c), (' ' + show(c, va[0], arg_a)) if va[0] else ''))
            else:
                out.append('%s %s -> %s' % (cmd(c), show(c, va[0], arg_a), show(c, vb[0], arg_b)))
            continue
        # keyed (#magicskill 3 2, #path 0 1, ability lines): by the first value
        ka = [v.split(None, 1) for v in va]
        kb = [v.split(None, 1) for v in vb]
        if all(len(x) == 2 for x in ka + kb) and len({x[0] for x in ka}) == len(ka) and len({x[0] for x in kb}) == len(kb):
            da, db = dict(ka), dict(kb)
            for k in list(da) + [k for k in db if k not in da]:
                if k not in db:
                    out.append('lost %s %s %s' % (cmd(c), k, da[k]))
                elif k not in da:
                    out.append('gained %s %s %s' % (cmd(c), k, db[k]))
                elif da[k] != db[k]:
                    out.append('%s %s: %s -> %s' % (cmd(c), k, da[k], db[k]))
            continue
        ca, cb = collections.Counter(va), collections.Counter(vb)
        gone, came = ca - cb, cb - ca
        if not gone and not came:
            out.append('%s in another order' % cmd(c))
            continue
        parts = ['+' + show(c, v, arg_b) for v in came.elements()] + ['-' + show(c, v, arg_a) for v in gone.elements()]
        out.append('%s %s' % (cmd(c), ', '.join(parts)))
    return out


def label_of(e, kind):
    name = e.name
    key = e.key
    if kind == 'event':
        return 'Event %s' % key
    if kind == 'merc':
        return name or key
    return '%s #%s' % (name, key) if name else '%s #%s' % (kind, key)


def nations_of_units(version):
    """monster number -> set of nation labels whose recruit lists, heroes, start units or gods have it."""
    out = collections.defaultdict(set)
    for key, n in version.entities.get('nation', {}).items():
        era = next((ERA.get(v) for c, v in n.lines if c == 'era'), None)
        label = '%s%s' % (n.name or 'Nation %s' % key, ' (%s)' % era if era else '')
        for c, v in n.lines:
            if ref_kind('nation', c) == 'monster' and v.split() and v.split()[0].lstrip('-').isdigit():
                out[v.split()[0]].add(label)
    return out


def text_changes(old, new):
    """(kind, key) whose text changed, came or went, by checksum."""
    out = []
    if not old.text_hashes or not new.text_hashes:
        return None
    for kind in sorted(set(old.text_hashes) | set(new.text_hashes)):
        a, b = old.text_hashes.get(kind, {}), new.text_hashes.get(kind, {})
        for key in sorted(set(a) | set(b), key=lambda k: int(k) if k.lstrip('-').isdigit() else 0):
            if a.get(key) != b.get(key):
                out.append((kind, key, 'added' if key not in a else 'removed' if key not in b else 'changed'))
    return out


def modding_changes(old, new):
    out = []
    if old.catalog and new.catalog:
        ta, tb = set(old.catalog.get('top', [])), set(new.catalog.get('top', []))
        for c in sorted(tb - ta):
            out.append('New top-level command #%s' % c)
        for c in sorted(ta - tb):
            out.append('Top-level command #%s is gone' % c)
        ca, cb = old.catalog.get('contexts', {}), new.catalog.get('contexts', {})
        for ctx in sorted(set(ca) | set(cb)):
            a = set(ca.get(ctx, {}).get('commands', []))
            b = set(cb.get(ctx, {}).get('commands', []))
            if b - a:
                out.append('%s: new commands %s' % (ctx, ', '.join('#' + c for c in sorted(b - a))))
            if a - b:
                out.append('%s: commands no longer read %s' % (ctx, ', '.join('#' + c for c in sorted(a - b))))
            ea, eb = ca.get(ctx, {}).get('effects', {}), cb.get(ctx, {}).get('effects', {})
            changed = sorted(c for c in set(ea) & set(eb) if ea[c] != eb[c])
            if changed:
                out.append('%s: what these commands store changed: %s' % (ctx, ', '.join(
                    '#%s (%s -> %s)' % (c, json.dumps(ea[c], sort_keys=True), json.dumps(eb[c], sort_keys=True)) for c in changed)))
    if old.dmread and new.dmread:
        fa, fb = old.dmread.get('arg_formats', {}), new.dmread.get('arg_formats', {})
        for ctx in sorted(set(fa) | set(fb)):
            a, b = fa.get(ctx, {}), fb.get(ctx, {})
            diffs = ['#%s %s -> %s' % (c, a.get(c, '(none)'), b.get(c, '(none)')) for c in sorted(set(a) | set(b)) if a.get(c) != b.get(c)]
            if diffs:
                out.append('%s: argument formats changed: %s' % (ctx, ', '.join(diffs)))
        sa, sb = old.dmread.get('string_commands', {}), new.dmread.get('string_commands', {})
        for ctx in sorted(set(sa) | set(sb)):
            a, b = sa.get(ctx, {}), sb.get(ctx, {})
            for c in sorted(set(a) | set(b)):
                if a.get(c) != b.get(c):
                    out.append('%s: text command #%s: %s -> %s' % (ctx, c, json.dumps(a.get(c), sort_keys=True), json.dumps(b.get(c), sort_keys=True)))
        for key, what in (('new_select_names', 'The #new/#select commands'), ('refuse_new_select_in_block', 'Types that refuse a #new/#select in an open block'),
                          ('refuse_missing_end_at_eof', 'Types that refuse a missing #end at the end of the file')):
            if old.dmread.get(key) != new.dmread.get(key):
                out.append('%s changed: %s -> %s' % (what, old.dmread.get(key), new.dmread.get(key)))
    if old.spell_effects and new.spell_effects:
        a, b = old.spell_effects.get('effects', {}), new.spell_effects.get('effects', {})
        for e in sorted(set(a) | set(b), key=int):
            x, y = (a.get(e) or {}).get('argument'), (b.get(e) or {}).get('argument')
            if x != y:
                out.append('Spell effect %s: #damage is %s -> %s' % (e, x or '(no such effect)', y or '(no such effect)'))
        for k in ('summon_special_codes', 'monster_tag_at_or_below', 'unique_monster_from', 'unique_list_keys', 'ranges'):
            if old.spell_effects.get(k) != new.spell_effects.get(k):
                out.append('Spell effects: %s changed' % k)
    return out


def changelog(old, new, show_texts=False):
    names = Names(new, old)
    lines = ['# Dominions changes: %s -> %s' % (old.label, new.label), '',
             'Written by tools/dom6exe changelog from what each Dominions6.exe holds (stats, abilities, '
             'read-only values, events, what the game reads in a mod). Commands are the modding '
             'commands that set each value; "-- ro:" values are ones no command sets.', '']
    summary, body = [], []
    any_change = False
    for kind, title in TITLES:
        a, b = old.entities.get(kind, {}), new.entities.get(kind, {})
        if not a and not b:
            continue
        added = [k for k in b if k not in a]
        removed = [k for k in a if k not in b]
        changed = []
        for k in b:
            if k in a:
                d = compare_entity(a[k], b[k], names, effect_argument(old, a[k]), effect_argument(new, b[k]))
                if d:
                    changed.append((k, d))
        if not (added or removed or changed):
            continue
        any_change = True
        summary.append('- %s: %d changed, %d new, %d removed' % (title, len(changed), len(added), len(removed)))
        body.append('## %s' % title)
        body.append('')
        if added:
            body.append('New: ' + ', '.join(label_of(b[k], kind) for k in added))
            body.append('')
        if removed:
            body.append('Removed: ' + ', '.join(label_of(a[k], kind) for k in removed))
            body.append('')
        if kind == 'monster':
            # by nation (the recruit lists, heroes, start units, gods), then shared, then the rest
            owners = nations_of_units(new)
            for k, ks in nations_of_units(old).items():
                owners[k] |= ks
            groups = collections.OrderedDict()
            for k, d in changed:
                ns = sorted(owners.get(k, ()))
                g = ns[0] if len(ns) == 1 else 'Units of several nations' if ns else 'Other units'
                groups.setdefault(g, []).append((k, d, ns))
            order = sorted(g for g in groups if g not in ('Units of several nations', 'Other units')) + \
                [g for g in ('Units of several nations', 'Other units') if g in groups]
            for g in order:
                body.append('### %s' % g)
                body.append('')
                for k, d, ns in groups[g]:
                    extra = ' [%s]' % ', '.join(ns) if len(ns) > 1 and len(ns) <= 6 else ' [%d nations]' % len(ns) if len(ns) > 6 else ''
                    body.append('- %s%s: %s' % (label_of(b[k], kind), extra, '; '.join(d)))
                body.append('')
        elif changed:
            for k, d in changed:
                body.append('- %s: %s' % (label_of(b[k], kind), '; '.join(d)))
            body.append('')
    texts = text_changes(old, new)
    if texts:
        any_change = True
        summary.append('- Texts: %d changed, came or went (descriptions, event messages)' % len(texts))
        body.append('## Texts')
        body.append('')
        body.append('The game\'s own texts aren\'t copied here: only which changed.' if not show_texts else
                    'With --texts: the old and new texts (the game\'s own words: keep this file to yourself).')
        body.append('')
        kinds_entity = {'monster': 'monster', 'item': 'item', 'nation': 'nation', 'spell': 'spell', 'event': 'event'}
        for kind, key, how in texts:
            ek = kinds_entity.get(kind.split()[0])
            ent = new.entities.get(ek, {}).get(key) or old.entities.get(ek, {}).get(key)
            who = label_of(ent, ek) if ent else '%s %s' % (kind.split()[0], key)
            body.append('- %s: %s %s' % (who, kind.split(None, 1)[1] if ' ' in kind else kind, how))
            if show_texts and old.texts and new.texts:
                ta = (old.texts.get(kind) or {}).get(key)
                tb = (new.texts.get(kind) or {}).get(key)
                if ta:
                    body.append('  - was: %s' % ta.replace('\n', ' '))
                if tb:
                    body.append('  - now: %s' % tb.replace('\n', ' '))
        body.append('')
    mod = modding_changes(old, new)
    if mod:
        any_change = True
        summary.append('- Modding: %d changes' % len(mod))
        body.append('## Modding')
        body.append('')
        body.extend('- ' + m for m in mod)
        body.append('')
    if not any_change:
        lines.append('No changes.')
        return '\n'.join(lines) + '\n'
    lines.extend(summary)
    lines.append('')
    lines.extend(body)
    return '\n'.join(lines).rstrip('\n') + '\n'
