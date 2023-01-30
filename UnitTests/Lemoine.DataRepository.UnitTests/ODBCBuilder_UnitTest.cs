// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

// Note: the TestBuild test does not work with nunit-console-runner
// but it works with VisualStudio
// The error is: System.PlatformNotSupportedException : System.Data.ODBC is not supported on this platform.
//#define ENABLE_ODBCBUILDER_UNITTEST_TESTBUILD

using System;
using System.IO;
using System.Xml;

using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.DataRepository.UnitTests
{
  /// <summary>
  /// Unit tests for the class ODBCBuilder.
  /// </summary>
  [TestFixture]
  public class ODBCBuilder_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ODBCBuilder_UnitTest).FullName);

    readonly XmlDocument document = new XmlDocument ();
    string m_unitTestsIn;
    
    /// <summary>
    /// Test of the constructor
    /// </summary>
    [Test]
    public void TestODBCBuilder()
    {
      ODBCBuilder b;
      
      // With a valid XML File
      b = new ODBCBuilder (XmlSourceType.URI,
                           System.IO.Path.Combine (m_unitTestsIn, "testODBCSynchroBuilder-Valid.xml"));
      Assert.AreEqual (b.ConnectionParameters.DsnName, "LemoineUnitTests");
      Assert.AreEqual (b.ConnectionParameters.Username, "DatabaseUser");
      Assert.AreEqual (b.ConnectionParameters.Password, "DatabasePassword");
      
      // With a valid odbcfactconfig XML File
      b = new ODBCBuilder (XmlSourceType.URI,
                           System.IO.Path.Combine (m_unitTestsIn, "testSynchroFactodbcconfig-Valid.xml"));
      Assert.AreEqual (b.ConnectionParameters.DsnName, "MSSQLuser");
      Assert.AreEqual (b.ConnectionParameters.Username, "User");
      Assert.AreEqual (b.ConnectionParameters.Password, "password");
      
      // With a valid unicode odbcfactconfig XML File
      b = new ODBCBuilder (XmlSourceType.URI,
                           System.IO.Path.Combine (m_unitTestsIn, "testSynchroFactodbcconfigUnicode-Valid.xml"));
      Assert.AreEqual (b.ConnectionParameters.DsnName, "MSSQLuser");
      Assert.AreEqual (b.ConnectionParameters.Username, "User");
      Assert.AreEqual (b.ConnectionParameters.Password, "password");
      
      // With an invalid XML file with an empty root element (no DSN name)
      Assert.Throws<ODBCBuilder.SchemaException>
        (delegate {
           b = new ODBCBuilder (XmlSourceType.URI,
                                System.IO.Path.Combine (m_unitTestsIn, "testODBCBuilder-Invalid1.xml"));
         },
         "Invalid XML file with an empty root element");
      
      // With an invalid XML file without any root element
      Assert.Throws<XmlException>
        (delegate {
           b = new ODBCBuilder (XmlSourceType.URI,
                                System.IO.Path.Combine (m_unitTestsIn, "testODBCBuilder-Invalid2.xml"));
         },
         "Invalid XML file without any root element");
      
      // With an invalid XML file with a bad XML syntax
      Assert.Throws<XmlException>
        (delegate {
           b = new ODBCBuilder (XmlSourceType.URI,
                                System.IO.Path.Combine (m_unitTestsIn, "testODBCBuilder-Invalid3.xml"));
         },
         "Invalid XML file with a bad XML syntax");
      
      // With an unknown XML file
      // Throws FileNotFoundException on Nicolas'computer and UriFormatException on Lionel's computer
      // Not clear why yet...: de-activate it for the moment
      /*
      Assert.Throws<Exception>
        (delegate {
           b = new ODBCBuilder (XmlSourceType.URI,
                                "file:///C:/tmp/unknown.xml");
         },
         "Unknown XML file bad exception");
         */
      
      // With a valid XML string
      string xmlData =
        "<?xml version='1.0'?>\n" +
        "<root xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:odbc' " +
        "      pulse:dsnname='LemoineUnitTests' " +
        "      pulse:user='DatabaseUser' pulse:password='DatabasePassword'>" +
        "  <element id='' " +
        "           pulse:request='UPDATE testodbcbuilder SET id={@id}' />" +
        "</root>";
      b = new ODBCBuilder (XmlSourceType.STRING,
                           xmlData);
      Assert.AreEqual (b.ConnectionParameters.DsnName, "LemoineUnitTests");
      Assert.AreEqual (b.ConnectionParameters.Username, "DatabaseUser");
      Assert.AreEqual (b.ConnectionParameters.Password, "DatabasePassword");
      
      // With an invalid XML string with no root element
      xmlData = "no root element";
      Assert.Throws<XmlException>
        (delegate {
           b = new ODBCBuilder (XmlSourceType.STRING,
                                xmlData);
         },
         "Invalid XML string with no root element bad exception");
      
      // With an invalid XML string with no connection parameters
      xmlData =
        "<?xml version='1.0'?>\n" +
        "<root />";
      Assert.Throws<ODBCBuilder.SchemaException>
        (delegate {
           b = new ODBCBuilder (XmlSourceType.STRING,
                                xmlData);
         });
    }
    
    /// <summary>
    /// Test InitConnectParam method
    /// </summary>
    [Test]
    public void TestInitConnectParam ()
    {
      // Exception cases were tester in TestODBCBuilder ()
      
      // Test the three values
      const string xmlData = "<?xml version='1.0'?>\n" + "<root xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:noNamespaceSchemaLocation='factodbcconfig.xsd' " + "      xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:odbc' " + "      pulse:dsnname='LemoineUnitTests' pulse:user='DatabaseUser' pulse:password='DatabasePassword'>" + "</root>";
      ODBCBuilder b = new ODBCBuilder (XmlSourceType.STRING,
                                       xmlData);
      Assert.AreEqual (b.ConnectionParameters.DsnName, "LemoineUnitTests");
      Assert.AreEqual (b.ConnectionParameters.Username, "DatabaseUser");
      Assert.AreEqual (b.ConnectionParameters.Password, "DatabasePassword");
    }

#if ENABLE_ODBCBUILDER_UNITTEST_TESTBUILD
    [Test]
#endif // ENABLE_ODBCBUILDER_UNITTEST_TESTBUILD
    public void TestBuild ()
    {
      const string xmlData = "<?xml version='1.0'?>\n" + "<root xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:odbc' " + "      pulse:dsnname='LemoineUnitTests' pulse:user='DatabaseUser' pulse:password='DatabasePassword'>" + "  <parent id=''>" + "    <element id='' pulse:request=" + "       'UPDATE testodbcbuilder SET id={@id}, parent={../@id}' />" + "  </parent>" + "  <anyelement attr='anyattr' />" + "</root>";
      ODBCBuilder b = new ODBCBuilder (XmlSourceType.STRING,
                                       xmlData);
      
      // Tests
      b.Build (document, cancellationToken: System.Threading.CancellationToken.None);
    }
    
    [OneTimeSetUp]
    public void Init()
    {
      document.LoadXml ("<?xml version='1.0'?>\n" +
                        "<root name='rootelement'>" +
                        "  <empty />" +
                        "  <parent id='9'>" +
                        "    <element id='2' />" +
                        "  </parent>" +
                        "</root>"
                       );
      
      m_unitTestsIn = TestContext.CurrentContext.TestDirectory;
    }
  }
}
