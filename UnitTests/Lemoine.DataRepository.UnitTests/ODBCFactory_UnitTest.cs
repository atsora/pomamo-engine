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
using Lemoine.Info;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Data.Odbc;
using NUnit.Framework;

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
      var f = new ODBCFactory (XmlSourceType.URI,
                               System.IO.Path.Combine (m_unitTestsIn, "testODBCSynchroFactory-Valid.xml"),
                               new ClassicConnectionParameters());
      
      // With an invalid XML file with an empty root element
      Assert.Throws<Lemoine.DataRepository.SchemaException>
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
      Assert.That (f, Is.Not.Null);
      
      // With an invalid XML string with no root element
      Assert.Throws<XmlException>
        (delegate { new ODBCFactory (XmlSourceType.STRING,
                                        "no root element",
                                        new ClassicConnectionParameters()); },
         "string with no root element");
      
      // With an invalid XML string with no connection parameters
      Assert.Throws<Lemoine.DataRepository.SchemaException>
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
        $@"<?xml version='1.0'?>
<root xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
      xsi:noNamespaceSchemaLocation='factodbcconfig.xsd'
      xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:odbc'
      pulse:dsnname='LemoineUnitTests' pulse:user='{Constants.DEFAULT_DATABASE_USER}' pulse:password='{Constants.DEFAULT_DATABASE_PASSWORD}'>
</root>";
      ODBCFactory f = new ODBCFactory (XmlSourceType.STRING,
                                       xmlData, new ClassicConnectionParameters());
      Assert.That ($"DSN=LemoineUnitTests;UID={Constants.DEFAULT_DATABASE_USER};Pwd={Constants.DEFAULT_DATABASE_PASSWORD};", Is.EqualTo (f.ConnectionParameters.OdbcConnectionString()));
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
    private void TestGetData ()
    {
      // Initialization
      string xmlData =
$$"""
<?xml version='1.0'?>
<root xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
      xsi:noNamespaceSchemaLocation='factodbcconfig.xsd'
      xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:odbc'
      pulse:dsnname='LemoineUnitTests' pulse:user='{{Constants.DEFAULT_DATABASE_USER}}' pulse:password='{{Constants.DEFAULT_DATABASE_PASSWORD}}'>
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
</root>
""";
      ODBCFactory f = new ODBCFactory (XmlSourceType.STRING, xmlData, new ClassicConnectionParameters());
      XmlDocument doc = f.GetData (cancellationToken: System.Threading.CancellationToken.None);

      // Tests
      Assert.That (doc, Is.Not.Null);
      XmlElement root = doc.DocumentElement;
      Assert.That (root, Is.Not.Null);
      Assert.Multiple (() => {
        Assert.That (root.Name, Is.EqualTo ("root"));

        // job[1]
        Assert.That (root.GetElementsByTagName ("job"), Has.Count.EqualTo (2));
      });
      XmlElement job1 = root.GetElementsByTagName ("job") [0] as XmlElement;
      Assert.That (job1, Is.Not.Null);
      Assert.Multiple (() => {
        Assert.That (job1.GetAttribute ("name"), Is.EqualTo ("JOBNAME"));
        Assert.That (job1.GetAttribute ("type"), Is.EqualTo ("1"));
        Assert.That (job1.GetAttribute ("status"), Is.EqualTo (""));
        Assert.That (job1.GetAttribute ("request", PulseResolver.PULSE_ODBC_NAMESPACE), Is.EqualTo (""));
        Assert.That (job1.GetAttribute ("attr"), Is.EqualTo ("anyotherattr"));

        // job[1]/component
        Assert.That (job1.GetElementsByTagName ("component"), Has.Count.EqualTo (2));
      });
      // job[1]/component[1]
      XmlElement component = job1.GetElementsByTagName ("component") [0] as XmlElement;
      Assert.That (component, Is.Not.Null);
      Assert.Multiple (() => {
        Assert.That (component.GetAttribute ("name"), Is.EqualTo ("COMPNAME"));
        Assert.That (component.GetAttribute ("starttime"), Is.EqualTo ("2007-07-01 00:00:00"));
        Assert.That (component.GetAttribute ("endtime"), Is.EqualTo ("2007-07-02 00:00:00"));
        Assert.That (component.GetAttribute ("hours"), Is.EqualTo ("5.1"));
        Assert.That (component.GetAttribute ("done"), Is.EqualTo ("False")); // In the ODBC settings, Bools as char must be off
        Assert.That (component.GetAttribute ("id"), Is.EqualTo ("7"));
        Assert.That (component.GetAttribute ("componenttype"), Is.EqualTo ("1"));

        Assert.That (job1.GetElementsByTagName ("testif"), Has.Count.EqualTo (1));
      });

      XmlElement job2 = root.GetElementsByTagName ("job") [1] as XmlElement;
      Assert.That (job2.GetElementsByTagName ("testif"), Is.Empty);
    }

    /// <summary>
    /// Test GetData method
    /// </summary>
