using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DriveExplorer.Views {
	public class FileImageConverter : IValueConverter {
		public static readonly FileImageConverter Instance = new FileImageConverter();
		/// <param name="value">File Name</param>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			var name = (string)value;
			var ext = Path.GetExtension(name);
			switch (ext) {
				
				default:
					break;
			}
			return new BitmapImage(new Uri($"pack://application:,,,/DriveExplorer;component/Resources/{value.ToString().ToLower()}.png"));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
