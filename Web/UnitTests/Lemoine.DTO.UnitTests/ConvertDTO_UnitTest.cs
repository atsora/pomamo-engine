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
      Assert.AreEqual ("2015-11-15", s);
    }

    /// <summary>
    /// Test DayToString()
    /// </summary>
    [Test]
    public void TestStringToDay()
    {
      const string s = "2015-11-15";
      DateTime? day = Lemoine.DTO.ConvertDTO.StringToDay (s);
      Assert.IsTrue (day.HasValue);
      Assert.AreEqual (new DateTime (2015, 11, 15), day.Value);
    }
  }
}
