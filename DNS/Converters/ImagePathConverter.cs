using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;

namespace DNS.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        private static readonly DrawingImage DefaultImage;

        static ImagePathConverter()
        {
            // Create default placeholder image
            var geometry = new RectangleGeometry(new Rect(0, 0, 100, 100));
            var drawing = new GeometryDrawing(
                new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE)),
                new Pen(new SolidColorBrush(Color.FromRgb(0xD5, 0xD5, 0xD5)), 1),
                geometry);
            DefaultImage = new DrawingImage(drawing);
            if (DefaultImage.CanFreeze) DefaultImage.Freeze();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || string.IsNullOrEmpty(value.ToString()))
                    return DefaultImage;

                string imagePath = value.ToString();
                string fullPath = imagePath;

                if (imagePath.StartsWith("/"))
                {
                    fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath.TrimStart('/'));
                }

                if (!File.Exists(fullPath))
                    return DefaultImage;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.None;
                using (var stream = File.OpenRead(fullPath))
                {
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    if (bitmap.CanFreeze) bitmap.Freeze();
                }
                return bitmap;
            }
            catch (Exception)
            {
                return DefaultImage;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
