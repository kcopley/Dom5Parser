using System.IO.Compression;

namespace Dom5Edit.Imaging
{
    /// <summary>
    /// Reads a PNG image (the game's other sprite format) into 32-bit BGRA pixels, top row first, on
    /// any system: grey, RGB, palette (with tRNS transparency), grey+alpha and RGBA, 1-16 bits per
    /// channel; not interlaced (Adam7 images give null: the editor's toolkit reads those).
    /// </summary>
    public static class Png
    {
        private static readonly byte[] Signature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        /// <summary>The image's pixels as BGRA (4 bytes each, rows top to bottom), or null if it isn't a PNG this reads.</summary>
        public static (int Width, int Height, byte[] Bgra)? Decode(byte[] data)
        {
            if (data.Length < 33 || !data.AsSpan(0, 8).SequenceEqual(Signature))
                return null;
            int width = 0, height = 0, depth = 0, colorType = 0, interlace = 0;
            byte[]? palette = null, trns = null;
            using var idat = new MemoryStream();
            for (int pos = 8; pos + 8 <= data.Length;)
            {
                int length = data[pos] << 24 | data[pos + 1] << 16 | data[pos + 2] << 8 | data[pos + 3];
                string type = System.Text.Encoding.ASCII.GetString(data, pos + 4, 4);
                int body = pos + 8;
                if (length < 0 || body + length > data.Length)
                    return null;
                switch (type)
                {
                    case "IHDR":
                        width = BigEndian(data, body);
                        height = BigEndian(data, body + 4);
                        depth = data[body + 8];
                        colorType = data[body + 9];
                        interlace = data[body + 12];
                        break;
                    case "PLTE":
                        palette = data.AsSpan(body, length).ToArray();
                        break;
                    case "tRNS":
                        trns = data.AsSpan(body, length).ToArray();
                        break;
                    case "IDAT":
                        idat.Write(data, body, length);
                        break;
                }
                if (type == "IEND")
                    break;
                pos = body + length + 4; // (the CRC)
            }
            int channels = colorType switch { 0 => 1, 2 => 3, 3 => 1, 4 => 2, 6 => 4, _ => 0 };
            if (width <= 0 || height <= 0 || channels == 0 || interlace != 0 || (long)width * height > 1 << 26
                || depth is not (1 or 2 or 4 or 8 or 16) || (colorType == 3 && palette == null))
                return null;

            int bitsPerPixel = channels * depth;
            int stride = (width * bitsPerPixel + 7) / 8;
            int filterBytes = Math.Max(1, bitsPerPixel / 8);
            var raw = new byte[height * (stride + 1)];
            try
            {
                idat.Position = 0;
                using var z = new ZLibStream(idat, CompressionMode.Decompress);
                int read = 0, n;
                while (read < raw.Length && (n = z.Read(raw, read, raw.Length - read)) > 0)
                    read += n;
                if (read < raw.Length)
                    return null;
            }
            catch (InvalidDataException)
            {
                return null;
            }

            var pixels = new byte[width * height * 4];
            var previous = new byte[stride];
            var line = new byte[stride];
            for (int y = 0; y < height; y++)
            {
                int filter = raw[y * (stride + 1)];
                Buffer.BlockCopy(raw, y * (stride + 1) + 1, line, 0, stride);
                Unfilter(filter, line, previous, filterBytes);
                for (int x = 0; x < width; x++)
                    Pixel(line, x, depth, colorType, palette, trns, pixels, (y * width + x) * 4);
                (previous, line) = (line, previous);
            }
            return (width, height, pixels);
        }

        private static int BigEndian(byte[] d, int at) => d[at] << 24 | d[at + 1] << 16 | d[at + 2] << 8 | d[at + 3];

        private static void Unfilter(int filter, byte[] line, byte[] prev, int bpp)
        {
            for (int i = 0; i < line.Length; i++)
            {
                int a = i >= bpp ? line[i - bpp] : 0, b = prev[i], c = i >= bpp ? prev[i - bpp] : 0;
                line[i] = filter switch
                {
                    1 => (byte)(line[i] + a),
                    2 => (byte)(line[i] + b),
                    3 => (byte)(line[i] + (a + b) / 2),
                    4 => (byte)(line[i] + Paeth(a, b, c)),
                    _ => line[i],
                };
            }
        }

        private static int Paeth(int a, int b, int c)
        {
            int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);
            return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
        }

        /// <summary>Sample <paramref name="index"/> of a line (depth bits each), scaled to 0-255 (palette indexes not scaled).</summary>
        private static int Sample(byte[] line, int index, int depth, bool scale)
        {
            int value;
            if (depth == 8) return line[index];
            if (depth == 16) return line[index * 2]; // (the high byte)
            int bit = index * depth;
            value = line[bit >> 3] >> (8 - depth - (bit & 7)) & ((1 << depth) - 1);
            return scale ? value * 255 / ((1 << depth) - 1) : value;
        }

        private static void Pixel(byte[] line, int x, int depth, int colorType, byte[]? palette, byte[]? trns, byte[] px, int o)
        {
            int r, g, b, a = 255;
            switch (colorType)
            {
                case 0:
                    r = g = b = Sample(line, x, depth, true);
                    if (trns is { Length: >= 2 } && depth <= 8 && Sample(line, x, depth, false) == (trns[0] << 8 | trns[1]))
                        a = 0;
                    break;
                case 2:
                    r = Sample(line, x * 3, depth, true);
                    g = Sample(line, x * 3 + 1, depth, true);
                    b = Sample(line, x * 3 + 2, depth, true);
                    if (trns is { Length: >= 6 } && depth == 8 && r == trns[1] && g == trns[3] && b == trns[5])
                        a = 0;
                    break;
                case 3:
                    int i = Sample(line, x, depth, false);
                    r = 3 * i + 2 < palette!.Length ? palette[3 * i] : 0;
                    g = 3 * i + 2 < palette.Length ? palette[3 * i + 1] : 0;
                    b = 3 * i + 2 < palette.Length ? palette[3 * i + 2] : 0;
                    if (trns != null && i < trns.Length)
                        a = trns[i];
                    break;
                case 4:
                    r = g = b = Sample(line, x * 2, depth, true);
                    a = Sample(line, x * 2 + 1, depth, true);
                    break;
                default:
                    r = Sample(line, x * 4, depth, true);
                    g = Sample(line, x * 4 + 1, depth, true);
                    b = Sample(line, x * 4 + 2, depth, true);
                    a = Sample(line, x * 4 + 3, depth, true);
                    break;
            }
            px[o] = (byte)b;
            px[o + 1] = (byte)g;
            px[o + 2] = (byte)r;
            px[o + 3] = (byte)a;
        }
    }
}
