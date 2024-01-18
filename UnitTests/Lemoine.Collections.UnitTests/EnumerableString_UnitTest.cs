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
        Assert.That (s, Is.EqualTo (";1;2;3"));
        var l = EnumerableString.ParseListString (s);
        Assert.That (l.ToListString (), Is.EqualTo (s));
        var l2 = EnumerableString.ParseListString<int> (s);
        Assert.Multiple (() => {
          Assert.That (l2.First (), Is.EqualTo (1));
          Assert.That (l2.Count (), Is.EqualTo (3));
          Assert.That (l2.Last (), Is.EqualTo (3));
        });
        var l3 = EnumerableString.ParseListString (s, typeof(int)).Cast<int> ();
        Assert.Multiple (() => {
          Assert.That (l3.First (), Is.EqualTo (1));
          Assert.That (l3.Count (), Is.EqualTo (3));
          Assert.That (l3.Last (), Is.EqualTo (3));
        });
      }
      { // int / type
        var list = new System.Collections.Generic.List<int> {1, 2, 3};
        var s = list.ToListString (true);
        Assert.That (s, Is.EqualTo ("<Int32>;1;2;3"));
        var l = ((System.Collections.IEnumerable)EnumerableString.ParseAuto (s)).Cast<int> ();
        Assert.Multiple (() => {
          Assert.That (l.First (), Is.EqualTo (1));
          Assert.That (l.Count (), Is.EqualTo (3));
          Assert.That (l.Last (), Is.EqualTo (3));
        });
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
        Assert.That (s, Is.EqualTo (":;a:1;b:2"));
        var l = EnumerableString.ParseDictionaryString (s);
        Assert.That (l.ToDictionaryString (), Is.EqualTo (s));
        var l2 = EnumerableString.ParseDictionaryString<string, int> (s);
        Assert.Multiple (() => {
          Assert.That (l2["a"], Is.EqualTo (1));
          Assert.That (l2["b"], Is.EqualTo (2));
        });
        var l3 = EnumerableString.ParseDictionaryString (s, typeof(string), typeof(int));
        Assert.Multiple (() => {
          Assert.That (l3["a"], Is.EqualTo (1));
          Assert.That (l3["b"], Is.EqualTo (2));
        });
      }
      { // int / type
        var d = new System.Collections.Generic.Dictionary<string, int> ();
        d["a"] = 1;
        d["b"] = 2;
        var s = d.ToDictionaryString (true);
        Assert.That (s, Is.EqualTo ("<String,Int32>:;a:1;b:2"));
        var l = ((System.Collections.IDictionary)EnumerableString.ParseAuto (s));
        Assert.Multiple (() => {
          Assert.That (l["a"], Is.EqualTo (1));
          Assert.That (l["b"], Is.EqualTo (2));
        });
      }
    }
  }
}
