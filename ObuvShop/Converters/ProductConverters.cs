using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ObuvShop.Models;

namespace ObuvShop.Converters
{
    public class ProductPhotoConverter : IValueConverter
    {
        private static readonly BitmapImage PlaceholderImage = CreatePlaceholder();

        private static BitmapImage CreatePlaceholder()
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri("pack://application:,,,/Resources/picture.png", UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var photoFile = value as string;
            if (string.IsNullOrWhiteSpace(photoFile))
                return PlaceholderImage;

            try
            {
                var fullPath = Path.IsPathRooted(photoFile)
                    ? photoFile
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", photoFile);

                if (!File.Exists(fullPath))
                    return PlaceholderImage;

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(fullPath, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return PlaceholderImage;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class ProductCardBackgroundConverter : IValueConverter
    {
        private static readonly Brush HighDiscountBrush = FreezeBrush("#2E8B57");
        private static readonly Brush OutOfStockBrush = FreezeBrush("#ADD8E6");
        private static readonly Brush DefaultBrush = FreezeBrush("#FFFFFF");

        private static SolidColorBrush FreezeBrush(string hex)
        {
            var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(hex);
            brush.Freeze();
            return brush;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Product product)
            {
                if (product.IsHighDiscount)
                    return HighDiscountBrush;
                if (product.IsOutOfStock)
                    return OutOfStockBrush;
            }
            return DefaultBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class ProductCardForegroundConverter : IValueConverter
    {
        private static readonly Brush LightTextBrush = Brushes.White;
        private static readonly Brush DarkTextBrush = Brushes.Black;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Product product && product.IsHighDiscount)
                return LightTextBrush;
            return DarkTextBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class ProductOldPriceForegroundConverter : IValueConverter
    {
        private static readonly Brush HighDiscountOldPriceBrush = FreezeBrush("#FFD6D6");
        private static readonly Brush RegularOldPriceBrush = FreezeBrush("#CC0000");

        private static SolidColorBrush FreezeBrush(string hex)
        {
            var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(hex);
            brush.Freeze();
            return brush;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Product product && product.IsHighDiscount)
                return HighDiscountOldPriceBrush;
            return RegularOldPriceBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;
            if (parameter is string p && string.Equals(p, "Invert", StringComparison.OrdinalIgnoreCase))
                flag = !flag;
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class OrderStatusBrushConverter : IValueConverter
    {
        private static readonly Brush NewStatusBrush = FreezeBrush("#00FA9A");
        private static readonly Brush CompletedStatusBrush = FreezeBrush("#E6E6E6");

        private static SolidColorBrush FreezeBrush(string hex)
        {
            var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(hex);
            brush.Freeze();
            return brush;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as string;
            if (string.Equals(status, "Новый", StringComparison.OrdinalIgnoreCase))
                return NewStatusBrush;
            return CompletedStatusBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
