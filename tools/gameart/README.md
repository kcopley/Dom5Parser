# Game art (.trs archives)

Dominions 6 keeps its sprites and icons in `data/*.trs` archives. `trs.py` reads them;
`Dom5Editor/Sprites/TrsArchive.cs` is the same decoder in C#, and `GameArt.cs` uses it to show
the game's own icons in the editor, read at run time from the user's install.

The game's art is not ours to distribute. Nothing extracted from it is committed:
`extract` and `sheet` refuse to write inside the repository. The repository holds only
`Dom5Editor/Data/game_icons.json`, which says which archive and index each icon is.

## Commands

```
python3 tools/gameart/trs.py list ARCHIVE                     # index, size, flags, encoding, offset, length, group
python3 tools/gameart/trs.py extract ARCHIVE OUTDIR [--only 3,10-20]   # one PNG per image
python3 tools/gameart/trs.py sheet ARCHIVE OUT.png [--only 0-99] [--zoom 2]  # numbered contact sheet
```

`ARCHIVE` is a path or a bare name (`res.trs`) looked up in `--data DIR`, `$DOM6_DATA`, or the
default Steam folders. Python 3.8+, standard library only (PNGs are written with zlib).

## Format

All numbers are big-endian.

Header:

| offset | size | field |
|---|---|---|
| 0 | 4 | `TCSF` (the game checks the first three bytes) |
| 4 | 2 | image count |
| 6 | 2 | version: 3 or 4 in 6.37 (2+ has the 12-byte records below; older ones 10-byte records from 0x0A without flags) |
| 8 | 2 | 800: the surface width version-3 run-length images are encoded against |
| 10 | 2 | 0 |

Then one 12-byte record per image, from 0x0C:

| offset | size | field |
|---|---|---|
| 0 | 1 | width (0 means 256) |
| 1 | 1 | height (0 means 256) |
| 2 | 2 | flags: 1 = the game draws it at half size (a 64x64 icon shown as 32x32); 2 = tested when the game counts a run of images, unused in 6.37; 4 = pixels carry alpha (also stated in the image's own header) |
| 4 | 4 | offset of raw pixels, or 0 |
| 8 | 4 | offset of run-length data, or 0 |

After the records: group labels, pairs of (u32 first image index, NUL-terminated name), ended by
`FF FF FF FF` (`sites.trs`: fire, air, ...; `monster.trs`: nation art; most archives have none).
Image data follows; each image runs to the next one's offset.

Encodings (the game picks by which offset is set, then by version):

- **raw** (raw offset set; item, flag, build): width x height RGB565 pixels, rows top down.
- **rle16** (version 3; monster, sites, mapstuff, tree): u16 runs-1, then per run u16 skip,
  u16 count-1, count RGB565 pixels. The skip is in bytes on an 800-pixel-wide surface (pixels =
  skip / 2; row = position / 800, column = position % 800). A count-1 of 0xFFFF is no pixels: a
  skip longer than 0xFFFF bytes is split that way. Skipped pixels are transparent.
- **rle8** (version 4; misc, misc2, res, blast): u16 mode, u16 runs, then per run a skip, a
  count, and count pixels. Skip and count are one byte, or 0xFF followed by a 24-bit value.
  Positions run across rows of the image's own width. Mode 0: RGB565 pixels. Mode 1: RGB565 plus
  an alpha byte (3 bytes per pixel; index flag 4); these are one run covering the image.

Pixels, converted as the game converts them: R = v>>8 & 0xF8, G = v>>3 & 0xFC, B = v<<3 & 0xF8.
Without an alpha byte, 0x0000 is transparent and magenta 0xF81F is a shadow drawn as black at
alpha 0x80 (the dark patch under units).

## How this was verified

- Every image in every archive decodes, and each run-length image consumes exactly its bytes.
- Contact sheets of all of misc, misc2 and res, and samples of the others, look right (unit
  sprites, items, flags, buildings, terrain, icons).
- The layout, encodings, flags and pixel conversion were checked against the game's loader in
  `Dominions6.exe` 6.37 (record reader, the three decoders and the pixel conversion).
- `TrsArchive.cs` and `trs.py` give byte-identical pixels for samples of every encoding.

## The exe

`Dominions6.exe` embeds no game art: its resources are the window icon and a manifest, and the
data section holds tables and strings, no image data. It holds the list of archive names
(guistuff, mapstuff, item, blast, tree, build, ship, debris, flag, bullets, res, army..army15,
mineral, misc, misc2, unit1, trade1, weapon, terrain, monster, ground, walls, sites, rituals;
only some exist as files) and the code that picks which image to draw. Draw calls name an
archive by its place in that list (res.trs = 10, misc.trs = 27) and an image index, which is how
`game_icons.json` was built.

## game_icons.json

`{"version": "6.37", "icons": {key: {"archive", "index"}}}`. Each entry was found in the exe and
checked on a contact sheet:

- Unit-window stats (res.trs): the unit window draws res 381-395 next to "Hit points", "Size",
  "Protection", "Magic Resistance", "Morale", "Strength", "Attack Skill", "Defence Skill",
  "Precision", "Combat Speed", "Map Move", "Encumbrance", "Fatigue", "Age", "XP". Leadership is
  396 + a level 0-4 (396 is the plain banner; 397-400 add stars); "Undead Ldr" 306, "Magic Ldr" 307.
- Costs (misc.trs): 160 "Gold", 161 "Resources", 162 recruitment points ("Limited recruitment").
- Magic paths (misc.trs): 98 + path (F A W E S D N G B H); 108 "random magic skill".
- Gems (misc.trs): 85 + gem (F A W E S D N G, 93 blood slaves).
- Weapon damage types (res.trs, the weapon window): slash 131, blunt 132, pierce 133, fire 134,
  cold 135, shock 136, magic 137, poison 139, acid 140, salt 345.
- Abilities (res.trs), keyed by the .dm command: the ability window draws res.trs icon N and then
  chooses its title and text by N ("Flying" for 28, "Fire Resistant (%d)" for 1, ...); where the
  window reads a numbered ability, its number matches the command's (fireres 198 for icon 1).

To find more: `trs.py sheet res.trs /tmp/res.png --zoom 2` and the game's ability window titles.
