using DriveExplorer.ViewModels;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace DriveExplorer.Views {

	public class DebugConverter : MarkupExtension, IValueConverter {

		#region Public Fields

		public static readonly DebugConverter Instance = new DebugConverter();

		#endregion Public Fields

		#region Public Methods

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

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return Instance;
		}

		#endregion Public Methods
	}
}