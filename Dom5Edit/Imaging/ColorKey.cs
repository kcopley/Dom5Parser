namespace Dom5Edit.Imaging
{
    /// <summary>
    /// How the game shows a mod's image without an alpha channel (the manual: "If the image has no
    /// alpha channel (24-bit), black will be replaced with a fully transparent color and magenta
    /// will be replaced by a shadow (black with 50% alpha)"; for item sprites "Black will be
    /// transparent unless the image is saved with alpha information"). An image with alpha keeps
    /// it. (The game's own images, from its archives, follow the same rule: TrsArchive.)
    /// </summary>
    public static class ColorKey
    {
        /// <summary>Whether a TGA or PNG file's pixels carry alpha (32-bit TGA, a 16-bit one with its attribute bit, a 32-bit palette; PNG with alpha or tRNS).</summary>
        public static bool HasAlpha(byte[] data)
        {
            if (data.Length >= 33 && data[0] == 0x89 && data[1] == (byte)'P' && data[2] == (byte)'N' && data[3] == (byte)'G')
            {
                int colorType = data[25];
                if (colorType == 4 || colorType == 6)
                    return true;
                // a tRNS chunk (palette or one colour transparent), before the pixels
                for (int pos = 8; pos + 8 <= data.Length;)
                {
                    int length = data[pos] << 24 | data[pos + 1] << 16 | data[pos + 2] << 8 | data[pos + 3];
                    var type = System.Text.Encoding.ASCII.GetString(data, pos + 4, 4);
                    if (type == "tRNS")
                        return true;
                    if (type == "IDAT" || length < 0)
                        return false;
                    pos += 12 + length;
                }
                return false;
            }
            if (data.Length < 18)
                return false;
            int imageType = data[2], mapBits = data[7], bits = data[16], descriptor = data[17];
            if (imageType is 1 or 9)
                return mapBits == 32;
            return bits == 32 || bits == 16 && (descriptor & 0x0F) > 0;
        }

        /// <summary>
        /// Black to transparent, magenta to the half-transparent black shadow, in BGRA pixels,
        /// compared as the game keeps images: 16-bit colour (RGB565, as for its own sprites:
        /// TrsArchive). So (248, 0, 248), magenta saved from 16 bits (Bloodwar's sprites), is a
        /// shadow too, and (1, 1, 2) transparent, as in game.
        /// </summary>
        public static void Apply(byte[] bgra)
        {
            for (int o = 0; o + 3 < bgra.Length; o += 4)
            {
                int b = bgra[o] >> 3, g = bgra[o + 1] >> 2, r = bgra[o + 2] >> 3;
                if (r == 0 && g == 0 && b == 0)
                    bgra[o + 3] = 0;
                else if (r == 31 && g == 0 && b == 31)
                {
                    bgra[o] = bgra[o + 1] = bgra[o + 2] = 0;
                    bgra[o + 3] = 128;
                }
            }
        }
    }
}
