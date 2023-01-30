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
      Assert.IsTrue (deserialized is bool);
      var x = (bool)deserialized;
      Assert.AreEqual (true, x);
      var y = Convert.ChangeType (deserialized, typeof (bool));
      Assert.AreEqual (true, y);
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestDeserializationInt ()
    {
      var json = "123";
      var deserialized = JsonConvert.DeserializeObject<object> (json);
      Assert.IsTrue (deserialized is long);
      var x = (long)deserialized;
      Assert.AreEqual (123, x);
      var y = Convert.ChangeType (deserialized, typeof (int));
      Assert.AreEqual (123, y);
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestDeserializationDouble ()
    {
      var json = "123.45";
      var deserialized = JsonConvert.DeserializeObject<object> (json);
      Assert.IsTrue (deserialized is double);
      var x = (double)deserialized;
      Assert.AreEqual (123.45, x);
      var y = Convert.ChangeType (deserialized, typeof (double));
      Assert.AreEqual (123.45, y);
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestDeserializationString ()
    {
      var json = "\"123.45\"";
      var deserialized = JsonConvert.DeserializeObject<object> (json);
      Assert.IsTrue (deserialized is string);
      var x = (string)deserialized;
      Assert.AreEqual ("123.45", x);
      var y = Convert.ChangeType (deserialized, typeof (string));
      Assert.AreEqual ("123.45", y);
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestDeserializationArray ()
    {
      var json = "[1, 2, 3]";
      var deserialized = JsonConvert.DeserializeObject<object> (json);
      Assert.IsTrue (deserialized is IEnumerable<Newtonsoft.Json.Linq.JToken>);
      var x = (IEnumerable<Newtonsoft.Json.Linq.JToken>)deserialized;
      var first = x.First ();
      var firstInt = (int)first;
      Assert.AreEqual (1, firstInt);
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestDeserializationDictionary ()
    {
      var json = "{\"a\": 1, \"b\": 2, \"c\": 3}";
      var deserialized = JsonConvert.DeserializeObject<object> (json);
      Assert.IsTrue (deserialized is IDictionary<string, Newtonsoft.Json.Linq.JToken>);
      Assert.IsTrue (deserialized is IEnumerable<KeyValuePair<string, Newtonsoft.Json.Linq.JToken>>);
      var x = (IDictionary<string, Newtonsoft.Json.Linq.JToken>)deserialized;
      var first = x.First ();
      var firstInt = (int)first.Value;
      Assert.AreEqual (1, firstInt);
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
      Assert.IsTrue (r);
      m = t.GetMethod ("RunMethodInt");
      var r2 = (bool)m.Invoke (this, ConvertLongToInt (deserialized.Parameters));
      Assert.IsTrue (r2);
      var r3 = (bool)m.Invoke (this, ConvertAuto (deserialized.Parameters, m.GetParameters ()));
      Assert.IsTrue (r3);
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

    public bool RunMethodLong (long x, double y, string s)
    {
      return true;
    }

    public bool RunMethodInt (int x, double y, string s)
    {
      return true;
    }

  }
}
