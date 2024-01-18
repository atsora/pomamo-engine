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
      Assert.Multiple (() => {
        Assert.That (result.X, Is.EqualTo (TimeSpan.FromSeconds (0)));
        Assert.That (result.Y, Is.EqualTo (0));
        Assert.That (result.Z, Is.EqualTo (0.0));
        Assert.That (result.X1, Is.EqualTo (TimeSpan.FromSeconds (0)));
        Assert.That (result.Y1, Is.EqualTo (0));
      });
      Assert.That (result.W, Is.Null);
      Assert.Multiple (() => {
        Assert.That (result.Xd, Is.EqualTo (TimeSpan.FromSeconds (3)));
        Assert.That (result.Yd, Is.EqualTo (3));
        Assert.That (result.Zd, Is.EqualTo (3.0));
      });
      Assert.Multiple (() => {
        Assert.That (result.S1, Is.Null);
        Assert.That (result.T, Is.Null);
        Assert.That (result.S2, Is.Null);
        Assert.That (result.V, Is.EqualTo (0));
      });

      var a = new A ();
      Assert.That (a.Yd, Is.EqualTo (4)); // DefaultValue attribute is not sufficient. It must be set in the constructor as well
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
      Assert.Multiple (() => {
        Assert.That (result.X, Is.EqualTo (TimeSpan.FromSeconds (0)));
        Assert.That (result.Y, Is.EqualTo (0));
        Assert.That (result.Z, Is.EqualTo (3.14));
        Assert.That (result.X1, Is.EqualTo (TimeSpan.FromSeconds (0)));
        Assert.That (result.Y1, Is.EqualTo (0));
      });
      Assert.That (result.W, Is.Null);
      Assert.Multiple (() => {
        Assert.That (result.Xd, Is.EqualTo (TimeSpan.FromSeconds (3)));
        Assert.That (result.Yd, Is.EqualTo (3));
        Assert.That (result.Zd, Is.EqualTo (3.0));
      });
      Assert.Multiple (() => {
        Assert.That (result.S1, Is.Null);
        Assert.That (result.S2, Is.Null);
        Assert.That (result.V, Is.EqualTo (3));
      });
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
      Assert.Multiple (() => {
        Assert.That (result.X, Is.EqualTo (TimeSpan.FromSeconds (0)));
        Assert.That (result.Y, Is.EqualTo (0));
        Assert.That (result.Z, Is.EqualTo (3.14));
        Assert.That (result.X1, Is.EqualTo (TimeSpan.FromSeconds (0)));
        Assert.That (result.Y1, Is.EqualTo (0));
      });
      Assert.That (result.W, Is.Null);
      Assert.Multiple (() => {
        Assert.That (result.Xd, Is.EqualTo (TimeSpan.FromSeconds (3)));
        Assert.That (result.Yd, Is.EqualTo (3));
        Assert.That (result.Zd, Is.EqualTo (3.0));
      });
      Assert.That (result.S1, Is.Not.Null);
      Assert.Multiple (() => {
        Assert.That (result.S1.Count (), Is.EqualTo (2));
        Assert.That (result.T.Value, Is.EqualTo (TimeSpan.FromSeconds (3)));
      });
      Assert.Multiple (() => {
        Assert.That (result.S2, Is.Null);
        Assert.That (result.V, Is.EqualTo (3));
      });
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
      Assert.That (result.V, Is.EqualTo (3));
    }
  }
}
