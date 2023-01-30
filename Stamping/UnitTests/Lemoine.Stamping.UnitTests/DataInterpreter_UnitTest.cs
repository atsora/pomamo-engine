// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Stamping.DataInterpreters;
using NUnit.Framework;

namespace Lemoine.Stamping.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class DataInterpreter_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (DataInterpreter_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DataInterpreter_UnitTest ()
    { }

    /// <summary>
    /// Test the regex is successful
    /// </summary>
    [Test]
    public void TestRegexSuccess ()
    {
      var stampingData = new StampingData ();
      var cadName = "Project_Name";
      TestCadName (stampingData, cadName);
      Assert.AreEqual ($"Project", stampingData.Get<string> ("ProjectName"));
      Assert.AreEqual ($"Name", stampingData.Get<string> ("ComponentName"));
    }

    /// <summary>
    /// Test the fallback is used
    /// </summary>
    [Test]
    public void TestFallback ()
    {
      var stampingData = new StampingData ();
      var cadName = "Project-Name";
      TestCadName (stampingData, cadName);
      Assert.AreEqual ($"?{cadName}?", stampingData.Get<string> ("ProjectName"));
      Assert.AreEqual ($"?{cadName}?", stampingData.Get<string> ("ComponentName"));
    }

    void TestCadName (StampingData stampingData, string cadName)
    {
      stampingData.Add ("CadName", cadName);
      var json = """
{
  "Source": "CadName"
}
""";
      var regexFallbackDataInterpreter = System.Text.Json.JsonSerializer.Deserialize<RegexFallbackInterpreter> (json);
      regexFallbackDataInterpreter.Regex = @"(?<ProjectName>\w+)_(?<ComponentName>\w+)";
      var result = regexFallbackDataInterpreter.Interpret (stampingData);
      Assert.IsTrue (result);
    }

    /// <summary>
    /// Test the fallback is used
    /// </summary>
    [Test]
    public void TestFallbackWithFormat ()
    {
      var stampingData = new StampingData ();
      var cadName = "Project-Name";

      stampingData.Add ("CadName", cadName);
      var json = """
{
  "Source": "CadName",
  "Format": "...{0}..."
}
""";
      var regexFallbackDataInterpreter = System.Text.Json.JsonSerializer.Deserialize<RegexFallbackInterpreter> (json);
      regexFallbackDataInterpreter.Regex = @"(?<ProjectName>\w+)_(?<ComponentName>\w+)";
      var result = regexFallbackDataInterpreter.Interpret (stampingData);
      Assert.IsTrue (result);

      Assert.AreEqual ($"...{cadName}...", stampingData.Get<string> ("ProjectName"));
      Assert.AreEqual ($"...{cadName}...", stampingData.Get<string> ("ComponentName"));
    }

    /// <summary>
    /// Test the fallback is used
    /// </summary>
    [Test]
    public void TestSuccessWithCustomRegex ()
    {
      var stampingData = new StampingData ();
      var cadName = "Project-Name";

      stampingData.Add ("CadName", cadName);
      var json = """
{
  "Source": "CadName",
  "Regex": "(?<ProjectName>\\w+)-(?<ComponentName>\\w+)"
}
""";
      var regexFallbackDataInterpreter = System.Text.Json.JsonSerializer.Deserialize<RegexFallbackInterpreter> (json);
      var result = regexFallbackDataInterpreter.Interpret (stampingData);
      Assert.IsTrue (result);

      Assert.AreEqual ($"Project", stampingData.Get<string> ("ProjectName"));
      Assert.AreEqual ($"Name", stampingData.Get<string> ("ComponentName"));
    }


  }
}
