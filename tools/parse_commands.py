#!/usr/bin/env python3
"""
Command Parser for Dominions 6 Modding Manual

Parses the extracted PDF text to create a structured command reference
with descriptions, argument types, and entity context.

Usage:
    python parse_commands.py [--input PATH] [--output PATH]
"""

import argparse
import json
import re
from pathlib import Path
from dataclasses import dataclass, asdict
from typing import Optional


@dataclass
class CommandDef:
    """Represents a parsed command definition."""
    name: str
    args: str
    description: str
    entity_types: list[str]
    page: int
    arg_types: list[str]  # integer, string, bitmask, percent, etc.


def parse_arg_types(args: str) -> list[str]:
    """Extract argument types from argument string."""
    if not args:
        return []

    types = []
    # Common patterns
    patterns = [
        (r'<\w*nbr\w*>', 'integer'),
        (r'<\w*number\w*>', 'integer'),
        (r'<\d+\s*-\s*\d+>', 'integer'),  # ranges like <0 - 3>
        (r'<percent\w*>', 'percent'),
        (r'<\w*name\w*>', 'string'),
        (r'<\w*text\w*>', 'string'),
        (r'<\w*file\w*>', 'filepath'),
        (r'<imgfile>', 'filepath'),
        (r'<filename>', 'filepath'),
        (r'"[^"]*"', 'string'),
        (r'<bitmask>', 'bitmask'),
        (r'<value>', 'integer'),
        (r'<\w+>', 'integer'),  # default to integer
    ]

    for pattern, arg_type in patterns:
        if re.search(pattern, args, re.IGNORECASE):
            if arg_type not in types:
                types.append(arg_type)

    return types if types else ['none']


def identify_entity_context(text: str, current_section: str) -> list[str]:
    """Identify which entity types a command applies to based on context."""
    entities = []

    # Section-based inference
    section_lower = current_section.lower()
    if 'monster' in section_lower or 'unit' in section_lower:
        entities.append('monster')
    if 'weapon' in section_lower:
        entities.append('weapon')
    if 'armor' in section_lower:
        entities.append('armor')
    if 'spell' in section_lower:
        entities.append('spell')
    if 'item' in section_lower or 'magic item' in section_lower:
        entities.append('item')
    if 'site' in section_lower:
        entities.append('site')
    if 'nation' in section_lower:
        entities.append('nation')
    if 'event' in section_lower:
        entities.append('event')
    if 'mercenary' in section_lower:
        entities.append('mercenary')
    if 'poptype' in section_lower:
        entities.append('poptype')
    if 'nametype' in section_lower or 'name modding' in section_lower:
        entities.append('nametype')

    # Text-based inference for generic commands
    text_lower = text.lower()
    if 'this monster' in text_lower or 'the monster' in text_lower:
        if 'monster' not in entities:
            entities.append('monster')
    if 'this weapon' in text_lower or 'the weapon' in text_lower:
        if 'weapon' not in entities:
            entities.append('weapon')

    return entities if entities else ['unknown']


def parse_commands_from_text(full_text: str) -> list[CommandDef]:
    """Parse all commands from the full extracted text."""
    commands = []
    seen_commands = {}  # Track best definition for each command

    # Split into pages
    pages = re.split(r'={50,}\nPAGE (\d+)\n={50,}', full_text)

    current_section = "Introduction"
    current_page = 1

    # Section detection patterns
    section_patterns = [
        (r'Weapon Modding', 'Weapon Modding'),
        (r'Armor Modding', 'Armor Modding'),
        (r'Monster Modding', 'Monster Modding'),
        (r'Spell Modding', 'Spell Modding'),
        (r'Magic Item Modding', 'Item Modding'),
        (r'Magic Site Modding', 'Site Modding'),
        (r'Nation Modding', 'Nation Modding'),
        (r'Event Modding', 'Event Modding'),
        (r'Mercenary Modding', 'Mercenary Modding'),
        (r'Poptype Modding', 'Poptype Modding'),
        (r'Name Modding', 'Name Modding'),
        (r'Bless Modding', 'Bless Modding'),
        (r'Sound Modding', 'Sound Modding'),
        (r'General Modding', 'General Modding'),
    ]

    # Process page by page
    i = 0
    while i < len(pages):
        if i + 1 < len(pages) and pages[i].strip().isdigit():
            current_page = int(pages[i])
            text = pages[i + 1] if i + 1 < len(pages) else ""
            i += 2
        else:
            text = pages[i]
            i += 1

        # Detect section changes
        for pattern, section in section_patterns:
            if re.search(pattern, text):
                current_section = section

        # Find all commands in this page
        # Pattern: #commandname followed by optional arguments
        command_pattern = r'(#[a-z_][a-z0-9_]*)\s*(?:(<[^>\n]+(?:\s*\|\s*<[^>\n]+)?>(?:\s+<[^>\n]+>)*)|("[^"]+"))?'

        for match in re.finditer(command_pattern, text, re.IGNORECASE):
            cmd_name = match.group(1).lower()
            cmd_args = (match.group(2) or match.group(3) or "").strip()

            # Get description - text after command until next command or line break
            start_pos = match.end()
            # Look for description in the next ~300 chars
            desc_text = text[start_pos:start_pos + 500]

            # Clean up description
            # Stop at next command
            next_cmd = re.search(r'\n#[a-z]', desc_text)
            if next_cmd:
                desc_text = desc_text[:next_cmd.start()]

            # Clean up
            desc_text = re.sub(r'\s+', ' ', desc_text).strip()
            desc_text = desc_text.split('.')[0] + '.' if '.' in desc_text else desc_text
            desc_text = desc_text[:200]  # Limit length

            # Skip if too short or looks like garbage
            if len(desc_text) < 5:
                desc_text = ""

            # Identify entity types
            entity_types = identify_entity_context(desc_text + " " + current_section, current_section)

            # Parse argument types
            arg_types = parse_arg_types(cmd_args)

            cmd_def = CommandDef(
                name=cmd_name,
                args=cmd_args,
                description=desc_text,
                entity_types=entity_types,
                page=current_page,
                arg_types=arg_types
            )

            # Keep the best definition (longest description)
            if cmd_name not in seen_commands or len(desc_text) > len(seen_commands[cmd_name].description):
                seen_commands[cmd_name] = cmd_def

    return list(seen_commands.values())


