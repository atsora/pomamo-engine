// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

// Note: some tests do not work with nunit-console-runner
// but they work with VisualStudio
// The error is: System.PlatformNotSupportedException : System.Data.ODBC is not supported on this platform.
//#define ENABLE_ODBCFACTORY_UNITTEST_VISUALSTUDIO

using System;
using System.IO;
using System.Xml;

using Lemoine.GDBPersistentClasses;
using Lemoine.GDBUtils;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Data.Odbc;
using NUnit.Framework;
using Lemoine.Info;

namespace Lemoine.DataRepository.UnitTests
{
  /// <summary>
  /// Unit tests for the class ODBCFactory.
  /// </summary>
  [TestFixture]
  public class ODBCFactory_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ODBCFactory_UnitTest).FullName);

    string m_unitTestsIn;
    string m_previousDSNName;

    /// <summary>
    /// Test the constructor
    /// </summary>
    [Test]
    public void TestODBCFactory()
    {
      // With a valid XML file
      ODBCFactory f = new ODBCFactory (XmlSourceType.URI,
                                       System.IO.Path.Combine (m_unitTestsIn, "testODBCSynchroFactory-Valid.xml"),
                                       new ClassicConnectionParameters());
      
      // With an invalid XML file with an empty root element
      Assert.Throws<ODBCFactory.SchemaException>
        (delegate { new ODBCFactory (XmlSourceType.URI,
                                     System.IO.Path.Combine (m_unitTestsIn, "testODBCFactory-Invalid1.xml"),
                                        new ClassicConnectionParameters()); },
         "testODBCFactory-Invalid1.xml");
      
      // With an invalid XML file without any root element
      Assert.Throws<XmlException>
        (delegate { new ODBCFactory (XmlSourceType.URI,
                                     System.IO.Path.Combine (m_unitTestsIn, "testODBCFactory-Invalid2.xml"),
                                        new ClassicConnectionParameters()); },
         "testODBCFactory-Invalid2.xml");
      
      // With an invalid XML file with a bad XML syntax
      Assert.Throws<XmlException>
        (delegate { new ODBCFactory (XmlSourceType.URI,
                                     System.IO.Path.Combine (m_unitTestsIn, "testODBCFactory-Invalid3.xml"),
                                        new ClassicConnectionParameters()); },
         "testODBCFactory-Invalid3.xml");
      
      // With an unknown XML file
      // Throws FileNotFoundException on Nicolas'computer and UriFormatException on Lionel's computer
      // Not clear why yet: de-activate it for the moment
      /*
      Assert.Throws<Exception>
        (delegate { new ODBCFactory (XmlSourceType.URI,
                                        @"file:///c:/tmp/unknown.xml",
                                        new ClassicConnectionParameters()); },
         @"C:\tmp\unknown.xml");
         */
      
      // With a valid XML string
      string xmlData =
        @"<?xml version='1.0'?>
<root xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:odbc'
      pulse:dsnname='MysqlDB' pulse:user='nicolas' pulse:password='soultz'>
  <job name='pulse:1' type='pulse:2:integer'
       pulse:request='SELECT name, type FROM `job` '>
    <component name='pulse:1'
               pulse:request='SELECT name FROM `component` WHERE jobname=""{../@name}""' />
  </job>
</root>";
      f = new ODBCFactory (XmlSourceType.STRING, xmlData, new ClassicConnectionParameters());
      Assert.NotNull (f);
      
      // With an invalid XML string with no root element
      Assert.Throws<XmlException>
        (delegate { new ODBCFactory (XmlSourceType.STRING,
                                        "no root element",
                                        new ClassicConnectionParameters()); },
         "string with no root element");
      
      // With an invalid XML string with no connection parameters
      Assert.Throws<ODBCFactory.SchemaException>
        (delegate { new ODBCFactory (XmlSourceType.STRING,
                                        "<root />",
                                        new ClassicConnectionParameters()); },
         "string with no connection parameters");
    }

    /// <summary>
    /// Test InitConnectParam method
    /// </summary>
    [Test]
    public void TestInitConnectParam ()
    {
      // Test the three values
      string xmlData =
        @"<?xml version='1.0'?>
<root xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
      xsi:noNamespaceSchemaLocation='factodbcconfig.xsd'
      xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:odbc'
      pulse:dsnname='LemoineUnitTests' pulse:user='DatabaseUser' pulse:password='DatabasePassword'>
</root>";
      ODBCFactory f = new ODBCFactory (XmlSourceType.STRING,
                                       xmlData, new ClassicConnectionParameters());
      Assert.AreEqual(f.ConnectionParameters.OdbcConnectionString(),
                      "DSN=LemoineUnitTests;UID=DatabaseUser;Pwd=DatabasePassword;");
      /*
      Assert.AreEqual (f.ConnectionParameters.DsnName, "LemoineUnitTests");
      Assert.AreEqual (f.ConnectionParameters.Username, "DatabaseUser");
      Assert.AreEqual (f.ConnectionParameters.Password, "DatabasePassword");
       */
    }

    /// <summary>
    /// Test GetData method
    /// </summary>
