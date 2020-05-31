using DriveExplorer.Models;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DriveExplorer.Views {
	public class TypeVisibilityConverter : IValueConverter {
		public static readonly TypeVisibilityConverter Instance = new TypeVisibilityConverter();
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var type = (ItemTypes)value;
			return type.Is(ItemTypes.Folders) ?
				Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
