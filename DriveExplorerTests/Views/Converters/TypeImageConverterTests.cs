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
#pragma warning disable IDE0059 // Unnecessary assignment of a value
			var app = Application.Current; // ensure pack uri scheme
#pragma warning restore IDE0059 // Unnecessary assignment of a value
			var converter = new FileImageConverter();
			var actual = (BitmapImage)converter.Convert(ItemTypes.Folder, null, null, null);
			var expected = new BitmapImage(new Uri($"pack://application:,,,/DriveExplorer;component/Resources/folder.png"));
			Assert.AreEqual(expected.BaseUri, actual.BaseUri);
		}

		[Ignore("Not implemented")]
		public void ConvertBackTest() {
			throw new NotImplementedException();
		}
	}
}