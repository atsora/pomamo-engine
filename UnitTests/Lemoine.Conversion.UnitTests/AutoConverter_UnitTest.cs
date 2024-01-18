// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Lemoine.Core.Log;

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
  }
}
