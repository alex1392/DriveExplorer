using NUnit.Framework;
using DriveExplorer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DriveExplorer.Models;
using System.Windows;

namespace DriveExplorer.Views.Tests {
	[TestFixture()]
	public class TypeVisibilityConverterTests {
		private TypeVisibilityConverter converter;

		[TestCase(ItemTypes.DOC, Visibility.Collapsed)]
		[TestCase(ItemTypes.OneDrive, Visibility.Visible)]
		public void ConvertTest(ItemTypes type, Visibility expected) {
			converter = new TypeVisibilityConverter();
			var actual = converter.Convert(type, null, null, null);
			Assert.AreEqual(expected, actual);
		}

		[Ignore("Not implemented")]
		public void ConvertBackTest() {
			throw new NotImplementedException();
		}
	}
}