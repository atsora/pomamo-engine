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
      Assert.AreEqual (123, converter.ConvertAuto<int> ("123"));
      Assert.AreEqual (123, converter.ConvertAuto<System.Int32> ("123"));
      Assert.AreEqual (123, converter.ConvertAuto ("123", typeof (System.Int32)));
      Assert.AreEqual (123.45, converter.ConvertAuto<double> ("123.45"));
      Assert.AreEqual (TimeSpan.FromDays (1.5), converter.ConvertAuto<TimeSpan> ("1.12:00:00"));
      Assert.AreEqual (new DateTime (2020, 11, 15, 10, 00, 00, DateTimeKind.Unspecified), converter.ConvertAuto<DateTime> ("2020-11-15 10:00:00"));
      Assert.AreEqual (new DateTime (2020, 11, 15, 10, 00, 00, DateTimeKind.Utc), converter.ConvertAuto<DateTime> ("2020-11-15T10:00:00Z"));
      Assert.AreEqual (EnumTest.First, converter.ConvertAuto<EnumTest> (1));
      Assert.AreEqual (EnumTest.First, converter.ConvertAuto<EnumTest> ("First"));
      Assert.AreEqual (EnumTest.First, converter.ConvertAuto<EnumTest> ("1"));
    }

    [Test]
    public void TestAutoConvertEnumerable ()
    {
      var converter = new DefaultAutoConverter ();
      var doubleList = new List<double> { 2.5 };
      var converted = converter.ConvertAuto<IEnumerable<int>> (doubleList);
      Assert.AreEqual (2, converted.First ());
    }
  }
}
