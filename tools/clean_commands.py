#!/usr/bin/env python3
"""
Command Data Cleaner for Dominions 6 Modding Manual

Cleans up and refines the extracted command data:
- Removes garbage from descriptions (table data, page numbers)
- Fixes entity type classifications
- Standardizes capitalization
- Removes duplicate/invalid entries
"""

import json
import re
from pathlib import Path
from dataclasses import dataclass, asdict, field
from typing import Optional


# Known command -> entity type mappings based on command prefixes/names
COMMAND_ENTITY_MAP = {
    # Monster commands
    'newmonster': ['monster'],
    'selectmonster': ['monster'],
    'copystats': ['monster'],
    'copyspr': ['monster', 'item'],
    'clearweapons': ['monster'],
    'cleararmor': ['monster'],
    'clearmagic': ['monster'],
    'clearspec': ['monster'],

    # Weapon commands
    'newweapon': ['weapon'],
    'selectweapon': ['weapon'],
    'copyweapon': ['weapon'],

    # Armor commands
    'newarmor': ['armor'],
    'selectarmor': ['armor'],
    'copyarmor': ['armor'],

    # Spell commands
    'newspell': ['spell'],
    'selectspell': ['spell'],
    'copyspell': ['spell'],

    # Item commands
    'newitem': ['item'],
    'selectitem': ['item'],
    'copyitem': ['item'],

    # Site commands
    'newsite': ['site'],
    'selectsite': ['site'],
    'copysite': ['site'],

    # Nation commands
    'newnation': ['nation'],
    'selectnation': ['nation'],
    'copynation': ['nation'],

    # Event commands
    'newevent': ['event'],
    'selectevent': ['event'],

    # Mercenary commands
    'newmerc': ['mercenary'],

    # Poptype commands
    'selectpoptype': ['poptype'],

    # Nametype commands
    'selectnametype': ['nametype'],

    # General/mod commands
    'modname': ['mod'],
    'description': ['mod'],
    'icon': ['mod'],
    'version': ['mod'],
    'domversion': ['mod'],

    # Sound commands
    'selectsound': ['sound'],
    'sample': ['sound'],
    'smpmode': ['sound'],
    'loop': ['sound'],
}

# Commands that apply to multiple entity types
MULTI_ENTITY_COMMANDS = {
    'name': ['monster', 'weapon', 'armor', 'spell', 'item', 'site', 'nation', 'mercenary'],
    'descr': ['monster', 'spell', 'item', 'site', 'nation'],
    'end': ['monster', 'weapon', 'armor', 'spell', 'item', 'site', 'nation', 'event', 'mercenary'],
    'clear': ['monster', 'weapon', 'armor', 'spell', 'item', 'site', 'nation'],
    'spr1': ['monster'],
    'spr2': ['monster'],
    'gcost': ['monster'],
    'rcost': ['monster', 'armor', 'weapon'],
    'hp': ['monster', 'item'],
    'str': ['monster'],
    'att': ['monster', 'weapon'],
    'def': ['monster', 'armor'],
    'prec': ['monster'],
    'prot': ['monster', 'armor'],
    'mr': ['monster'],
    'mor': ['monster'],
    'enc': ['monster', 'armor'],
    'ap': ['monster'],
    'mapmove': ['monster'],
    'size': ['monster'],
    'ressize': ['monster'],
    'weapon': ['monster', 'item'],
    'armor': ['monster', 'item'],
    'dmg': ['weapon'],
    'nratt': ['weapon'],
    'range': ['weapon', 'spell'],
    'ammo': ['weapon'],
    'effect': ['spell'],
    'damage': ['spell'],
    'fatiguecost': ['spell'],
    'path': ['spell'],
    'pathlevel': ['spell'],
    'school': ['spell'],
    'researchlevel': ['spell'],
    'aoe': ['spell', 'weapon'],
    'gemcost': ['item'],
    'mainpath': ['item'],
    'mainlevel': ['item'],
    'constlevel': ['item'],
    'type': ['armor', 'site'],
    'homemon': ['site'],
    'homecom': ['site'],
    'com': ['site', 'event'],
    'mon': ['site'],
    'rarity': ['site', 'event'],
    'loc': ['site'],
    'level': ['site'],
    'gold': ['site', 'event'],
    'res': ['site'],
    'gems': ['site'],
    'era': ['nation'],
    'startsite': ['nation'],
    'addrecunit': ['nation'],
    'addreccom': ['nation'],
    'homerealm': ['nation'],
    'likesterr': ['nation'],
    'hatesterr': ['nation'],
}

