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
        Assert.Multiple (() => {
          Assert.That (timeConfig.GetTimeFactor (), Is.EqualTo (1.0));
          Assert.That (timeConfig.GetMilestoneTriggerFrequency (), Is.EqualTo (TimeSpan.FromMinutes (2)));
        });
      }

      {
        var json = """
{
  "TimeFactor": 0.9,
  "MilestoneTriggerFrequency": "0:10:00"
}
""";
        var timeConfig = JsonSerializer.Deserialize<Lemoine.Stamping.TimeConfigs.TimeConfig> (json);
        Assert.Multiple (() => {
          Assert.That (timeConfig.GetTimeFactor (), Is.EqualTo (0.9));
          Assert.That (timeConfig.GetMilestoneTriggerFrequency (), Is.EqualTo (TimeSpan.FromMinutes (10)));
        });
      }
    }
  }
}
