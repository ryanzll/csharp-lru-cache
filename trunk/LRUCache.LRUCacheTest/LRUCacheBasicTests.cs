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

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LRUCache
{
    /// <summary>
    /// Basic LRUCache tests.  These tests are all self-contained methods; more complex tests
    /// are in other classes.
    /// </summary>
    [TestClass]
    public class LRUCacheBasicTests
    {
        private const int Iterations = 100;

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CacheContainsItemsAfterAdd()
        {
            LRUCache<object> test = new LRUCache<object>();

            for (int i = 0; i < Iterations; i++ )
            {
                object o = new object();
                test.Add(o);
                Assert.AreEqual(test.Count, i + 1);
                Assert.AreEqual(test.Contains(o), true);
            }
        }

        [TestMethod]
        public void CacheIsNotReadOnly()
        {
            LRUCache<object> cache = new LRUCache<object>();
            Assert.IsFalse(cache.IsReadOnly);
        }

        [TestMethod]
        public void CacheIsEnumerable()
        {
            LRUCache<int> cache = new LRUCache<int>();
            int i;

            for (i = 0; i < 100; i++)
            {
                cache.Add(i);
            }

            i = 0;
            foreach (int item in cache)
            {
                Assert.AreEqual(item, i++);
            }

        }

        [TestMethod]
        public void CacheDoesNotContainItemAfterRemoval()
        {
            LRUCache<object> test = new LRUCache<object>();
            object toRemove = null;

            for (int i = 0; i < Iterations; i++)
            {
                object o = new object();
                test.Add(o);
                if (i == 17)
                {
                    toRemove = o;
                }
            }

            Assert.IsTrue(test.Contains(toRemove));
            Assert.IsTrue(test.Count == Iterations);
            test.Remove(toRemove);
            Assert.IsFalse(test.Contains(toRemove));
            Assert.IsTrue(test.Count == 99);

        }

        [TestMethod]
        public void OldestItemIsLastItemAdded()
        {
            LRUCache<object> test = new LRUCache<object>();
            object firstAdded = null;

            for (int i = 0; i < Iterations; i++)
            {
                object o = new object();
                if (i == 0)
                {
                    firstAdded = o;
                }
                test.Add(o);
            }
            
            Assert.AreEqual(test.Oldest, firstAdded);
        }

        [TestMethod]
        public void ItemsAreRemovedInReverseOfOrderAdded()
        {
            LRUCache<object> test = new LRUCache<object>();
            List<object> itemsAdded = new List<object>();            

            for (int i = 0; i < Iterations; i++)
            {
                object o = new object();
                test.Add(o);
                itemsAdded.Add(o);
            }

            foreach (object o in itemsAdded)
            {
                Assert.AreEqual(test.Oldest, o);
                Assert.IsTrue(test.Remove(o));
            }

            Assert.AreEqual(test.Count, 0);
        }

        [TestMethod]
        public void CopyToReturnsCorrectArray()
        {
            const int itemCount = 100;
            object[] items = new object[itemCount];
            object[] contents = new object[itemCount];
            LRUCache<object> target = new LRUCache<object>();

            for (int i = 0; i < itemCount; i++)
            {
                items[i] = new object();
                target.Add(items[i]);
            }

            target.CopyTo(contents, 0);
            for (int i = 0; i < itemCount; i++)
            {
                Assert.AreEqual(items[i], contents[i]);
            }

        }

        [TestMethod]
        public void AddingItemMakesItNotLeastRecentlyUsed()
        {
            LRUCache<object> test = new LRUCache<object>();

            for (int i = 0; i < Iterations; i++)
            {
                object o = new object();
                test.Add(o);
            }

            object[] contents = new object[test.Count()];
            test.CopyTo(contents, 0);

            for (int i = 0; i < Iterations; i++)
            {
                object item = contents[i];
                test.Add(item);
                Assert.AreNotEqual(test.Oldest, item);
            }

        }

        [TestMethod]
        public void OKToRemoveItemNotInCache()
        {
            LRUCache<object> test = new LRUCache<object>();

            for (int i = 0; i < Iterations; i++)
            {
                object item = new object();
                test.Add(item);
            }
            object not_in_cache = new object();
            Assert.IsFalse(test.Remove(not_in_cache));
        }
    }
}
