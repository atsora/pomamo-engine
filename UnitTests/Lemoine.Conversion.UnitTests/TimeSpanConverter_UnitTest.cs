// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Conversion.JavaScript;
using Lemoine.Core.Log;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Lemoine.Conversion.UnitTests
{
  class Test
  {
    public TimeSpan Duration { get; set; }
  }
  
  /// <summary>
  /// Unit tests for the class TimeSpanConverter.
  /// </summary>
  [TestFixture]
  public class TimeSpanConverter_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (TimeSpanConverter_UnitTest).FullName);

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void TestSerialize()
    {
      var test = new Test ();
      test.Duration = TimeSpan.FromMinutes (3);

      string s = JsonConvert.SerializeObject (test, new Lemoine.Conversion.JavaScript.TimeSpanConverter ());
      Assert.AreEqual ("{\"Duration\":\"00:03:00\"}", s);
    }
    
    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void TestDeserialize()
    {
      const string s = "{\"Duration\":\"0:03:00\"}";
      Test test = JsonConvert.DeserializeObject<Test> (s);
      Assert.AreEqual (TimeSpan.FromMinutes (3), test.Duration);
    }
  }
}
