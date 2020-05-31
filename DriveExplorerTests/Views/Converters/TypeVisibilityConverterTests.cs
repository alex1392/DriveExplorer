using DriveExplorer.Models;

using NUnit.Framework;

using System;
using System.Windows;

namespace DriveExplorer.Views.Tests {
	[TestFixture()]
	public class TypeVisibilityConverterTests {
		private TypeVisibilityConverter converter;

		[TestCase(ItemTypes.File, Visibility.Collapsed)]
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