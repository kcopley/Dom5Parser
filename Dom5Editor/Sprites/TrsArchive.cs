using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dom5Editor.Sprites
{
    /// <summary>
    /// A Dominions 6 .trs image archive (data/res.trs, misc.trs, ...), read from the user's own
    /// install. Opening reads only the index; an image is decoded when first asked for, converted
    /// the way the game converts it, frozen and cached. Format: tools/gameart/README.md.
    /// </summary>
    public sealed class TrsArchive
    {
        /// <summary>Index flag: the game draws the image at half its pixel size (a 64x64 icon shown as 32x32).</summary>
        public const int FlagHalfSize = 0x01;

        public readonly struct ImageInfo
        {
            public int Width { get; init; }
            public int Height { get; init; }
            public int Flags { get; init; }
            public long RawOffset { get; init; }
            public long RleOffset { get; init; }
            public int Length { get; init; }
            public bool HalfSize => (Flags & FlagHalfSize) != 0;
        }

        private readonly ImageInfo[] _images;
        private readonly int[] _groups;
        // decoded images, kept while something shows them (a unit list scrolled to the end would
        // otherwise hold every sprite at full size); null: the image can't be decoded
        private readonly Dictionary<int, WeakReference<BitmapSource>?> _cache = new Dictionary<int, WeakReference<BitmapSource>?>();
        private readonly object _lock = new object();

        public string Path { get; }
        public int Version { get; }
        public int Pitch { get; }
        public int Count => _images.Length;

        private TrsArchive(string path, int version, int pitch, ImageInfo[] images, int[] groups)
        {
            Path = path;
            Version = version;
            Pitch = pitch;
            _images = images;
            _groups = groups;
        }

        public ImageInfo Info(int index) => _images[index];

        /// <summary>
        /// Where group <paramref name="group"/> begins: 0 for group 0, else the first image of the
        /// archive's group-th label (monster.trs: nation art; item.trs: 1h, 2h, ...; sites.trs: fire,
        /// air, ...); -1 past the last label. The game's lookup (version 3+ archives) does the same.
        /// </summary>
        public int GroupStart(int group) => group <= 0 ? 0 : group <= _groups.Length ? _groups[group - 1] : -1;

        /// <summary>
        /// The image the game draws for a sprite number: below 1000 the number itself, else
        /// GroupStart(n / 1000) + n % 1000 (how the game converts the monster and item tables at
        /// start, tools/dom6exe/sprites.py). -1 if the group isn't there.
        /// </summary>
        public int SpriteIndex(int number)
        {
            if (number < 1000)
                return number;
            int start = GroupStart(number / 1000);
            return start < 0 ? -1 : start + number % 1000;
        }

        /// <summary>Reads a .trs file's index. Throws if the file isn't a .trs archive.</summary>
        public static TrsArchive Open(string path)
        {
            using var f = File.OpenRead(path);
            var header = ReadExactly(f, 10);
            if (header[0] != (byte)'T' || header[1] != (byte)'C' || header[2] != (byte)'S')
                throw new InvalidDataException(path + ": not a .trs archive");
            int count = U16(header, 4), version = U16(header, 6), pitch = U16(header, 8);
            if (pitch <= 0)
                pitch = 800;
            // version 2+: 12-byte records from 0x0C (width, height, u16 flags, u32 raw offset,
            // u32 run-length offset); older: 10-byte records from 0x0A without the flags.
            int first = version >= 2 ? 12 : 10, size = version >= 2 ? 12 : 10;
            f.Position = first;
            var index = ReadExactly(f, count * size);
            var raw = new long[count];
            var rle = new long[count];
            var images = new ImageInfo[count];
            var offsets = new SortedSet<long> { f.Length };
            for (int i = 0; i < count; i++)
            {
                int o = i * size;
                raw[i] = U32(index, o + size - 8);
                rle[i] = U32(index, o + size - 4);
                long start = raw[i] != 0 ? raw[i] : rle[i];
                if (start != 0)
                    offsets.Add(start);
            }
            for (int i = 0; i < count; i++)
            {
                int o = i * size;
                long start = raw[i] != 0 ? raw[i] : rle[i];
                long end = start == 0 ? 0 : offsets.GetViewBetween(start + 1, long.MaxValue).Min;
                images[i] = new ImageInfo
                {
                    Width = index[o] == 0 ? 256 : index[o],   // one byte each: 0 means 256
                    Height = index[o + 1] == 0 ? 256 : index[o + 1],
                    Flags = size == 12 ? U16(index, o + 2) : 0,
                    RawOffset = raw[i],
                    RleOffset = rle[i],
                    Length = (int)Math.Max(0, end - start),
                };
            }
            // group labels, between the index and the first image: (u32 first image, NUL-terminated
            // name) pairs, ended by FF FF FF FF
            var groups = new List<int>();
            long labelsEnd = offsets.Min;
            if (labelsEnd > f.Position)
            {
                var labels = ReadExactly(f, (int)Math.Min(labelsEnd - f.Position, 1 << 20));
                for (int p = 0; p + 4 <= labels.Length;)
                {
                    long start = U32(labels, p);
                    p += 4;
                    if (start == 0xFFFFFFFF)
                        break;
                    groups.Add((int)start);
                    while (p < labels.Length && labels[p] != 0)
                        p++;
                    p++;
                }
            }
            return new TrsArchive(path, version, pitch, images, groups.ToArray());
        }

        /// <summary>
        /// Image <paramref name="index"/>, frozen; null if the index is out of range or the image
        /// can't be decoded. A half-size image gets 192 dpi, so WPF shows it at the size the game does.
        /// Decoded again only if nothing kept it since.
        /// </summary>
        public BitmapSource? Image(int index)
        {
            if (index < 0 || index >= _images.Length)
                return null;
            lock (_lock)
            {
                if (_cache.TryGetValue(index, out var cached))
                {
                    if (cached == null)
                        return null;
                    if (cached.TryGetTarget(out var alive))
                        return alive;
                }
                BitmapSource? image = null;
                try
                {
                    var info = _images[index];
                    var pixels = DecodeBgra(index);
                    if (pixels != null)
                    {
                        double dpi = info.HalfSize ? 192 : 96;
                        image = BitmapSource.Create(info.Width, info.Height, dpi, dpi, PixelFormats.Bgra32, null, pixels, info.Width * 4);
                        image.Freeze();
                    }
                }
                catch (Exception)
                {
                    image = null; // a damaged or unexpected image: no icon
                }
                _cache[index] = image != null ? new WeakReference<BitmapSource>(image) : null;
                return image;
            }
        }

        /// <summary>Image <paramref name="index"/> as straight (not premultiplied) BGRA bytes, width * height * 4.</summary>
        public byte[]? DecodeBgra(int index)
        {
            var info = _images[index];
            long start = info.RawOffset != 0 ? info.RawOffset : info.RleOffset;
            if (start == 0 || info.Length <= 0)
                return null;
            byte[] d;
            using (var f = File.OpenRead(Path))
            {
                f.Position = start;
                d = ReadExactly(f, info.Length);
            }
            int w = info.Width, h = info.Height;
            var outp = new byte[w * h * 4];
            int p = 0;
            if (info.RawOffset != 0)
            {
                // w*h big-endian RGB565 pixels
                for (int k = 0; k < w * h && p + 1 < d.Length; k++, p += 2)
                    Put565(outp, k * 4, U16(d, p), -1);
            }
            else if (Version >= 4)
            {
                // u16 mode (1: pixels carry an alpha byte), u16 runs; runs: skip, count, pixels.
                // skip and count are a byte, or 0xFF and a 24-bit value; positions run across rows
                // of the image's own width.
                bool alpha = U16(d, 0) == 1;
                int runs = U16(d, 2);
                p = 4;
                int pos = 0, bpp = alpha ? 3 : 2;
                for (int r = 0; r < runs; r++)
                {
                    int skip = d[p++];
                    if (skip == 0xFF) { skip = U24(d, p); p += 3; }
                    int cnt = d[p++];
                    if (cnt == 0xFF) { cnt = U24(d, p); p += 3; }
                    pos += skip;
                    for (int k = 0; k < cnt; k++, p += bpp, pos++)
                        if (pos < w * h)
                            Put565(outp, pos * 4, U16(d, p), alpha ? d[p + 2] : -1);
                }
            }
            else
            {
                // u16 runs-1; runs: u16 skip in bytes on a surface Pitch (800) pixels wide,
                // u16 count-1 (0xFFFF: no pixels), count RGB565 pixels.
                int runs = U16(d, 0) + 1;
                p = 2;
                long pos = 0;
                for (int r = 0; r < runs; r++)
                {
                    int skip = U16(d, p), cnt = (U16(d, p + 2) + 1) & 0xFFFF;
                    p += 4;
                    pos += skip / 2;
                    for (int k = 0; k < cnt; k++, p += 2, pos++)
                    {
                        long y = pos / Pitch, x = pos % Pitch;
                        if (x < w && y < h)
                            Put565(outp, (int)(y * w + x) * 4, U16(d, p), -1);
                    }
                }
            }
            return outp;
        }

        /// <summary>
        /// One pixel as the game converts it: R = v>>8 &amp; 0xF8, G = v>>3 &amp; 0xFC, B = v<<3 &amp; 0xF8.
        /// Without an alpha byte (alpha &lt; 0), 0x0000 is transparent and magenta 0xF81F is a shadow
        /// (black at alpha 0x80).
        /// </summary>
        private static void Put565(byte[] outp, int o, int v, int alpha)
        {
            byte r = (byte)((v >> 8) & 0xF8), g = (byte)((v >> 3) & 0xFC), b = (byte)((v << 3) & 0xF8);
            if (alpha < 0)
            {
                if (v == 0)
                    return;
                if (v == 0xF81F) { r = g = b = 0; alpha = 0x80; }
                else alpha = 0xFF;
            }
            outp[o] = b;
            outp[o + 1] = g;
            outp[o + 2] = r;
            outp[o + 3] = (byte)alpha;
        }

        private static int U16(byte[] d, int o) => (d[o] << 8) | d[o + 1];
        private static int U24(byte[] d, int o) => (d[o] << 16) | (d[o + 1] << 8) | d[o + 2];
        private static long U32(byte[] d, int o) => ((long)d[o] << 24) | ((long)d[o + 1] << 16) | ((long)d[o + 2] << 8) | d[o + 3];

        private static byte[] ReadExactly(Stream s, int n)
        {
            var buf = new byte[n];
            int got = 0;
            while (got < n)
            {
                int k = s.Read(buf, got, n - got);
                if (k <= 0)
                    throw new EndOfStreamException();
                got += k;
            }
            return buf;
        }
    }
}
