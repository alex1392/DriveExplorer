using Xunit;
using DriveExplorer;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DriveExplorer.Tests
{
    public class MainWindowTests
    {
        [Fact()]
        public void Window_LoadedTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void Test()
        {
            var drives = Directory.GetLogicalDrives();

            foreach (var drive in drives)
            {
                Console.WriteLine(drive);
            }
        }
    }
}