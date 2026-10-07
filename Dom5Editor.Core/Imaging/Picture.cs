namespace Dom5Editor.Imaging
{
    /// <summary>
    /// An image as the pages hold it, without any UI toolkit: its size, its pixels (BGRA, 4 bytes
    /// each, straight alpha, rows top to bottom) and the DPI it's shown at (192: drawn at half its
    /// pixel size, as the game's high-resolution icons). Each editor turns it into its own bitmap
    /// (the Windows one: PictureConverter).
    /// </summary>
    public sealed class Picture
    {
        public Picture(int width, int height, byte[] bgra, double dpi = 96)
        {
            Width = width;
            Height = height;
            Bgra = bgra;
            Dpi = dpi;
        }

        public int Width { get; }
        public int Height { get; }
        public byte[] Bgra { get; }
        public double Dpi { get; }

        /// <summary>
        /// Reads an image file the core can't (PNG, JPEG, BMP, GIF: TGA it reads itself); set by the
        /// editor from its toolkit. Null result: not readable.
        /// </summary>
        public static Func<string, Picture?>? Decoder { get; set; }

        /// <summary>An image file (BMP, JPEG, GIF, ...) as PNG bytes, for a sprite converted when added to a mod; set by the editor. Null: not converted.</summary>
        public static Func<string, byte[]?>? ConvertToPng { get; set; }

        /// <summary>An image file: TGA read here, the rest by <see cref="Decoder"/>; null if it can't be read.</summary>
        public static Picture? Load(string path)
        {
            try
            {
                if (Path.GetExtension(path).Equals(".tga", StringComparison.OrdinalIgnoreCase))
                    return Dom5Edit.Imaging.Tga.Decode(File.ReadAllBytes(path)) is { } tga ? new Picture(tga.Width, tga.Height, tga.Bgra) : null;
                return Decoder?.Invoke(path);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