#if ENABLE_ODBCFACTORY_UNITTEST_VISUALSTUDIO
    [Test]
#endif // ENABLE_ODBCFACTORY_UNITTEST_VISUALSTUDIO
    public void TestGetData ()
    {
      // Initialization
      string xmlData =
        @"<?xml version='1.0'?>
<root xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
      xsi:noNamespaceSchemaLocation='factodbcconfig.xsd'
      xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:odbc'
      pulse:dsnname='LemoineUnitTests' pulse:user='DatabaseUser' pulse:password='DatabasePassword'>
  <job name='pulse:1' type='pulse:2:integer'
       customer='pulse:3' hours='pulse:4' status='pulse:5'
       isjobname='pulse:6'
       pulse:request=""SELECT name, type, customer, hours, status, (name = 'JOBNAME') AS isjobname
                       FROM testjob ORDER BY name DESC""
       pulse:attr='anypulseattr' attr='anyotherattr' >
    <component name='pulse:1:string'
               starttime='pulse:2' endtime='pulse:3'
               hours='pulse:4' done='pulse:5'
               id='pulse:6' componenttype='{@pulse:type}'
               pulse:request=""SELECT name, starttime, endtime, hours, done, id, type
                               FROM testcomponent
                               WHERE jobname='{../@name}'
                               AND id=7 AND type={../@type}""
               pulse:if=""{../@isjobname}"" />
    <testif pulse:if=""{../@isjobname}"" />
  </job>
  <anyelement attr='anyattr' />
</root>";
      ODBCFactory f = new ODBCFactory (XmlSourceType.STRING, xmlData, new ClassicConnectionParameters());
      XmlDocument doc = f.GetData (cancellationToken: System.Threading.CancellationToken.None);
      
      // Tests
      Assert.IsNotNull (doc);
      XmlElement root = doc.DocumentElement;
      Assert.IsNotNull (root);
      Assert.AreEqual ("root", root.Name);
      
      // job[1]
      Assert.AreEqual (2, root.GetElementsByTagName ("job").Count);
      XmlElement job1 = root.GetElementsByTagName ("job") [0] as XmlElement;
      Assert.IsNotNull (job1);
      Assert.AreEqual ("JOBNAME", job1.GetAttribute ("name"));
      Assert.AreEqual ("1", job1.GetAttribute ("type"));
      Assert.AreEqual ("", job1.GetAttribute ("status"));
      Assert.AreEqual ("", job1.GetAttribute ("request", PulseResolver.PULSE_ODBC_NAMESPACE));
      Assert.AreEqual ("anyotherattr", job1.GetAttribute ("attr"));
      
      // job[1]/component
      Assert.AreEqual (2, job1.GetElementsByTagName ("component").Count);
      // job[1]/component[1]
      XmlElement component = job1.GetElementsByTagName ("component") [0] as XmlElement;
      Assert.IsNotNull (component);
      Assert.AreEqual ("COMPNAME", component.GetAttribute ("name"));
      Assert.AreEqual ("2007-07-01 00:00:00", component.GetAttribute ("starttime"));
      Assert.AreEqual ("2007-07-02 00:00:00", component.GetAttribute ("endtime"));
      Assert.AreEqual ("5.1", component.GetAttribute ("hours"));
      Assert.AreEqual ("False", component.GetAttribute ("done")); // In the ODBC settings, Bools as char must be off
      Assert.AreEqual ("7", component.GetAttribute ("id"));
      Assert.AreEqual ("1", component.GetAttribute ("componenttype"));
      
      Assert.AreEqual (1, job1.GetElementsByTagName ("testif").Count);
      
      XmlElement job2 = root.GetElementsByTagName ("job") [1] as XmlElement;
      Assert.AreEqual (0, job2.GetElementsByTagName ("testif").Count);
    }

    /// <summary>
    /// Test GetData method
    /// </summary>
