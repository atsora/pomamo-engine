// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Xml.Serialization;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using System.Linq;
using System.CodeDom.Compiler;
using System.Xml;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the event classes
  /// </summary>
  [TestFixture]
  public class Event_UnitTest
  {
    private string m_previousDSNName;

    static readonly ILog log = LogManager.GetLogger (typeof (Config_UnitTest).FullName);

    /// <summary>
    /// Test the class eventmessage
    /// </summary>
    [Test]
    public void TestEventMessage ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var level = ModelDAOHelper.DAOFactory.EventLevelDAO.FindById (2);
          var eventTest = ModelDAOHelper.ModelFactory.CreateEventMessage (level, "Test");
          ModelDAOHelper.DAOFactory.EventMessageDAO.MakePersistent (eventTest);
          ModelDAOHelper.DAOFactory.Flush ();
          int id = eventTest.Id;
          {
            var e = ModelDAOHelper.DAOFactory.EventMessageDAO.FindById (id);
            Assert.IsNotNull (e);
          }
          {
            var e = ModelDAOHelper.DAOFactory.EventDAO.FindById (id);
            Assert.AreEqual (id, e.Id);
            Assert.IsInstanceOf<IEventMessage> (e);
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the class eventmachinemessage
    /// </summary>
    [Test]
    public void TestEventMachineMessage ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var level = ModelDAOHelper.DAOFactory.EventLevelDAO.FindById (2);
          var machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (2);
          var eventTest = ModelDAOHelper.ModelFactory.CreateEventMachineMessage (level, machine, "Test");
          ModelDAOHelper.DAOFactory.EventMachineMessageDAO.MakePersistent (eventTest);
          ModelDAOHelper.DAOFactory.Flush ();
          int id = eventTest.Id;
          {
            var e = ModelDAOHelper.DAOFactory.EventMachineMessageDAO.FindById (id, machine);
            Assert.IsNotNull (e);
          }
          {
            var e = ModelDAOHelper.DAOFactory.EventDAO.FindById (id);
            Assert.AreEqual (id, e.Id);
            Assert.IsInstanceOf<IEventMachineMessage> (e);
          }
          {
            var events = ModelDAOHelper.DAOFactory.EventDAO.FindAll ();
            var e = events.First (x => x.Id == id);
            Assert.AreEqual (id, e.Id);
            Assert.IsInstanceOf<IEventMachineMessage> (e);
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the class eventtoollife
    /// </summary>
    [Test]
    public void TestEventToolLife ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var level = ModelDAOHelper.DAOFactory.EventLevelDAO.FindById (2);
          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          var machineModule = machine.MainMachineModule;
          var eventTest = ModelDAOHelper.ModelFactory.CreateEventToolLife (level, EventToolLifeType.CurrentLifeReset, DateTime.UtcNow, machineModule);
          eventTest.ToolNumber = "T1";
          eventTest.ToolId = "T1";
          eventTest.OldToolState = Core.SharedData.ToolState.Available;
          eventTest.NewToolState = Core.SharedData.ToolState.Available;
          eventTest.Direction = Core.SharedData.ToolLifeDirection.Unknown;
          ModelDAOHelper.DAOFactory.EventToolLifeDAO.MakePersistent (eventTest);
          ModelDAOHelper.DAOFactory.Flush ();
          int id = eventTest.Id;
          {
            var e = ModelDAOHelper.DAOFactory.EventToolLifeDAO.FindById (id, machineModule);
            Assert.IsNotNull (e);
          }
          {
            var e = ModelDAOHelper.DAOFactory.EventDAO.FindById (id);
            Assert.AreEqual (id, e.Id);
            Assert.IsInstanceOf<IEventToolLife> (e);
          }
          {
            var events = ModelDAOHelper.DAOFactory.EventDAO.FindAll ();
            var e = events.First (x => x.Id == id);
            Assert.AreEqual (id, e.Id);
            Assert.IsInstanceOf<IEventToolLife> (e);
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the XML serialization of the EventMachineMessage class
    /// </summary>
    [Test]
    public void TestEventMachineMessageSerialization ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IEventLevel eventLevel = ModelDAO.ModelDAOHelper.DAOFactory
          .EventLevelDAO.FindById (1);
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory
          .MonitoredMachineDAO.FindByIdForXmlSerialization (1);
        var ev = ModelDAOHelper.ModelFactory.CreateEventMachineMessage (eventLevel, machine, "Test");
        {
          var extraTypes = new Type[] { typeof (EventMachineMessage) };
          var serializer = new XmlSerializer (typeof (Event), extraTypes);
          using (var stringWriter = new StringWriter ()) {
            using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
              stringWriter.NewLine = "\n";
              serializer.Serialize (stringWriter, ev);
              var xml = stringWriter.ToString ();
              Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<Event xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xsi:type="EventMachineMessage" DateTime="2022-06-24 09:52:23" LocalDateTimeString="24/06/2022 11:52:23" LocalDateTimeG="24/06/2022 11:52:23">
  <Level Name="" TranslationKey="EventLevelAlert" Display="Alert" Priority="100" />
  <Machine xsi:type="MonitoredMachine" Name="MACHINE_A17" Id="1">
    <MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" />
  </Machine>
</Event>
""".ReplaceLineEndings ().Length, xml.ReplaceLineEndings ().Length);
            }
          }
        }
      }
    }

    [OneTimeSetUp]
    public void Init ()
    {
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }
    }
  }
}
