using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dom5Editor.Imaging;

namespace Dom5Editor.UI.Converters
{
    /// <summary>
    /// The core's pictures (Dom5Editor.Imaging.Picture: BGRA pixels) as WPF bitmaps, made once per
    /// picture and frozen; and the WPF side of the core's hooks (Ui, Picture.Decoder), set at start.
    /// </summary>
    public static class Pictures
    {
        private static readonly ConditionalWeakTable<Picture, BitmapSource> _images = new();

        /// <summary>The picture as a frozen WPF bitmap (the same one each time while the picture lives); null for null.</summary>
        public static BitmapSource? ToImage(Picture? picture) =>
            picture == null ? null : _images.GetValue(picture, p =>
            {
                var image = BitmapSource.Create(p.Width, p.Height, p.Dpi, p.Dpi, PixelFormats.Bgra32, null, p.Bgra, p.Width * 4);
                image.Freeze();
                return image;
            });

        /// <summary>A WPF bitmap as a picture (BGRA).</summary>
        public static Picture FromImage(BitmapSource image)
        {
            BitmapSource source = image.Format == PixelFormats.Bgra32 ? image : new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);
            int w = source.PixelWidth, h = source.PixelHeight;
            var pixels = new byte[w * h * 4];
            source.CopyPixels(pixels, w * 4, 0);
            return new Picture(w, h, pixels, image.DpiX);
        }

        /// <summary>
        /// Sets the core's hooks to WPF: bindings turn a Picture into an ImageSource by themselves
        /// (a type converter), images the core doesn't read are read by WPF, dialogs, the dispatcher,
        /// and commands asked again when WPF suggests it.
        /// </summary>
        public static void Install(Application app)
        {
            TypeDescriptor.AddAttributes(typeof(Picture), new TypeConverterAttribute(typeof(PictureTypeConverter)));
            Picture.Decoder = path =>
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return FromImage(bitmap);
            };
            Picture.ConvertToPng = path =>
            {
                var decoder = BitmapDecoder.Create(new Uri(path), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(decoder.Frames[0]));
                using var stream = new MemoryStream();
                encoder.Save(stream);
                return stream.ToArray();
            };
            Ui.PickFile = (title, filter) =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog { Title = title, Filter = filter };
                return dialog.ShowDialog() == true ? dialog.FileName : null;
            };
            Ui.Post = work => app.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, work);
            Ui.AddRequery = handler => CommandManager.RequerySuggested += handler;
            Ui.RemoveRequery = handler => CommandManager.RequerySuggested -= handler;
        }
    }

    /// <summary>Lets bindings show a Picture where an ImageSource goes (registered for Picture at start).</summary>
    public sealed class PictureTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
            destinationType != null && destinationType.IsAssignableFrom(typeof(BitmapSource)) || base.CanConvertTo(context, destinationType);

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
            value is Picture p && destinationType.IsAssignableFrom(typeof(BitmapSource)) ? Pictures.ToImage(p) : base.ConvertTo(context, culture, value, destinationType);
    }
}
