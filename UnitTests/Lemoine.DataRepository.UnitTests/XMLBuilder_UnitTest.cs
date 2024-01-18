// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Xml;

using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.DataRepository.UnitTests
{
  /// <summary>
  /// Unit tests for the class XMLBuilder
  /// </summary>
  [TestFixture]
  public class XMLBuilder_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (XMLBuilder_UnitTest).FullName);

    /// <summary>
    /// Test the constructor
    /// </summary>
    [Test]
    public void TestXMLBuilder()
    {
      XMLBuilder builder = new XMLBuilder (".\\UnitTests-out\\testXMLBuilder.xml");
      Assert.That (builder.Filename, Is.EqualTo (".\\UnitTests-out\\testXMLBuilder.xml"));

    }
    
    [Test]
    public void TestBuild ()
    {
      string xmlData =
        "<root>" +
        "  <job name=\"Test\" customer=\"SOFTEK\" " +
        "       hours=\"5.3\" status=\"VALID\" " +
        "       due=\"2007-06-29 00:00:00\" projtype=\"IP\" >" +
        "    <component name=\"Test\" hours=\"1.5\" done=\"1\" " +
        "               starttime=\"2007-06-25 00:00:00\" " +
        "               endtime=\"2007-06-26 00:00:00\" " +
        "               comptype=\"CORE\" />" +
        "  </job>" +
        "</root>";
      XmlDocument document = new XmlDocument ();
      document.LoadXml (xmlData);
      
      // Test1
      Directory.CreateDirectory (".\\UnitTests-out");
      XMLBuilder builder = new XMLBuilder (".\\UnitTests-out\\testXMLBuilder.xml");
      builder.Build (document, cancellationToken: System.Threading.CancellationToken.None);
    }
  }
}
