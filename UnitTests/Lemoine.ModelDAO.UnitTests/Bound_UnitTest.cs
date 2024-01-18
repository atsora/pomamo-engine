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
      Assert.That (new DateTime () < new UpperBound<DateTime> (null), Is.True);
      Assert.IsFalse (new UpperBound<DateTime> (null) < new DateTime ());
      Assert.That (new LowerBound<DateTime> (null) < new DateTime (), Is.True);
      Assert.IsFalse (new DateTime () < new LowerBound<DateTime> (null));
    }
    
    /// <summary>
    /// Test the Compare method
    /// </summary>
    [Test]
    public void TestCompare()
    {
      Assert.IsFalse (Bound.Compare<DateTime> (new UpperBound<DateTime> (),
                                               new DateTime (1970, 1, 1, 00, 00, 00, DateTimeKind.Utc)) < 0);
    }
    
    public void TestToString ()
    {
      Assert.That (new UpperBound<DateTime> (null).ToString (), Is.EqualTo ("+oo"));
      Assert.That (new LowerBound<DateTime> (null).ToString (), Is.EqualTo ("-oo"));
      Assert.That (new Bound<DateTime> (null, BoundType.Upper).ToString (), Is.EqualTo ("+oo"));
    }
  }
}
