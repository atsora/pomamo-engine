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
  public class DaySlotFromDay_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (DaySlotFromDay_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DaySlotFromDay_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test ()
    {
      var day = new DateTime (2018, 05, 03);
      var request = new Lemoine.Business.Time.DaySlotFromDay (day);
      var response = Lemoine.Business.ServiceProvider.Get (request);
      Assert.Multiple (() => {
        Assert.That (response.DateTimeRange.Lower.Value, Is.EqualTo (new DateTime (2018, 05, 02, 20, 00, 00, DateTimeKind.Utc)));
        Assert.That (response.DateTimeRange.Upper.Value, Is.EqualTo (new DateTime (2018, 05, 03, 20, 00, 00, DateTimeKind.Utc)));
      });
    }
  }
}
