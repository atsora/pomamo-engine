// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Lemoine.Core.Log;
using Newtonsoft.Json;

namespace Lemoine.Collections.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class JsonAsDictionary_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (JsonAsDictionary_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public JsonAsDictionary_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test ()
    {
      var json = @"
{
  ""a"": ""a value"",
  ""b"": 123
}
";
      var dictionary = JsonConvert.DeserializeObject<IDictionary<string, object>> (json);
      Assert.That (dictionary["a"], Is.EqualTo ("a value"));
      var b = dictionary["b"]; // b is long
      var bInt = Convert.ToInt32 (b);
      Assert.That (dictionary["b"], Is.EqualTo (123));
    }
  }
}
