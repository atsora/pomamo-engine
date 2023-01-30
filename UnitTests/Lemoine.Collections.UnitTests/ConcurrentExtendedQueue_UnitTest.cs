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
  /// Unit tests for the class ConcurrentExtendedQueue
  /// </summary>
  [TestFixture]
  public class ConcurrentExtendedQueue_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CachedQueue_UnitTest).FullName);

    /// <summary>
    /// Generic test
    /// </summary>
    [Test]
    public void Test ()
    {
      var queue = new ConcurrentExtendedQueue<int> ();
      queue.Enqueue (0);
      Assert.AreEqual (1, queue.Count);
      Assert.AreEqual (0, queue.Peek ());
      Assert.AreEqual (0, queue.Dequeue ());
      Assert.AreEqual (0, queue.Count);
      queue.Enqueue (0);
      queue.UnsafeDequeue ();
      queue.Enqueue (0);
      queue.Enqueue (1);
      Assert.AreEqual (2, queue.Count);
      {
        IList<int> list = queue.Peek (2);
        Assert.AreEqual (0, list[0]);
        Assert.AreEqual (1, list[1]);
      }
      queue.Enqueue (2);
      Assert.AreEqual (3, queue.Count);
      {
        IList<int> list = queue.Peek (3);
        Assert.AreEqual (0, list[0]);
        Assert.AreEqual (1, list[1]);
        Assert.AreEqual (2, list[2]);
      }
      Assert.AreEqual (0, queue.Dequeue ());
      Assert.AreEqual (1, queue.Peek ());
      queue.UnsafeDequeue ();
      Assert.AreEqual (2, queue.Dequeue ());
      Assert.AreEqual (0, queue.Count);
    }
  }
}
