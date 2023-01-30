// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.Xml.UnitTests
{
  /// <summary>
  /// Unit tests for the class XslDefinition
  /// </summary>
  [TestFixture]
  public class XslDefinition_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (XslDefinition_UnitTest).FullName);

    /// <summary>
    /// Test the serialization
    /// </summary>
    [Test]
    public void TestSerialize()
    {
      XslTextDefinition xslDefinition = new XslTextDefinition ();
      xslDefinition.XslText = "test";
      {
        XmlSerializer serializer = new XmlSerializer (typeof (XslDefinition));
        using (var stringWriter = new StringWriter ()) {
          using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
            stringWriter.NewLine = "\n";
            serializer.Serialize (stringWriter, xslDefinition);
            var s = stringWriter.ToString ();
            Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<XslDefinition xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xsi:type="XslTextDefinition">
  <XslText>test</XslText>
</XslDefinition>
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
<XslDefinition xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xsi:type=""XslTextDefinition"">
  <XslText>Test</XslText>
</XslDefinition>");
      XmlSerializer serializer = new XmlSerializer (typeof (XslDefinition));
      XslDefinition xslDefinition = (XslDefinition)
        serializer.Deserialize (textReader);
      Assert.IsInstanceOf (typeof (XslTextDefinition), xslDefinition);
      Assert.AreEqual (@"<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"">
<xsl:output method=""text"" omit-xml-declaration=""yes"" indent=""no"" />
<xsl:template match=""/"">
  <xsl:text>Test</xsl:text>
</xsl:template>
</xsl:stylesheet>", ((XslTextDefinition)xslDefinition).XslFull);
    }
  }
}
