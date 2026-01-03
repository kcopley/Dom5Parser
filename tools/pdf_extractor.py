#!/usr/bin/env python3
"""
PDF Text Extractor for Dominions 6 Modding Manual

Extracts text from dom6modman.pdf and saves it to organized text files
for analysis without overwhelming conversation context.

Usage:
    python pdf_extractor.py [--input PATH] [--output-dir PATH]
"""

import argparse
import os
import re
from pathlib import Path

try:
    import pdfplumber
    PDF_LIBRARY = 'pdfplumber'
except ImportError:
    try:
        import PyPDF2
        PDF_LIBRARY = 'PyPDF2'
    except ImportError:
        PDF_LIBRARY = None


def extract_with_pdfplumber(pdf_path: str) -> list[tuple[int, str]]:
    """Extract text using pdfplumber (better formatting)."""
    pages = []
    with pdfplumber.open(pdf_path) as pdf:
        for i, page in enumerate(pdf.pages):
            text = page.extract_text() or ""
            pages.append((i + 1, text))
    return pages


def extract_with_pypdf2(pdf_path: str) -> list[tuple[int, str]]:
    """Extract text using PyPDF2 (fallback)."""
    pages = []
    with open(pdf_path, 'rb') as f:
        reader = PyPDF2.PdfReader(f)
        for i, page in enumerate(reader.pages):
            text = page.extract_text() or ""
            pages.append((i + 1, text))
    return pages


def extract_pdf(pdf_path: str) -> list[tuple[int, str]]:
    """Extract text from PDF using available library."""
    if PDF_LIBRARY == 'pdfplumber':
        return extract_with_pdfplumber(pdf_path)
    elif PDF_LIBRARY == 'PyPDF2':
        return extract_with_pypdf2(pdf_path)
    else:
        raise ImportError("No PDF library available. Install pdfplumber or PyPDF2:\n"
                          "  pip install pdfplumber\n"
                          "  pip install PyPDF2")


def identify_sections(pages: list[tuple[int, str]]) -> dict[str, list[tuple[int, str]]]:
    """
    Attempt to identify major sections in the document.
    Returns a dict mapping section names to their page content.
    """
    sections = {}
    current_section = "Introduction"
    current_pages = []

    # Common section headers in modding manuals
    section_patterns = [
        r'^(?:\d+\.?\s+)?(Monster|Monsters)\s*$',
        r'^(?:\d+\.?\s+)?(Weapon|Weapons)\s*$',
        r'^(?:\d+\.?\s+)?(Armor|Armors)\s*$',
        r'^(?:\d+\.?\s+)?(Spell|Spells)\s*$',
        r'^(?:\d+\.?\s+)?(Item|Items)\s*$',
        r'^(?:\d+\.?\s+)?(Site|Sites)\s*$',
        r'^(?:\d+\.?\s+)?(Nation|Nations)\s*$',
        r'^(?:\d+\.?\s+)?(Event|Events)\s*$',
        r'^(?:\d+\.?\s+)?(Mercenary|Mercenaries)\s*$',
        r'^(?:\d+\.?\s+)?Modding Commands?\s*$',
        r'^(?:\d+\.?\s+)?Command Reference\s*$',
        r'^(?:\d+\.?\s+)?Appendix',
    ]

    for page_num, text in pages:
        lines = text.split('\n')

        # Check first few lines for section headers
        for line in lines[:5]:
            line = line.strip()
            for pattern in section_patterns:
                if re.match(pattern, line, re.IGNORECASE):
                    # Save previous section
                    if current_pages:
                        sections[current_section] = current_pages
                    current_section = line.strip()
                    current_pages = []
                    break

        current_pages.append((page_num, text))

    # Save final section
    if current_pages:
        sections[current_section] = current_pages

    return sections


def extract_commands_from_text(text: str) -> list[dict]:
    """
    Extract command definitions from text.
    Looks for patterns like: #commandname <arg> or #commandname
    """
    commands = []

    # Pattern for command definitions
    # Matches: #command, #command <arg>, #command <arg1> <arg2>
    pattern = r'(#\w+)(?:\s+(<[^>]+>(?:\s+<[^>]+>)*))?'

    for match in re.finditer(pattern, text):
        cmd_name = match.group(1)
        cmd_args = match.group(2) or ""

        # Try to capture description (text following on same/next line)
        start = match.end()
        end = min(start + 200, len(text))
        context = text[start:end].split('\n')[0].strip()

        commands.append({
            'command': cmd_name,
            'args': cmd_args.strip(),
            'context': context[:100] if context else ""
        })

    return commands


