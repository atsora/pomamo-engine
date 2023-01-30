// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.Collections.UnitTests
{
  /// <summary>
  /// Unit tests for the class CachedQueue
  /// </summary>
  [TestFixture]
  public class CachedQueue_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CachedQueue_UnitTest).FullName);

    /// <summary>
    /// Generic test
    /// </summary>
    [Test]
    public void Test()
    {
      CachedQueue<int> cachedQueue = new CachedQueue<int> (new ListQueue<int> (), new ListQueue<int> ());
      cachedQueue.MaxVolatileElements = 2;
      Assert.AreEqual (0, cachedQueue.Count);
      cachedQueue.Enqueue (0);
      Assert.AreEqual (1, cachedQueue.Count);
      Assert.AreEqual (0, cachedQueue.Peek ());
      Assert.AreEqual (0, cachedQueue.Dequeue ());
      Assert.AreEqual (0, cachedQueue.Count);
      cachedQueue.Enqueue (0);
      cachedQueue.UnsafeDequeue ();
      cachedQueue.Enqueue (0);
      cachedQueue.Enqueue (1);
      Assert.AreEqual (2, cachedQueue.Count);
      {
        IList<int> list = cachedQueue.Peek (2);
        Assert.AreEqual (0, list [0]);
        Assert.AreEqual (1, list [1]);
      }
      cachedQueue.Enqueue (2);
      Assert.AreEqual (3, cachedQueue.Count);
      {
        IList<int> list = cachedQueue.Peek (3);
        Assert.AreEqual (0, list [0]);
        Assert.AreEqual (1, list [1]);
        Assert.AreEqual (2, list [2]);
      }
      Assert.AreEqual (0, cachedQueue.Dequeue ());
      Assert.AreEqual (1, cachedQueue.Peek ());
      cachedQueue.UnsafeDequeue ();
      Assert.AreEqual (2, cachedQueue.Dequeue ());
      Assert.AreEqual (0, cachedQueue.Count);
    }
  }
}
