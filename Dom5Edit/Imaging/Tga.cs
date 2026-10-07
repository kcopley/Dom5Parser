namespace Dom5Edit.Imaging
{
    /// <summary>
    /// Reads a Truevision TGA image (the game's sprite format, beside PNG) into 32-bit BGRA pixels,
    /// top row first, on any system (no System.Drawing). Raw and run-length encoded; true colour
    /// (15/16, 24, 32 bit), grey and colour-mapped. Mods use RLE and raw 24/32-bit, mostly stored
    /// bottom row first.
    /// </summary>
    public static class Tga
    {
        /// <summary>The image's pixels as BGRA (4 bytes each, rows top to bottom), or null if it isn't a TGA this reads.</summary>
        public static (int Width, int Height, byte[] Bgra)? Decode(byte[] data)
        {
            if (data.Length < 18)
                return null;
            int idLength = data[0], mapType = data[1], imageType = data[2];
            int mapFirst = data[3] | data[4] << 8, mapLength = data[5] | data[6] << 8, mapBits = data[7];
            int width = data[12] | data[13] << 8, height = data[14] | data[15] << 8, bits = data[16], descriptor = data[17];
            bool rle = imageType >= 9 && imageType <= 11;
            int kind = rle ? imageType - 8 : imageType; // 1 colour-mapped, 2 true colour, 3 grey
            if (width == 0 || height == 0 || kind < 1 || kind > 3 || (long)width * height > 1 << 26)
                return null;
            int pos = 18 + idLength;

            // the colour map (colour-mapped images), as BGRA
            byte[][]? map = null;
            if (mapType == 1)
            {
                int entry = (mapBits + 7) / 8;
                map = new byte[mapFirst + mapLength][];
                for (int i = 0; i < mapLength; i++)
                {
                    if (pos + entry > data.Length)
                        return null;
                    map[mapFirst + i] = Color(data, pos, mapBits);
                    pos += entry;
                }
            }
            if (kind == 1 && map == null)
                return null;

            int bytes = (bits + 7) / 8;
            var pixels = new byte[width * height * 4];
            int count = width * height, n = 0;
            byte[] Pixel(int at)
            {
                if (kind == 1)
                {
                    int index = bytes == 1 ? data[at] : data[at] | data[at + 1] << 8;
                    return index < map!.Length && map[index] != null ? map[index] : new byte[4];
                }
                if (kind == 3)
                    return new[] { data[at], data[at], data[at], bytes > 1 ? data[at + 1] : (byte)255 };
                return Color(data, at, bits);
            }
            void Put(byte[] c)
            {
                // stored rows: bottom up unless the descriptor's bit 5 says top down; right to left if bit 4
                int x = n % width, y = n / width;
                if ((descriptor & 0x10) != 0) x = width - 1 - x;
                if ((descriptor & 0x20) == 0) y = height - 1 - y;
                Buffer.BlockCopy(c, 0, pixels, (y * width + x) * 4, 4);
                n++;
            }

            while (n < count)
            {
                if (!rle)
                {
                    if (pos + bytes > data.Length)
                        break;
                    Put(Pixel(pos));
                    pos += bytes;
                    continue;
                }
                if (pos >= data.Length)
                    break;
                int header = data[pos++], run = (header & 0x7f) + 1;
                if ((header & 0x80) != 0)
                {
                    if (pos + bytes > data.Length)
                        break;
                    var c = Pixel(pos);
                    pos += bytes;
                    for (int i = 0; i < run && n < count; i++)
                        Put(c);
                }
                else
                    for (int i = 0; i < run && n < count; i++)
                    {
                        if (pos + bytes > data.Length)
                            break;
                        Put(Pixel(pos));
                        pos += bytes;
                    }
            }
            return n == count ? (width, height, pixels) : null;
        }

        /// <summary>One true-colour value as BGRA: 15/16-bit (5-5-5, the top bit as alpha for 16), 24-bit, 32-bit.</summary>
        private static byte[] Color(byte[] d, int at, int bits)
        {
            switch (bits)
            {
                case 15:
                case 16:
                    int v = d[at] | d[at + 1] << 8;
                    byte B(int x) => (byte)((x << 3) | (x >> 2));
                    return new[] { B(v & 0x1f), B(v >> 5 & 0x1f), B(v >> 10 & 0x1f), bits == 16 && (v & 0x8000) == 0 ? (byte)0 : (byte)255 };
                case 24:
                    return new[] { d[at], d[at + 1], d[at + 2], (byte)255 };
                case 32:
                    return new[] { d[at], d[at + 1], d[at + 2], d[at + 3] };
                default:
                    return new[] { d[at], d[at], d[at], (byte)255 };
            }
        }
    }
}
