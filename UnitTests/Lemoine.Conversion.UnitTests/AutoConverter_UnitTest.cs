// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Lemoine.Core.Log;
using System.Text.Json;

namespace Lemoine.Conversion.UnitTests
{
  enum EnumTest
  {
    First = 1,
    Second,
  }

  /// <summary>
  /// 
  /// </summary>
  public class AutoConverter_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (AutoConverter_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoConverter_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestAutoConvert ()
    {
      var converter = new DefaultAutoConverter ();
      Assert.Multiple (() => {
        Assert.That (converter.ConvertAuto<int> ("123"), Is.EqualTo (123));
        Assert.That (converter.ConvertAuto<System.Int32> ("123"), Is.EqualTo (123));
        Assert.That (converter.ConvertAuto ("123", typeof (System.Int32)), Is.EqualTo (123));
        Assert.That (converter.ConvertAuto<double> ("123.45"), Is.EqualTo (123.45));
        Assert.That (converter.ConvertAuto<TimeSpan> ("1.12:00:00"), Is.EqualTo (TimeSpan.FromDays (1.5)));
        Assert.That (converter.ConvertAuto<DateTime> ("2020-11-15 10:00:00"), Is.EqualTo (new DateTime (2020, 11, 15, 10, 00, 00, DateTimeKind.Unspecified)));
        Assert.That (converter.ConvertAuto<DateTime> ("2020-11-15T10:00:00Z"), Is.EqualTo (new DateTime (2020, 11, 15, 10, 00, 00, DateTimeKind.Utc)));
        Assert.That (converter.ConvertAuto<EnumTest> (1), Is.EqualTo (EnumTest.First));
        Assert.That (converter.ConvertAuto<EnumTest> ("First"), Is.EqualTo (EnumTest.First));
        Assert.That (converter.ConvertAuto<EnumTest> ("1"), Is.EqualTo (EnumTest.First));
      });
    }

    [Test]
    public void TestAutoConvertEnumerable ()
    {
      var converter = new DefaultAutoConverter ();
      var doubleList = new List<double> { 2.5 };
      var converted = converter.ConvertAuto<IEnumerable<int>> (doubleList);
      Assert.That (converted.First (), Is.EqualTo (2));
    }

    [Test]
    public void TestAutoConvertDictionary ()
    {
      {
        var converter = new DefaultAutoConverter ();
        IDictionary<string, double> a = new Dictionary<string, double> ();
        a["test"] = 1.2;
        var converted = converter.ConvertAuto<IDictionary<string, object>> (a);
        Assert.That (converted["test"], Is.EqualTo (1.2));
      }
      {
        var converter = new DefaultAutoConverter ();
        var a = new Dictionary<string, double> ();
        a["test"] = 1.2;
        var converted = converter.ConvertAuto<IDictionary<string, object>> (a);
        Assert.That (converted["test"], Is.EqualTo (1.2));
      }
    }

    [Test]
    public void TestJsonElement ()
    {
      {
        var converter = new DefaultAutoConverter ();
        object d = 123.4;
        var jsonElement = JsonSerializer.Deserialize<object> (JsonSerializer.Serialize (d));
        var converted = converter.ConvertAuto<double> (jsonElement);
        Assert.That (converted, Is.EqualTo (d));
        var convertedString = converter.ConvertAuto<string> (jsonElement);
        Assert.That (convertedString, Is.EqualTo ("123.4"));
      }
      {
        var converter = new DefaultAutoConverter ();
        object n = 123;
        var jsonElement = JsonSerializer.Deserialize<object> (JsonSerializer.Serialize (n));
        var converted = converter.ConvertAuto<double> (jsonElement);
        Assert.That (converted, Is.EqualTo (n));
        var converted2 = converter.ConvertAuto<long> (jsonElement);
        Assert.That (converted2, Is.EqualTo (n));
        var convertedString = converter.ConvertAuto<string> (jsonElement);
        Assert.That (convertedString, Is.EqualTo ("123"));
      }
      {
        var converter = new DefaultAutoConverter ();
        object s = "123";
        var jsonElement = JsonSerializer.Deserialize<object> (JsonSerializer.Serialize (s));
        var converted = converter.ConvertAuto<string> (jsonElement);
        Assert.That (converted, Is.EqualTo (s));
        Assert.Throws<InvalidCastException> (() => converter.ConvertAuto<double> (jsonElement));
        Assert.Throws<InvalidCastException> (() => converter.ConvertAuto<long> (jsonElement));
      }
    }
  }
}
