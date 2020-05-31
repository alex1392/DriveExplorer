using DriveExplorer.Models;

using NUnit.Framework;

using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DriveExplorer.Views.Tests {

	[TestFixture()]
	public class FileImageConverterTests {

		#region Public Methods

		[Ignore("Not implemented")]
		public void ConvertBackTest()
		{
			throw new NotImplementedException();
		}

		[Test()]
		public void ConvertTest()
		{
			var app = Application.Current; // ensure pack uri scheme
			var converter = new TypeImageConverter();
			var actual = (BitmapImage)converter.Convert(new LocalItem(@"C:\"), null, null, null);
			var expected = new BitmapImage(new Uri($"pack://application:,,,/DriveExplorer;component/Resources/localdrive.png"));
			Assert.AreEqual(expected.BaseUri, actual.BaseUri);
		}

		#endregion Public Methods
	}
}