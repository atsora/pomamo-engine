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
    static readonly ILog log = LogManager.GetLogger (typeof (Range_UnitTests).FullName);

    /// <summary>
    /// Test the constructors, IsEmpty and ToString
    /// </summary>
    [Test]
    public void TestConstructor ()
    {
      {
        Range<double> range = new Range<double> (1, 4);
        Assert.Multiple (() => {
          Assert.That (range.Lower.Value, Is.EqualTo (1));
          Assert.That (range.Upper.Value, Is.EqualTo (4));
          Assert.That (range.LowerInclusive, Is.EqualTo (true));
          Assert.That (range.UpperInclusive, Is.EqualTo (false));
          Assert.That (range.IsEmpty (), Is.EqualTo (false));
          Assert.That (range.ToString (), Is.EqualTo ("[1,4)"));
        });
      }
      {
        Range<double> range = new Range<double> (1, 4, "()");
        Assert.Multiple (() => {
          Assert.That (range.Lower.Value, Is.EqualTo (1));
          Assert.That (range.Upper.Value, Is.EqualTo (4));
          Assert.That (range.LowerInclusive, Is.EqualTo (false));
          Assert.That (range.UpperInclusive, Is.EqualTo (false));
          Assert.That (range.IsEmpty (), Is.EqualTo (false));
          Assert.That (range.ToString (), Is.EqualTo ("(1,4)"));
        });
      }
      {
        Range<double> range = new Range<double> (1, 4, "[]");
        Assert.Multiple (() => {
          Assert.That (range.Lower.Value, Is.EqualTo (1));
          Assert.That (range.Upper.Value, Is.EqualTo (4));
          Assert.That (range.LowerInclusive, Is.EqualTo (true));
          Assert.That (range.UpperInclusive, Is.EqualTo (true));
          Assert.That (range.IsEmpty (), Is.EqualTo (false));
          Assert.That (range.ToString (), Is.EqualTo ("[1,4]"));
        });
      }
      {
        Range<double> range = new Range<double> (4, 4, "[]");
        Assert.Multiple (() => {
          Assert.That (range.Lower.Value, Is.EqualTo (4));
          Assert.That (range.Upper.Value, Is.EqualTo (4));
          Assert.That (range.LowerInclusive, Is.EqualTo (true));
          Assert.That (range.UpperInclusive, Is.EqualTo (true));
          Assert.That (range.IsEmpty (), Is.EqualTo (false));
          Assert.That (range.ToString (), Is.EqualTo ("[4,4]"));
        });
      }
      {
        Range<double> range = new Range<double> (1, 4, "(]");
        Assert.Multiple (() => {
          Assert.That (range.Lower.Value, Is.EqualTo (1));
          Assert.That (range.Upper.Value, Is.EqualTo (4));
          Assert.That (range.LowerInclusive, Is.EqualTo (false));
          Assert.That (range.UpperInclusive, Is.EqualTo (true));
          Assert.That (range.IsEmpty (), Is.EqualTo (false));
          Assert.That (range.ToString (), Is.EqualTo ("(1,4]"));
        });
      }
      {
        Range<double> range = new Range<double> (1, 4, "[)");
        Assert.Multiple (() => {
          Assert.That (range.Lower.Value, Is.EqualTo (1));
          Assert.That (range.Upper.Value, Is.EqualTo (4));
          Assert.That (range.LowerInclusive, Is.EqualTo (true));
          Assert.That (range.UpperInclusive, Is.EqualTo (false));
          Assert.That (range.IsEmpty (), Is.EqualTo (false));
          Assert.That (range.ToString (), Is.EqualTo ("[1,4)"));
        });
      }
      {
        Range<double> range = new Range<double> ();
        Assert.That (range.IsEmpty (), Is.EqualTo (true));
        Assert.Throws<InvalidOperationException> (new TestDelegate (delegate () { LowerBound<double> lower = range.Lower; }));
        Assert.Throws<InvalidOperationException> (new TestDelegate (delegate () { UpperBound<double> upper = range.Upper; }));
        Assert.Multiple (() => {
          Assert.That (range.LowerInclusive, Is.EqualTo (false));
          Assert.That (range.UpperInclusive, Is.EqualTo (false));
          Assert.That (range.ToString (), Is.EqualTo ("empty"));
        });
      }
      {
        Range<double> range = new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "[]");
        Assert.Multiple (() => {
          Assert.That (range.Lower.HasValue, Is.EqualTo (false));
          Assert.That (range.Upper.HasValue, Is.EqualTo (false));
          Assert.That (range.LowerInclusive, Is.EqualTo (false));
          Assert.That (range.UpperInclusive, Is.EqualTo (false));
          Assert.That (range.IsEmpty (), Is.EqualTo (false));
          Assert.That (range.ToString (), Is.EqualTo ("(,)"));
        });
      }
      {
        Range<double> range = new Range<double> (new LowerBound<double> (null), 4, "[]");
        Assert.Multiple (() => {
          Assert.That (range.Lower.HasValue, Is.EqualTo (false));
          Assert.That (range.Upper.HasValue, Is.EqualTo (true));
          Assert.That (range.LowerInclusive, Is.EqualTo (false));
          Assert.That (range.UpperInclusive, Is.EqualTo (true));
          Assert.That (range.IsEmpty (), Is.EqualTo (false));
          Assert.That (range.ToString (), Is.EqualTo ("(,4]"));
        });
      }
      {
        Range<double> range = new Range<double> (1, new UpperBound<double> (null), "[]");
        Assert.Multiple (() => {
          Assert.That (range.Lower.HasValue, Is.EqualTo (true));
          Assert.That (range.Upper.HasValue, Is.EqualTo (false));
          Assert.That (range.LowerInclusive, Is.EqualTo (true));
          Assert.That (range.UpperInclusive, Is.EqualTo (false));
          Assert.That (range.IsEmpty (), Is.EqualTo (false));
          Assert.That (range.ToString (), Is.EqualTo ("[1,)"));
        });
      }
    }

    /// <summary>
    /// Test the Clone method
    /// </summary>
    [Test]
    public void TestClone ()
    {
      Range<double> src = new Range<double> (1, 4, "[)");
      Range<double> range = (Range<double>)src.Clone ();
      Assert.Multiple (() => {
        Assert.That (range.Lower.Value, Is.EqualTo (1));
        Assert.That (range.Upper.Value, Is.EqualTo (4));
        Assert.That (range.LowerInclusive, Is.EqualTo (true));
        Assert.That (range.UpperInclusive, Is.EqualTo (false));
        Assert.That (range.IsEmpty (), Is.EqualTo (false));
        Assert.That (range.ToString (), Is.EqualTo ("[1,4)"));
        Assert.That (src, Is.EqualTo (range));
      });
    }

    /// <summary>
    /// Test the contains range method
    /// </summary>
    [Test]
    public void TestContainsRange ()
    {
      Assert.Multiple (() => {
        Assert.That (new Range<double> ().ContainsRange (new Range<double> ()), Is.False);
        Assert.That (new Range<double> ().ContainsRange (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (1, 4).ContainsRange (new Range<double> ()), Is.True);
        Assert.That (new Range<double> (1, 4).ContainsRange (new Range<double> (1, 3)), Is.True);
        Assert.That (new Range<double> (1, 4).ContainsRange (new Range<double> (1, 4)), Is.True);
        Assert.That (new Range<double> (1, 4, "(]").ContainsRange (new Range<double> (1, 4, "()")), Is.True);
        Assert.That (new Range<double> (1, 4, "()").ContainsRange (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (1, 4, "[)").ContainsRange (new Range<double> (1, 4, "[]")), Is.False);
        Assert.That (new Range<double> (1, 4).ContainsRange (new Range<double> (4, 4, "[]")), Is.False);
        Assert.That (new Range<double> (1, 4).ContainsRange (new Range<double> (2, 5)), Is.False);
        Assert.That (new Range<double> (1, 4).ContainsRange (new Range<double> (0, 3)), Is.False);
        Assert.That (new Range<double> (1, 4).ContainsRange (new Range<double> (6, 50)), Is.False);
        Assert.That (new Range<double> (1, 4).ContainsRange (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, 4).ContainsRange (new Range<double> (1, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, 4).ContainsRange (new Range<double> (new LowerBound<double> (null), 4)), Is.False);

        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> ()), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (1, 4)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "(]").ContainsRange (new Range<double> (1, 4, "()")), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "()").ContainsRange (new Range<double> (1, 4)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "[)").ContainsRange (new Range<double> (1, 4, "[]")), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (4, 4, "[]")), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (2, 5)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (0, 3)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (6, 50)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (1, new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsRange (new Range<double> (new LowerBound<double> (null), 4)), Is.True);

        Assert.That (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> ()), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (1, 4)), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "(]").ContainsRange (new Range<double> (1, 4, "()")), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "()").ContainsRange (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "[)").ContainsRange (new Range<double> (1, 4, "[]")), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (1, 1, "[]")), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (2, 5)), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (0, 3)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (6, 50)), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (1, new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).ContainsRange (new Range<double> (new LowerBound<double> (null), 4)), Is.False);

        Assert.That (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> ()), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (1, 4)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "(]").ContainsRange (new Range<double> (1, 4, "()")), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "()").ContainsRange (new Range<double> (1, 4)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "[)").ContainsRange (new Range<double> (1, 4, "[]")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (4, 4, "[]")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (2, 5)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (0, 3)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (6, 50)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (1, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).ContainsRange (new Range<double> (new LowerBound<double> (null), 4)), Is.True);
      });
    }

    /// <summary>
    /// Test the ContainsElement method
    /// </summary>
    [Test]
    public void TestContainsElement ()
    {
      Assert.Multiple (() => {
        Assert.That (new Range<double> (1, 4).ContainsElement (2), Is.True);
        Assert.That (new Range<double> (1, 4).ContainsElement (1), Is.True);
        Assert.That (new Range<double> (1, 4).ContainsElement (4), Is.False);
        Assert.That (new Range<double> (1, 4).ContainsElement (-3), Is.False);

        Assert.That (new Range<double> (1, 4, "(]").ContainsElement (2), Is.True);
        Assert.That (new Range<double> (1, 4, "(]").ContainsElement (1), Is.False);
        Assert.That (new Range<double> (1, 4, "(]").ContainsElement (4), Is.True);
        Assert.That (new Range<double> (1, 4, "(]").ContainsElement (-3), Is.False);

        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).ContainsElement (2), Is.True);
      });
    }

    /// <summary>
    /// Test the Overlaps method
    /// </summary>
    [Test]
    public void TestOverlaps ()
    {
      Assert.Multiple (() => {
        Assert.That (new Range<double> ().Overlaps (new Range<double> ()), Is.False);
        Assert.That (new Range<double> ().Overlaps (new Range<double> (1, 4)), Is.False);

        Assert.That (new Range<double> (1, 4).Overlaps (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (1, 4).Overlaps (new Range<double> (1, 3)), Is.True);
        Assert.That (new Range<double> (1, 4).Overlaps (new Range<double> (1, 4)), Is.True);
        Assert.That (new Range<double> (1, 4, "(]").Overlaps (new Range<double> (1, 4, "()")), Is.True);
        Assert.That (new Range<double> (1, 4, "()").Overlaps (new Range<double> (1, 4)), Is.True);
        Assert.That (new Range<double> (1, 4, "[)").Overlaps (new Range<double> (1, 4, "[]")), Is.True);
        Assert.That (new Range<double> (1, 4).Overlaps (new Range<double> (4, 4, "[]")), Is.False);
        Assert.That (new Range<double> (1, 4).Overlaps (new Range<double> (2, 5)), Is.True);
        Assert.That (new Range<double> (1, 4).Overlaps (new Range<double> (0, 3)), Is.True);
        Assert.That (new Range<double> (1, 4).Overlaps (new Range<double> (6, 50)), Is.False);
        Assert.That (new Range<double> (1, 4).Overlaps (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (1, 4).Overlaps (new Range<double> (1, new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (1, 4).Overlaps (new Range<double> (new LowerBound<double> (null), 4)), Is.True);

        // Bug 2016-02-19
        Assert.That (new Range<int> (1, 3, "[)").Overlaps (new Range<int> (3, new UpperBound<int> (null), "[)")), Is.False);
        Assert.That (new Range<int> (3, new UpperBound<int> (null), "[)").Overlaps (new Range<int> (1, 3, "[)")), Is.False);

        Assert.That (new Range<int> (2, 4).Overlaps (new Range<int> (10, 15)), Is.False);
        Assert.That (new Range<int> (10, 15).Overlaps (new Range<int> (2, 4)), Is.False);
        Assert.That (new Range<int> (10, 15).Overlaps (new Range<int> (2, 10)), Is.False);
        Assert.That (new Range<int> (10, 15).Overlaps (new Range<int> (2, 10, "[]")), Is.True);

        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (1, 4)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "(]").Overlaps (new Range<double> (1, 4, "()")), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "()").Overlaps (new Range<double> (1, 4)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "[)").Overlaps (new Range<double> (1, 4, "[]")), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (4, 4, "[]")), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (2, 5)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (0, 3)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (6, 50)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (1, new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Overlaps (new Range<double> (new LowerBound<double> (null), 4)), Is.True);

        Assert.That (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (1, 4)), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "(]").Overlaps (new Range<double> (1, 4, "()")), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "()").Overlaps (new Range<double> (1, 4)), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "[)").Overlaps (new Range<double> (1, 4, "[]")), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (1, 1, "[]")), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (2, 5)), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (0, 3)), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (6, 50)), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (1, new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (new LowerBound<double> (null), 4)), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).Overlaps (new Range<double> (new LowerBound<double> (null), 1)), Is.False);

        Assert.That (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (1, 4)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "(]").Overlaps (new Range<double> (1, 4, "()")), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "()").Overlaps (new Range<double> (1, 4)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "[)").Overlaps (new Range<double> (1, 4, "[]")), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (4, 4, "[]")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (2, 5)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (0, 3)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (6, 50)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (1, new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (4, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).Overlaps (new Range<double> (new LowerBound<double> (null), 4)), Is.True);
      });
    }

    /// <summary>
    /// Test the IsStrictlyLeftOf method
    /// </summary>
    [Test]
    public void TestIsStrictlyLeftOf ()
    {
      Assert.Multiple (() => {
        Assert.That (new Range<double> ().IsStrictlyLeftOf (new Range<double> ()), Is.False);
        Assert.That (new Range<double> ().IsStrictlyLeftOf (new Range<double> (1, 4)), Is.False);

        Assert.That (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (1, 3)), Is.False);
        Assert.That (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (4, 5)), Is.True);
        Assert.That (new Range<double> (1, 4, "(]").IsStrictlyLeftOf (new Range<double> (4, 5, "()")), Is.True);
        Assert.That (new Range<double> (1, 4, "()").IsStrictlyLeftOf (new Range<double> (4, 5)), Is.True);
        Assert.That (new Range<double> (1, 4, "[)").IsStrictlyLeftOf (new Range<double> (4, 5, "[]")), Is.True);
        Assert.That (new Range<double> (1, 4, "[]").IsStrictlyLeftOf (new Range<double> (4, 5, "[]")), Is.False);
        Assert.That (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (4, 4, "[]")), Is.True);
        Assert.That (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (2, 5)), Is.False);
        Assert.That (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (0, 3)), Is.False);
        Assert.That (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (6, 50)), Is.True);
        Assert.That (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (1, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (4, new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (1, 4).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), 1)), Is.False);

        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "(]").IsStrictlyLeftOf (new Range<double> (1, 4, "()")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "()").IsStrictlyLeftOf (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "[)").IsStrictlyLeftOf (new Range<double> (1, 4, "[]")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (4, 4, "[]")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (2, 5)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (0, 3)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (6, 50)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (1, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), 4)), Is.False);

        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "(]").IsStrictlyLeftOf (new Range<double> (1, 4, "()")), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "()").IsStrictlyLeftOf (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "[)").IsStrictlyLeftOf (new Range<double> (1, 4, "[]")), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (1, 1, "[]")), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (2, 5)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (0, 3)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (6, 50)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (1, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), 4)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), 1)), Is.False);

        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "(]").IsStrictlyLeftOf (new Range<double> (1, 4, "()")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "()").IsStrictlyLeftOf (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "[)").IsStrictlyLeftOf (new Range<double> (1, 4, "[]")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (4, 4, "[]")), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (2, 5)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (0, 3)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (6, 50)), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (1, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (4, new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (new LowerBound<double> (null), 4)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (3, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyLeftOf (new Range<double> (5, new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "[]").IsStrictlyLeftOf (new Range<double> (4, new UpperBound<double> (null))), Is.False);
      });
    }

    /// <summary>
    /// Test the IsStrictlyRightOf method
    /// </summary>
    [Test]
    public void TestIsStrictlyRightOf ()
    {
      Assert.Multiple (() => {
        Assert.That (new Range<double> ().IsStrictlyRightOf (new Range<double> ()), Is.False);
        Assert.That (new Range<double> ().IsStrictlyRightOf (new Range<double> (1, 4)), Is.False);

        Assert.That (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (1, 3)), Is.False);
        Assert.That (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (0, 1)), Is.True);
        Assert.That (new Range<double> (1, 4, "(]").IsStrictlyRightOf (new Range<double> (0, 1, "()")), Is.True);
        Assert.That (new Range<double> (1, 4, "()").IsStrictlyRightOf (new Range<double> (0, 1)), Is.True);
        Assert.That (new Range<double> (1, 4, "(]").IsStrictlyRightOf (new Range<double> (0, 1, "[]")), Is.True);
        Assert.That (new Range<double> (1, 4, "[]").IsStrictlyRightOf (new Range<double> (0, 1, "[]")), Is.False);
        Assert.That (new Range<double> (1, 4, "(]").IsStrictlyRightOf (new Range<double> (1, 1, "[]")), Is.True);
        Assert.That (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (0, 3)), Is.False);
        Assert.That (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (2, 5)), Is.False);
      });
      Assert.Multiple (() => {
        Assert.That (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (0, 1)), Is.True);
        Assert.That (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (1, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (4, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), 1)), Is.True);
        Assert.That (new Range<double> (1, 4).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), 1, "[]")), Is.False);

        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "(]").IsStrictlyRightOf (new Range<double> (1, 4, "()")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "()").IsStrictlyRightOf (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "[)").IsStrictlyRightOf (new Range<double> (1, 4, "[]")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (4, 4, "[]")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (2, 5)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (0, 3)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (6, 50)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (1, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), 4)), Is.False);

        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "(]").IsStrictlyRightOf (new Range<double> (1, 4, "()")), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "()").IsStrictlyRightOf (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "[)").IsStrictlyRightOf (new Range<double> (1, 4, "[]")), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (1, 1, "[]")), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "()").IsStrictlyRightOf (new Range<double> (1, 1, "[]")), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (-1, 0)), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (0, 3)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (6, 50)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (1, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), 4)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), 1)), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), 1, "[]")), Is.False);

        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "(]").IsStrictlyRightOf (new Range<double> (1, 4, "()")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "()").IsStrictlyRightOf (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "[)").IsStrictlyRightOf (new Range<double> (1, 4, "[]")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (4, 4, "[]")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (2, 5)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (0, 3)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (6, 50)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (1, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (new LowerBound<double> (null), 4)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (3, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsStrictlyRightOf (new Range<double> (5, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "[]").IsStrictlyRightOf (new Range<double> (4, new UpperBound<double> (null))), Is.False);
      });
    }

    /// <summary>
    /// Test the IsAdjacentTo method
    /// </summary>
    [Test]
    public void TestIsAdjacentTo ()
    {
      Assert.Multiple (() => {
        Assert.That (new Range<double> ().IsAdjacentTo (new Range<double> ()), Is.False);
        Assert.That (new Range<double> ().IsAdjacentTo (new Range<double> (1, 4)), Is.False);

        Assert.That (new Range<double> (1, 4).IsAdjacentTo (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (1, 3)), Is.False);
        Assert.That (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (4, 5)), Is.True);
        Assert.That (new Range<double> (1, 4, "(]").IsAdjacentTo (new Range<double> (4, 5, "()")), Is.True);
        Assert.That (new Range<double> (1, 4, "()").IsAdjacentTo (new Range<double> (4, 5)), Is.True);
        Assert.That (new Range<double> (1, 4, "[)").IsAdjacentTo (new Range<double> (4, 5, "[]")), Is.True);
        Assert.That (new Range<double> (1, 4, "[]").IsAdjacentTo (new Range<double> (4, 5, "[]")), Is.False);
        Assert.That (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (4, 4, "[]")), Is.True);
        Assert.That (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (2, 5)), Is.False);
        Assert.That (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (0, 3)), Is.False);
        Assert.That (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (0, 1)), Is.True);
        Assert.That (new Range<double> (1, 4, "()").IsAdjacentTo (new Range<double> (0, 1, "[]")), Is.True);
        Assert.That (new Range<double> (1, 4, "()").IsAdjacentTo (new Range<double> (0, 1)), Is.False);
        Assert.That (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (6, 50)), Is.False);
        Assert.That (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (1, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (4, new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (1, 4).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 1)), Is.True);
        Assert.That (new Range<double> (1, 4, "()").IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 1)), Is.False);
        Assert.That (new Range<double> (1, 4, "()").IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 1, "(]")), Is.True);

        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "(]").IsAdjacentTo (new Range<double> (1, 4, "()")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "()").IsAdjacentTo (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null), "[)").IsAdjacentTo (new Range<double> (1, 4, "[]")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (4, 4, "[]")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (2, 5)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (0, 3)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (6, 50)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (1, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 4)), Is.False);

        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "(]").IsAdjacentTo (new Range<double> (1, 4, "()")), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "()").IsAdjacentTo (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "[)").IsAdjacentTo (new Range<double> (1, 4, "[]")), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (1, 1, "[]")), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (2, 5)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (0, 3)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (6, 50)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (1, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 4)), Is.False);
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 1)), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "()").IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 1, "[]")), Is.True);
        Assert.That (new Range<double> (1, new UpperBound<double> (null), "()").IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 1, "()")), Is.False);

        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> ()), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "(]").IsAdjacentTo (new Range<double> (1, 4, "()")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "()").IsAdjacentTo (new Range<double> (1, 4)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "[)").IsAdjacentTo (new Range<double> (1, 4, "[]")), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (4, 4, "[]")), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (2, 5)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (0, 3)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (6, 50)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (1, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (4, new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (new LowerBound<double> (null), 4)), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (3, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4).IsAdjacentTo (new Range<double> (5, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "[]").IsAdjacentTo (new Range<double> (4, new UpperBound<double> (null))), Is.False);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "()").IsAdjacentTo (new Range<double> (4, new UpperBound<double> (null))), Is.True);
        Assert.That (new Range<double> (new LowerBound<double> (null), 4, "[]").IsAdjacentTo (new Range<double> (4, new UpperBound<double> (null), "()")), Is.True);

        Assert.That (new DayRange (new DateTime (2016, 01, 05), new DateTime (2016, 01, 06)).IsAdjacentTo (new DayRange (new DateTime (2016, 01, 07), new DateTime (2016, 01, 08))), Is.True);
        Assert.That (new DayRange (new DateTime (2016, 01, 05), new DateTime (2016, 01, 06)).IsAdjacentTo (new DayRange (new DateTime (2016, 01, 03), new DateTime (2016, 01, 04))), Is.True);
        Assert.That (new DayRange (new DateTime (2016, 01, 05), new DateTime (2016, 01, 06)).IsAdjacentTo (new DayRange (new DateTime (2016, 01, 03), new DateTime (2016, 01, 05))), Is.False);
      });
    }

    /// <summary>
    /// Test the union of two ranges
    /// </summary>
    [Test]
    public void TestUnion ()
    {
      Assert.Throws<ArgumentException> (new TestDelegate (delegate { new Range<double> (1, 3).Union (new Range<double> (4, 5)); }));
      Assert.Throws<ArgumentException> (new TestDelegate (delegate { new Range<double> (new LowerBound<double> (null), 1).Union (new Range<double> (2, new UpperBound<double> (null))); }));
      Assert.Throws<ArgumentException> (new TestDelegate (delegate { new Range<double> (new LowerBound<double> (null), 1, "()").Union (new Range<double> (1, new UpperBound<double> (null), "()")); }));

      Assert.Multiple (() => {
        Assert.That (new Range<double> ().Union (new Range<double> ()), Is.EqualTo (new Range<double> ()));
        Assert.That (new Range<double> ().Union (new Range<double> (1, 5)), Is.EqualTo (new Range<double> (1, 5)));
        Assert.That (new Range<double> (1, 5).Union (new Range<double> ()), Is.EqualTo (new Range<double> (1, 5)));
        Assert.That (new Range<double> (1, 3).Union (new Range<double> (3, 5)), Is.EqualTo (new Range<double> (1, 5)));
        Assert.That (new Range<double> (1, 5).Union (new Range<double> (2, 3)), Is.EqualTo (new Range<double> (1, 5)));
        Assert.That (new Range<double> (1, 3).Union (new Range<double> (2, 5)), Is.EqualTo (new Range<double> (1, 5)));
        Assert.That (new Range<double> (1, 3, "[]").Union (new Range<double> (3, 5, "[]")), Is.EqualTo (new Range<double> (1, 5, "[]")));
        Assert.That (new Range<double> (1, 3, "()").Union (new Range<double> (2, 5, "()")), Is.EqualTo (new Range<double> (1, 5, "()")));
        Assert.That (new Range<double> (3, 5).Union (new Range<double> (1, 3)), Is.EqualTo (new Range<double> (1, 5)));
        Assert.That (new Range<double> (2, 5).Union (new Range<double> (1, 3)), Is.EqualTo (new Range<double> (1, 5)));
        Assert.That (new Range<double> (3, 5, "[]").Union (new Range<double> (1, 3, "[]")), Is.EqualTo (new Range<double> (1, 5, "[]")));
        Assert.That (new Range<double> (2, 5, "()").Union (new Range<double> (1, 3, "()")), Is.EqualTo (new Range<double> (1, 5, "()")));

        Assert.That (new Range<double> (1, 3).Union (new Range<double> (3, new UpperBound<double> (null))), Is.EqualTo (new Range<double> (1, new UpperBound<double> (null))));
        Assert.That (new Range<double> (1, 3).Union (new Range<double> (2, new UpperBound<double> (null))), Is.EqualTo (new Range<double> (1, new UpperBound<double> (null))));
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).Union (new Range<double> (3, new UpperBound<double> (null))), Is.EqualTo (new Range<double> (1, new UpperBound<double> (null))));
        Assert.That (new Range<double> (1, new UpperBound<double> (null)).Union (new Range<double> (3, 5)), Is.EqualTo (new Range<double> (1, new UpperBound<double> (null))));

        Assert.That (new Range<double> (new LowerBound<double> (null), 5).Union (new Range<double> (3, 5)), Is.EqualTo (new Range<double> (new LowerBound<double> (null), 5)));
        Assert.That (new Range<double> (new LowerBound<double> (null), 5).Union (new Range<double> (new LowerBound<double> (null), 3)), Is.EqualTo (new Range<double> (new LowerBound<double> (null), 5)));
        Assert.That (new Range<double> (2, 5).Union (new Range<double> (new LowerBound<double> (null), 3)), Is.EqualTo (new Range<double> (new LowerBound<double> (null), 5)));
        Assert.That (new Range<double> (2, 5, "[]").Union (new Range<double> (new LowerBound<double> (null), 3)), Is.EqualTo (new Range<double> (new LowerBound<double> (null), 5, "[]")));

        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Union (new Range<double> (3, 5)), Is.EqualTo (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
        Assert.That (new Range<double> (new LowerBound<double> (null), 3).Union (new Range<double> (3, new UpperBound<double> (null))), Is.EqualTo (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
        Assert.That (new Range<double> (new LowerBound<double> (null), 5).Union (new Range<double> (3, new UpperBound<double> (null))), Is.EqualTo (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));
        Assert.That (new Range<double> (new LowerBound<double> (null), 3, "[]").Union (new Range<double> (3, new UpperBound<double> (null), "()")), Is.EqualTo (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))));

        Assert.That (new DayRange (new DateTime (2016, 01, 05), new DateTime (2016, 01, 06)).Union (new DayRange (new DateTime (2016, 01, 07), new DateTime (2016, 01, 08))), Is.EqualTo (new DayRange (new DateTime (2016, 01, 05), new DateTime (2016, 01, 08))));
        Assert.That (new DayRange (new DateTime (2016, 01, 05), new DateTime (2016, 01, 06)).Union (new DayRange (new DateTime (2016, 01, 03), new DateTime (2016, 01, 04))), Is.EqualTo (new DayRange (new DateTime (2016, 01, 03), new DateTime (2016, 01, 06))));
      });
    }

    /// <summary>
    /// Test the Intersects method of two ranges
    /// </summary>
    [Test]
    public void TestIntersects ()
    {
      Assert.Multiple (() => {
        Assert.That (new Range<double> ().Intersects (new Range<double> ()), Is.EqualTo (new Range<double> ()));
        Assert.That (new Range<double> ().Intersects (new Range<double> (1, 3)), Is.EqualTo (new Range<double> ()));
        Assert.That (new Range<double> (2, 4).Intersects (new Range<double> ()), Is.EqualTo (new Range<double> ()));
        Assert.That (new Range<double> (1, 3).Intersects (new Range<double> (4, 5)), Is.EqualTo (new Range<double> ()));
        Assert.That (new Range<double> (1, 3, "()").Intersects (new Range<double> (3, 5, "()")), Is.EqualTo (new Range<double> ()));
        Assert.That (new Range<double> (1, 3, "[]").Intersects (new Range<double> (3, 5)), Is.EqualTo (new Range<double> (3, 3, "[]")));
        Assert.That (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null)).Intersects (new Range<double> (1, 3)), Is.EqualTo (new Range<double> (1, 3)));
        Assert.That (new Range<double> (1, 3).Intersects (new Range<double> (new LowerBound<double> (null), new UpperBound<double> (null))), Is.EqualTo (new Range<double> (1, 3)));
        Assert.That (new Range<double> (new LowerBound<double> (null), 3).Intersects (new Range<double> (2, new UpperBound<double> (null))), Is.EqualTo (new Range<double> (2, 3)));
        Assert.That (new Range<double> (new LowerBound<double> (null), 3, "[]").Intersects (new Range<double> (2, new UpperBound<double> (null))), Is.EqualTo (new Range<double> (2, 3, "[]")));
        Assert.That (new Range<double> (new LowerBound<double> (null), 3).Intersects (new Range<double> (4, new UpperBound<double> (null))), Is.EqualTo (new Range<double> ()));
      });
    }

    [Test]
    public void TestParse ()
    {
      {
        UtcDateTimeRange range = new UtcDateTimeRange ("[\"2015-06-03 04:00:00\",)");
        Assert.Multiple (() => {
          Assert.That (range.Upper.HasValue, Is.False);
          Assert.That (range.Lower.Value, Is.EqualTo (new DateTime (2015, 06, 03, 04, 00, 00, DateTimeKind.Utc)));
        });
      }
      {
        UtcDateTimeRange range = new UtcDateTimeRange ("'[\"2015-06-03 04:00:00\",)'");
        Assert.Multiple (() => {
          Assert.That (range.Upper.HasValue, Is.False);
          Assert.That (range.Lower.Value, Is.EqualTo (new DateTime (2015, 06, 03, 04, 00, 00, DateTimeKind.Utc)));
        });
      }
    }
  }
}
