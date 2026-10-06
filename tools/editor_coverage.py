#!/usr/bin/env python3
"""Editor coverage: for each entity type, the commands the game reads (Dom5Edit/GameData,
from tools/dom6exe) that the editor offers no way to edit.

A command counts as covered when a badge definition (Dom5Editor/Data/*_badges.json) names it or
an entity view model refers to it (Command.X in Dom5Editor/UI/ViewModels). That's a lower bound
on "editable", not a check that the panel works: see docs/EDIT_FLOW.md.

    python3 tools/editor_coverage.py [--missing]
"""
import glob
import json
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
# game parser context -> (badge file stem, view model stem)
TYPES = [('monster', 'monster', 'monster'), ('weapon', 'weapon', 'weapon'), ('armor', 'armor', 'armor'),
         ('item', 'item', 'item'), ('spell', 'spell', 'spell'), ('site', 'site', 'site'),
         ('nation', 'nation', 'nation'), ('merc', 'mercenary', 'mercenary'), ('poptype', 'poptype', 'poptype'),
         ('nametype', 'nametype', 'nametype'), ('event', 'event', 'event'), ('bless', 'bless', 'bless'),
         ('template', 'template', 'template')]
# handled by the editor's structure rather than as a command
STRUCTURAL = {'end', 'name'}


def main():
    catalog = json.load(open(sorted(glob.glob(os.path.join(ROOT, 'Dom5Edit/GameData/game-commands-*.json')))[-1]))
    top = set(catalog['top'])
    src = open(os.path.join(ROOT, 'Dom5Edit/Commands/Command.cs'), encoding='utf-8-sig').read()
    name_of = {m.group(2): m.group(1) for m in re.finditer(r'_commandMap\.Add\("#([a-z0-9_]+)", Command\.(\w+)\)', src)}
    show_missing = '--missing' in sys.argv
    print('%-9s %6s %8s %8s' % ('type', 'game', 'covered', 'missing'))
    for ctx, badge_stem, vm_stem in TYPES:
        game = set(catalog['contexts'].get(ctx, {}).get('commands', [])) - top - STRUCTURAL
        covered = set()
        path = os.path.join(ROOT, 'Dom5Editor/Data/%s_badges.json' % badge_stem)
        if os.path.exists(path):
            for section in json.load(open(path, encoding='utf-8-sig'))['sections']:
                covered |= {c['name'].lstrip('#') for c in section['commands']}
        path = os.path.join(ROOT, 'Dom5Editor/UI/ViewModels/%sViewModel.cs' % vm_stem.capitalize())
        for f in glob.glob(os.path.join(ROOT, 'Dom5Editor/UI/ViewModels/*ViewModel.cs')):
            if os.path.basename(f).lower() == ('%sviewmodel.cs' % vm_stem):
                covered |= {name_of[m] for m in re.findall(r'Command\.([A-Z0-9_]+)', open(f, encoding='utf-8-sig').read()) if m in name_of}
        missing = sorted(game - covered)
        print('%-9s %6d %8d %8d' % (ctx, len(game), len(game) - len(missing), len(missing)))
        if show_missing and missing:
            print('          ' + ' '.join(missing))


if __name__ == '__main__':
    main()
