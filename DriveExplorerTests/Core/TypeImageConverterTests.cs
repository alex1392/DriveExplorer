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
        private TypeImageConverter converter;

        public TypeImageConverterTests() {
            converter = new TypeImageConverter();
            var ensureUriScheme = Application.Current;
        }

        [Theory]
        [InlineData(ItemModel.Types.IMG)]
        [InlineData(ItemModel.Types.File)]
        public void ConvertTest(ItemModel.Types type) {
            var source = converter.Convert(type, typeof(ImageSource), null, null);
            Assert.NotNull(source);
        }

        [Fact(Skip = "Not Implemented")]
        public void ConvertBackTest() {
            throw new NotImplementedException();
        }
    }
}