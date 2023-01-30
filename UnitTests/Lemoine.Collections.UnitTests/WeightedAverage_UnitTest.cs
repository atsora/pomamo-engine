// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.Collections.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class WeightedAverage_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (WeightedAverage_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public WeightedAverage_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test ()
    {
      var list = new List<(double, TimeSpan)> { (10.0, TimeSpan.FromSeconds (2)), (20.0, TimeSpan.FromSeconds (4)), (30, TimeSpan.FromSeconds (4)) };
      var weightedAverage = list.WeightedAverage (x => x.Item1, x => x.Item2);
      Assert.AreEqual (18,0, weightedAverage.Item1);
      Assert.AreEqual (TimeSpan.FromSeconds (10), weightedAverage.Item2);
    }
  }
}
