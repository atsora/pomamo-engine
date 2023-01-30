// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Lemoine.Core.Log;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Lemoine.Collections.UnitTests
{
  class P
  {
    public int V { get; set; }
  }

  class A: P
  {
    public A ()
    {
      X1 = TimeSpan.FromSeconds (2);
      Y1 = 2;
    }

    public TimeSpan X { get; set; }

    public int Y { get; set; }

    public double Z { get; set; }

    public TimeSpan X1 { get; set; }

    public int Y1 { get; set; }

    public int? W { get; set; }

    [DefaultValue("0:00:03")]
    public TimeSpan Xd { get; set; }

    [DefaultValue (3)]
    public int Yd { get; set; } = 4;

    [DefaultValue (3.0)]
    public double Zd { get; set; } = 4.0;

    public IEnumerable<int> S1 { get; set; } = null;

    [DefaultValue (null)]
    public TimeSpan? T { get; set; } = null;

    public IEnumerable<int> S2 { get; set; } = null;
  }

  /// <summary>
  /// 
  /// </summary>
  public class JsonDeserialization_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (JsonDeserialization_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public JsonDeserialization_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestEmpty ()
    {
      var text = @"{}";
      var jsonSettings = new JsonSerializerSettings
      {
        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
        NullValueHandling = NullValueHandling.Ignore,
        Converters = new List<JsonConverter>
      {
        new Lemoine.Conversion.JavaScript.TimeSpanConverter ()
      }
      };
      var result = JsonConvert.DeserializeObject<A> (text, jsonSettings);
      Assert.AreEqual (TimeSpan.FromSeconds (0), result.X);
      Assert.AreEqual (0, result.Y);
      Assert.AreEqual (0.0, result.Z);
      Assert.AreEqual (TimeSpan.FromSeconds (0), result.X1);
      Assert.AreEqual (0, result.Y1);
      Assert.IsNull (result.W);
      Assert.AreEqual (TimeSpan.FromSeconds (3), result.Xd);
      Assert.AreEqual (3, result.Yd);
      Assert.AreEqual (3.0, result.Zd);
      Assert.Null (result.S1);
      Assert.Null (result.T);
      Assert.Null (result.S2);
      Assert.AreEqual (0, result.V);

      var a = new A ();
      Assert.AreEqual (4, a.Yd); // DefaultValue attribute is not sufficient. It must be set in the constructor as well
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestNotEmpty1 ()
    {
      var text = @"
{
  ""Z"": 3.14,
  ""V"": 3
}";
      var jsonSettings = new JsonSerializerSettings
      {
        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
        NullValueHandling = NullValueHandling.Ignore,
        Converters = new List<JsonConverter>
      {
        new Lemoine.Conversion.JavaScript.TimeSpanConverter ()
      }
      };
      var result = JsonConvert.DeserializeObject<A> (text, jsonSettings);
      Assert.AreEqual (TimeSpan.FromSeconds (0), result.X);
      Assert.AreEqual (0, result.Y);
      Assert.AreEqual (3.14, result.Z);
      Assert.AreEqual (TimeSpan.FromSeconds (0), result.X1);
      Assert.AreEqual (0, result.Y1);
      Assert.IsNull (result.W);
      Assert.AreEqual (TimeSpan.FromSeconds (3), result.Xd);
      Assert.AreEqual (3, result.Yd);
      Assert.AreEqual (3.0, result.Zd);
      Assert.Null (result.S1);
      Assert.Null (result.S2);
      Assert.AreEqual (3, result.V);
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestNotEmpty2 ()
    {
      var text = @"
{
  ""Z"": 3.14,
  ""S1"": [1, 2],
  ""T"": ""0:00:03"",
  ""V"": 3
}";
      var jsonSettings = new JsonSerializerSettings
      {
        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
        NullValueHandling = NullValueHandling.Ignore,
        Converters = new List<JsonConverter>
      {
        new Lemoine.Conversion.JavaScript.TimeSpanConverter ()
      }
      };
      var result = JsonConvert.DeserializeObject<A> (text, jsonSettings);
      Assert.AreEqual (TimeSpan.FromSeconds (0), result.X);
      Assert.AreEqual (0, result.Y);
      Assert.AreEqual (3.14, result.Z);
      Assert.AreEqual (TimeSpan.FromSeconds (0), result.X1);
      Assert.AreEqual (0, result.Y1);
      Assert.IsNull (result.W);
      Assert.AreEqual (TimeSpan.FromSeconds (3), result.Xd);
      Assert.AreEqual (3, result.Yd);
      Assert.AreEqual (3.0, result.Zd);
      Assert.NotNull (result.S1);
      Assert.AreEqual (2, result.S1.Count ());
      Assert.AreEqual (TimeSpan.FromSeconds (3), result.T.Value);
      Assert.Null (result.S2);
      Assert.AreEqual (3, result.V);
    }

    [Test]
    public void TestPartial ()
    {
      var text = @"
{
  ""V"": 3,
  ""login"": ""my_login"",
  ""id"": 1,
  ""plan"": {
    ""name"": ""nn""
  }
}";
      var result = JsonConvert.DeserializeObject<P> (text);
      Assert.AreEqual (3, result.V);
    }
  }
}
