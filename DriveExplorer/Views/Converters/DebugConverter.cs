using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace DriveExplorer.Views {
	public class DebugConverter : IValueConverter {
		public static readonly DebugConverter Instance = new DebugConverter();
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Debugger.Break();
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Debugger.Break();
			return value;
		}
	}
}
