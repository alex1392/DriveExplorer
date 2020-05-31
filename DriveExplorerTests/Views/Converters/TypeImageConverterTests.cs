using DriveExplorer.Models;

using NUnit.Framework;

using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DriveExplorer.Views.Tests {
	[TestFixture()]
	public class TypeImageConverterTests {
		[Test()]
		public void ConvertTest() {
			var app = Application.Current; // ensure pack uri scheme
			var converter = new TypeImageConverter();
			var actual = (BitmapImage)converter.Convert(ItemTypes.OneDrive, null, null, null);
			var expected = new BitmapImage(new Uri($"pack://application:,,,/DriveExplorer;component/Resources/onedrive.png"));
			Assert.AreEqual(expected.BaseUri, actual.BaseUri);
		}

		[Ignore("Not implemented")]
		public void ConvertBackTest() {
			throw new NotImplementedException();
		}
	}
}