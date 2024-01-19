// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Alert.GDBListeners;
using Lemoine.Alert.TestActions;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using System.Threading;
using Lemoine.Extensions.Alert;

namespace Lemoine.Alert.UnitTests
{
  /// <summary>
  /// Unit tests for the class AlertEngine
  /// </summary>
  [TestFixture]
  public class AlertEngine_UnitTest
  {
    private string m_previousDSNName;

    static readonly ILog log = LogManager.GetLogger(typeof (AlertEngine_UnitTest).FullName);

    /// <summary>
    /// Test AlertEngine with the GDBListener.Event
    /// </summary>
    [Test]
    public void TestGDBListenerEvent()
    {
      IList<string> result = new List<string> ();
      ICollection<IListener> listeners = new List<IListener> ();
      listeners.Add (new EventListener ());
      ICollection<TriggeredAction> triggeredActions = new List<TriggeredAction> ();
      TriggeredAction triggeredAction = new TriggeredAction ();
      triggeredAction.Trigger = new XPathTrigger ("/Event");
      triggeredAction.Actions.Add (new CollectionAction (result));
      triggeredActions.Add (triggeredAction);
      AlertEngine engine = new AlertEngine (listeners, triggeredActions);
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        engine.RunOnePass (CancellationToken.None);
        Assert.That (result, Has.Count.EqualTo (1));
        IEventLevel level = ModelDAOHelper.DAOFactory.EventLevelDAO.FindById (3); // WARN, 400
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindByIdForXmlSerialization (1);
        IMachineMode machineMode = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (1);
        IMachineObservationState machineObservationState = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById (1);
        IEventLongPeriod ev = ModelDAOHelper.ModelFactory
          .CreateEventLongPeriod (level,
                                  new DateTime (2013, 01, 01),
                                  machine,
                                  machineMode,
                                  machineObservationState,
                                  TimeSpan.FromMinutes (10));
        ModelDAOHelper.DAOFactory.EventLongPeriodDAO.MakePersistent (ev);
        ModelDAOHelper.DAOFactory.Flush ();
        engine.RunOnePass (CancellationToken.None);
        Assert.That (result, Has.Count.EqualTo (2));
        // Note: result[0] is a EventCncValue that is already in database
        var expected = "<Event xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                       "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xsi:type=\"EventLongPeriod\" " +
                       "DateTime=\"2013-01-01 00:00:00\" LocalDateTimeG=\"01/01/2013 01:00:00\" LocalDateTimeString=\"01/01/2013 01:00:00\" TriggerDuration=\"00:10:00\">" +
                       "<Level Name=\"\" TranslationKey=\"EventLevelWarn\" Display=\"Warning\" Priority=\"400\" />" +
                       "<MonitoredMachine Id=\"1\" Name=\"MACHINE_A17\"><MonitoringType TranslationKey=\"MonitoringTypeMonitored\" Display=\"Monitored\" Id=\"2\" />" +
                       "</MonitoredMachine>" +
                       "<MachineMode TranslationKey=\"MachineModeInactive\" Display=\"Inactive\" Running=\"false\" AutoSequence=\"false\" />" +
                       "<MachineObservationState TranslationKey=\"MachineObservationStateAttended\" Display=\"Machine ON with operator (attended)\" UserRequired=\"true\" ShiftRequired=\"false\" OnSite=\"true\" IsProduction=\"true\" />" +
                       "</Event>";
        Assert.That (result [1], Has.Length.EqualTo (expected.Length));
        IEventLongPeriod ev1 = ModelDAOHelper.ModelFactory
          .CreateEventLongPeriod (level,
                                  new DateTime (2013, 01, 02),
                                  machine,
                                  machineMode,
                                  machineObservationState,
                                  TimeSpan.FromMinutes (20));
        ModelDAOHelper.DAOFactory.EventLongPeriodDAO.MakePersistent (ev1);
        IEventLongPeriod ev2 = ModelDAOHelper.ModelFactory
          .CreateEventLongPeriod (level,
                                  new DateTime (2013, 01, 03),
                                  machine,
                                  machineMode,
                                  machineObservationState,
                                  TimeSpan.FromMinutes (30));
        ModelDAOHelper.DAOFactory.EventLongPeriodDAO.MakePersistent (ev2);
        ModelDAOHelper.DAOFactory.Flush ();
        engine.RunOnePass (CancellationToken.None);
        Assert.That (result, Has.Count.EqualTo (4));
        transaction.Rollback ();
      }
      
    }
    
    /// <summary>
    /// Test AlertEngine with the GDBListener.Log
    /// </summary>
    [Test]
    public void TestGDBListenerLog()
    {
      IList<string> result = new List<string> ();
      ICollection<IListener> listeners = new List<IListener> ();
      listeners.Add (new LogListener ());
      ICollection<TriggeredAction> triggeredActions = new List<TriggeredAction> ();
      TriggeredAction triggeredAction = new TriggeredAction ();
      triggeredAction.Trigger = new XPathTrigger ("/Log[./@xsi:type=\"DetectionAnalysisLog\"]");
      triggeredAction.Actions.Add (new CollectionAction (result));
      triggeredActions.Add (triggeredAction);
      AlertEngine engine = new AlertEngine (listeners, triggeredActions);
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        engine.RunOnePass (CancellationToken.None);
        Assert.That (result, Is.Empty);
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindByIdForXmlSerialization (1);
        IDetectionAnalysisLog l = ModelDAOHelper.ModelFactory.CreateDetectionAnalysisLog (LogLevel.ERROR, "Test", machine);
        l.DateTime = new DateTime (2013, 3, 3);
        ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO.MakePersistent (l);
        ModelDAOHelper.DAOFactory.Flush ();
        engine.RunOnePass (CancellationToken.None);
        Assert.That (result, Has.Count.EqualTo (1));
        Assert.That (result [0], Has.Length.EqualTo (("<Log xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                         "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                         "xsi:type=\"DetectionAnalysisLog\" " +
                         "DateTime=\"2013-03-03T00:00:00\" " +
                         "Level=\"ERROR\" Message=\"Test\">" +
                         "<Machine xsi:type=\"MonitoredMachine\" Id=\"1\" Name=\"MACHINE_A17\"><MonitoringType TranslationKey=\"MonitoringTypeMonitored\" Display=\"Monitored\" Id=\"2\" /></Machine>" +
                         "</Log>").Length));
        IDetectionAnalysisLog l1 = ModelDAOHelper.ModelFactory.CreateDetectionAnalysisLog (LogLevel.ERROR, "Test1", machine);
        ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO.MakePersistent (l1);
        IDetectionAnalysisLog l2 = ModelDAOHelper.ModelFactory.CreateDetectionAnalysisLog (LogLevel.ERROR, "Test2", machine);
        ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO.MakePersistent (l2);
        engine.RunOnePass (CancellationToken.None);
        Assert.That (result, Has.Count.EqualTo (3));
        transaction.Rollback ();
      }
      
    }

    [OneTimeSetUp]
    public void Init()
    {
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new Lemoine.GDBPersistentClasses.GDBPersistentClassFactory ();
      
      {
        var multiCatalog = new MultiCatalog ();
        multiCatalog.Add (new StorageCatalog (new Lemoine.ModelDAO.TranslationDAOCatalog (),
                                              new TextFileCatalog ("AlertServiceI18N",
                                                                   Lemoine.Info.PulseInfo.LocalConfigurationDirectory)));
        //multiCatalog.Add (new DefaultTextFileCatalog ());
        PulseCatalog.Implementation = new CachedCatalog (multiCatalog);
      }
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
