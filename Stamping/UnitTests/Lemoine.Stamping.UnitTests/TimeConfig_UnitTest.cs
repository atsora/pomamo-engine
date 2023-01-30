// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.Stamping.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class TimeConfig_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (TimeConfig_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public TimeConfig_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestDeserialize ()
    {
      {
        var json = """
{
}
""";
        var timeConfig = JsonSerializer.Deserialize<Lemoine.Stamping.TimeConfigs.TimeConfig> (json);
        Assert.AreEqual (1.0, timeConfig.GetTimeFactor ());
        Assert.AreEqual (TimeSpan.FromMinutes (2), timeConfig.GetMilestoneTriggerFrequency ());
      }

      {
        var json = """
{
  "TimeFactor": 0.9,
  "MilestoneTriggerFrequency": "0:10:00"
}
""";
        var timeConfig = JsonSerializer.Deserialize<Lemoine.Stamping.TimeConfigs.TimeConfig> (json);
        Assert.AreEqual (0.9, timeConfig.GetTimeFactor ());
        Assert.AreEqual (TimeSpan.FromMinutes (10), timeConfig.GetMilestoneTriggerFrequency ());
      }
    }
  }
}
