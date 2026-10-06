using System.Globalization;
using System.Text.Json;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit
{
    /// <summary>
    /// A picture in one of the game's own .trs archives, by the number the game stores for it:
    /// "game:monster.trs/24036" (a monster's sprite), "game:monster.trs/24036+1" (the image after
    /// it: the attack frame). The editor reads the picture from the player's install; the numbers
    /// come from tools/dom6exe sprites. Used as the value of a display asset (#spr1, an item's #spr).
    /// </summary>
    public readonly record struct GameSprite(string Archive, int Number, int Frame = 0)
    {
        public const string Scheme = "game:";

        public override string ToString() => $"{Scheme}{Archive}/{Number}" + (Frame != 0 ? "+" + Frame : "");

        public static bool TryParse(string? text, out GameSprite sprite)
        {
            sprite = default;
            if (text == null || !text.StartsWith(Scheme, StringComparison.Ordinal))
                return false;
            int slash = text.LastIndexOf('/');
            if (slash <= Scheme.Length)
                return false;
            var rest = text.Substring(slash + 1).Split('+');
            if (!int.TryParse(rest[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
                return false;
            int frame = 0;
            if (rest.Length > 1 && !int.TryParse(rest[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out frame))
                return false;
            sprite = new GameSprite(text.Substring(Scheme.Length, slash - Scheme.Length), number, frame);
            return true;
        }
    }

    /// <summary>
    /// How the game draws a nation's flag (tools/dom6exe/flags.py, the "flag" entry of
    /// vanilla-sprites.json): image nation + <see cref="ImageOffset"/> of <see cref="Archive"/>.
    /// Nations below <see cref="FirstComposed"/> (Independents, the special monster slots) have
    /// theirs in the file. The others get one built at start and again after the mods are read:
    /// the pole, the cloth times #color, the cloth's border times #secondarycolor, and for nations
    /// up to <see cref="EmblemLastNation"/> the emblem (image nation + <see cref="EmblemOffset"/>),
    /// each blended over the last and kept as RGB565. A mod's #flag file replaces it.
    /// </summary>
    public sealed record NationFlagRule(string Archive, int ImageOffset, int FirstComposed, int LastComposed,
                                        int Pole, int Cloth, int Border, int EmblemOffset, int EmblemLastNation)
    {
        /// <summary>Whether the game builds this nation's flag (else it is the archive's image, or there is none).</summary>
        public bool IsComposed(int nation) => nation >= FirstComposed && nation <= LastComposed;

        /// <summary>The emblem image a composed flag has, or -1 (a nation past the last one with an emblem).</summary>
        public int Emblem(int nation) => nation <= EmblemLastNation ? nation + EmblemOffset : -1;

        /// <summary>
        /// The tint byte for a #color channel, as the game computes it: clamped to 0-1 (the parser
        /// does), times 255 in single precision, truncated.
        /// </summary>
        public static int Tint(float channel) => (int)(float)(Math.Clamp(channel, 0f, 1f) * 255f);
    }

    /// <summary>
    /// The game's own pictures for vanilla monsters and items: the sprite numbers the game stores
    /// (tools/dom6exe/data/sprites-6.37.json, shipped as vanilla-sprites.json next to vanilla.dm)
    /// become display assets pointing into the player's install (<see cref="GameSprite"/>): a
    /// monster's #spr1 and #spr2 (and #unmountedspr1/2 for a vanilla rider's), an item's #spr.
    /// Nothing of the game's art is shipped. A picture the editor was given (VanillaAssetLoader's
    /// icons folder) is kept: only entities without one get these. A nation's flag isn't an asset:
    /// the game builds it from the nation's colors, which a mod can change, so the editor builds it
    /// when shown, by <see cref="Flag"/>.
    /// </summary>
    public static class VanillaSprites
    {
        /// <summary>How many were set (or why none were), for the status bar and the snapshot log.</summary>
        public static string? Status { get; private set; }

        /// <summary>How the game draws a nation's flag, or null (no vanilla-sprites.json, or one without the rule).</summary>
        public static NationFlagRule? Flag { get; private set; }

        public static void Load(Mod vanilla, string dmPath)
        {
            Flag = null;
            var file = FindFile(dmPath);
            if (file == null)
            {
                Status = "no vanilla-sprites.json: the game's sprites aren't shown";
                return;
            }
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                using var doc = JsonDocument.Parse(File.ReadAllText(file));
                int monsters = 0, items = 0;
                if (doc.RootElement.TryGetProperty("monster", out var m) && vanilla.Database.TryGetValue(EntityType.MONSTER, out var mons))
                {
                    var archive = m.GetProperty("archive").GetString() ?? "monster.trs";
                    var sprites = Numbers(m.GetProperty("sprites"));
                    var unmounted = m.TryGetProperty("unmounted", out var u) ? Numbers(u) : new Dictionary<int, int>();
                    foreach (var e in mons.GetFullList())
                    {
                        if (!sprites.TryGetValue(e.ID, out int n))
                            continue;
                        var add = new List<(Command, GameSprite)> { (Command.SPR1, new GameSprite(archive, n)), (Command.SPR2, new GameSprite(archive, n, 1)) };
                        if (unmounted.TryGetValue(e.ID, out int un))
                        {
                            add.Add((Command.UNMOUNTEDSPR1, new GameSprite(archive, un)));
                            add.Add((Command.UNMOUNTEDSPR2, new GameSprite(archive, un, 1)));
                        }
                        if (Add(e, add).Contains(Command.SPR1))
                            monsters++;
                    }
                }
                if (doc.RootElement.TryGetProperty("item", out var it) && vanilla.Database.TryGetValue(EntityType.ITEM, out var itemSet))
                {
                    var archive = it.GetProperty("archive").GetString() ?? "item.trs";
                    var sprites = Numbers(it.GetProperty("sprites"));
                    foreach (var e in itemSet.GetFullList())
                        if (sprites.TryGetValue(e.ID, out int n) && Add(e, new List<(Command, GameSprite)> { (Command.SPR, new GameSprite(archive, n)) }).Count > 0)
                            items++;
                }
                if (doc.RootElement.TryGetProperty("flag", out var flag))
                    Flag = FlagRule(flag);
                Status = $"game sprites for {monsters} monsters and {items} items{(Flag != null ? " and the nations' flags" : "")} ({watch.ElapsedMilliseconds} ms; the pictures are read from the install when shown)";
            }
            catch (Exception ex) when (ex is IOException || ex is JsonException || ex is InvalidOperationException || ex is KeyNotFoundException || ex is FormatException)
            {
                Status = Path.GetFileName(file) + " unreadable: " + ex.Message;
            }
        }

        private static NationFlagRule FlagRule(JsonElement f)
        {
            var composed = f.GetProperty("composed");
            var emblem = f.GetProperty("emblem");
            return new NationFlagRule(f.GetProperty("archive").GetString() ?? "flag.trs", f.GetProperty("image_offset").GetInt32(),
                composed.GetProperty("first").GetInt32(), composed.GetProperty("last").GetInt32(),
                f.GetProperty("pole").GetInt32(), f.GetProperty("cloth").GetInt32(), f.GetProperty("border").GetInt32(),
                emblem.GetProperty("offset").GetInt32(), emblem.GetProperty("last_nation").GetInt32());
        }

        /// <summary>An {"id": number} object as a dictionary (looking each ID up in the JSON would scan it).</summary>
        private static Dictionary<int, int> Numbers(JsonElement map)
        {
            var numbers = new Dictionary<int, int>();
            foreach (var p in map.EnumerateObject())
                if (int.TryParse(p.Name, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
                    numbers[id] = p.Value.GetInt32();
            return numbers;
        }

        /// <summary>
        /// Adds the assets for the commands the entity has none for (a supplied picture, or a line
        /// of vanilla.dm, stays), in one go; returns the commands added.
        /// </summary>
        private static List<Command> Add(IDEntity entity, List<(Command Command, GameSprite Sprite)> assets)
        {
            var props = new List<Property>();
            foreach (var (command, sprite) in assets)
            {
                if (entity.Properties.Any(p => p.Command == command) || FilePathProperty.Create() is not FilePathProperty prop)
                    continue;
                prop.Parse(command, sprite.ToString(), "");
                prop.IsDisplayAsset = true; // for showing, not game data: the resolver keeps it apart
                props.Add(prop);
            }
            if (props.Count > 0)
                entity.AddProperties(props);
            return props.Select(p => p.Command).ToList();
        }

        /// <summary>Next to vanilla.dm (vanilla-sprites.json, as the editor ships it), next to the editor, or the repo's tools/dom6exe/data.</summary>
        private static string? FindFile(string dmPath)
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(dmPath)) ?? "";
            foreach (var path in new[]
            {
                Path.Combine(dir, "vanilla-sprites.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vanilla-sprites.json"),
                Path.Combine(dir, "tools", "dom6exe", "data", "sprites-6.37.json"),
            })
                if (File.Exists(path))
                    return path;
            return null;
        }
    }
}
