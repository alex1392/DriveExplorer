using NUnit.Framework;

using System;
using System.Threading.Tasks;

namespace DriveExplorer.Models.Tests {

	[TestFixture()]
	public class LocalItemTests {

		#region Public Methods

		[TestCase(@"C:\\")]
		[TestCase(@"C:\Users\alex\Desktop\DriveExplorer\DriveExplorer\Resources\")]
		public async Task GetChildrenAsyncTestAsync(string fullPath)
		{
			var item = new LocalItem(fullPath);
			await foreach (var child in item.GetChildrenAsync()) {
				Console.WriteLine(child.FullPath);
				Assert.NotNull(child);
			}
		}

		[TestCase(@"C:\Users\alex\Desktop\DriveExplorer\DriveExplorer\Resources\doc.png", ItemTypes.File, "doc.png")]
		[TestCase(@"C:\Users\alex\Desktop\DriveExplorer\DriveExplorer\Models\LocalItem.cs", ItemTypes.File, "LocalItem.cs")]
		public void LocalItemFileTest(string fullPath, ItemTypes expectedType, string expectedName)
		{
			var item = new LocalItem(fullPath, false);
			Assert.NotNull(item);
			Assert.AreEqual(expectedType, item.Type);
			Assert.AreEqual(expectedName, item.Name);
		}

		[TestCase(@"C:\Users\alex\Desktop\DriveExplorer\DriveExplorer\Resources\", ItemTypes.Folder, "Resources")]
		public void LocalItemFolderTest(string fullPath, ItemTypes expectedType, string expectedName)
		{
			var item = new LocalItem(fullPath, true);
			Assert.NotNull(item);
			Assert.AreEqual(expectedType, item.Type);
			Assert.AreEqual(expectedName, item.Name);
		}

		[TestCase(@"C:\", ItemTypes.LocalDrive, @"C:\")]
		public void LocalItemRootTest(string fullPath, ItemTypes expectedType, string expectedName)
		{
			var item = new LocalItem(fullPath);
			Assert.NotNull(item);
			Assert.AreEqual(expectedType, item.Type);
			Assert.AreEqual(expectedName, item.Name);
		}

		#endregion Public Methods
	}
}