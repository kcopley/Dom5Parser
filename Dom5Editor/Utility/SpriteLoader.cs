using System.IO;
using System.Windows.Media.Imaging;
using Paloma;

namespace Dom5Editor.Sprites
{
    /// <summary>Loads a sprite (TGA, PNG, ...) for display: a mod's path is relative to the mod's folder.</summary>
    public static class SpriteLoader
    {
        private static readonly Dictionary<string, BitmapSource?> _cache = new Dictionary<string, BitmapSource?>(StringComparer.OrdinalIgnoreCase);

        public static BitmapSource? Load(string? spritePath, string? modFile)
        {
            if (string.IsNullOrWhiteSpace(spritePath))
                return null;
            string path;
            if (Path.IsPathRooted(spritePath))
                path = spritePath;
            else
            {
                if (string.IsNullOrEmpty(modFile))
                    return null;
                var relative = spritePath.Trim().TrimStart('.').TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
                path = Path.Combine(Path.GetDirectoryName(modFile) ?? "", relative);
            }
            if (_cache.TryGetValue(path, out var cached))
                return cached;
            BitmapSource? image = null;
            try
            {
                if (File.Exists(path))
                {
                    var ext = Path.GetExtension(path).ToLowerInvariant();
                    if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp")
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(path, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        image = bitmap;
                    }
                    else
                    {
                        image = TargaImage.LoadTargaImage(path).ConvertToImage();
                    }
                }
            }
            catch (Exception)
            {
                image = null;
            }
            return _cache[path] = image;
        }
    }
}
