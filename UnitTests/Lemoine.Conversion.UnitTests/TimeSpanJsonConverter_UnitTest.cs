// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Lemoine.Conversion.Json;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.Conversion.UnitTests
{
  class Class
  {
    [JsonConverter(typeof (TimeSpanJsonConverter))]
    public TimeSpan Duration { get; set; }
  }

  /// <summary>
  /// Unit test of <see cref="TimeSpanJsonConverter"/>
  /// </summary>
  public class TimeSpanJsonConverter_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (TimeSpanJsonConverter_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public TimeSpanJsonConverter_UnitTest ()
    { }

    /// <summary>
    /// Test serialize
    /// </summary>
    [Test]
    public void TestSerialize ()
    {
      var x = new Class { Duration = TimeSpan.FromMinutes (2.5) };
      var json = System.Text.Json.JsonSerializer.Serialize (x);
      Assert.AreEqual ("""
{"Duration":"00:02:30"}
""", json);
    }

    /// <summary>
    /// Test deserialize
    /// </summary>
    [Test]
    public void TestDeserialize ()
    {
      var json = """
{"Duration":"0:02:30"}
""";
      var x = System.Text.Json.JsonSerializer.Deserialize<Class> (json);
      Assert.AreEqual (TimeSpan.FromMinutes (2.5), x.Duration);
    }
  }
}