#if ENABLE_ODBCFACTORY_UNITTEST_VISUALSTUDIO
    [Test]
#endif // ENABLE_ODBCFACTORY_UNITTEST_VISUALSTUDIO
    public void TestGetData2 ()
    {
      // Initialization
      string xmlData =
        @"<?xml version='1.0'?>
<root xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
      xmlns:gdb=""urn:pulse.lemoinetechnologies.com:synchro:gdb""
      xmlns:odbc='urn:pulse.lemoinetechnologies.com:synchro:odbc'
      odbc:dsnname='LemoineUnitTests' odbc:user='DatabaseUser' odbc:password='DatabasePassword'>
  <req gdb:action=""none""
       machine=""pulse:0""
       odbc:request=""SELECT machinename AS machine FROM machine WHERE machineid=1"">
    <Machine Name=""{../@machine}""
             gdb:action='reference'
             gdb:notfound='log create'
             gdb:relation='none'
             gdb:newattributes='Id'>
      <MonitoringType TranslationKey='UndefinedValue' gdb:action='reference' gdb:notfound='fail' />
    </Machine>
  </req>
</root>";
      ODBCFactory f = new ODBCFactory (XmlSourceType.STRING, xmlData, new ClassicConnectionParameters());
      XmlDocument doc = f.GetData (cancellationToken: System.Threading.CancellationToken.None);
      
      // Tests
      Assert.IsNotNull (doc);
      XmlElement root = doc.DocumentElement;
      Assert.IsNotNull (root);
      Assert.AreEqual ("root", root.Name);
      
      // Machine[1]
      Assert.AreEqual (1, root.GetElementsByTagName ("Machine").Count);
      XmlElement machine1 = root.GetElementsByTagName ("Machine") [0] as XmlElement;
      Assert.IsNotNull (machine1);
      Assert.AreEqual ("MACHINE_A17", machine1.GetAttribute ("Name"));
    }

    /// <summary>
    /// Test GetConfigurationValue method
    /// </summary>
    [Test]
    public void TestGetConfigurationValue ()
    {
      // Initialization
      string xmlData =
        @"<?xml version='1.0'?>
<root xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
      xsi:noNamespaceSchemaLocation='factodbcconfig.xsd'
      xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:odbc'
      pulse:dsnname='MysqlDB' pulse:user='nicolas' pulse:password='soultz'
      confkey='confvalue'>
</root>";
      ODBCFactory f = new ODBCFactory (XmlSourceType.STRING, xmlData, new ClassicConnectionParameters());
      string result = f.GetConfigurationValue ("@confkey");
      Assert.AreEqual ("confvalue", result);
      result = f.GetConfigurationValue ("@unknown");
      Assert.Null (result);
    }

    /// <summary>
    /// Test FlagSynchronizationAsSuccess method
    /// </summary>
#if ENABLE_ODBCFACTORY_UNITTEST_VISUALSTUDIO
    [Test]
