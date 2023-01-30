// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using NUnit.Framework;

using Lemoine.Collections;

namespace Lemoine.Collections.UnitTests
{
  /// <summary>
  /// Unit tests for the class EnumerableString
  /// </summary>
  [TestFixture]
  public class EnumerableString_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (EnumerableString_UnitTest).FullName);

    /// <summary>
    /// Generic ToListString / Parse
    /// </summary>
    [Test]
    public void ListStringTest()
    {
      { // int
        var list = new System.Collections.Generic.List<int> {1, 2, 3};
        var s = list.ToListString ();
        Assert.AreEqual (";1;2;3", s);
        var l = EnumerableString.ParseListString (s);
        Assert.AreEqual (s, l.ToListString ());
        var l2 = EnumerableString.ParseListString<int> (s);
        Assert.AreEqual (1, l2.First());
        Assert.AreEqual (3, l2.Count ());
        Assert.AreEqual (3, l2.Last ());
        var l3 = EnumerableString.ParseListString (s, typeof(int)).Cast<int> ();
        Assert.AreEqual (1, l3.First());
        Assert.AreEqual (3, l3.Count ());
        Assert.AreEqual (3, l3.Last ());        
      }
      { // int / type
        var list = new System.Collections.Generic.List<int> {1, 2, 3};
        var s = list.ToListString (true);
        Assert.AreEqual ("<Int32>;1;2;3", s);
        var l = ((System.Collections.IEnumerable)EnumerableString.ParseAuto (s)).Cast<int> ();
        Assert.AreEqual (1, l.First());
        Assert.AreEqual (3, l.Count ());
        Assert.AreEqual (3, l.Last ());        
      }
    }

    /// <summary>
    /// Generic ToDictionaryString / Parse
    /// </summary>
    [Test]
    public void DictionaryStringTest()
    {
      { // int
        var d = new System.Collections.Generic.Dictionary<string, int> ();
        d["a"] = 1;
        d["b"] = 2;
        var s = d.ToDictionaryString ();
        Assert.AreEqual (":;a:1;b:2", s);
        var l = EnumerableString.ParseDictionaryString (s);
        Assert.AreEqual (s, l.ToDictionaryString ());
        var l2 = EnumerableString.ParseDictionaryString<string, int> (s);
        Assert.AreEqual (1, l2["a"]);
        Assert.AreEqual (2, l2["b"]);
        var l3 = EnumerableString.ParseDictionaryString (s, typeof(string), typeof(int));
        Assert.AreEqual (1, l3["a"]);
        Assert.AreEqual (2, l3["b"]);
      }
      { // int / type
        var d = new System.Collections.Generic.Dictionary<string, int> ();
        d["a"] = 1;
        d["b"] = 2;
        var s = d.ToDictionaryString (true);
        Assert.AreEqual ("<String,Int32>:;a:1;b:2", s);
        var l = ((System.Collections.IDictionary)EnumerableString.ParseAuto (s));
        Assert.AreEqual (1, l["a"]);
        Assert.AreEqual (2, l["b"]);
      }
    }
  }
}
