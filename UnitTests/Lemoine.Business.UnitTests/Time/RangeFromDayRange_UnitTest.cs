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
  public class RangeFromDayRange_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (RangeFromDayRange_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public RangeFromDayRange_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test ()
    {
      var lowerDay = new DateTime (2018, 05, 02);
      var upperDay = new DateTime (2018, 05, 04);
      var request = new Lemoine.Business.Time.RangeFromDayRange (new DayRange (lowerDay, upperDay));
      var response = Lemoine.Business.ServiceProvider.Get (request);
      Assert.AreEqual (new DateTime (2018, 05, 01, 20, 00, 00, DateTimeKind.Utc), response.Lower.Value);
      Assert.AreEqual (new DateTime (2018, 05, 04, 20, 00, 00, DateTimeKind.Utc), response.Upper.Value);
    }
  }
}
