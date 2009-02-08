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
using System.Diagnostics;
using System.Threading;
using LRUCache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LRUCache
{
    
    /// <summary>
    /// The tests in this class exercise the LRUCache under multithreaded read and write
    /// operations.
    /// </summary>
    /// <remarks>The tests all have a similar structure:  a number of threads are created,
    /// each executing a method that writes to the LRUCache.  The main thread waits until
    /// the threads are done (and may access the LRUCache itself while this is happening),
    /// and then checks to see if any exceptions were raised during the operation.
    /// 
    /// Note that because waiting uses WaitHandle.WaitAll(), these tests can't run under
    /// the STA model; that's why LocalTestRun.testconfig has an ExecutionThread element
    /// that sets apartmentState to 1.</remarks>
    [TestClass]
    public class LRUCacheThreadTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private readonly List<Exception> Exceptions = new List<Exception>();
        private readonly object Lock = new object();
        private LRUCache<object> Target = new LRUCache<object>();

        private const int AddCount = 100;

        /// <remarks>
        /// A more robust test would check to see if the contents of the cache at the end 
        /// of the test actually came from multiple threads - by, say, adding instances of a 
        /// class with a ThreadHashCode property instead of just an object.
        /// </remarks>
        [TestMethod]
        public void AddFromMultipleThreads()
        {
            Target.Clear();
            Exceptions.Clear();

            const int ThreadCount = 20;
            ManualResetEvent[] events = new ManualResetEvent[ThreadCount];
            for (int i = 0; i < ThreadCount; i++)
            {
                events[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(Add, events[i]);
            }
            WaitHandle.WaitAll(events);
            
            string msg = "";
            if (Exceptions.Count != 0)
            {
                msg = String.Format("Unexpected exception: {0}", Exceptions[0].Message);
            }
            Assert.AreEqual(Exceptions.Count, 0, msg);
        }

        [TestMethod]
        public void RemoveItemWhileOtherThreadsAreAddingItems()
        {
            const int ThreadCount = 20;

            Exceptions.Clear();

            // set the capacity high enough so that the item to remove won't
            // get discarded.
            Target = new LRUCache<object>(ThreadCount * AddCount + 1);
            object itemToRemove = new object();
            Target.Add(itemToRemove);

            ManualResetEvent[] events = new ManualResetEvent[ThreadCount];

            for (int i = 0; i < ThreadCount; i++)
            {
                events[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(Add, events[i]);
            }

            // wait until at least some items have been added to the cache.
            while(true)
            {
                if (Target.Count > 0)
                {
                    break;
                }
            }

            Debug.WriteLine(String.Format("RemoveItemWhileOtherThreadsAreAddingItems: {0} items in cache.", Target.Count));
            Assert.IsTrue(Target.Remove(itemToRemove));

            WaitHandle.WaitAll(events);

            string msg = "";
            if (Exceptions.Count != 0)
            {
                msg = String.Format("Unexpected exception: {0}", Exceptions[0].Message);
            }
            Assert.AreEqual(Exceptions.Count, 0, msg);
            
        }

        [TestMethod]
        public void CopyToWorksWhileItemsAreBeingAdded()
        {
            Target = new LRUCache<object>();
            Exceptions.Clear();

            const int ThreadCount = 20;
            ManualResetEvent[] events = new ManualResetEvent[ThreadCount];
            for (int i = 0; i < ThreadCount; i++)
            {
                events[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(Add, events[i]);
            }

            // wait until at least some items have been added to the cache.
            while(true)
            {
                if (Target.Count > 0)
                {
                    break;
                }
            }

            // this has to be sized to the target's capacity, not count, because
            // the count may change before CopyTo executes.
            object[] copy = new object[Target.Capacity];
            try
            {
                Target.CopyTo(copy, 0);
                int count = 0;
                foreach (object o in copy)
                {
                    if (o != null)
                    {
                        count++;
                    }
                }
                Debug.WriteLine(String.Format("CopyToWorksWhileItemsAreBeingAdded: {0} items in copy after CopyTo.", count));
            }
            catch (Exception e)
            {
                Assert.Fail(String.Format("Exception during CopyTo: {0}", e.Message));
            }

            WaitHandle.WaitAll(events);

            string msg = "";
            if (Exceptions.Count != 0)
            {
                msg = String.Format("Unexpected exception: {0}", Exceptions[0].Message);
            }
            Assert.AreEqual(Exceptions.Count, 0, msg);
            
        }

        /// <summary>
        /// Add AddCount items to the target LRUCache.
        /// </summary>
        /// <remarks>
        /// This is designed to be called from a thread that the main execution thread
        /// is waiting on, so it sets a ManualResetEvent when it's done to notify the
        /// main thread.  It saves any exceptions encountered in the Exceptions list.
        /// </remarks>
        /// <param name="resetEvent">The ManualResetEvent to set when this operation is
        /// complete.</param>
        private void Add(object resetEvent)
        {
            try
            {
                for (int i = 0; i < AddCount; i++)
                {
                    object o = new object();
                    Target.Add(o);
                }                
            }
            catch (Exception e)
            {
                lock(Lock)
                {
                    Exceptions.Add(e);
                }
            }
            finally
            {
                ((ManualResetEvent) resetEvent).Set();
            }
        }

    }
}
