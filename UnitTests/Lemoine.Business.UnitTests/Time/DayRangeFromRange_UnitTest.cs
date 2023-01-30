// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.Business.UnitTests.Time
{
  /// <summary>
  /// 
  /// </summary>
  public class DayRangeFromRange_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (DayRangeFromRange_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DayRangeFromRange_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test ()
    {
      var lower = new DateTime (2018, 05, 01, 23, 00, 00, DateTimeKind.Utc);
      var upper = new DateTime (2018, 05, 04, 03, 00, 00, DateTimeKind.Utc);
      var request = new Lemoine.Business.Time.DayRangeFromRange (new UtcDateTimeRange (lower, upper));
      var response = Lemoine.Business.ServiceProvider.Get (request);
      Assert.AreEqual (new DateTime (2018, 05, 02), response.Lower.Value);
      Assert.AreEqual (new DateTime (2018, 05, 04), response.Upper.Value);
    }
  }
}
