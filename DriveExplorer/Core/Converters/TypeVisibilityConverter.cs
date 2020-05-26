using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace DriveExplorer {
    public class TypeVisibilityConverter : IValueConverter {
        public static readonly TypeVisibilityConverter Instance = new TypeVisibilityConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch ((ItemTypes)value) {
                default:
                case ItemTypes.Folder:
                case ItemTypes.LocaDrive:
                    return Visibility.Visible;
                case ItemTypes.File:
                case ItemTypes.IMG:
                case ItemTypes.TXT:
                case ItemTypes.DOC:
                case ItemTypes.XLS:
                case ItemTypes.PPT:
                case ItemTypes.ZIP:
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
