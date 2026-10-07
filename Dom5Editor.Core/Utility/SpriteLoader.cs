using System.IO;
using Dom5Editor.Imaging;
using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Edit.Resolve;

namespace Dom5Editor.Sprites
{
    /// <summary>
    /// Loads a sprite (TGA, PNG, ...) for display: a mod's path is relative to the mod's folder. A
    /// vanilla entity's picture is a supplied file (the icons folder) or, without one, the game's
    /// own from the player's install ("game:monster.trs/24036", Dom5Edit.GameSprite).
    /// </summary>
    public static class SpriteLoader
    {
        // loaded files, kept while something shows them; null: missing or unreadable
        private static readonly Dictionary<string, WeakReference<Picture>?> _cache = new Dictionary<string, WeakReference<Picture>?>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// A list row's picture: the sprite cut to its visible pixels (a unit fills the row instead
        /// of a corner of its 64x64 canvas), as a small copy of its own, so a list doesn't keep every
        /// full-size sprite it has shown. Null for null.
        /// </summary>
        public static Picture? Thumbnail(Picture? image)
        {
            if (image == null)
                return null;
            try
            {
                int w = image.Width, h = image.Height;
                var pixels = image.Bgra;
                int x0 = w, y0 = h, x1 = -1, y1 = -1;
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        if (pixels[(y * w + x) * 4 + 3] != 0)
                        {
                            x0 = Math.Min(x0, x); x1 = Math.Max(x1, x);
                            y0 = Math.Min(y0, y); y1 = Math.Max(y1, y);
                        }
                if (x1 < 0 || (x0 == 0 && y0 == 0 && x1 == w - 1 && y1 == h - 1))
                    return image; // nothing shown, or nothing to cut (a site's picture)
                int cw = x1 - x0 + 1, ch = y1 - y0 + 1;
                var cut = new byte[cw * ch * 4];
                for (int y = 0; y < ch; y++)
                    Buffer.BlockCopy(pixels, ((y0 + y) * w + x0) * 4, cut, y * cw * 4, cw * 4);
                return new Picture(cw, ch, cut, image.Dpi);
            }
            catch (Exception)
            {
                return image;
            }
        }

        /// <summary>
        /// The picture an entity shows: a monster's #spr1 or an item's #spr (the mod's file; else the
        /// vanilla one it has, also through #copyspr), a site's from its #path, #level and #look, a
        /// nation's flag (its #flag file, else the one the game makes from its colors).
        /// Null if it has none or the file or the game isn't there.
        /// </summary>
        public static Picture? Of(ResolvedEntity r, EntityType type, string? modFile)
        {
            switch (type)
            {
                case EntityType.MONSTER:
                case EntityType.ITEM:
                    var c = type == EntityType.MONSTER ? Command.SPR1 : Command.SPR;
                    var p = r.Get(c)?.Property ?? r.Assets.GetValueOrDefault(c);
                    return p is FilePathProperty f ? Load(f.Value, modFile) : null;
                case EntityType.SITE:
                    // a vanilla site without #look has -1 (its level picks the picture), a new site 0
                    int Value(Command cmd, int fallback) =>
                        r.Get(cmd) is { } v && int.TryParse(v.Arguments.Split(' ')[0], out int n) ? n : fallback;
                    return GameArt.SitePicture(Value(Command.PATH, 0), Value(Command.LEVEL, 0), Value(Command.LOOK, r.Entity.Selected ? -1 : 0));
                case EntityType.NATION:
                    return r.Get(Command.FLAG)?.Property is FilePathProperty flag && !string.IsNullOrWhiteSpace(flag.Value)
                        ? Load(flag.Value, modFile) : NationFlag(r);
                default:
                    return null;
            }
        }

        /// <summary>
        /// The flag the game makes for a nation that has no #flag file, from its number and its
        /// #color and #secondarycolor as they are after the mod (GameArt.NationFlag): a mod that
        /// recolors a vanilla nation recolors its flag, and a mod's new nation gets a plain one.
        /// A color the nation doesn't have is black, as in the game's empty nation slots.
        /// </summary>
        public static Picture? NationFlag(ResolvedEntity r)
        {
            (float, float, float) Color(Command c) =>
                r.Get(c)?.Property is FloatFloatFloatProperty f && f.HasValue ? (f.Value1, f.Value2, f.Value3) : (0f, 0f, 0f);
            return GameArt.NationFlag(r.Entity.ID, Color(Command.COLOR), Color(Command.SECONDARYCOLOR));
        }

        public static Picture? Load(string? spritePath, string? modFile)
        {
            if (string.IsNullOrWhiteSpace(spritePath))
                return null;
            if (GameSprite.TryParse(spritePath, out var game))
                return GameArt.Sprite(game.Archive, game.Number, game.Frame);
            // relative to the mod's file, either slash (Dom5Edit.Imaging.ModFiles)
            if (Dom5Edit.Imaging.ModFiles.Resolve(spritePath, modFile) is not string path)
                return null;
            if (_cache.TryGetValue(path, out var cached))
            {
                if (cached == null)
                    return null;
                if (cached.TryGetTarget(out var alive))
                    return alive;
            }
            // TGA read here, PNG and the rest by the editor's toolkit (Picture.Load)
            var image = File.Exists(path) ? Picture.Load(path) : null;
            _cache[path] = image != null ? new WeakReference<Picture>(image) : null;
            return image;
        }
    }
}
