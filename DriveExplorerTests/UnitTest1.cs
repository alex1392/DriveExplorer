using System;
using DriveExplorer.Tests;
using NUnit.Framework;

namespace DriveExplorerTests {
	[TestFixtureSource(typeof(source))]
	public class UnitTest1 {
		private int a;

		public UnitTest1(int a)
		{
			this.a = a;
		}

		[OneTimeSetUp]
		public void setup()
		{

		}

		[TestCase]
		public void TestMethod1()
		{
			Assert.AreEqual(a, 1);
		}
	}
}
