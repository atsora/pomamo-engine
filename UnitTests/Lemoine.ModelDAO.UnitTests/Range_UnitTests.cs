// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using NUnit.Framework;

using Lemoine.Model;

namespace Lemoine.ModelDAO.UnitTests
{
  /// <summary>
  /// Unit tests for the generic Range class
  /// </summary>
  [TestFixture]
  public class Range_UnitTests
  {
    static readonly ILog log = LogManager.GetLogger(typeof (Range_UnitTests).FullName);

    /// <summary>
    /// Test the constructors, IsEmpty and ToString
    /// </summary>
    [Test]
    public void TestConstructor ()
    {
      {
        Range<double> range = new Range<double> (1, 4);
        Assert.AreEqual (1, range.Lower.Value);
        Assert.AreEqual (4, range.Upper.Value);
        Assert.AreEqual (true, range.LowerInclusive);
        Assert.AreEqual (false, range.UpperInclusive);
        Assert.AreEqual (false, range.IsEmpty ());
        Assert.AreEqual ("[1,4)", range.ToString ());
      }
      {
        Range<double> range = new Range<double> (1, 4, "()");
        Assert.AreEqual (1, range.Lower.Value);
        Assert.AreEqual (4, range.Upper.Value);
        Assert.AreEqual (false, range.LowerInclusive);
        Assert.AreEqual (false, range.UpperInclusive);
        Assert.AreEqual (false, range.IsEmpty ());
        Assert.AreEqual ("(1,4)", range.ToString ());
      }
      {
        Range<double> range = new Range<double> (1, 4, "[]");
        Assert.AreEqual (1, range.Lower.Value);
        Assert.AreEqual (4, range.Upper.Value);
        Assert.AreEqual (true, range.LowerInclusive);
        Assert.AreEqual (true, range.UpperInclusive);
        Assert.AreEqual (false, range.IsEmpty ());
        Assert.AreEqual ("[1,4]", range.ToString ());
      }
      {
        Range<double> range = new Range<double> (4, 4, "[]");
        Assert.AreEqual (4, range.Lower.Value);
        Assert.AreEqual (4, range.Upper.Value);
        Assert.AreEqual (true, range.LowerInclusive);
        Assert.AreEqual (true, range.UpperInclusive);
        Assert.AreEqual (false, range.IsEmpty ());
        Assert.AreEqual ("[4,4]", range.ToString ());
      }
      {
        Range<double> range = new Range<double> (1, 4, "(]");
        Assert.AreEqual (1, range.Lower.Value);
        Assert.AreEqual (4, range.Upper.Value);
        Assert.AreEqual (false, range.LowerInclusive);
        Assert.AreEqual (true, range.UpperInclusive);
        Assert.AreEqual (false, range.IsEmpty ());
        Assert.AreEqual ("(1,4]", range.ToString ());
      }
      {
        Range<double> range = new Range<double> (1, 4, "[)");
        Assert.AreEqual (1, range.Lower.Value);
        Assert.AreEqual (4, range.Upper.Value);
        Assert.AreEqual (true, range.LowerInclusive);
        Assert.AreEqual (false, range.UpperInclusive);
        Assert.AreEqual (false, range.IsEmpty ());
        Assert.AreEqual ("[1,4)", range.ToString ());
      }
      {
        Range<double> range = new Range<double> ();
        Assert.AreEqual (true, range.IsEmpty ());
        Assert.Throws<InvalidOperationException> (new TestDelegate (delegate () { LowerBound<double> lower = range.Lower; }));
        Assert.Throws<InvalidOperationException> (new TestDelegate (delegate () { UpperBound<double> upper = range.Upper; }));
        Assert.AreEqual (false, range.LowerInclusive);
        Assert.AreEqual (false, range.UpperInclusive);
        Assert.AreEqual ("empty", range.ToString ());
      }
      {
        Range<double> range = new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "[]");
        Assert.AreEqual (false, range.Lower.HasValue);
        Assert.AreEqual (false, range.Upper.HasValue);
        Assert.AreEqual (false, range.LowerInclusive);
        Assert.AreEqual (false, range.UpperInclusive);
        Assert.AreEqual (false, range.IsEmpty ());
        Assert.AreEqual ("(,)", range.ToString ());
      }
      {
        Range<double> range = new Range<double> (new LowerBound<double> (null), 4, "[]");
        Assert.AreEqual (false, range.Lower.HasValue);
        Assert.AreEqual (true, range.Upper.HasValue);
        Assert.AreEqual (false, range.LowerInclusive);
        Assert.AreEqual (true, range.UpperInclusive);
        Assert.AreEqual (false, range.IsEmpty ());
        Assert.AreEqual ("(,4]", range.ToString ());
      }
      {
        Range<double> range = new Range<double> (1, new UpperBound<double> (null), "[]");
        Assert.AreEqual (true, range.Lower.HasValue);
        Assert.AreEqual (false, range.Upper.HasValue);
        Assert.AreEqual (true, range.LowerInclusive);
        Assert.AreEqual (false, range.UpperInclusive);
        Assert.AreEqual (false, range.IsEmpty ());
        Assert.AreEqual ("[1,)", range.ToString ());
      }
    }
    
