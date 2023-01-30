// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Lemoine.Alert;
using Lemoine.Core.Log;
using Lemoine.Extensions.Alert;
using NUnit.Framework;

namespace Lemoine.Alert.TestListeners.UnitTests
{
  /// <summary>
  /// Unit tests for the class TODO: MyClassName.
  /// </summary>
  [TestFixture]
  public class LoopListener_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (LoopListener_UnitTest).FullName);

    /// <summary>
    /// Test the serialization
    /// </summary>
    [Test]
    public void TestSerialize()
    {
      LoopListener listener = new LoopListener ();
      listener.Frequency = TimeSpan.FromSeconds (1.5);
      listener.Data = @"<Test />";
      {
        XmlSerializer serializer = new XmlSerializer (typeof (LoopListener));
        using (var stringWriter = new StringWriter ()) {
          using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
            stringWriter.NewLine = "\n";
            serializer.Serialize (stringWriter, listener);
            var s = stringWriter.ToString ();
            Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<LoopListener xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" Frequency="00:00:01.5000000">
  <Data>&lt;Test /&gt;</Data>
</LoopListener>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
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
<LoopListener xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Frequency=""00:00:04"">
  <Data><![CDATA[<Test />]]></Data>
</LoopListener>");
      XmlSerializer serializer = new XmlSerializer (typeof (LoopListener));
      IListener listener =
        (IListener) serializer.Deserialize (textReader);
      Assert.IsTrue (listener is LoopListener);
      LoopListener loopListener = listener as LoopListener;
      Assert.AreEqual ("<Test />", loopListener.Data);
    }

  }
}
