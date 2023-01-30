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
  public class DayAt_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (DayAt_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DayAt_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test ()
    {
      var at = new DateTime (2018, 05, 01, 23, 00, 00, DateTimeKind.Utc);
      var request = new Lemoine.Business.Time.DayAt (at);
      var response = Lemoine.Business.ServiceProvider.Get (request);
      Assert.AreEqual (new DateTime (2018, 05, 02), response.Day);
    }
  }
}
