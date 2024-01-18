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
      var cachedQueue = new CachedQueue<int> (new ListQueue<int> (), new ListQueue<int> ());
      cachedQueue.MaxVolatileElements = 2;
      Assert.That (cachedQueue.Count, Is.EqualTo (0));
      cachedQueue.Enqueue (0);
      Assert.That (cachedQueue.Count, Is.EqualTo (1));
      Assert.Multiple (() => {
        Assert.That (cachedQueue.Peek (), Is.EqualTo (0));
        Assert.That (cachedQueue.Dequeue (), Is.EqualTo (0));
        Assert.That (cachedQueue.Count, Is.EqualTo (0));
      });
      cachedQueue.Enqueue (0);
      cachedQueue.UnsafeDequeue ();
      cachedQueue.Enqueue (0);
      cachedQueue.Enqueue (1);
      Assert.That (cachedQueue.Count, Is.EqualTo (2));
      {
        IList<int> list = cachedQueue.Peek (2);
        Assert.Multiple (() => {
          Assert.That (list[0], Is.EqualTo (0));
          Assert.That (list[1], Is.EqualTo (1));
        });
      }
      cachedQueue.Enqueue (2);
      Assert.That (cachedQueue.Count, Is.EqualTo (3));
      {
        IList<int> list = cachedQueue.Peek (3);
        Assert.Multiple (() => {
          Assert.That (list[0], Is.EqualTo (0));
          Assert.That (list[1], Is.EqualTo (1));
          Assert.That (list[2], Is.EqualTo (2));
        });
      }

      Assert.Multiple (() => {
        Assert.That (cachedQueue.Dequeue (), Is.EqualTo (0));
        Assert.That (cachedQueue.Peek (), Is.EqualTo (1));
      });
      cachedQueue.UnsafeDequeue ();
      Assert.Multiple (() => {
        Assert.That (cachedQueue.Dequeue (), Is.EqualTo (2));
        Assert.That (cachedQueue.Count, Is.EqualTo (0));
      });
    }
  }
}