# Patterns to remove from descriptions (table data, garbage)
GARBAGE_PATTERNS = [
    r'^\d+\s+[A-Z][a-z]+\s+',  # "16 Fire flare" type patterns
    r'\d+\s+[A-Z][a-z]+(?:\s+[a-z]+)?$',  # Trailing table entries
    r'^\d+\s*$',  # Just numbers
    r'\d+-\d+\s+\d+\s+',  # Range patterns like "10001-10317"
    r'Table \w+',  # Table references
    r'^\d+\s+(?:Mono|Stereo)',  # Sound format notes
    r'\| <\w+ nbr>',  # Incomplete argument patterns
    r'^[A-Z][a-z]+\s+\d+$',  # "Fire 128" type patterns
]

# Known good descriptions for common commands (manual overrides)
MANUAL_DESCRIPTIONS = {
    '#newmonster': 'Creates a new monster and selects it for modding. Optional monster number parameter.',
    '#selectmonster': 'Selects an existing monster for modification by name or ID.',
    '#newweapon': 'Creates a new weapon and selects it for modding. Weapon number should be 1000+.',
    '#selectweapon': 'Selects an existing weapon for modification by name or ID.',
    '#newarmor': 'Creates a new armor and selects it for modding. Armor number should be 400+.',
    '#selectarmor': 'Selects an existing armor for modification by name or ID.',
    '#newspell': 'Creates a new spell and selects it for modding.',
    '#selectspell': 'Selects an existing spell for modification by name or ID.',
    '#newitem': 'Creates a new magic item and selects it for modding.',
    '#selectitem': 'Selects an existing magic item for modification by name or ID.',
    '#newsite': 'Creates a new magic site and selects it for modding.',
    '#selectsite': 'Selects an existing magic site for modification by name or ID.',
    '#newnation': 'Creates a new nation and selects it for modding.',
    '#selectnation': 'Selects an existing nation for modification by name or ID.',
    '#newevent': 'Creates a new event and selects it for modding.',
    '#newmerc': 'Creates a new mercenary unit.',
    '#end': 'Ends the current entity definition block.',
    '#name': 'Sets the name of the entity. Must be first command after new/select.',
    '#descr': 'Sets the description text for the entity.',
    '#clear': 'Clears all properties from the selected entity.',
    '#modname': 'Sets the mod name displayed in preferences.',
    '#description': 'Sets the mod description text.',
    '#icon': 'Sets the mod icon image (128x32 or 256x64 TGA).',
    '#version': 'Sets the mod version number.',
    '#domversion': 'Sets minimum required Dominions version.',
    '#copystats': 'Copies all stats from another monster.',
    '#copyweapon': 'Copies all stats from another weapon.',
    '#copyarmor': 'Copies all stats from another armor.',
    '#copyspell': 'Copies all stats from another spell.',
    '#copyitem': 'Copies all stats from another item.',
    '#spr1': 'Sets the normal sprite image file for a monster.',
    '#spr2': 'Sets the attack sprite image file for a monster.',
    '#gcost': 'Sets the gold cost for recruitment.',
    '#rcost': 'Sets the resource cost.',
    '#hp': 'Sets hit points.',
    '#str': 'Sets strength.',
    '#att': 'Sets attack skill.',
    '#def': 'Sets defense skill.',
    '#prec': 'Sets precision.',
    '#prot': 'Sets natural protection.',
    '#mr': 'Sets magic resistance.',
    '#mor': 'Sets morale.',
    '#enc': 'Sets encumbrance.',
    '#ap': 'Sets action points (movement in combat).',
    '#mapmove': 'Sets strategic map movement.',
    '#size': 'Sets unit size (1-6).',
    '#weapon': 'Assigns a weapon to the monster.',
    '#armor': 'Assigns armor to the monster.',
    '#dmg': 'Sets weapon damage.',
    '#nratt': 'Sets number of attacks.',
    '#range': 'Sets weapon range or spell range.',
    '#ammo': 'Sets ammunition count for ranged weapons.',
    '#effect': 'Sets the spell effect number.',
    '#damage': 'Sets spell damage.',
    '#fatiguecost': 'Sets spell fatigue cost.',
    '#school': 'Sets spell research school.',
    '#researchlevel': 'Sets spell research level.',
    '#path': 'Sets magic path requirement.',
    '#pathlevel': 'Sets required magic path level.',
    '#aoe': 'Sets area of effect.',
    '#gemcost': 'Sets gem cost to forge item.',
    '#mainpath': 'Sets main magic path for item.',
    '#constlevel': 'Sets construction level for item.',
    '#era': 'Sets nation era (1=Early, 2=Middle, 3=Late).',
    '#startsite': 'Sets starting magic site for nation.',
    '#rarity': 'Sets rarity for sites/events.',
    '#gold': 'Sets gold income/cost.',
    '#gems': 'Sets gem income.',
    '#homemon': 'Adds recruitable monster to site.',
    '#homecom': 'Adds recruitable commander to site.',
}