#if ENABLE_ODBCFACTORY_UNITTEST_VISUALSTUDIO
    [Test]
#endif // ENABLE_ODBCFACTORY_UNITTEST_VISUALSTUDIO
    private void TestGetData2 ()
    {
      // Initialization
      string xmlData =
$$"""
<?xml version='1.0'?>
<root xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
      xmlns:gdb=""urn:pulse.lemoinetechnologies.com:synchro:gdb""
      xmlns:odbc='urn:pulse.lemoinetechnologies.com:synchro:odbc'
      odbc:dsnname='LemoineUnitTests' odbc:user='{{Constants.DEFAULT_DATABASE_USER}}' odbc:password='{{Constants.DEFAULT_DATABASE_PASSWORD}}'>
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
</root>
""";
      ODBCFactory f = new ODBCFactory (XmlSourceType.STRING, xmlData, new ClassicConnectionParameters());
      XmlDocument doc = f.GetData (cancellationToken: System.Threading.CancellationToken.None);

      // Tests
      Assert.That (doc, Is.Not.Null);
      XmlElement root = doc.DocumentElement;
      Assert.That (root, Is.Not.Null);
      Assert.Multiple (() => {
        Assert.That (root.Name, Is.EqualTo ("root"));

        // Machine[1]
        Assert.That (root.GetElementsByTagName ("Machine"), Has.Count.EqualTo (1));
      });
      XmlElement machine1 = root.GetElementsByTagName ("Machine") [0] as XmlElement;
      Assert.That (machine1, Is.Not.Null);
      Assert.That (machine1.GetAttribute ("Name"), Is.EqualTo ("MACHINE_A17"));
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
      Assert.That (result, Is.EqualTo ("confvalue"));
      result = f.GetConfigurationValue ("@unknown");
      Assert.That (result, Is.Null);
    }

    /// <summary>
    /// Test FlagSynchronizationAsSuccess method
    /// </summary>
#if ENABLE_ODBCFACTORY_UNITTEST_VISUALSTUDIO
    [Test]
#endif // ENABLE_ODBCFACTORY_UNITTEST_VISUALSTUDIO
    private void TestFlagSynchronizationAsSuccess ()
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
      pulse:dsnname='LemoineUnitTests' pulse:user='LemoineUser' pulse:password='LemoinePass'>
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
      Assert.That (doc, Is.Not.Null);
      XmlElement root = doc.DocumentElement;
      Assert.That (root, Is.Not.Null);
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
          Assert.Multiple (() => {
            Assert.That (reader.FieldCount, Is.EqualTo (1));
            Assert.That (reader.IsDBNull (0), Is.False, "status is DBNull");
            Assert.That ((int)reader.GetValue (0), Is.GreaterThanOrEqualTo (1));
          });
        }
        reader.Close ();
        Assert.That (nbRows, Is.EqualTo (2));
        
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
        Assert.That (applicationState, Is.Not.Null);
        Assert.That (applicationState.Value, Is.EqualTo ("3"));
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