def save_pages_to_file(pages: list[tuple[int, str]], output_path: str):
    """Save extracted pages to a text file."""
    with open(output_path, 'w', encoding='utf-8') as f:
        for page_num, text in pages:
            f.write(f"\n{'='*60}\n")
            f.write(f"PAGE {page_num}\n")
            f.write(f"{'='*60}\n\n")
            f.write(text)
            f.write("\n")


def save_commands_to_file(commands: list[dict], output_path: str):
    """Save extracted commands to a structured file."""
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write("# Extracted Commands from dom6modman.pdf\n\n")

        seen = set()
        for cmd in commands:
            cmd_key = cmd['command']
            if cmd_key not in seen:
                seen.add(cmd_key)
                f.write(f"{cmd['command']}")
                if cmd['args']:
                    f.write(f" {cmd['args']}")
                f.write("\n")
                if cmd['context']:
                    f.write(f"  // {cmd['context']}\n")
                f.write("\n")


def main():
    parser = argparse.ArgumentParser(description='Extract text from Dominions 6 modding manual PDF')
    parser.add_argument('--input', '-i', default='dom6modman.pdf',
                        help='Path to input PDF file')
    parser.add_argument('--output-dir', '-o', default='docs/pdf_extracted',
                        help='Output directory for extracted text')
    parser.add_argument('--chunk-size', '-c', type=int, default=10,
                        help='Number of pages per chunk file')
    args = parser.parse_args()

    # Resolve paths
    script_dir = Path(__file__).parent.parent
    pdf_path = Path(args.input)
    if not pdf_path.is_absolute():
        pdf_path = script_dir / pdf_path

    output_dir = Path(args.output_dir)
    if not output_dir.is_absolute():
        output_dir = script_dir / output_dir

    # Check PDF exists
    if not pdf_path.exists():
        print(f"Error: PDF not found at {pdf_path}")
        return 1

    print(f"Using PDF library: {PDF_LIBRARY}")
    print(f"Extracting from: {pdf_path}")
    print(f"Output directory: {output_dir}")

    # Create output directory
    output_dir.mkdir(parents=True, exist_ok=True)

    # Extract text
    print("\nExtracting text from PDF...")
    pages = extract_pdf(str(pdf_path))
    print(f"Extracted {len(pages)} pages")

    # Save full text
    full_output = output_dir / "full_text.txt"
    save_pages_to_file(pages, str(full_output))
    print(f"Saved full text to: {full_output}")

    # Save chunked files
    chunks_dir = output_dir / "chunks"
    chunks_dir.mkdir(exist_ok=True)

    for i in range(0, len(pages), args.chunk_size):
        chunk_pages = pages[i:i + args.chunk_size]
        start_page = chunk_pages[0][0]
        end_page = chunk_pages[-1][0]
        chunk_file = chunks_dir / f"pages_{start_page:03d}-{end_page:03d}.txt"
        save_pages_to_file(chunk_pages, str(chunk_file))

    print(f"Saved {(len(pages) + args.chunk_size - 1) // args.chunk_size} chunk files to: {chunks_dir}")

    # Extract and save commands
    print("\nExtracting command references...")
    all_commands = []
    for page_num, text in pages:
        commands = extract_commands_from_text(text)
        all_commands.extend(commands)

    commands_file = output_dir / "commands_extracted.txt"
    save_commands_to_file(all_commands, str(commands_file))
    print(f"Extracted {len(set(c['command'] for c in all_commands))} unique commands")
    print(f"Saved commands to: {commands_file}")

    # Try to identify sections
    print("\nIdentifying document sections...")
    sections = identify_sections(pages)

    sections_dir = output_dir / "sections"
    sections_dir.mkdir(exist_ok=True)

    for section_name, section_pages in sections.items():
        # Clean filename
        clean_name = re.sub(r'[^\w\s-]', '', section_name).strip()
        clean_name = re.sub(r'\s+', '_', clean_name)[:50]
        section_file = sections_dir / f"{clean_name}.txt"
        save_pages_to_file(section_pages, str(section_file))

    print(f"Identified {len(sections)} sections:")
    for name, pages_list in sections.items():
        print(f"  - {name}: {len(pages_list)} pages")

    print("\nExtraction complete!")
    print(f"\nOutput files:")
    print(f"  {full_output}")
    print(f"  {commands_file}")
    print(f"  {chunks_dir}/")
    print(f"  {sections_dir}/")

    return 0


if __name__ == '__main__':
    exit(main())
