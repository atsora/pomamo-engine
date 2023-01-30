// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.ModelDAO.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class CncAlarmSeverityPattern_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncAlarmSeverityPattern_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncAlarmSeverityPattern_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestSerialize ()
    {
      var rule = new CncAlarmSeverityPatternRules ();
      rule.Message = "This is a message";
      rule.Properties.Add ("A", "1");
      var json = JsonSerializer.Serialize (rule, new JsonSerializerOptions {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
      });
      log.Debug ($"TestDeserialize: json={json}");
      Assert.AreEqual ("""
{
  "message": "This is a message",
  "properties": {
    "A": "1"
  }
}
""".ReplaceLineEndings (), json.ReplaceLineEndings ());
    }

    [Test]
    public void TestDeserialize ()
    {
      var json = """
{
  "Message": "This is a message",
  "Properties": {
    "A": "1"
  }
}
""";
      var rule = JsonSerializer.Deserialize<CncAlarmSeverityPatternRules> (json, new JsonSerializerOptions {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault,
        PropertyNameCaseInsensitive = true,
      });
      Assert.AreEqual ("This is a message", rule.Message);
      Assert.IsNull (rule.Number);
      Assert.IsNull (rule.CncSubInfo);
      Assert.IsNull (rule.Type);
    }
  }
}
