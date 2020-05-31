using DriveExplorer.Models;

using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DriveExplorer.Views {

	public class TypeImageConverter : IValueConverter {

		#region Public Fields

		public static readonly TypeImageConverter Instance = new TypeImageConverter();

		#endregion Public Fields

		#region Public Methods

		/// <param name="value"><see cref="IItem"/></param>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var item = (IItem)value;
			var baseUri = "pack://application:,,,/DriveExplorer;component/Resources/";
			var ext = ".png";
			if (item.Type.Is(ItemTypes.Folders)) {
				return new BitmapImage(new Uri($"{baseUri}{item.Type}{ext}"));
			}
			var type = Path.GetExtension(item.FullPath).ToLower();
			if (Uri.TryCreate($"{baseUri}{type}{ext}", UriKind.Absolute, out var uri) || Uri.TryCreate($"{baseUri}{type}x{ext}", UriKind.Absolute, out uri)) {
				return new BitmapImage(uri);
			}

			var typeMap = type switch
			{
				"rar" => "zip",
				var x when
				x == "jpg" ||
				x == "jpeg" ||
				x == "png" ||
				x == "tif" ||
				x == "bmp" => "img",
				_ => "file",
			};
			return new BitmapImage(new Uri($"{baseUri}{typeMap}{ext}"));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion Public Methods
	}
}