using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DriveExplorer {
    public class TypeImageConverter : IValueConverter {
        public static readonly TypeImageConverter Instance = new TypeImageConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                return new BitmapImage(new Uri($"pack://application:,,,/DriveExplorer;component/Resources/{value.ToString().ToLower()}.png"));
            } catch (Exception) {   
                return new BitmapImage(new Uri($"pack://application:,,,/DriveExplorer;component/Resources/file.png"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
