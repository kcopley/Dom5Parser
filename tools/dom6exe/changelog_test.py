#!/usr/bin/env python3
"""Checks dom6exe.py changelog on known changes, without a second game version.

The repository's 6.37 data (vanilla.dm, data/events-6.37.dm, the catalog, the reading rules,
the spell effects table) is the old version; a copy with a few deliberate changes is the new
one (a unit's hit points and a read-only value, a unit that gains an ability, a weapon removed,
a unit added, an event's rarity, a command the game reads, a spell effect's #damage, a
description). The change log must list exactly those, nothing else; and the data against itself
must give "No changes."

  python3 tools/dom6exe/changelog_test.py
"""
import json, os, re, shutil, sys, tempfile

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(os.path.dirname(HERE))
sys.path.insert(0, HERE)
import changelog  # noqa: E402


def repo_snapshot(folder):
    os.makedirs(folder)
    for src, dst in ((os.path.join(ROOT, 'vanilla.dm'), 'vanilla.dm'),
                     (os.path.join(HERE, 'data', 'events-6.37.dm'), 'events.dm'),
                     (os.path.join(ROOT, 'Dom5Edit', 'GameData', 'game-commands-6.37.json'), 'catalog.json'),
                     (os.path.join(HERE, 'data', 'dmread-6.37.json'), 'dmread.json'),
                     (os.path.join(HERE, 'data', 'spell-effects-6.37.json'), 'spell-effects.json')):
        shutil.copy(src, os.path.join(folder, dst))
    json.dump({'monster descr': {'14': 'aaaa', '20': 'bbbb'}}, open(os.path.join(folder, 'text-hashes.json'), 'w'))
    json.dump({'game_version': '6.37'}, open(os.path.join(folder, 'meta.json'), 'w'))


def edit(path, old, new, count=1):
    s = open(path, encoding='utf-8').read()
    if s.count(old) != count:
        raise SystemExit('test setup: %r found %d times in %s' % (old, s.count(old), path))
    open(path, 'w', encoding='utf-8').write(s.replace(old, new))


def changes(text):
    """The log's change lines (bullets, New:, Removed:), headings and summary left out."""
    body = text.split('\n## ', 1)[1] if '\n## ' in text else ''
    return [l for l in body.splitlines() if l.startswith('- ') or l.startswith('New: ') or l.startswith('Removed: ')]


def main():
    tmp = tempfile.mkdtemp(prefix='dom6-changelog-test-')
    try:
        old, new = os.path.join(tmp, 'old'), os.path.join(tmp, 'new')
        repo_snapshot(old)
        same = changelog.changelog(changelog.Version(old), changelog.Version(old))
        assert same.rstrip().endswith('No changes.'), same
        shutil.copytree(old, new)
        v = os.path.join(new, 'vanilla.dm')
        # Hoplite: hp 11 -> 12, a read-only value
        edit(v, '#name "Hoplite"\n#okleader\n#ap 12\n#mapmove 16\n#size 3\n#hp 11\n', '#name "Hoplite"\n#okleader\n#ap 12\n#mapmove 16\n#size 3\n#hp 12\n')
        hop = re.search(r'#selectmonster 14\n.*?#end\n', open(v).read(), re.S).group(0)
        edit(v, hop, hop.replace('-- ro: ability 358 = 1', '-- ro: ability 358 = 2'))
        # Heavy Cavalry gains #flying
        edit(v, '#selectmonster 20\n#name "Heavy Cavalry"\n', '#selectmonster 20\n#name "Heavy Cavalry"\n#flying\n')
        # the Pike is gone
        pike = re.search(r'#selectweapon 2\n.*?#end\n\n', open(v).read(), re.S).group(0)
        edit(v, pike, '')
        # a new unit
        edit(v, '#selectmonster 14\n', '#selectmonster 4900\n#name "Test Unit"\n#hp 5\n#end\n\n#selectmonster 14\n')
        # event 3: rarity -2 -> -1
        edit(os.path.join(new, 'events.dm'), '#selectevent 3\n#rarity -2\n', '#selectevent 3\n#rarity -1\n')
        # a command the game reads now
        c = json.load(open(os.path.join(new, 'catalog.json')))
        c['contexts']['monster']['commands'].append('testcommand')
        json.dump(c, open(os.path.join(new, 'catalog.json'), 'w'))
        # what #damage is for effect 10089
        s = json.load(open(os.path.join(new, 'spell-effects.json')))
        s['effects']['10089']['argument'] = 'monster'
        json.dump(s, open(os.path.join(new, 'spell-effects.json'), 'w'))
        # Hoplite's description
        json.dump({'monster descr': {'14': 'cccc', '20': 'bbbb'}}, open(os.path.join(new, 'text-hashes.json'), 'w'))

        text = changelog.changelog(changelog.Version(old), changelog.Version(new))
        got = changes(text)
        expected = [
            r'^New: Test Unit #4900$',
            r'^- Hoplite #14( \[.*\])?: #hp 11 -> 12; ro: ability 358 1 -> 2$',
            r'^- Heavy Cavalry #20( \[.*\])?: gained #flying$',
            r'^Removed: Pike #2$',
            r'^- Event 3: #rarity -2 -> -1$',
            r'^- Hoplite #14: descr changed$',
            r'^- monster: new commands #testcommand$',
            r'^- Spell effect 10089: #damage is unique_or_monster_or_tag -> monster$',
        ]
        missing = [e for e in expected if not any(re.match(e, g) for g in got)]
        extra = [g for g in got if not any(re.match(e, g) for e in expected)]
        if missing or extra:
            print(text)
            print('MISSING:', missing)
            print('EXTRA:', extra)
            return 1
        print('changelog: the data against itself: no changes; %d deliberate changes, each listed, nothing else' % len(expected))
        return 0
    finally:
        shutil.rmtree(tmp, ignore_errors=True)


if __name__ == '__main__':
    sys.exit(main())
