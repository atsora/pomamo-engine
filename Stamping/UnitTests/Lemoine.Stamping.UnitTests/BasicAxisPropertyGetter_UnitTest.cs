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
  public class BasicAxisPropertyGetter_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (BasicAxisPropertyGetter_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public BasicAxisPropertyGetter_UnitTest ()
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
        var axisPropertyGetter = JsonSerializer.Deserialize<Lemoine.Stamping.AxisPropertyGetters.BasicAxisPropertyGetter> (json);
        Assert.Multiple (() => {
          Assert.That (axisPropertyGetter.GetDefaultUnit ("X"), Is.EqualTo (AxisUnit.Default));
          Assert.That (axisPropertyGetter.GetMaxVelocity ("X"), Is.Null);
        });
      }

      {
        var json = """
{
  "DefaultUnit": "Mm",
  "MaxVelocity": 1000.0
}
""";
        var axisPropertyGetter = JsonSerializer.Deserialize<Lemoine.Stamping.AxisPropertyGetters.BasicAxisPropertyGetter> (json);
        Assert.Multiple (() => {
          Assert.That (axisPropertyGetter.GetDefaultUnit ("X"), Is.EqualTo (AxisUnit.Mm));
          Assert.That (axisPropertyGetter.GetMaxVelocity ("X"), Is.EqualTo (1000));
        });
      }
    }
  }
}
