using System.IO;
using System.Windows.Media.Imaging;

namespace Dom5Editor.Sprites
{
    /// <summary>
    /// Brings an image into a mod: the game reads a mod's images from its own folder (paths
    /// relative to the .dm, with forward slashes; .tga or .png; file names without spaces or
    /// special characters; manual, "Files and File Formats"). An image from elsewhere is copied
    /// into the mod's "sprites" folder; one already in the mod's folder is used where it is.
    /// </summary>
    public static class SpriteImport
    {
        /// <summary>
        /// What happened: the path for the mod's line ("sprites/dandan.png"), where the image was
        /// copied (null if it was used where it is), and a note for the user about the image.
        /// </summary>
        public sealed record Result(string RelativePath, string? CopiedTo, string? Note);

        private static readonly int[] Sizes = { 8, 16, 32, 64, 128 };

        public static Result Import(string source, string modFile)
        {
            if (!File.Exists(source))
                throw new FileNotFoundException("The image isn't there", source);
            var modDir = Path.GetFullPath(Path.GetDirectoryName(modFile) ?? ".");
            var full = Path.GetFullPath(source);
            var ext = Path.GetExtension(full).ToLowerInvariant();
            bool usable = ext == ".tga" || ext == ".png";

            // already in the mod's folder (or below it) and a format the game reads: used where it is
            if (usable && full.StartsWith(modDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                var relative = Path.GetRelativePath(modDir, full).Replace('\\', '/');
                var note = Check(full);
                if (relative.Any(ch => !IsSafe(ch)))
                    note = Join(note, "its path has spaces or special characters, which keep a mod from working in the network lobby (manual)");
                return new Result(relative, null, note);
            }

            // copied into the mod's sprites folder, with a safe name (a png for other formats)
            var dir = Path.Combine(modDir, "sprites");
            Directory.CreateDirectory(dir);
            var name = new string(Path.GetFileNameWithoutExtension(full).Select(ch => IsSafe(ch) && ch != '.' ? ch : '_').ToArray());
            if (name.Length == 0)
                name = "sprite";
            var targetExt = usable ? ext : ".png";
            byte[] bytes = usable ? File.ReadAllBytes(full) : ToPng(full);
            var target = Path.Combine(dir, name + targetExt);
            for (int n = 2; File.Exists(target) && !File.ReadAllBytes(target).AsSpan().SequenceEqual(bytes); n++)
                target = Path.Combine(dir, $"{name}_{n}{targetExt}");
            // (the same image brought in before, for another unit: the copy there is used, not made again)
            bool there = File.Exists(target);
            if (!there)
                File.WriteAllBytes(target, bytes);
            var result = Check(target);
            if (!usable)
                result = Join($"converted from {ext} to .png (the game reads .tga and .png)", result);
            return new Result(Path.GetRelativePath(modDir, target).Replace('\\', '/'), there ? null : target, result);
        }

        private static bool IsSafe(char ch) => ch < 128 && (char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch == '.' || ch == '/' || ch == '\\');

        /// <summary>A note when the image's size isn't one the manual asks for, or it can't be read.</summary>
        private static string? Check(string path)
        {
            var image = SpriteLoader.Load(path, null);
            if (image == null)
                return "the editor can't read this image: check it's a 24- or 32-bit .tga, or a .png";
            int w = image.PixelWidth, h = image.PixelHeight;
            return Sizes.Contains(w) || Sizes.Contains(h) ? null
                : $"it's {w}x{h} pixels; the manual asks for 8, 16, 32, 64 or 128 pixels wide or high";
        }

        private static byte[] ToPng(string path)
        {
            var decoder = BitmapDecoder.Create(new Uri(path), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(decoder.Frames[0]));
            using var stream = new MemoryStream();
            encoder.Save(stream);
            return stream.ToArray();
        }

        private static string? Join(string? a, string? b) => a == null ? b : b == null ? a : a + "; " + b;
    }
}
