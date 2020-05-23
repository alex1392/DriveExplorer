using Xunit;
using DriveExplorer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.IO.Packaging;
using System.Windows;

namespace DriveExplorer.Core {
    public class TypeImageConverterTests {
        private readonly TypeImageConverter converter;

        public TypeImageConverterTests() {
            converter = new TypeImageConverter();
            var ensureUriScheme = Application.Current;
        }

        [Theory]
        [InlineData(ItemTypes.IMG)]
        [InlineData(ItemTypes.File)]
        public void ConvertTest(ItemTypes type) {
            var source = converter.Convert(type, typeof(ImageSource), null, null);
            Assert.NotNull(source);
        }

        [Fact(Skip = "Not Implemented")]
        public void ConvertBackTest() {
            throw new NotImplementedException();
        }
    }
}