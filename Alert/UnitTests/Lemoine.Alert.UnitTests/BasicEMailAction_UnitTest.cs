// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Lemoine.Core.Log;
using Lemoine.Extensions.Alert;
using NUnit.Framework;

namespace Lemoine.Alert.UnitTests
{
  /// <summary>
  /// Unit tests for the class BasicEMailAction
  /// </summary>
  [TestFixture]
  public class BasicEMailAction_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (BasicEMailAction_UnitTest).FullName);

    /// <summary>
    /// Test the serialization
    /// </summary>
    [Test]
    public void TestSerialize()
    {
      BasicEMailAction basicEMailAction = new BasicEMailAction ();
      basicEMailAction.To = "Recipient <recipient@company>";
      basicEMailAction.Subject = "Subject";
      basicEMailAction.Body = "The body";
      basicEMailAction.WeekDays = Model.WeekDay.Monday | Model.WeekDay.Tuesday;
      IAction action = basicEMailAction;
      {
        XmlSerializer serializer = new XmlSerializer (typeof (BasicEMailAction));
        using (var stringWriter = new StringWriter ()) {
          using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
            stringWriter.NewLine = "\n";
            serializer.Serialize (stringWriter, action);
            var s = stringWriter.ToString ();
            Assert.That (s.ReplaceLineEndings (), Is.EqualTo ("""
<?xml version="1.0" encoding="utf-16"?>
<BasicEMailAction xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" To="Recipient &lt;recipient@company&gt;" CC="" Bcc="" WeekDays="3" Subject="Subject">
  <TimePeriod>00:00:00-00:00:00</TimePeriod>
  <Body>The body</Body>
</BasicEMailAction>
""".ReplaceLineEndings ()));
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
<BasicEMailAction type=""Lemoine.Alert.BasicEMailAction, Lemoine.Alert"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" To=""Recipient &lt;recipient@company&gt;"" CC="""" Bcc="""" Subject=""Subject"" WeekDays=""62"">
  <Body>The body</Body>
</BasicEMailAction>");
      XmlSerializer serializer = new XmlSerializer (typeof (BasicEMailAction));
      IAction action =
        (IAction) serializer.Deserialize (textReader);
      Assert.That (action is BasicEMailAction, Is.True);
      BasicEMailAction basicEMailAction = action as BasicEMailAction;
      Assert.Multiple (() => {
        Assert.That (basicEMailAction.Subject, Is.EqualTo ("Subject"));
        Assert.That (basicEMailAction.WeekDays, Is.EqualTo (Model.WeekDay.Tuesday | Model.WeekDay.Wednesday | Model.WeekDay.Thursday | Model.WeekDay.Friday | Model.WeekDay.Saturday));
      });
    }
  }
}