    /// <summary>
    /// Test the Clone method
    /// </summary>
    [Test]
    public void TestClone ()
    {
      Range<double> src = new Range<double> (1, 4, "[)");
      Range<double> range = (Range<double>) src.Clone ();
      Assert.AreEqual (1, range.Lower.Value);
      Assert.AreEqual (4, range.Upper.Value);
      Assert.AreEqual (true, range.LowerInclusive);
      Assert.AreEqual (false, range.UpperInclusive);
      Assert.AreEqual (false, range.IsEmpty ());
      Assert.AreEqual ("[1,4)", range.ToString ());
      Assert.IsTrue (src.Equals (range));
    }
    
    /// <summary>
    /// Test the contains range method
    /// </summary>
    [Test]
    public void TestContainsRange ()
    {
      Assert.IsFalse (new Range<double> ().ContainsRange (new Range<double>()));
      Assert.IsFalse (new Range<double> ().ContainsRange (new Range<double> (1, 4)));

      Assert.IsTrue (new Range<double> (1, 4).ContainsRange (new Range<double> ()));
      Assert.IsTrue (new Range<double> (1, 4).ContainsRange (new Range<double> (1, 3)));
      Assert.IsTrue (new Range<double> (1, 4).ContainsRange (new Range<double> (1, 4)));
      Assert.IsTrue (new Range<double> (1, 4, "(]").ContainsRange (new Range<double> (1, 4, "()")));
      Assert.IsFalse (new Range<double> (1, 4, "()").ContainsRange (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (1, 4, "[)").ContainsRange (new Range<double> (1, 4, "[]")));
      Assert.IsFalse (new Range<double> (1, 4).ContainsRange (new Range<double> (4, 4, "[]")));
      Assert.IsFalse (new Range<double> (1, 4).ContainsRange (new Range<double> (2, 5)));
      Assert.IsFalse (new Range<double> (1, 4).ContainsRange (new Range<double> (0, 3)));
      Assert.IsFalse (new Range<double> (1, 4).ContainsRange (new Range<double> (6, 50)));
      Assert.IsFalse (new Range<double> (1, 4).ContainsRange (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (1, 4).ContainsRange (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (1, 4).ContainsRange (new Range<double> (new LowerBound<double> (null), 4)));

      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> ()));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (1, 4)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "(]").ContainsRange (new Range<double> (1, 4, "()")));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "()").ContainsRange (new Range<double> (1, 4)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "[)").ContainsRange (new Range<double> (1, 4, "[]")));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (4, 4, "[]")));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (2, 5)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (0, 3)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (6, 50)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (new LowerBound<double> (null), 4)));

      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> ()));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (1, 4)));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null), "(]").ContainsRange (new Range<double> (1, 4, "()")));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null), "()").ContainsRange (new Range<double> (1, 4)));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null), "[)").ContainsRange (new Range<double> (1, 4, "[]")));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (1, 1, "[]")));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (2, 5)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (0, 3)));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (6, 50)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (new LowerBound<double> (null), 4)));
      
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> ()));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (1, 4)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4, "(]").ContainsRange (new Range<double> (1, 4, "()")));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4, "()").ContainsRange (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4, "[)").ContainsRange (new Range<double> (1, 4, "[]")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (4, 4, "[]")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (2, 5)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (0, 3)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (6, 50)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (new LowerBound<double> (null), 4)));
    }

    /// <summary>
    /// Test the ContainsElement method
    /// </summary>
    [Test]
    public void TestContainsElement ()
    {
      Assert.IsTrue (new Range<double> (1, 4).ContainsElement (2));
      Assert.IsTrue (new Range<double> (1, 4).ContainsElement (1));
      Assert.IsFalse (new Range<double> (1, 4).ContainsElement (4));
      Assert.IsFalse (new Range<double> (1, 4).ContainsElement (-3));

      Assert.IsTrue (new Range<double> (1, 4, "(]").ContainsElement (2));
      Assert.IsFalse (new Range<double> (1, 4, "(]").ContainsElement (1));
      Assert.IsTrue (new Range<double> (1, 4, "(]").ContainsElement (4));
      Assert.IsFalse (new Range<double> (1, 4, "(]").ContainsElement (-3));

      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsElement (2));
    }

    /// <summary>
    /// Test the Overlaps method
    /// </summary>
    [Test]
    public void TestOverlaps ()
    {
      Assert.IsFalse (new Range<double> ().Overlaps (new Range<double>()));
      Assert.IsFalse (new Range<double> ().Overlaps (new Range<double> (1, 4)));

      Assert.IsFalse (new Range<double> (1, 4).Overlaps (new Range<double> ()));
      Assert.IsTrue (new Range<double> (1, 4).Overlaps (new Range<double> (1, 3)));
      Assert.IsTrue (new Range<double> (1, 4).Overlaps (new Range<double> (1, 4)));
      Assert.IsTrue (new Range<double> (1, 4, "(]").Overlaps (new Range<double> (1, 4, "()")));
      Assert.IsTrue (new Range<double> (1, 4, "()").Overlaps (new Range<double> (1, 4)));
      Assert.IsTrue (new Range<double> (1, 4, "[)").Overlaps (new Range<double> (1, 4, "[]")));
      Assert.IsFalse (new Range<double> (1, 4).Overlaps (new Range<double> (4, 4, "[]")));
      Assert.IsTrue (new Range<double> (1, 4).Overlaps (new Range<double> (2, 5)));
      Assert.IsTrue (new Range<double> (1, 4).Overlaps (new Range<double> (0, 3)));
      Assert.IsFalse (new Range<double> (1, 4).Overlaps (new Range<double> (6, 50)));
      Assert.IsTrue (new Range<double> (1, 4).Overlaps (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (1, 4).Overlaps (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (1, 4).Overlaps (new Range<double> (new LowerBound<double> (null), 4)));
      
      // Bug 2016-02-19
      Assert.IsFalse (new Range<int> (1,3, "[)").Overlaps (new Range<int> (3, new UpperBound<int> (null), "[)")));
      Assert.IsFalse (new Range<int> (3, new UpperBound<int> (null), "[)").Overlaps (new Range<int> (1, 3, "[)")));
      
      Assert.IsFalse (new Range<int> (2, 4).Overlaps (new Range<int> (10, 15)));
      Assert.IsFalse (new Range<int> (10, 15).Overlaps (new Range<int> (2, 4)));
      Assert.IsFalse (new Range<int> (10, 15).Overlaps (new Range<int> (2, 10)));
      Assert.IsTrue (new Range<int> (10, 15).Overlaps (new Range<int> (2, 10, "[]")));

      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> ()));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (1, 4)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "(]").Overlaps (new Range<double> (1, 4, "()")));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "()").Overlaps (new Range<double> (1, 4)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "[)").Overlaps (new Range<double> (1, 4, "[]")));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (4, 4, "[]")));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (2, 5)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (0, 3)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (6, 50)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (new LowerBound<double> (null), 4)));

      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> ()));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (1, 4)));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null), "(]").Overlaps (new Range<double> (1, 4, "()")));
      Assert.IsTrue(new Range<double> (1, new UpperBound<double> (null), "()").Overlaps (new Range<double> (1, 4)));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null), "[)").Overlaps (new Range<double> (1, 4, "[]")));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (1, 1, "[]")));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (2, 5)));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (0, 3)));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (6, 50)));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (new LowerBound<double> (null), 4)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (new LowerBound<double> (null), 1)));
      
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> ()));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (1, 4)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4, "(]").Overlaps (new Range<double> (1, 4, "()")));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4, "()").Overlaps (new Range<double> (1, 4)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4, "[)").Overlaps (new Range<double> (1, 4, "[]")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (4, 4, "[]")));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (2, 5)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (0, 3)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (6, 50)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (4, new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (new LowerBound<double> (null), 4)));
    }

    /// <summary>
    /// Test the IsStrictlyLeftOf method
    /// </summary>
    [Test]
    public void TestIsStrictlyLeftOf ()
    {
      Assert.IsFalse (new Range<double> ().IsStrictlyLeftOf (new Range<double>()));
      Assert.IsFalse (new Range<double> ().IsStrictlyLeftOf (new Range<double> (1, 4)));

      Assert.IsFalse (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> ()));
      Assert.IsFalse (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (1, 3)));
      Assert.IsTrue (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (4, 5)));
      Assert.IsTrue (new Range<double> (1, 4, "(]").IsStrictlyLeftOf (new Range<double> (4, 5, "()")));
      Assert.IsTrue (new Range<double> (1, 4, "()").IsStrictlyLeftOf (new Range<double> (4, 5)));
      Assert.IsTrue (new Range<double> (1, 4, "[)").IsStrictlyLeftOf (new Range<double> (4, 5, "[]")));
      Assert.IsFalse (new Range<double> (1, 4, "[]").IsStrictlyLeftOf (new Range<double> (4, 5, "[]")));
      Assert.IsTrue (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (4, 4, "[]")));
      Assert.IsFalse (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (2, 5)));
      Assert.IsFalse (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (0, 3)));
      Assert.IsTrue (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (6, 50)));
      Assert.IsFalse (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (4, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), 1)));

      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> ()));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "(]").IsStrictlyLeftOf (new Range<double> (1, 4, "()")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "()").IsStrictlyLeftOf (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "[)").IsStrictlyLeftOf (new Range<double> (1, 4, "[]")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (4, 4, "[]")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (2, 5)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (0, 3)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (6, 50)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), 4)));

      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> ()));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null), "(]").IsStrictlyLeftOf (new Range<double> (1, 4, "()")));
      Assert.IsFalse(new Range<double> (1, new UpperBound<double> (null), "()").IsStrictlyLeftOf (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null), "[)").IsStrictlyLeftOf (new Range<double> (1, 4, "[]")));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (1, 1, "[]")));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (2, 5)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (0, 3)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (6, 50)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), 4)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), 1)));
      
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> ()));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4, "(]").IsStrictlyLeftOf (new Range<double> (1, 4, "()")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4, "()").IsStrictlyLeftOf (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4, "[)").IsStrictlyLeftOf (new Range<double> (1, 4, "[]")));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (4, 4, "[]")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (2, 5)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (0, 3)));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (6, 50)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (4, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (3, new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (5, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4, "[]").IsStrictlyLeftOf (new Range<double> (4, new UpperBound<double> (null))));
    }

    /// <summary>
    /// Test the IsStrictlyRightOf method
    /// </summary>
    [Test]
    public void TestIsStrictlyRightOf ()
    {
      Assert.IsFalse (new Range<double> ().IsStrictlyRightOf (new Range<double>()));
      Assert.IsFalse (new Range<double> ().IsStrictlyRightOf (new Range<double> (1, 4)));

      Assert.IsFalse (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> ()));
      Assert.IsFalse (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (1, 3)));
      Assert.IsTrue (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (0, 1)));
      Assert.IsTrue (new Range<double> (1, 4, "(]").IsStrictlyRightOf (new Range<double> (0, 1, "()")));
      Assert.IsTrue (new Range<double> (1, 4, "()").IsStrictlyRightOf (new Range<double> (0, 1)));
      Assert.IsTrue (new Range<double> (1, 4, "(]").IsStrictlyRightOf (new Range<double> (0, 1, "[]")));
      Assert.IsFalse (new Range<double> (1, 4, "[]").IsStrictlyRightOf (new Range<double> (0, 1, "[]")));
      Assert.IsTrue (new Range<double> (1, 4, "(]").IsStrictlyRightOf (new Range<double> (1, 1, "[]")));
      Assert.IsFalse (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (0, 3)));
      Assert.IsFalse (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (2, 5)));
      Assert.IsTrue (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (0, 1)));
      Assert.IsFalse (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (4, new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), 1)));
      Assert.IsFalse (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), 1, "[]")));

      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> ()));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "(]").IsStrictlyRightOf (new Range<double> (1, 4, "()")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "()").IsStrictlyRightOf (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "[)").IsStrictlyRightOf (new Range<double> (1, 4, "[]")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (4, 4, "[]")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (2, 5)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (0, 3)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (6, 50)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), 4)));

      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> ()));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null), "(]").IsStrictlyRightOf (new Range<double> (1, 4, "()")));
      Assert.IsFalse(new Range<double> (1, new UpperBound<double> (null), "()").IsStrictlyRightOf (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null), "[)").IsStrictlyRightOf (new Range<double> (1, 4, "[]")));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (1, 1, "[]")));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null), "()").IsStrictlyRightOf (new Range<double> (1, 1, "[]")));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (-1, 0)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (0, 3)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (6, 50)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), 4)));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), 1)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), 1, "[]")));
      
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> ()));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4, "(]").IsStrictlyRightOf (new Range<double> (1, 4, "()")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4, "()").IsStrictlyRightOf (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4, "[)").IsStrictlyRightOf (new Range<double> (1, 4, "[]")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (4, 4, "[]")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (2, 5)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (0, 3)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (6, 50)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (3, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (5, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4, "[]").IsStrictlyRightOf (new Range<double> (4, new UpperBound<double> (null))));
    }
    
    /// <summary>
    /// Test the IsAdjacentTo method
    /// </summary>
    [Test]
    public void TestIsAdjacentTo ()
    {
      Assert.IsFalse (new Range<double> ().IsAdjacentTo (new Range<double>()));
      Assert.IsFalse (new Range<double> ().IsAdjacentTo (new Range<double> (1, 4)));

      Assert.IsFalse (new Range<double> (1, 4).IsAdjacentTo (new Range<double> ()));
      Assert.IsFalse (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (1, 3)));
      Assert.IsTrue (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (4, 5)));
      Assert.IsTrue (new Range<double> (1, 4, "(]").IsAdjacentTo (new Range<double> (4, 5, "()")));
      Assert.IsTrue (new Range<double> (1, 4, "()").IsAdjacentTo (new Range<double> (4, 5)));
      Assert.IsTrue (new Range<double> (1, 4, "[)").IsAdjacentTo (new Range<double> (4, 5, "[]")));
      Assert.IsFalse (new Range<double> (1, 4, "[]").IsAdjacentTo (new Range<double> (4, 5, "[]")));
      Assert.IsTrue (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (4, 4, "[]")));
      Assert.IsFalse (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (2, 5)));
      Assert.IsFalse (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (0, 3)));
      Assert.IsTrue (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (0, 1)));
      Assert.IsTrue (new Range<double> (1, 4, "()").IsAdjacentTo (new Range<double> (0, 1, "[]")));
      Assert.IsFalse (new Range<double> (1, 4, "()").IsAdjacentTo (new Range<double> (0, 1)));
      Assert.IsFalse (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (6, 50)));
      Assert.IsFalse (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (4, new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 1)));
      Assert.IsFalse (new Range<double> (1, 4, "()").IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 1)));
      Assert.IsTrue (new Range<double> (1, 4, "()").IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 1, "(]")));

      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> ()));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "(]").IsAdjacentTo (new Range<double> (1, 4, "()")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "()").IsAdjacentTo (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "[)").IsAdjacentTo (new Range<double> (1, 4, "[]")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (4, 4, "[]")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (2, 5)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (0, 3)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (6, 50)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 4)));

      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> ()));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null), "(]").IsAdjacentTo (new Range<double> (1, 4, "()")));
      Assert.IsFalse(new Range<double> (1, new UpperBound<double> (null), "()").IsAdjacentTo (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null), "[)").IsAdjacentTo (new Range<double> (1, 4, "[]")));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (1, 1, "[]")));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (2, 5)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (0, 3)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (6, 50)));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 4)));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 1)));
      Assert.IsTrue (new Range<double> (1, new UpperBound<double> (null), "()").IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 1, "[]")));
      Assert.IsFalse (new Range<double> (1, new UpperBound<double> (null), "()").IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 1, "()")));
      
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> ()));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4, "(]").IsAdjacentTo (new Range<double> (1, 4, "()")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4, "()").IsAdjacentTo (new Range<double> (1, 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4, "[)").IsAdjacentTo (new Range<double> (1, 4, "[]")));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (4, 4, "[]")));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (2, 5)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (0, 3)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (6, 50)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (1, new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (4, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 4)));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (3, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (5, new UpperBound<double> (null))));
      Assert.IsFalse (new Range<double> (new LowerBound<double> (null), 4, "[]").IsAdjacentTo (new Range<double> (4, new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4, "()").IsAdjacentTo (new Range<double> (4, new UpperBound<double> (null))));
      Assert.IsTrue (new Range<double> (new LowerBound<double> (null), 4, "[]").IsAdjacentTo (new Range<double> (4, new UpperBound<double> (null), "()")));
      
      Assert.IsTrue (new DayRange (new DateTime (2016, 01, 05), new DateTime (2016, 01, 06)).IsAdjacentTo (new DayRange (new DateTime (2016, 01, 07), new DateTime (2016, 01, 08))));
      Assert.IsTrue (new DayRange (new DateTime (2016, 01, 05), new DateTime (2016, 01, 06)).IsAdjacentTo (new DayRange (new DateTime (2016, 01, 03), new DateTime (2016, 01, 04))));
      Assert.IsFalse (new DayRange (new DateTime (2016, 01, 05), new DateTime (2016, 01, 06)).IsAdjacentTo (new DayRange (new DateTime (2016, 01, 03), new DateTime (2016, 01, 05))));
    }

    /// <summary>
    /// Test the union of two ranges
    /// </summary>
    [Test]
    public void TestUnion ()
    {
      Assert.Throws<ArgumentException> (new TestDelegate (delegate {new Range<double> (1, 3).Union (new Range<double> (4, 5)); }));
      Assert.Throws<ArgumentException> (new TestDelegate (delegate {new Range<double> (new LowerBound<double> (null), 1).Union (new Range<double> (2, new UpperBound<double> (null))); }));
      Assert.Throws<ArgumentException> (new TestDelegate (delegate {new Range<double> (new LowerBound<double> (null), 1, "()").Union (new Range<double> (1, new UpperBound<double> (null), "()")); }));
      
      Assert.AreEqual (new Range<double> (),
                       new Range<double> ().Union (new Range<double> ()));
      Assert.AreEqual (new Range<double> (1,5),
                       new Range<double> ().Union (new Range<double> (1, 5)));
      Assert.AreEqual (new Range<double> (1,5),
                       new Range<double> (1, 5).Union (new Range<double> ()));
      Assert.AreEqual (new Range<double> (1,5),
                       new Range<double> (1, 3).Union (new Range<double> (3, 5)));
      Assert.AreEqual (new Range<double> (1,5),
                       new Range<double> (1, 5).Union (new Range<double> (2, 3)));
      Assert.AreEqual (new Range<double> (1,5),
                       new Range<double> (1, 3).Union (new Range<double> (2, 5)));
      Assert.AreEqual (new Range<double> (1,5, "[]"),
                       new Range<double> (1, 3, "[]").Union (new Range<double> (3, 5, "[]")));
      Assert.AreEqual (new Range<double> (1,5, "()"),
                       new Range<double> (1, 3, "()").Union (new Range<double> (2, 5, "()")));
      Assert.AreEqual (new Range<double> (1,5),
                       new Range<double> (3, 5).Union (new Range<double> (1, 3)));
      Assert.AreEqual (new Range<double> (1,5),
                       new Range<double> (2, 5).Union (new Range<double> (1, 3)));
      Assert.AreEqual (new Range<double> (1,5, "[]"),
                       new Range<double> (3, 5, "[]").Union (new Range<double> (1, 3, "[]")));
      Assert.AreEqual (new Range<double> (1,5, "()"),
                       new Range<double> (2, 5, "()").Union (new Range<double> (1, 3, "()")));
      
      Assert.AreEqual (new Range<double> (1, new UpperBound<double> (null)),
                       new Range<double> (1, 3).Union (new Range<double> (3, new UpperBound<double> (null))));
      Assert.AreEqual (new Range<double> (1, new UpperBound<double> (null)),
                       new Range<double> (1, 3).Union (new Range<double> (2, new UpperBound<double> (null))));
      Assert.AreEqual (new Range<double> (1, new UpperBound<double> (null)),
                       new Range<double> (1, new UpperBound<double> (null)).Union (new Range<double> (3, new UpperBound<double> (null))));
      Assert.AreEqual (new Range<double> (1, new UpperBound<double> (null)),
                       new Range<double> (1, new UpperBound<double> (null)).Union (new Range<double> (3, 5)));

      Assert.AreEqual (new Range<double> (new LowerBound<double> (null), 5),
                       new Range<double> (new LowerBound<double> (null), 5).Union (new Range<double> (3, 5)));
      Assert.AreEqual (new Range<double> (new LowerBound<double> (null), 5),
                       new Range<double> (new LowerBound<double> (null), 5).Union (new Range<double> (new LowerBound<double> (null), 3)));
      Assert.AreEqual (new Range<double> (new LowerBound<double> (null), 5),
                       new Range<double> (2, 5).Union (new Range<double> (new LowerBound<double> (null), 3)));
      Assert.AreEqual (new Range<double> (new LowerBound<double> (null), 5, "[]"),
                       new Range<double> (2, 5, "[]").Union (new Range<double> (new LowerBound<double> (null), 3)));

      Assert.AreEqual (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)),
                       new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Union (new Range<double> (3, 5)));
      Assert.AreEqual (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)),
                       new Range<double> (new LowerBound<double> (null), 3).Union (new Range<double> (3, new UpperBound<double> (null))));
      Assert.AreEqual (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)),
                       new Range<double> (new LowerBound<double> (null), 5).Union (new Range<double> (3, new UpperBound<double> (null))));
      Assert.AreEqual (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)),
                       new Range<double> (new LowerBound<double> (null), 3, "[]").Union (new Range<double> (3, new UpperBound<double> (null), "()")));
      
      Assert.AreEqual (new DayRange (new DateTime (2016, 01, 05), new DateTime (2016, 01, 08)),
                       new DayRange (new DateTime (2016, 01, 05), new DateTime (2016, 01, 06)).Union (new DayRange (new DateTime (2016, 01, 07), new DateTime (2016, 01, 08))));
      Assert.AreEqual (new DayRange (new DateTime (2016, 01, 03), new DateTime (2016, 01, 06)),
                       new DayRange (new DateTime (2016, 01, 05), new DateTime (2016, 01, 06)).Union (new DayRange (new DateTime (2016, 01, 03), new DateTime (2016, 01, 04))));
    }

    /// <summary>
    /// Test the Intersects method of two ranges
    /// </summary>
    [Test]
    public void TestIntersects ()
    {
      Assert.AreEqual (new Range<double> (),
                       new Range<double> ().Intersects (new Range<double> ()));
      Assert.AreEqual (new Range<double> (),
                       new Range<double> ().Intersects (new Range<double> (1, 3)));
      Assert.AreEqual (new Range<double> (),
                       new Range<double> (2, 4).Intersects (new Range<double> ()));
      Assert.AreEqual (new Range<double> (),
                       new Range<double> (1, 3).Intersects (new Range<double> (4, 5)));
      Assert.AreEqual (new Range<double> (),
                       new Range<double> (1, 3, "()").Intersects (new Range<double> (3, 5, "()")));
      Assert.AreEqual (new Range<double> (3, 3, "[]"),
                       new Range<double> (1, 3, "[]").Intersects (new Range<double> (3, 5)));
      Assert.AreEqual (new Range<double> (1, 3),
                       new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Intersects (new Range<double> (1, 3)));
      Assert.AreEqual (new Range<double> (1, 3),
                       new Range<double> (1, 3).Intersects (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
      Assert.AreEqual (new Range<double> (2, 3),
                       new Range<double> (new LowerBound<double> (null), 3).Intersects (new Range<double> (2, new UpperBound<double> (null))));
      Assert.AreEqual (new Range<double> (2, 3, "[]"),
                       new Range<double> (new LowerBound<double> (null), 3, "[]").Intersects (new Range<double> (2, new UpperBound<double> (null))));
      Assert.AreEqual (new Range<double> (),
                       new Range<double> (new LowerBound<double> (null), 3).Intersects (new Range<double> (4, new UpperBound<double> (null))));
    }
    
    [Test]
    public void TestParse ()
    {
      {
        UtcDateTimeRange range = new UtcDateTimeRange ("[\"2015-06-03 04:00:00\",)");
        Assert.IsFalse (range.Upper.HasValue);
        Assert.AreEqual (new DateTime (2015, 06, 03, 04, 00, 00, DateTimeKind.Utc), range.Lower.Value);
      }
      {
        UtcDateTimeRange range = new UtcDateTimeRange ("'[\"2015-06-03 04:00:00\",)'");
        Assert.IsFalse (range.Upper.HasValue);
        Assert.AreEqual (new DateTime (2015, 06, 03, 04, 00, 00, DateTimeKind.Utc), range.Lower.Value);
      }
    }
  }
}