# Argument standardization
ARG_STANDARDIZATION = {
    '<monster nbr>': '<monster_id>',
    '<weapon nbr>': '<weapon_id>',
    '<armor nbr>': '<armor_id>',
    '<spell nbr>': '<spell_id>',
    '<item nbr>': '<item_id>',
    '<site nbr>': '<site_id>',
    '<nation nbr>': '<nation_id>',
    '<nbr>': '<id>',
    '<number>': '<value>',
    '<percent>': '<percent>',
    '"<name>"': '<"name">',
    '"<monster name>"': '<"monster_name">',
    '"<weapon name>"': '<"weapon_name">',
    '"<armor name>"': '<"armor_name">',
    '"<imgfile>"': '<"image_path">',
    '"<filename>"': '<"file_path">',
    '<0 or 1>': '<0|1>',
    '<0 - 1>': '<0|1>',
    '<1 - 2>': '<1-2>',
    '<1 - 3>': '<1-3>',
}


def clean_description(desc: str, cmd_name: str) -> str:
    """Clean up a command description."""
    if not desc:
        return ""

    # Check for manual override first
    if cmd_name in MANUAL_DESCRIPTIONS:
        return MANUAL_DESCRIPTIONS[cmd_name]

    # Remove garbage patterns
    cleaned = desc
    for pattern in GARBAGE_PATTERNS:
        cleaned = re.sub(pattern, '', cleaned)

    # Remove leading/trailing garbage
    cleaned = re.sub(r'^[\s\d\.\-\|]+', '', cleaned)
    cleaned = re.sub(r'[\s\d\.\-\|]+$', '', cleaned)

    # Remove embedded command references that look like garbage
    cleaned = re.sub(r'#[a-z_]+\s+"[^"]*"', '', cleaned)

    # Clean up whitespace
    cleaned = re.sub(r'\s+', ' ', cleaned).strip()

    # Capitalize first letter
    if cleaned and cleaned[0].islower():
        cleaned = cleaned[0].upper() + cleaned[1:]

    # Ensure ends with period if it's a sentence
    if cleaned and len(cleaned) > 10 and not cleaned.endswith('.'):
        cleaned += '.'

    # Truncate if too long
    if len(cleaned) > 200:
        # Try to cut at sentence boundary
        sentences = cleaned.split('.')
        if len(sentences) > 1:
            cleaned = sentences[0] + '.'
        else:
            cleaned = cleaned[:197] + '...'

    return cleaned


def standardize_args(args: str) -> str:
    """Standardize argument format."""
    if not args:
        return ""

    result = args
    for old, new in ARG_STANDARDIZATION.items():
        result = result.replace(old, new)

    # Clean up pipe separators
    result = re.sub(r'\s*\|\s*', ' | ', result)

    return result.strip()


