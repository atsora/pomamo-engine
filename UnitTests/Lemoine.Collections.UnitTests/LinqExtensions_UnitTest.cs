// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.Collections.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class LinqExtensions_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (LinqExtensions_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public LinqExtensions_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestSame ()
    {
      var set1 = new List<string> { "test", "test", "test" };
      Assert.IsTrue (set1.Same ());

      var set2 = new List<string> { "test", "Test", "test" };
      Assert.IsFalse (set2.Same ());
    }

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void TestUniqueOrDefault ()
    {
      {
        var set = new List<string> { "test", "test", "test" };
        Assert.AreEqual ("test", set.UniqueOrDefault ());
      }

      {
        var set = new List<string> { "test", "Test", "test" };
        Assert.AreEqual (null, set.UniqueOrDefault ());
      }

      {
        var set = new List<string> { };
        Assert.AreEqual (null, set.UniqueOrDefault ());
      }
    }
  }
}
