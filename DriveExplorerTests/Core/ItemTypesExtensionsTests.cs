using Xunit;
using DriveExplorer;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveExplorer.Core {
    public class ItemTypesExtensionsTests {
        private ItemTypes two;
        private ItemTypes one;
        private ItemTypes four;

        public ItemTypesExtensionsTests() {
            one = ItemTypes.Folder; // 1
            two = ItemTypes.LocaDrive; // 2
            four = ItemTypes.File; // 4
        }
        [Fact()]
        public void AddTest() {
            var three = one.Add(two);
            Assert.Equal(3, (int)three);
        }

        [Fact()]
        public void RemoveTest() {
            var six = one.Add(two).Add(four).Remove(one);
            Assert.Equal(6, (int)six);
        }

        [Fact()]
        public void ContainsTest() {
            Assert.True(one.Add(two).Add(four).Contains(one.Add(two)));
        }

        [Fact()]
        public void IsTest() {
            Assert.True(one.Add(four).Is(one.Add(two).Add(four)));
        }
    }
}