def classify_entity_types(cmd_name: str, current_types: list, desc: str) -> list:
    """Determine correct entity types for a command."""
    # Remove the # prefix for lookup
    cmd_key = cmd_name[1:].lower() if cmd_name.startswith('#') else cmd_name.lower()

    # Check direct mapping first
    if cmd_key in COMMAND_ENTITY_MAP:
        return COMMAND_ENTITY_MAP[cmd_key]

    # Check multi-entity mapping
    if cmd_key in MULTI_ENTITY_COMMANDS:
        return MULTI_ENTITY_COMMANDS[cmd_key]

    # Infer from command name patterns
    if cmd_key.startswith('req_'):
        return ['event']
    if cmd_key.startswith('dt_') or cmd_key.endswith('ifhit') or cmd_key.endswith('ifdmg'):
        return ['weapon']
    if cmd_key.startswith('spell') or cmd_key.endswith('spell'):
        return ['spell', 'item']
    if cmd_key.startswith('magic') or cmd_key.endswith('magic'):
        return ['monster']
    if cmd_key.startswith('fire') or cmd_key.startswith('cold') or cmd_key.startswith('shock') or cmd_key.startswith('poison'):
        return ['monster', 'item']
    if 'res' in cmd_key and len(cmd_key) < 12:  # resistance commands
        return ['monster', 'item']
    if cmd_key.startswith('add') or cmd_key.startswith('remove'):
        return ['nation', 'site']
    if cmd_key.startswith('home'):
        return ['site', 'nation']
    if 'unit' in cmd_key or 'com' in cmd_key:
        return ['nation', 'site']
    if 'god' in cmd_key or 'pretender' in cmd_key:
        return ['nation']
    if 'bless' in cmd_key:
        return ['nation', 'bless']

    # Check description for hints
    desc_lower = desc.lower() if desc else ""
    if 'monster' in desc_lower or 'unit' in desc_lower:
        return ['monster']
    if 'weapon' in desc_lower:
        return ['weapon']
    if 'spell' in desc_lower:
        return ['spell']
    if 'item' in desc_lower:
        return ['item']
    if 'site' in desc_lower:
        return ['site']
    if 'nation' in desc_lower:
        return ['nation']

    # Keep current if not unknown
    if current_types and current_types != ['unknown']:
        return current_types

    return ['unknown']


def get_arg_types(args: str) -> list:
    """Determine argument types from argument string."""
    if not args:
        return []

    types = []

    if '<"' in args or '"' in args:
        types.append('string')
    if 'path' in args.lower():
        types.append('filepath')
    if re.search(r'<\d+[-|]\d+>', args):
        types.append('integer')
    if re.search(r'<\w*id>', args.lower()):
        types.append('id')
    if re.search(r'<value>|<\w*nbr>|<percent>|<level>|<size>', args.lower()):
        types.append('integer')
    if 'bitmask' in args.lower():
        types.append('bitmask')

    if not types:
        types.append('integer')  # Default

    return list(set(types))


def clean_commands(input_path: str, output_path: str):
    """Main cleaning function."""
    print(f"Loading commands from {input_path}")

    with open(input_path, 'r', encoding='utf-8') as f:
        commands = json.load(f)

    print(f"Loaded {len(commands)} commands")

    cleaned = []
    seen = set()

    for cmd in commands:
        name = cmd['name'].lower()

        # Skip duplicates
        if name in seen:
            continue
        seen.add(name)

        # Clean description
        desc = clean_description(cmd.get('description', ''), name)

        # Standardize arguments
        args = standardize_args(cmd.get('args', ''))

        # Classify entity types
        entity_types = classify_entity_types(
            name,
            cmd.get('entity_types', []),
            desc
        )

        # Get argument types
        arg_types = get_arg_types(args)

        cleaned.append({
            'name': name,
            'args': args,
            'description': desc,
            'entity_types': sorted(entity_types),
            'arg_types': sorted(arg_types),
            'page': cmd.get('page', 0)
        })

    # Sort by name
    cleaned.sort(key=lambda x: x['name'])

    print(f"Cleaned to {len(cleaned)} unique commands")

    # Save cleaned data
    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(cleaned, f, indent=2)

    print(f"Saved to {output_path}")

    # Generate summary
    entity_counts = {}
    for cmd in cleaned:
        for et in cmd['entity_types']:
            entity_counts[et] = entity_counts.get(et, 0) + 1

    print("\nEntity type distribution:")
    for et, count in sorted(entity_counts.items()):
        print(f"  {et}: {count}")

    # Count commands with good descriptions
    with_desc = sum(1 for c in cleaned if len(c['description']) > 10)
    print(f"\nCommands with descriptions: {with_desc}/{len(cleaned)}")

    return cleaned


