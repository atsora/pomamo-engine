// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.Core.FileRepository.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class SynchronizerFile_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (SynchronizerFile_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public SynchronizerFile_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestConvertDateTime ()
    {
      var format = @"yyyy'/'MM'/'dd HH':'mm':'ss.fff";
      var dateTime = new DateTime (2021, 01, 02, 03, 04, 05);
      var s = dateTime.ToString (format);
      Assert.That (s, Is.EqualTo ("2021/01/02 03:04:05.000"));
      var s1 = dateTime.ToString (format, System.Globalization.CultureInfo.InvariantCulture);
      Assert.That (s1, Is.EqualTo (s));
      var dateTime2 = DateTime.ParseExact (s, format, System.Globalization.CultureInfo.InvariantCulture);
      Assert.That (dateTime2, Is.EqualTo (dateTime));
    }
  }
}
