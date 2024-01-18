// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.DTO.UnitTests
{
  /// <summary>
  /// Unit tests for the class TODO: MyClassName.
  /// </summary>
  [TestFixture]
  public class ConvertDTO_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ConvertDTO_UnitTest).FullName);

    /// <summary>
    /// Test DayToString()
    /// </summary>
    [Test]
    public void TestDayToString()
    {
      DateTime day = new DateTime (2015, 11, 15);
      string s = Lemoine.DTO.ConvertDTO.DayToString (day);
      Assert.That (s, Is.EqualTo ("2015-11-15"));
    }

    /// <summary>
    /// Test DayToString()
    /// </summary>
    [Test]
    public void TestStringToDay()
    {
      const string s = "2015-11-15";
      DateTime? day = Lemoine.DTO.ConvertDTO.StringToDay (s);
      Assert.Multiple (() => {
        Assert.That (day.HasValue, Is.True);
        Assert.That (day.Value, Is.EqualTo (new DateTime (2015, 11, 15)));
      });
    }
  }
}
