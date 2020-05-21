using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DriveExplorer {
    public class TypeImageConverter : IValueConverter {
        public static readonly TypeImageConverter Instance = new TypeImageConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return new BitmapImage(new Uri($"pack://application:,,,/DriveExplorer;component/Resources/{value.ToString().ToLower()}.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
