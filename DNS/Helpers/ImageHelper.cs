using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;

namespace DNS.Helpers
{
    public static class ImageHelper
    {
        public static BitmapImage LoadImage(string path, bool isResource = false)
        {
            try
            {
                var bitmap = new BitmapImage();
                
                if (isResource)
                {
                    var resourcePath = $"pack://application:,,,{path}";
                    var stream = Application.GetResourceStream(new Uri(resourcePath))?.Stream;
                    if (stream == null) return null;

                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = BitmapCreateOptions.None;
                    bitmap.EndInit();
                    if (bitmap.CanFreeze) bitmap.Freeze();
                    stream.Close();
                    return bitmap;
                }
                else if (File.Exists(path))
                {
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.CreateOptions = BitmapCreateOptions.None;
                        bitmap.EndInit();
                        if (bitmap.CanFreeze) bitmap.Freeze();
                        return bitmap;
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Image Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }
    }
}