def organize_by_entity(commands: list[CommandDef]) -> dict[str, list[CommandDef]]:
    """Organize commands by entity type."""
    by_entity = {}

    for cmd in commands:
        for entity in cmd.entity_types:
            if entity not in by_entity:
                by_entity[entity] = []
            by_entity[entity].append(cmd)

    return by_entity


def generate_markdown(commands: list[CommandDef], by_entity: dict) -> str:
    """Generate a markdown reference document."""
    lines = [
        "# Dominions 6 Command Reference",
        "",
        "Auto-generated from dom6modman.pdf",
        "",
        f"Total commands: {len(commands)}",
        "",
        "## Commands by Entity Type",
        "",
    ]

    for entity, cmds in sorted(by_entity.items()):
        lines.append(f"### {entity.title()} ({len(cmds)} commands)")
        lines.append("")
        lines.append("| Command | Arguments | Description |")
        lines.append("|---------|-----------|-------------|")

        for cmd in sorted(cmds, key=lambda c: c.name):
            args = cmd.args.replace("|", "\\|") if cmd.args else "-"
            desc = cmd.description.replace("|", "\\|")[:80] if cmd.description else "-"
            lines.append(f"| `{cmd.name}` | {args} | {desc} |")

        lines.append("")

    return "\n".join(lines)


def generate_csharp_stubs(commands: list[CommandDef]) -> str:
    """Generate C# enum entries for new commands."""
    lines = [
        "// Auto-generated command stubs from dom6modman.pdf",
        "// Add these to Command.cs if missing",
        "",
    ]

    for cmd in sorted(commands, key=lambda c: c.name):
        enum_name = cmd.name[1:].upper()  # Remove # and uppercase
        comment = cmd.description[:60] if cmd.description else ""
        lines.append(f"// {cmd.name} {cmd.args}")
        if comment:
            lines.append(f"// {comment}")
        lines.append(f"{enum_name},")
        lines.append("")

    return "\n".join(lines)


def main():
    parser = argparse.ArgumentParser(description='Parse commands from extracted PDF text')
    parser.add_argument('--input', '-i', default='docs/pdf_extracted/full_text.txt',
                        help='Path to extracted text file')
    parser.add_argument('--output', '-o', default='docs/pdf_extracted',
                        help='Output directory')
    args = parser.parse_args()

    script_dir = Path(__file__).parent.parent
    input_path = Path(args.input)
    if not input_path.is_absolute():
        input_path = script_dir / input_path

    output_dir = Path(args.output)
    if not output_dir.is_absolute():
        output_dir = script_dir / output_dir

    print(f"Reading from: {input_path}")

    with open(input_path, 'r', encoding='utf-8') as f:
        full_text = f.read()

    print("Parsing commands...")
    commands = parse_commands_from_text(full_text)
    print(f"Found {len(commands)} unique commands")

    by_entity = organize_by_entity(commands)
    print(f"Organized into {len(by_entity)} entity categories:")
    for entity, cmds in sorted(by_entity.items()):
        print(f"  {entity}: {len(cmds)} commands")

    # Save JSON
    json_path = output_dir / "commands.json"
    with open(json_path, 'w', encoding='utf-8') as f:
        json.dump([asdict(c) for c in commands], f, indent=2)
    print(f"Saved JSON to: {json_path}")

    # Save Markdown reference
    md_path = output_dir / "command_reference.md"
    with open(md_path, 'w', encoding='utf-8') as f:
        f.write(generate_markdown(commands, by_entity))
    print(f"Saved Markdown to: {md_path}")

    # Save C# stubs
    cs_path = output_dir / "command_stubs.cs"
    with open(cs_path, 'w', encoding='utf-8') as f:
        f.write(generate_csharp_stubs(commands))
    print(f"Saved C# stubs to: {cs_path}")

    # Save by-entity JSON
    by_entity_json = output_dir / "commands_by_entity.json"
    entity_data = {k: [asdict(c) for c in v] for k, v in by_entity.items()}
    with open(by_entity_json, 'w', encoding='utf-8') as f:
        json.dump(entity_data, f, indent=2)
    print(f"Saved by-entity JSON to: {by_entity_json}")

    print("\nDone!")


if __name__ == '__main__':
    main()
