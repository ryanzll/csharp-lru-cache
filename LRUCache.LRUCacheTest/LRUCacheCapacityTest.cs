// Copyright (C) 2009 Robert Rossney <rrossney@gmail.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LRUCache
{
    /// <summary>
    /// Test LRUCache to verify that when it's at its capacity and an item is added,
    /// the oldest item in the cache gets discarded and the DiscardingOldestItem event
    /// is raised.
    /// </summary>
    [TestClass]
    public class LRUCacheCapacityTest
    {
        private object ItemBeingDiscarded;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void OldestObjectIsRemovedWhenCacheIsAtCapacity()
        {
            const int capacity = 100;
            LRUCache<object> target = new LRUCache<object>(capacity);

            for (int i = 0; i < capacity; i++)
            {
                target.Add(new object());
            }

            target.DiscardingOldestItem += OnDiscardingOldestItem;

            for (int i = 0; i < 100; i ++)
            {
                object o = target.Oldest;
                Assert.IsTrue(target.Contains(o));
                target.Add(new object());
                Assert.AreEqual(o, ItemBeingDiscarded);
                Assert.IsFalse(target.Contains(o));
            }
        }

        private void OnDiscardingOldestItem(object sender, EventArgs e)
        {
            LRUCache<object> cache = (LRUCache<object>) sender;
            ItemBeingDiscarded = cache.Oldest;
        }
    }
}
