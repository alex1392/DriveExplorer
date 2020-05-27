using NUnit.Framework;
using DriveExplorer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DriveExplorer.Models;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO.Packaging;

namespace DriveExplorer.Views.Tests {
	[TestFixture()]
	public class TypeImageConverterTests {
		[Test()]
		public void ConvertTest() {
			var app = Application.Current; // ensure pack uri scheme
			var converter = new TypeImageConverter();
			var actual = (BitmapImage)converter.Convert(ItemTypes.DOC, null, null, null);
			var expected = new BitmapImage(new Uri($"pack://application:,,,/DriveExplorer;component/Resources/doc.png"));
			Assert.AreEqual(expected.BaseUri, actual.BaseUri);
		}

		[Ignore("Not implemented")]
		public void ConvertBackTest() {
			throw new NotImplementedException();
		}
	}
}