def generate_organized_output(cleaned: list, output_dir: Path):
    """Generate organized output files."""

    # By entity type
    by_entity = {}
    for cmd in cleaned:
        for et in cmd['entity_types']:
            if et not in by_entity:
                by_entity[et] = []
            by_entity[et].append(cmd)

    # Save by-entity JSON
    by_entity_path = output_dir / "commands_by_entity_clean.json"
    with open(by_entity_path, 'w', encoding='utf-8') as f:
        json.dump(by_entity, f, indent=2)
    print(f"Saved entity grouping to {by_entity_path}")

    # Generate clean markdown
    md_lines = [
        "# Dominions 6 Command Reference (Cleaned)",
        "",
        "Auto-generated and cleaned from dom6modman.pdf",
        "",
        f"Total unique commands: {len(cleaned)}",
        "",
    ]

    for entity, cmds in sorted(by_entity.items()):
        md_lines.append(f"## {entity.title()} Commands ({len(cmds)})")
        md_lines.append("")

        for cmd in sorted(cmds, key=lambda x: x['name']):
            md_lines.append(f"### `{cmd['name']}`")
            if cmd['args']:
                md_lines.append(f"**Arguments:** `{cmd['args']}`")
            if cmd['description']:
                md_lines.append(f"\n{cmd['description']}")
            md_lines.append("")

    md_path = output_dir / "command_reference_clean.md"
    with open(md_path, 'w', encoding='utf-8') as f:
        f.write('\n'.join(md_lines))
    print(f"Saved markdown to {md_path}")


def extract_existing_commands(code_dir: Path) -> dict:
    """Extract existing command definitions from Command.cs and entity files."""
    existing = {
        'commands': {},  # command name -> enum value
        'property_types': {},  # command name -> property type used
    }

    # Parse Command.cs for enum values
    command_cs = code_dir / "Dom5Edit/Commands/Command.cs"
    if command_cs.exists():
        with open(command_cs, 'r', encoding='utf-8') as f:
            content = f.read()

        # Find enum values
        enum_pattern = r'^\s*(\w+)\s*,?\s*(?://.*)?$'
        in_enum = False
        for line in content.split('\n'):
            if 'public enum Command' in line:
                in_enum = True
                continue
            if in_enum:
                if line.strip().startswith('}'):
                    in_enum = False
                    continue
                match = re.match(enum_pattern, line)
                if match:
                    enum_name = match.group(1)
                    cmd_name = '#' + enum_name.lower()
                    existing['commands'][cmd_name] = enum_name

        # Find command string mappings
        map_pattern = r'_commandMap\.Add\s*\(\s*"(#\w+)"\s*,\s*Command\.(\w+)\s*\)'
        for match in re.finditer(map_pattern, content):
            cmd_str = match.group(1).lower()
            existing['commands'][cmd_str] = match.group(2)

    # Parse entity files for property type mappings
    entity_files = [
        "Monster.cs", "Weapon.cs", "Armor.cs", "Spell.cs",
        "Item.cs", "Site.cs", "Nation.cs", "Event.cs"
    ]

    property_pattern = r'_propertyMap\.Add\s*\(\s*Command\.(\w+)\s*,\s*(\w+)\.Create\s*\)'

    for entity_file in entity_files:
        filepath = code_dir / f"Dom5Edit/Entities/{entity_file}"
        if filepath.exists():
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()

            entity_name = entity_file.replace('.cs', '').lower()

            for match in re.finditer(property_pattern, content):
                cmd_enum = match.group(1)
                prop_type = match.group(2)
                cmd_name = '#' + cmd_enum.lower()

                if cmd_name not in existing['property_types']:
                    existing['property_types'][cmd_name] = {}
                existing['property_types'][cmd_name][entity_name] = prop_type

    return existing


