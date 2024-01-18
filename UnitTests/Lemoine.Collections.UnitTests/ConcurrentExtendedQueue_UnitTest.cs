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
      Assert.That (queue.Count, Is.EqualTo (1));
      Assert.Multiple (() => {
        Assert.That (queue.Peek (), Is.EqualTo (0));
        Assert.That (queue.Dequeue (), Is.EqualTo (0));
        Assert.That (queue.Count, Is.EqualTo (0));
      });
      queue.Enqueue (0);
      queue.UnsafeDequeue ();
      queue.Enqueue (0);
      queue.Enqueue (1);
      Assert.That (queue.Count, Is.EqualTo (2));
      {
        IList<int> list = queue.Peek (2);
        Assert.Multiple (() => {
          Assert.That (list[0], Is.EqualTo (0));
          Assert.That (list[1], Is.EqualTo (1));
        });
      }
      queue.Enqueue (2);
      Assert.That (queue.Count, Is.EqualTo (3));
      {
        IList<int> list = queue.Peek (3);
        Assert.Multiple (() => {
          Assert.That (list[0], Is.EqualTo (0));
          Assert.That (list[1], Is.EqualTo (1));
          Assert.That (list[2], Is.EqualTo (2));
        });
      }

      Assert.Multiple (() => {
        Assert.That (queue.Dequeue (), Is.EqualTo (0));
        Assert.That (queue.Peek (), Is.EqualTo (1));
      });
      queue.UnsafeDequeue ();
      Assert.Multiple (() => {
        Assert.That (queue.Dequeue (), Is.EqualTo (2));
        Assert.That (queue.Count, Is.EqualTo (0));
      });
    }
  }
}
