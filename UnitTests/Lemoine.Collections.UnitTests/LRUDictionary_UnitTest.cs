// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.Collections.UnitTests
{
  /// <summary>
  /// Unit tests for the class LRUDictionary
  /// </summary>
  [TestFixture]
  public class LRUDictionary_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (LRUDictionary_UnitTest).FullName);

    /// <summary>
    /// First set of test for LRUDictionary
    /// </summary>
    [Test]
    public void TestOne()
    {
      LRUDictionary<int, string> lru = new LRUDictionary<int, string> (3);
      string s;
      bool result = lru.TryGetValue (0, out s);
      Assert.That (result, Is.False);
      lru.Add (1, "1");
      lru.Add (2, "2");
      lru.Add (3, "3");
      lru.Add (4, "4");
      result = lru.TryGetValue (1, out s);
      Assert.That (result, Is.False);
      result = lru.TryGetValue (2, out s);
      Assert.Multiple (() => {
        Assert.That (result, Is.True);
        Assert.That (s, Is.EqualTo ("2"));
      });
      result = lru.TryGetValue (4, out s);
      Assert.Multiple (() => {
        Assert.That (result, Is.True);
        Assert.That (s, Is.EqualTo ("4"));
      });
      lru.Remove (2);
      result = lru.TryGetValue (2, out s);
      Assert.That (result, Is.False);
      lru.Add (5, "5");
      lru.Touch (3);
      lru.Add (6, "6");
      lru.Add (7, "7");
      result = lru.TryGetValue (5, out s);
      Assert.That (result, Is.False);
      result = lru.TryGetValue (3, out s);
      Assert.Multiple (() => {
        Assert.That (result, Is.True);
        Assert.That (s, Is.EqualTo ("3"));
      });
      result = lru.TryGetValue (6, out s);
      Assert.Multiple (() => {
        Assert.That (result, Is.True);
        Assert.That (s, Is.EqualTo ("6"));
      });
    }
  }
}