#endif // ENABLE_ODBCFACTORY_UNITTEST_VISUALSTUDIO
    public void TestFlagSynchronizationAsSuccess ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        Lemoine.Model.IApplicationState applicationState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
          .GetApplicationState ("synchro.test.a");
        if (null == applicationState) {
          applicationState = ModelDAOHelper.ModelFactory.CreateApplicationState ("synchro.test.a");
        }
        applicationState.Value = "name";
        ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (applicationState);
        Lemoine.Model.IConfig config = ModelDAOHelper.DAOFactory.ConfigDAO
          .GetConfig ("synchro.test.b");
        if (null == config) {
          config = ModelDAOHelper.ModelFactory.CreateConfig ("synchro.test.b");
        }
        config.Value = "type";
        ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
        transaction.Commit ();
      }

      // Initialization
      string xmlData =
        @"<?xml version='1.0'?>
<root xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
      xsi:noNamespaceSchemaLocation='factodbcconfig.xsd'
      xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:odbc'
      pulse:dsnname='LemoineUnitTests' pulse:user='DatabaseUser' pulse:password='DatabasePassword'>
  <job name='pulse:1' type='pulse:2:integer'
       customer='pulse:3' hours='pulse:4' status='pulse:5'
       isjobname='pulse:6'
       pulse:request=""SELECT [%applicationstate.synchro.test.a%], [%config.synchro.test.b%], customer, hours, status, (name = 'JOBNAME') AS isjobname
                       FROM testjob ORDER BY name DESC""
       pulse:attr='anypulseattr' attr='anyotherattr'
       pulse:synchronizationok=""UPDATE testjob SET status={//job/@type}""
       pulse:applicationstatestatus=""synchro.test=3"">
    <component name='pulse:1:string'
               starttime='pulse:2' endtime='pulse:3'
               hours='pulse:4' done='pulse:5'
               id='pulse:6' componenttype='{@pulse:type}'
               pulse:request=""SELECT name, starttime, endtime, hours, done, id, type
                               FROM testcomponent
                               WHERE jobname='{../@name}'
                               AND id=7 AND type={../@type}""
               pulse:if=""{../@isjobname}"" />
    <testif pulse:if=""{../@isjobname}"" />
  </job>
  <anyelement attr='anyattr' />
</root>";
      ODBCFactory f = new ODBCFactory (XmlSourceType.STRING, xmlData, new ClassicConnectionParameters());
      XmlDocument doc = f.GetData (cancellationToken: System.Threading.CancellationToken.None);
      Assert.IsNotNull (doc);
      XmlElement root = doc.DocumentElement;
      Assert.IsNotNull (root);
      f.FlagSynchronizationAsSuccess (doc);
      
      ConnectionParameters connectionParameters =
        new ConnectionParameters ("LemoineUnitTests");
      connectionParameters.LoadParameters();
      
      OdbcConnection connection =
        new OdbcConnection (connectionParameters.OdbcConnectionString);
      try {
        connection.Open ();
        
        // Test the values
        OdbcCommand select =
          new OdbcCommand (@"SELECT status FROM testjob",
                           connection);
        OdbcDataReader reader = select.ExecuteReader ();
        int nbRows = 0;
        while (reader.Read ()) {
          ++nbRows;
          Assert.AreEqual (1, reader.FieldCount);
          Assert.False (reader.IsDBNull (0), "status is DBNull");
          Assert.LessOrEqual (1, (int) reader.GetValue (0));
        }
        reader.Close ();
        Assert.AreEqual (2, nbRows);
        
        // Clean the database
        OdbcCommand update =
          new OdbcCommand (@"UPDATE testjob SET status=NULL",
                           connection);
        update.ExecuteNonQuery ();
      }
      finally {
        connection.Dispose ();
      }
      
      // Test the values and reset the database
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        Lemoine.Model.IApplicationState applicationState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
          .GetApplicationState ("synchro.test");
        Assert.IsNotNull (applicationState);
        Assert.AreEqual ("3", applicationState.Value);
        ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakeTransient (applicationState);
        transaction.Commit ();
      }
    }
    
    [OneTimeSetUp]
    public void Init()
    {
      m_unitTestsIn = TestContext.CurrentContext.TestDirectory;
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }
    }
    
  }
}