def compare_with_existing(cleaned: list, existing: dict, output_path: Path):
    """Compare cleaned commands with existing code and generate conflict report."""
    conflicts = []
    missing_in_code = []
    missing_in_pdf = []

    cleaned_names = {c['name'] for c in cleaned}
    existing_names = set(existing['commands'].keys())

    # Commands in PDF but not in code
    for cmd in cleaned:
        if cmd['name'] not in existing_names:
            missing_in_code.append({
                'command': cmd['name'],
                'args': cmd['args'],
                'entity_types': cmd['entity_types'],
                'description': cmd['description'][:100] if cmd['description'] else ''
            })

    # Commands in code but not in PDF (might be valid - internal commands)
    for cmd_name in existing_names:
        if cmd_name not in cleaned_names:
            missing_in_pdf.append({
                'command': cmd_name,
                'enum': existing['commands'][cmd_name]
            })

    # Check for type conflicts
    for cmd in cleaned:
        if cmd['name'] in existing['property_types']:
            prop_types = existing['property_types'][cmd['name']]

            # Check if PDF says string but code uses IntProperty, etc.
            pdf_expects_string = 'string' in cmd['arg_types']
            pdf_expects_int = 'integer' in cmd['arg_types'] or 'id' in cmd['arg_types']

            for entity, prop_type in prop_types.items():
                code_is_string = 'String' in prop_type or 'Name' in prop_type
                code_is_int = 'Int' in prop_type and 'Int' not in ['IntInt']
                code_is_ref = 'Ref' in prop_type

                # Detect conflicts
                if pdf_expects_string and code_is_int and not code_is_ref:
                    conflicts.append({
                        'command': cmd['name'],
                        'entity': entity,
                        'issue': f'PDF expects string, code uses {prop_type}',
                        'pdf_args': cmd['args'],
                        'code_type': prop_type
                    })
                elif pdf_expects_int and code_is_string:
                    conflicts.append({
                        'command': cmd['name'],
                        'entity': entity,
                        'issue': f'PDF expects integer, code uses {prop_type}',
                        'pdf_args': cmd['args'],
                        'code_type': prop_type
                    })

    # Generate report
    report_lines = [
        "# Command Comparison Report",
        "",
        f"Generated comparing PDF extraction with existing code.",
        "",
        f"## Summary",
        f"- Commands in PDF: {len(cleaned)}",
        f"- Commands in code: {len(existing_names)}",
        f"- Missing from code: {len(missing_in_code)}",
        f"- Missing from PDF: {len(missing_in_pdf)}",
        f"- Type conflicts: {len(conflicts)}",
        "",
    ]

    if conflicts:
        report_lines.append("## Type Conflicts (REVIEW NEEDED)")
        report_lines.append("")
        report_lines.append("These commands may have argument type mismatches:")
        report_lines.append("")
        for c in conflicts:
            report_lines.append(f"### `{c['command']}` in {c['entity']}")
            report_lines.append(f"- Issue: {c['issue']}")
            report_lines.append(f"- PDF args: `{c['pdf_args']}`")
            report_lines.append(f"- Code type: `{c['code_type']}`")
            report_lines.append("")

    if missing_in_code:
        report_lines.append("## Commands Missing from Code")
        report_lines.append("")
        report_lines.append("These commands are in the PDF but not found in Command.cs:")
        report_lines.append("")
        # Group by entity type
        by_entity = {}
        for m in missing_in_code:
            for et in m['entity_types']:
                if et not in by_entity:
                    by_entity[et] = []
                by_entity[et].append(m)

        for entity, cmds in sorted(by_entity.items()):
            report_lines.append(f"### {entity.title()}")
            for c in sorted(cmds, key=lambda x: x['command']):
                report_lines.append(f"- `{c['command']}` {c['args']}")
            report_lines.append("")

    if missing_in_pdf:
        report_lines.append("## Commands in Code but not PDF")
        report_lines.append("")
        report_lines.append("These may be internal commands or typos:")
        report_lines.append("")
        for m in sorted(missing_in_pdf, key=lambda x: x['command']):
            report_lines.append(f"- `{m['command']}` (enum: {m['enum']})")

    # Save report
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write('\n'.join(report_lines))

    print(f"\nComparison report saved to {output_path}")
    print(f"  Type conflicts: {len(conflicts)}")
    print(f"  Missing from code: {len(missing_in_code)}")
    print(f"  Missing from PDF: {len(missing_in_pdf)}")

    # Also save as JSON for programmatic access
    json_path = output_path.with_suffix('.json')
    with open(json_path, 'w', encoding='utf-8') as f:
        json.dump({
            'conflicts': conflicts,
            'missing_in_code': missing_in_code,
            'missing_in_pdf': missing_in_pdf
        }, f, indent=2)


def main():
    script_dir = Path(__file__).parent.parent
    input_path = script_dir / "docs/pdf_extracted/commands.json"
    output_path = script_dir / "docs/pdf_extracted/commands_clean.json"
    output_dir = script_dir / "docs/pdf_extracted"

    cleaned = clean_commands(str(input_path), str(output_path))
    generate_organized_output(cleaned, output_dir)

    # Compare with existing code
    print("\nComparing with existing code...")
    existing = extract_existing_commands(script_dir)
    print(f"Found {len(existing['commands'])} commands in code")
    print(f"Found property mappings for {len(existing['property_types'])} commands")

    compare_with_existing(
        cleaned,
        existing,
        output_dir / "command_comparison_report.md"
    )

    print("\nCleaning complete!")


if __name__ == '__main__':
    main()
