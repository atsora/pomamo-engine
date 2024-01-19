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
  /// Unit tests for the class Bound.
  /// </summary>
  [TestFixture]
  public class Bound_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (Bound_UnitTest).FullName);

    /// <summary>
    /// The the operator &lt;
    /// </summary>
    [Test]
    public void TestLt()
    {
      Assert.Multiple (() => {
#pragma warning disable NUnit2043 // Use ComparisonConstraint for better assertion messages in case of failure
        Assert.That (new DateTime () < new UpperBound<DateTime> (null));
        Assert.That (new UpperBound<DateTime> (null) >= new DateTime ());
        Assert.That (new LowerBound<DateTime> (null) < new DateTime ());
        Assert.That (new DateTime () >= new LowerBound<DateTime> (null));
#pragma warning restore NUnit2043 // Use ComparisonConstraint for better assertion messages in case of failure
      });
    }
    
    /// <summary>
    /// Test the Compare method
    /// </summary>
    [Test]
    public void TestCompare()
    {
      Assert.That (Bound.Compare<DateTime> (new UpperBound<DateTime> (),
                                               new DateTime (1970, 1, 1, 00, 00, 00, DateTimeKind.Utc)), Is.GreaterThanOrEqualTo (0));
    }
    
    private void TestToString ()
    {
      Assert.Multiple (() => {
        Assert.That (new UpperBound<DateTime> (null).ToString (), Is.EqualTo ("+oo"));
        Assert.That (new LowerBound<DateTime> (null).ToString (), Is.EqualTo ("-oo"));
        Assert.That (new Bound<DateTime> (null, BoundType.Upper).ToString (), Is.EqualTo ("+oo"));
      });
    }
  }
}
