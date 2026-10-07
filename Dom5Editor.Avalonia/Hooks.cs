using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Dom5Editor.Imaging;
using Dom5Editor.UI;

namespace Dom5Editor.Ava
{
    /// <summary>
    /// The Avalonia side of the core's hooks (UI.Ui, Picture.Decoder): file dialogs, the
    /// dispatcher, images the core doesn't read, and the core's pictures as Avalonia bitmaps.
    /// </summary>
    public static class Hooks
    {
        /// <summary>The window dialogs belong to.</summary>
        public static TopLevel? Owner { get; set; }

        public static void Install()
        {
            Ui.Post = work => Dispatcher.UIThread.Post(work, DispatcherPriority.Background);
            Ui.PickFile = (title, filter, then) => _ = PickAsync(title, filter, then);
            Picture.ConvertToPng = path =>
            {
                using var bitmap = new Bitmap(path);
                using var stream = new MemoryStream();
                bitmap.Save(stream); // (PNG)
                return stream.ToArray();
            };
            // what the core doesn't read itself (JPEG, BMP, GIF): through PNG
            Picture.Decoder = path =>
                Picture.ConvertToPng(path) is byte[] png && Dom5Edit.Imaging.Png.Decode(png) is { } p ? new Picture(p.Width, p.Height, p.Bgra) : null;
        }

        private static async Task PickAsync(string title, string filter, Action<string> then)
        {
            if (Owner?.StorageProvider is not IStorageProvider storage)
                return;
            var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions { Title = title, AllowMultiple = false, FileTypeFilter = Filters(filter) });
            if (files.Count > 0 && files[0].TryGetLocalPath() is string path)
                then(path);
        }

        /// <summary>A WPF-style filter ("Images (*.tga;*.png)|*.tga;*.png|All files|*.*") as Avalonia's file types.</summary>
        public static List<FilePickerFileType> Filters(string filter)
        {
            var parts = filter.Split('|');
            var types = new List<FilePickerFileType>();
            for (int i = 0; i + 1 < parts.Length; i += 2)
                types.Add(new FilePickerFileType(parts[i]) { Patterns = parts[i + 1].Split(';', StringSplitOptions.RemoveEmptyEntries) });
            return types;
        }

        private static readonly ConditionalWeakTable<Picture, Bitmap> _bitmaps = new();

        /// <summary>A picture as an Avalonia bitmap (made once per picture); null for null.</summary>
        public static Bitmap? ToBitmap(Picture? picture) =>
            picture == null ? null : _bitmaps.GetValue(picture, p =>
            {
                var bitmap = new WriteableBitmap(new PixelSize(p.Width, p.Height), new Vector(p.Dpi, p.Dpi), PixelFormat.Bgra8888, AlphaFormat.Unpremul);
                using var frame = bitmap.Lock();
                for (int y = 0; y < p.Height; y++)
                    Marshal.Copy(p.Bgra, y * p.Width * 4, frame.Address + y * frame.RowBytes, p.Width * 4);
                return bitmap;
            });
    }

    /// <summary>{Binding Sprite, Converter={x:Static local:PictureConverter.Instance}}: a Picture shown as an image.</summary>
    public sealed class PictureConverter : IValueConverter
    {
        public static readonly PictureConverter Instance = new();
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => Hooks.ToBitmap(value as Picture);
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
