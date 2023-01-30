// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Xml.Serialization;

using Lemoine.Xml;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Extensions.Alert;
using System.Xml;

namespace Lemoine.Alert.UnitTests
{
  /// <summary>
  /// Unit tests for the class XslEMailAction
  /// </summary>
  [TestFixture]
  public class XslEMailAction_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (XslEMailAction_UnitTest).FullName);

    /// <summary>
    /// Test the serialization
    /// </summary>
    [Test]
    public void TestSerialize()
    {
      XslEMailAction xslEMailAction = new XslEMailAction ();
      xslEMailAction.To = "Recipient <recipient@company>";
      xslEMailAction.Subject = new XslTextDefinition ("Subject");
      xslEMailAction.Body = new XslTextDefinition ("The body");
      xslEMailAction.WeekDays = Model.WeekDay.Monday | Model.WeekDay.Wednesday;
      IAction action = xslEMailAction;
      {
        XmlSerializer serializer = new XmlSerializer (typeof (XslEMailAction));
        using (var stringWriter = new StringWriter ()) {
          using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
            stringWriter.NewLine = "\n";
            serializer.Serialize (stringWriter, action);
            var s = stringWriter.ToString ();
            Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<XslEMailAction xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" To="Recipient &lt;recipient@company&gt;" CC="" Bcc="" WeekDays="5">
  <TimePeriod>00:00:00-00:00:00</TimePeriod>
  <Subject xsi:type="XslTextDefinition">
    <XslText>Subject</XslText>
  </Subject>
  <Body xsi:type="XslTextDefinition">
    <XslText>The body</XslText>
  </Body>
  <UtcDateTime xsi:type="XslTextDefinition">
    <XslText />
  </UtcDateTime>
</XslEMailAction>
""".ReplaceLineEndings (),
                             s.ReplaceLineEndings ());
          }
        }
      }
    }

    /// <summary>
    /// Test the de-serialization
    /// </summary>
    [Test]
    public void TestDeserialize()
    {
      TextReader textReader = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
<BasicEMailAction type=""Lemoine.Alert.BasicEMailAction, Lemoine.Alert"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" To=""Recipient &lt;recipient@company&gt;"" CC="""" Bcc="""" Subject=""Subject"">
  <Body>The body</Body>
</BasicEMailAction>");
      XmlSerializer serializer = new XmlSerializer (typeof (BasicEMailAction));
      IAction action =
        (IAction) serializer.Deserialize (textReader);
      Assert.IsTrue (action is BasicEMailAction);
      BasicEMailAction basicEMailAction = action as BasicEMailAction;
      Assert.AreEqual ("Subject", basicEMailAction.Subject);
    }
  }
}
