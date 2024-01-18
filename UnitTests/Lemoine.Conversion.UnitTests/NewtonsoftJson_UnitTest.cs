// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Lemoine.Core.Log;
using Newtonsoft.Json;
using System.Reflection;

namespace Lemoine.Conversion.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class NewtonsoftJson_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (NewtonsoftJson_UnitTest).FullName);

    class MethodParameters
    {
      public string Method { get; set; }

      public object[] Parameters { get; set; }

      public MethodParameters (string method, params object[] parameters)
      {
        this.Method = method;
        this.Parameters = parameters;
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public NewtonsoftJson_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestDeserializationBool ()
    {
      var json = "true";
      var deserialized = JsonConvert.DeserializeObject<object> (json);
      Assert.That (deserialized is bool, Is.True);
      var x = (bool)deserialized;
      Assert.That (x, Is.EqualTo (true));
      var y = Convert.ChangeType (deserialized, typeof (bool));
      Assert.That (y, Is.EqualTo (true));
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestDeserializationInt ()
    {
      var json = "123";
      var deserialized = JsonConvert.DeserializeObject<object> (json);
      Assert.That (deserialized is long, Is.True);
      var x = (long)deserialized;
      Assert.That (x, Is.EqualTo (123));
      var y = Convert.ChangeType (deserialized, typeof (int));
      Assert.That (y, Is.EqualTo (123));
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestDeserializationDouble ()
    {
      var json = "123.45";
      var deserialized = JsonConvert.DeserializeObject<object> (json);
      Assert.That (deserialized is double, Is.True);
      var x = (double)deserialized;
      Assert.That (x, Is.EqualTo (123.45));
      var y = Convert.ChangeType (deserialized, typeof (double));
      Assert.That (y, Is.EqualTo (123.45));
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestDeserializationString ()
    {
      var json = "\"123.45\"";
      var deserialized = JsonConvert.DeserializeObject<object> (json);
      Assert.That (deserialized is string, Is.True);
      var x = (string)deserialized;
      Assert.That (x, Is.EqualTo ("123.45"));
      var y = Convert.ChangeType (deserialized, typeof (string));
      Assert.That (y, Is.EqualTo ("123.45"));
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestDeserializationArray ()
    {
      var json = "[1, 2, 3]";
      var deserialized = JsonConvert.DeserializeObject<object> (json);
      Assert.That (deserialized is IEnumerable<Newtonsoft.Json.Linq.JToken>, Is.True);
      var x = (IEnumerable<Newtonsoft.Json.Linq.JToken>)deserialized;
      var first = x.First ();
      var firstInt = (int)first;
      Assert.That (firstInt, Is.EqualTo (1));
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestDeserializationDictionary ()
    {
      var json = "{\"a\": 1, \"b\": 2, \"c\": 3}";
      var deserialized = JsonConvert.DeserializeObject<object> (json);
      Assert.Multiple (() => {
        Assert.That (deserialized is IDictionary<string, Newtonsoft.Json.Linq.JToken>, Is.True);
        Assert.That (deserialized is IEnumerable<KeyValuePair<string, Newtonsoft.Json.Linq.JToken>>, Is.True);
      });
      var x = (IDictionary<string, Newtonsoft.Json.Linq.JToken>)deserialized;
      var first = x.First ();
      var firstInt = (int)first.Value;
      Assert.That (firstInt, Is.EqualTo (1));
    }

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void TestMethodParameters ()
    {
      var a = new MethodParameters ("MethodName", new object[] { 1, 3.4, "param" });
      var json = JsonConvert.SerializeObject (a);
      var deserialized = JsonConvert.DeserializeObject<MethodParameters> (json);
      var t = typeof (NewtonsoftJson_UnitTest);
      var m = t.GetMethod ("RunMethodLong");
      var r = (bool)m.Invoke (this, deserialized.Parameters);
      Assert.That (r, Is.True);
      m = t.GetMethod ("RunMethodInt");
      var r2 = (bool)m.Invoke (this, ConvertLongToInt (deserialized.Parameters));
      Assert.That (r2, Is.True);
      var r3 = (bool)m.Invoke (this, ConvertAuto (deserialized.Parameters, m.GetParameters ()));
      Assert.That (r3, Is.True);
    }

    object[] ConvertLongToInt (object[] x)
    {
      return x
        .Select (a => (a is long) ? Convert.ChangeType (a, typeof (int)) : a)
        .ToArray ();
    }

    object[] ConvertAuto (object[] x, ParameterInfo[] y)
    {
      var result = new object[x.Length];
      for (int i = 0; i < x.Length - 1; ++i) {
        result[i] = Convert.ChangeType (x[i], y[i].ParameterType);
      }
      return result;
    }

    private bool RunMethodLong (long x, double y, string s)
    {
      return true;
    }

    private bool RunMethodInt (int x, double y, string s)
    {
      return true;
    }

  }
}
