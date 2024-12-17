// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Extensions.AutoReason;
using System.Linq;
using Lemoine.Database.Persistent;
using Pulse.Extensions.Database;
using Pulse.Extensions;
using Lemoine.Extensions;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Extensions.Plugin;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  public class AutoReasonCoffee : Pulse.Extensions.Database.IReasonExtension
  {
    public bool UniqueInstance
    {
      get { return true; }
    }

    public void EndBatch ()
    {
    }

    public double? GetMaximumScore (IReasonSlot newReasonSlot)
    {
      return 100;
    }

    public RequiredResetKind GetRequiredResetKind (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange)
    {
      return RequiredResetKind.None;
    }

    public bool Initialize (IMachine machine)
    {
      return true;
    }

    public bool IsCompatible (UtcDateTimeRange range, IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, double reasonScore, ReasonSource reasonSource)
    {
      return true;
    }

    public bool IsResetApplicable (ReasonSource reasonSource, double reasonScore, int autoReasonNumber)
    {
      if (reasonSource.HasFlag (ReasonSource.UnsafeAutoReasonNumber)) {
        return true;
      }
      if (1 < autoReasonNumber) {
        return true;
      }
      else if (1 == autoReasonNumber) {
        return !reasonSource.HasFlag (ReasonSource.DefaultIsAuto);
      }
      else {
        return false;
      }
    }

    public bool MayApplyAutoReasons (IReasonSlot newReasonSlot)
    {
      return true;
    }

    public bool MayApplyManualReasons (IReasonSlot newReasonSlot)
    {
      return false;
    }

    public void PreLoad (UtcDateTimeRange range)
    {
    }

    public void StartBatch ()
    {
    }

    public IEnumerable<IPossibleReason> TryGetActiveAt (DateTime at, IMachineMode machineMode, IMachineObservationState machineObservationState, bool autoManualOnly)
    {
      return new List<IPossibleReason> { };
    }

    public void TryResetReason (ref IReasonSlot reasonSlot)
    {
    }
  }

  /// <summary>
  /// Unit tests for the class ReasonMachineAssociation
  /// </summary>
  [TestFixture]
  public class ReasonMachineAssociation_UnitTest : Lemoine.UnitTests.WithHourTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReasonMachineAssociation_UnitTest).FullName);

    public ReasonMachineAssociation_UnitTest ()
      : base (UtcDateTime.From (2018, 10, 01, 08, 00, 00))
    {
    }

    void ExecuteRequest (string request)
    {
      var connection = ModelDAOHelper.DAOFactory.GetConnection ();
      using (var command = connection.CreateCommand ()) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
        command.CommandText = request;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
        command.ExecuteNonQuery ();
      }
    }

    /// <summary>
    /// Test FindById
    /// </summary>
    [Test]
    public void TestFindById1 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        const string request1 = @"INSERT INTO machinemodification (machinemodificationmachineid, revisionid, parentglobalmodificationid, parentmachinemodificationid, modificationpriority, modificationdatetime, modificationauto, modificationreferencedtable, modificationid)
 VALUES (1, NULL, NULL, NULL, 100, '2016-01-27 15:10:02', FALSE, 'ReasonMachineAssociation', 10829);
";
        ExecuteRequest (request1);
        const string request2 = @"INSERT INTO ReasonMachineAssociation (machineid, reasonid, reasonmachineassociationbegin, reasonmachineassociationend, reasondetails, reasonmachineassociationoption, reasonmachineassociationreasonscore, reasonmachineassociationkind, modificationid)
 VALUES (1, 21, '2011-08-01 00:00:00', '2011-08-02 00:00:00', NULL, NULL, 100.0, 4, 10829);
";
        ExecuteRequest (request2);

        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (1);

        var reasonMachineAssociation = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
          .FindById (10829, machine);
        Assert.That (reasonMachineAssociation, Is.Not.Null);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the method MakeAnalysis
    /// </summary>
    [Test]
    public void TestMakeAnalysis ()
    {
      try {
        Lemoine.Info.ConfigSet
          .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

        Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();

        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          try {
            // Production state
            Lemoine.Extensions.Package.PackageFile
              .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""ReasonMachineAssociation_TestMakeAnalysis_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""RunningMachineModeIsProduction"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""Score"": 10.0,
  ""ExcludeManual"": false,
  ""UnknownIsNotRunning"": false
          }
        }
      ]
    }
  ]
}
", true, true);
            var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
            var pluginFilter = new PluginFilterFromFlag (PluginFlag.Analysis);
            var pluginsLoader = new PluginsLoader (assemblyLoader);
            var extensionsProvider = new ExtensionsProvider (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory, pluginFilter, Lemoine.Extensions.Analysis.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, new DummyPluginsLoader ());
            Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);
            Lemoine.Extensions.ExtensionManager.Activate (false);
            Lemoine.Extensions.ExtensionManager.Load ();

            ISession session = NHibernateHelper.GetCurrentSession ();
            // Reference data
            IUser user1 = daoFactory.UserDAO.FindById (1);
            IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
            IMachineObservationState attended =
              daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);

            IMachineObservationState unattended =
              daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Unattended);
            IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
            IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
            IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
            IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
            IReason reasonSetup = daoFactory.ReasonDAO.FindById (16);
            IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
            IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
            IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);

            // Existing ReasonSlot
            {
              IReasonSlot existingSlot =
                new ReasonSlot (machine1,
                                new UtcDateTimeRange (UtcDateTime.From (2011, 08, 01, 12, 00, 00),
                                                      UtcDateTime.From (2011, 08, 01, 12, 10, 00)));
              existingSlot.MachineMode = inactive;
              existingSlot.MachineObservationState = attended;
              ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnanswered, 10.0, true, false);
              daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
            }
            {
              IReasonSlot existingSlot =
                new ReasonSlot (machine1,
                                new UtcDateTimeRange (UtcDateTime.From (2011, 08, 01, 12, 10, 00),
                                                      UtcDateTime.From (2011, 08, 01, 12, 20, 00)));
              existingSlot.MachineMode = active;
              existingSlot.MachineObservationState = attended;
              ((ReasonSlot)existingSlot).SetDefaultReason (reasonMotion, 10.0, true, true);
              daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
            }
            {
              IReasonSlot existingSlot =
                new ReasonSlot (machine1,
                                new UtcDateTimeRange (UtcDateTime.From (2011, 08, 01, 12, 20, 00),
                                                      UtcDateTime.From (2011, 08, 01, 12, 30, 00)));
              existingSlot.MachineMode = inactive;
              existingSlot.MachineObservationState = attended;
              ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnanswered, 10.0, true, false);
              daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
            }
            // Existing MachineActivitySummary
            {
              IMachineActivitySummary summary;
              summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                                  UtcDateTime.From (2011, 08, 01),
                                                                                  attended, inactive);
              summary.Time = TimeSpan.FromMinutes (20);
              daoFactory.MachineActivitySummaryDAO.MakePersistent (summary);
              summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                                  UtcDateTime.From (2011, 08, 01),
                                                                                  attended, active);
              summary.Time = TimeSpan.FromMinutes (10);
              daoFactory.MachineActivitySummaryDAO.MakePersistent (summary);
            }
            // Existing ReasonSummary
            {
              IReasonSummary summary;
              summary = new ReasonSummary (machine1,
                                            UtcDateTime.From (2011, 08, 01), null,
                                            attended, reasonUnanswered);
              summary.Time = TimeSpan.FromMinutes (20);
              summary.Number = 2;
              daoFactory.ReasonSummaryDAO.MakePersistent (summary);
              summary = new ReasonSummary (machine1,
                                            UtcDateTime.From (2011, 08, 01), null,
                                            attended, reasonMotion);
              summary.Time = TimeSpan.FromMinutes (10);
              summary.Number = 1;
              daoFactory.ReasonSummaryDAO.MakePersistent (summary);
            }
            // MachineStatus
            {
              IMachineStatus machineStatus =
                new MachineStatus (machine1);
              machineStatus.CncMachineMode = inactive;
              machineStatus.MachineMode = inactive;
              machineStatus.MachineObservationState = attended;
              machineStatus.ManualActivity = false;
              machineStatus.Reason = reasonUnanswered;
              machineStatus.ReasonSlotEnd =
                UtcDateTime.From (2011, 08, 01, 12, 30, 00);
              daoFactory.MachineStatusDAO.MakePersistent (machineStatus);
            }

            // New association 4 -> oo
            {
              var association =
                new ReasonMachineAssociation ();
              association.XmlSerializationMachine = (Machine)machine1;
              association.SetManualReason (reasonSetup, (double?)null);
              association.DateTime = UtcDateTime.From (2011, 08, 01, 12, 15, 00);
              association.Begin = UtcDateTime.From (2011, 08, 01, 12, 00, 00);
              association.End = new UpperBound<DateTime> (null);
              association = (ReasonMachineAssociation)new ReasonMachineAssociationDAO ().MakePersistent (association);
              association.MakeAnalysis (); // InProgress
              association.MakeAnalysis ();
            }

            DAOFactory.EmptyAccumulators ();
            ModelDAOHelper.DAOFactory.FlushData ();

            var productionStates = ModelDAOHelper.DAOFactory.ProductionStateDAO.FindAll ();
            var productionStateProduction = productionStates.Single (p => p.TranslationKey.Equals ("ProductionStateProduction"));
            var productionStateNoProduction = productionStates.Single (p => p.TranslationKey.Equals ("ProductionStateNoProduction"));

            // Check the values
            {
              IList<ReasonSlot> slots =
                session.CreateCriteria<ReasonSlot> ()
                .Add (Expression.Eq ("Machine", machine1))
                .AddOrder (Order.Asc ("DateTimeRange"))
                .List<ReasonSlot> ();
              Assert.That (slots, Has.Count.EqualTo (3), "Number of reason slots");
              int i = 0;
              Assert.Multiple (() => {
                Assert.That (slots[i].Machine, Is.EqualTo (machine1));
                Assert.That (slots[i].Reason, Is.EqualTo (reasonSetup));
                Assert.That (slots[i].DefaultReason, Is.EqualTo (false));
                Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
                Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
                Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01, 12, 00, 00)));
                Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01, 12, 10, 00)));
                Assert.That (slots[i].ProductionState, Is.EqualTo (productionStateNoProduction));
              });
              ++i;
              Assert.Multiple (() => {
                Assert.That (slots[i].Machine, Is.EqualTo (machine1));
                Assert.That (slots[i].Reason, Is.EqualTo (reasonMotion));
                Assert.That (slots[i].DefaultReason, Is.EqualTo (true));
                Assert.That (slots[i].MachineMode, Is.EqualTo (active));
                Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
                Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01, 12, 10, 00)));
                Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01, 12, 20, 00)));
                Assert.That (slots[i].ProductionState, Is.EqualTo (productionStateProduction));
              });
              ++i;
              Assert.Multiple (() => {
                Assert.That (slots[i].Machine, Is.EqualTo (machine1));
                Assert.That (slots[i].Reason, Is.EqualTo (reasonUnanswered));
                Assert.That (slots[i].DefaultReason, Is.EqualTo (true));
                Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
                Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
                Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01, 12, 20, 00)));
                Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01, 12, 30, 00)));
              });
            }
            // - AnalysisLogs
            AnalysisUnitTests.CheckNumberOfAnalysisLogs (session, 1); // Warning on reason slot motion
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Test the method MakeAnalysis
    /// </summary>
    [Test]
    public void TestBug605Story162544502 ()
    {
      try {
        Lemoine.Info.ConfigSet
          .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

        Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();

        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          // Reference data
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);

          IMachineObservationState unattended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Unattended);
          IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
          IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
          IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
          IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
          IReason reasonSetup = daoFactory.ReasonDAO.FindById (16);
          IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
          IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
          IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);

          // Existing ReasonSlot
          {
            IReasonSlot existingSlot =
              new ReasonSlot (machine1, R (0, 1));
            existingSlot.MachineMode = active;
            existingSlot.MachineObservationState = attended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonMotion, 10.0, true, false);
            daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
          }
          {
            IReasonSlot existingSlot =
              new ReasonSlot (machine1, R (1, 3));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = attended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnanswered, 10.0, true, false);
            daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
          }
          // Existing MachineActivitySummary
          {
            IMachineActivitySummary summary;
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
              T (0).Date,
                                                                       attended, active);
            summary.Time = TimeSpan.FromHours (1);
            daoFactory.MachineActivitySummaryDAO.MakePersistent (summary);
          }
          {
            IMachineActivitySummary summary;
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
              T (0).Date,
                                                                       attended, inactive);
            summary.Time = TimeSpan.FromHours (2);
            daoFactory.MachineActivitySummaryDAO.MakePersistent (summary);
          }
          // Existing ReasonSummary
          {
            IReasonSummary summary;
            summary = new ReasonSummary (machine1,
              T (0).Date,
                                          null,
                                          attended, reasonMotion);
            summary.Time = TimeSpan.FromHours (1);
            summary.Number = 1;
            daoFactory.ReasonSummaryDAO.MakePersistent (summary);
          }
          {
            IReasonSummary summary;
            summary = new ReasonSummary (machine1,
              T (0).Date,
                                          null,
                                          attended, reasonUnanswered);
            summary.Time = TimeSpan.FromHours (2);
            summary.Number = 1;
            daoFactory.ReasonSummaryDAO.MakePersistent (summary);
          }
          // MachineStatus
          {
            IMachineStatus machineStatus =
              new MachineStatus (machine1);
            machineStatus.CncMachineMode = inactive;
            machineStatus.MachineMode = inactive;
            machineStatus.MachineObservationState = attended;
            machineStatus.ManualActivity = false;
            machineStatus.Reason = reasonUnanswered;
            machineStatus.ReasonSlotEnd = T (3);
            daoFactory.MachineStatusDAO.MakePersistent (machineStatus);
          }

          // New association 3 -> oo
          {
            var association =
              new ReasonMachineAssociation ();
            association.XmlSerializationMachine = (Machine)machine1;
            association.SetManualReason (reasonSetup, (double?)null);
            association.DateTime = T (3);
            association.Begin = T (3);
            association.End = new UpperBound<DateTime> (null);
            association = (ReasonMachineAssociation)new ReasonMachineAssociationDAO ().MakePersistent (association);
            association.MakeAnalysis (); // InProgress
            association.MakeAnalysis ();
          }

          DAOFactory.EmptyAccumulators ();

          // Check the values
          {
            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (2), "Number of reason slots");
            int i = 1;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (3)));
            });
          }

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Test the method MakeAnalysis
    /// </summary>
    [Test]
    public void TestResetManualStory162759044 ()
    {
      try {
        Lemoine.Info.ConfigSet
          .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

        Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();

        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          // Reference data
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);

          IMachineObservationState unattended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Unattended);
          IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
          IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
          IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
          IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
          IReason reasonSetup = daoFactory.ReasonDAO.FindById (16);
          IReason reasonTv = daoFactory.ReasonDAO.FindById (30);
          IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
          IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
          IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);

          // Existing ReasonSlot
          {
            IReasonSlot existingSlot =
              new ReasonSlot (machine1, R (0, 1));
            existingSlot.MachineMode = active;
            existingSlot.MachineObservationState = attended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonMotion, 10.0, true, false);
            daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
          }
          {
            IReasonSlot existingSlot =
              new ReasonSlot (machine1, R (1, 3));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = attended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnanswered, 10.0, true, false);
            daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
          }
          // Existing MachineActivitySummary
          {
            IMachineActivitySummary summary;
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
              T (0).Date,
                                                                       attended, active);
            summary.Time = TimeSpan.FromHours (1);
            daoFactory.MachineActivitySummaryDAO.MakePersistent (summary);
          }
          {
            IMachineActivitySummary summary;
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
              T (0).Date,
                                                                       attended, inactive);
            summary.Time = TimeSpan.FromHours (2);
            daoFactory.MachineActivitySummaryDAO.MakePersistent (summary);
          }
          // Existing ReasonSummary
          {
            IReasonSummary summary;
            summary = new ReasonSummary (machine1,
              T (0).Date,
                                          null,
                                          attended, reasonMotion);
            summary.Time = TimeSpan.FromHours (1);
            summary.Number = 1;
            daoFactory.ReasonSummaryDAO.MakePersistent (summary);
          }
          {
            IReasonSummary summary;
            summary = new ReasonSummary (machine1,
              T (0).Date,
                                          null,
                                          attended, reasonUnanswered);
            summary.Time = TimeSpan.FromHours (2);
            summary.Number = 1;
            daoFactory.ReasonSummaryDAO.MakePersistent (summary);
          }
          // MachineStatus
          {
            IMachineStatus machineStatus =
              new MachineStatus (machine1);
            machineStatus.CncMachineMode = inactive;
            machineStatus.MachineMode = inactive;
            machineStatus.MachineObservationState = attended;
            machineStatus.ManualActivity = false;
            machineStatus.Reason = reasonUnanswered;
            machineStatus.ReasonSlotEnd = T (3);
            daoFactory.MachineStatusDAO.MakePersistent (machineStatus);
          }

          // New association 1 -> 3
          {
            var association =
              new ReasonMachineAssociation ();
            association.XmlSerializationMachine = (Machine)machine1;
            association.SetManualReason (reasonSetup, (double?)null);
            association.DateTime = T (3);
            association.Begin = T (1);
            association.End = T (3);
            association = (ReasonMachineAssociation)new ReasonMachineAssociationDAO ().MakePersistent (association);
            association.MakeAnalysis (); // InProgress
            association.MakeAnalysis ();
          }
          // New association 1 -> 3 (reset)
          {
            var association =
              new ReasonMachineAssociation ();
            association.XmlSerializationMachine = (Machine)machine1;
            association.ResetManualReason ();
            association.DateTime = T (3);
            association.Begin = T (1);
            association.End = T (3);
            association = (ReasonMachineAssociation)new ReasonMachineAssociationDAO ().MakePersistent (association);
            association.MakeAnalysis ();
          }

          ModelDAOHelper.DAOFactory.FlushData (); // To get an ID on the second modification
          AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
          DAOFactory.EmptyAccumulators ();

          // Check the values
          {
            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (2), "Number of reason slots");
            int i = 1;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (3)));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonUnanswered));
            });
          }

          // New association 1 -> 3
          {
            var association =
              new ReasonMachineAssociation ();
            association.XmlSerializationMachine = (Machine)machine1;
            association.SetManualReason (reasonTv, (double?)null);
            association.DateTime = T (3);
            association.Begin = T (1);
            association.End = T (3);
            association = (ReasonMachineAssociation)new ReasonMachineAssociationDAO ().MakePersistent (association);
            association.MakeAnalysis ();
          }
          // New association 1 -> 3 (reset)
          {
            var association =
              new ReasonMachineAssociation ();
            association.XmlSerializationMachine = (Machine)machine1;
            association.ResetManualReason ();
            association.DateTime = T (3);
            association.Begin = T (1);
            association.End = T (3);
            association = (ReasonMachineAssociation)new ReasonMachineAssociationDAO ().MakePersistent (association);
            association.MakeAnalysis ();
          }

          DAOFactory.EmptyAccumulators ();

          // Check the values
          {
            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (2), "Number of reason slots");
            int i = 1;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (3)));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonUnanswered));
            });
          }

          // New association 1 -> 3
          {
            var association =
              new ReasonMachineAssociation ();
            association.XmlSerializationMachine = (Machine)machine1;
            association.SetManualReason (reasonTv, (double?)null);
            association.DateTime = T (3);
            association.Begin = T (1);
            association.End = T (3);
            association = (ReasonMachineAssociation)new ReasonMachineAssociationDAO ().MakePersistent (association);
            association.MakeAnalysis ();
          }
          // New association 1 -> 2 (reset)
          {
            var association =
              new ReasonMachineAssociation ();
            association.XmlSerializationMachine = (Machine)machine1;
            association.ResetManualReason ();
            association.DateTime = T (3);
            association.Begin = T (1);
            association.End = T (2);
            association = (ReasonMachineAssociation)new ReasonMachineAssociationDAO ().MakePersistent (association);
            association.MakeAnalysis ();
          }
          // New association 2 -> 3 (reset)
          {
            var association =
              new ReasonMachineAssociation ();
            association.XmlSerializationMachine = (Machine)machine1;
            association.ResetManualReason ();
            association.DateTime = T (3);
            association.Begin = T (2);
            association.End = T (3);
            association = (ReasonMachineAssociation)new ReasonMachineAssociationDAO ().MakePersistent (association);
            association.MakeAnalysis ();
          }

          DAOFactory.EmptyAccumulators ();

          // Check the values
          {
            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (2), "Number of reason slots");
            int i = 1;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (3)));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonUnanswered));
            });
          }

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    class DynamicEndExtension
      : Lemoine.UnitTests.WithHourTimeStamp
      , Lemoine.Extensions.Business.DynamicTimes.IDynamicTimeExtension
    {
      static int s_step = 0;

      public DynamicEndExtension ()
        : base (UtcDateTime.From (2018, 10, 01, 08, 00, 00))
      {
      }

      public bool Initialize (IMachine machine, string parameter)
      {
        this.Machine = machine;
        return true;
      }

      public IMachine Machine
      {
        get; set;
      }

      public string Name
      {
        get {
          return "Test";
        }
      }

      public bool UniqueInstance
      {
        get {
          return true;
        }
      }

      public bool IsApplicable ()
      {
        return true;
      }

      public DynamicTimeApplicableStatus IsApplicableAt (DateTime at)
      {
        throw new NotImplementedException ();
      }

      public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
      {
        var step = s_step++;
        switch (step) {
          case 0:
            return this.CreatePending ();
          case 1:
          case 2:
            return this.CreateWithHint (R (2));
          default:
            return this.CreateFinal (T (3));
        }
      }

      public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
      {
        return TimeSpan.FromTicks (0);
      }

      public static void Reset ()
      {
        s_step = 0;
      }
    }

    /// <summary>
    /// Test the dynamic end
    /// </summary>
    [Test]
    public void TestDynamicEndProgressive ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
      Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (UnitTests.AutoReasonCoffee));

      try {
        Lemoine.Info.ConfigSet
          .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          // Reference data
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);

          IMachineObservationState unattended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Unattended);
          IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
          IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
          IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
          IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
          IReason reasonCoffee = daoFactory.ReasonDAO.FindById (28);
          IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
          IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
          IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);

          // Existing ReasonSlot
          {
            IReasonSlot existingSlot =
              new ReasonSlot (machine1, R (1, 3));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = attended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnanswered, 10.0, true, false);
            daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
          }
          // Existing MachineActivitySummary
          {
            IMachineActivitySummary summary;
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
              T (0).Date,
                                                                       attended, inactive);
            summary.Time = TimeSpan.FromHours (2);
            daoFactory.MachineActivitySummaryDAO.MakePersistent (summary);
          }
          // Existing ReasonSummary
          {
            IReasonSummary summary;
            summary = new ReasonSummary (machine1,
              T (0).Date,
                                          null,
                                          attended, reasonUnanswered);
            summary.Time = TimeSpan.FromHours (2);
            summary.Number = 1;
            daoFactory.ReasonSummaryDAO.MakePersistent (summary);
          }
          // MachineStatus
          {
            IMachineStatus machineStatus =
              new MachineStatus (machine1);
            machineStatus.CncMachineMode = inactive;
            machineStatus.MachineMode = inactive;
            machineStatus.MachineObservationState = attended;
            machineStatus.ManualActivity = false;
            machineStatus.Reason = reasonUnanswered;
            machineStatus.ReasonSlotEnd = T (3);
            daoFactory.MachineStatusDAO.MakePersistent (machineStatus);
          }

          // New association 1 -> Test
          {
            ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
              .InsertAutoReason (machine1, R (1), reasonCoffee, 100.0, "", ",Test", false, AssociationOption.ProgressiveStrategy);
          }

          { // First run: nothing is done, pending
            AnalysisUnitTests.RunFirst ();
            DAOFactory.EmptyAccumulators ();

            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (1), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (3)));
            });
          }
          { // 2nd run: until T(2)
            AnalysisUnitTests.RunFirst ();
            // Note: RunProcessingReasonSlotsAnalysis is not required any more
            // since I improved some plugins
            DAOFactory.EmptyAccumulators ();

            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (2), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (2)));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (2)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (3)));
            });
          }
          { // 3rd run: until T(3)
            AnalysisUnitTests.RunMakeAnalysis ();
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
            DAOFactory.EmptyAccumulators ();

            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (1), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (3)));
            });
          }

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        DynamicEndExtension.Reset ();
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Test the dynamic end (with the + option)
    /// </summary>
    [Test]
    public void TestDynamicEndProgressivePlusOption ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
      Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (UnitTests.AutoReasonCoffee));

      try {
        Lemoine.Info.ConfigSet
          .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          // Reference data
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);

          IMachineObservationState unattended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Unattended);
          IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
          IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
          IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
          IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
          IReason reasonCoffee = daoFactory.ReasonDAO.FindById (28);
          IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
          IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
          IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);

          // Existing ReasonSlot
          {
            IReasonSlot existingSlot =
              new ReasonSlot (machine1, R (1, 3));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = attended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnanswered, 10.0, true, false);
            daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
          }
          // Existing MachineActivitySummary
          {
            IMachineActivitySummary summary;
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
              T (0).Date,
                                                                       attended, inactive);
            summary.Time = TimeSpan.FromHours (2);
            daoFactory.MachineActivitySummaryDAO.MakePersistent (summary);
          }
          // Existing ReasonSummary
          {
            IReasonSummary summary;
            summary = new ReasonSummary (machine1,
              T (0).Date,
                                          null,
                                          attended, reasonUnanswered);
            summary.Time = TimeSpan.FromHours (2);
            summary.Number = 1;
            daoFactory.ReasonSummaryDAO.MakePersistent (summary);
          }
          // MachineStatus
          {
            IMachineStatus machineStatus =
              new MachineStatus (machine1);
            machineStatus.CncMachineMode = inactive;
            machineStatus.MachineMode = inactive;
            machineStatus.MachineObservationState = attended;
            machineStatus.ManualActivity = false;
            machineStatus.Reason = reasonUnanswered;
            machineStatus.ReasonSlotEnd = T (3);
            daoFactory.MachineStatusDAO.MakePersistent (machineStatus);
          }

          // New association 1 -> Test
          {
            ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
              .InsertAutoReason (machine1, R (1, 2), reasonCoffee, 100.0, "", ",Test+", false, AssociationOption.ProgressiveStrategy);
          }

          { // First run: nothing is done, pending
            AnalysisUnitTests.RunFirst ();
            DAOFactory.EmptyAccumulators ();

            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (1), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (3)));
            });
          }
          { // 2nd run: until T(2)
            AnalysisUnitTests.RunFirst ();
            // Note: RunProcessingReasonSlotsAnalysis is not required any more
            // since I improved some plugins
            DAOFactory.EmptyAccumulators ();

            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (2), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (2)));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (2)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (3)));
            });
          }
          { // 3rd run: until T(3)
            AnalysisUnitTests.RunMakeAnalysis ();
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
            DAOFactory.EmptyAccumulators ();

            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (1), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (3)));
            });
          }

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        DynamicEndExtension.Reset ();
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Test the dynamic end (aggressive mode)
    /// </summary>
    [Test]
    public void TestDynamicEndAggressive ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
      Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (UnitTests.AutoReasonCoffee));

      try {
        Lemoine.Info.ConfigSet
          .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          // Reference data
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);

          IMachineObservationState unattended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Unattended);
          IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
          IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
          IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
          IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
          IReason reasonCoffee = daoFactory.ReasonDAO.FindById (28);
          IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
          IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
          IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);

          // Existing ReasonSlot
          {
            IReasonSlot existingSlot =
              new ReasonSlot (machine1, R (1, 5));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = attended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnanswered, 10.0, true, false);
            daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
          }
          // Existing MachineActivitySummary
          {
            IMachineActivitySummary summary;
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
              T (0).Date,
                                                                       attended, inactive);
            summary.Time = TimeSpan.FromHours (4);
            daoFactory.MachineActivitySummaryDAO.MakePersistent (summary);
          }
          // Existing ReasonSummary
          {
            IReasonSummary summary;
            summary = new ReasonSummary (machine1,
              T (0).Date,
                                          null,
                                          attended, reasonUnanswered);
            summary.Time = TimeSpan.FromHours (4);
            summary.Number = 1;
            daoFactory.ReasonSummaryDAO.MakePersistent (summary);
          }
          // MachineStatus
          {
            IMachineStatus machineStatus =
              new MachineStatus (machine1);
            machineStatus.CncMachineMode = inactive;
            machineStatus.MachineMode = inactive;
            machineStatus.MachineObservationState = attended;
            machineStatus.ManualActivity = false;
            machineStatus.Reason = reasonUnanswered;
            machineStatus.ReasonSlotEnd = T (5);
            daoFactory.MachineStatusDAO.MakePersistent (machineStatus);
          }

          // New association 1 -> Test
          {
            ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
              .InsertAutoReason (machine1, R (1, 5), reasonCoffee, 100.0, "", ",Test", false, null);
          }

          { // First run: until T(5)
            AnalysisUnitTests.RunFirst ();
            DAOFactory.EmptyAccumulators ();
            ModelDAOHelper.DAOFactory.FlushData ();

            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (1), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (5)));
              Assert.That (slots[i].AutoReasonNumber, Is.GreaterThan (0));
            });
          }
          { // 2rd run: until T(3)
            AnalysisUnitTests.RunMakeAnalysis ();
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
            DAOFactory.EmptyAccumulators ();

            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (2), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (3)));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (3)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (5)));
            });
          }

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        DynamicEndExtension.Reset ();
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Test the dynamic end (aggressive mode) when the range is empty
    /// </summary>
    [Test]
    public void TestDynamicEndAggressiveEmptyRange ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
      Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtension));

      try {
        Lemoine.Info.ConfigSet
          .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          // Reference data
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);

          IMachineObservationState unattended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Unattended);
          IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
          IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
          IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
          IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
          IReason reasonCoffee = daoFactory.ReasonDAO.FindById (28);
          IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
          IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
          IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);

          // Existing ReasonSlot
          {
            IReasonSlot existingSlot =
              new ReasonSlot (machine1, R (1, 5));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = attended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnanswered, 10.0, true, false);
            daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
          }
          // Existing MachineActivitySummary
          {
            IMachineActivitySummary summary;
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
              T (0).Date,
                                                                       attended, inactive);
            summary.Time = TimeSpan.FromHours (4);
            daoFactory.MachineActivitySummaryDAO.MakePersistent (summary);
          }
          // Existing ReasonSummary
          {
            IReasonSummary summary;
            summary = new ReasonSummary (machine1,
              T (0).Date,
                                          null,
                                          attended, reasonUnanswered);
            summary.Time = TimeSpan.FromHours (4);
            summary.Number = 1;
            daoFactory.ReasonSummaryDAO.MakePersistent (summary);
          }
          // MachineStatus
          {
            IMachineStatus machineStatus =
              new MachineStatus (machine1);
            machineStatus.CncMachineMode = inactive;
            machineStatus.MachineMode = inactive;
            machineStatus.MachineObservationState = attended;
            machineStatus.ManualActivity = false;
            machineStatus.Reason = reasonUnanswered;
            machineStatus.ReasonSlotEnd = T (5);
            daoFactory.MachineStatusDAO.MakePersistent (machineStatus);
          }

          // New association 1 -> Test
          {
            ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
              .InsertAutoReason (machine1, R (1), reasonCoffee, 100.0, "", ",Test", false, null);
          }

          { // First run: until T(5)
            AnalysisUnitTests.RunFirst ();
            DAOFactory.EmptyAccumulators ();

            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (1), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (5)));
            });
          }

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        DynamicEndExtension.Reset ();
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Test the dynamic end (aggressive mode), dynamic end with + option
    /// </summary>
    [Test]
    public void TestDynamicEndAggressivePlusOption ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
      Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (UnitTests.AutoReasonCoffee));

      try {
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          // Reference data
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);

          IMachineObservationState unattended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Unattended);
          IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
          IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
          IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
          IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
          IReason reasonCoffee = daoFactory.ReasonDAO.FindById (28);
          IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
          IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
          IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);

          Lemoine.Info.ConfigSet.ForceValue<TimeSpan> ("ReasonSlotDAO.FindProcessing.LowerLimit",
            TimeSpan.FromDays (10 * 365));

          // Existing ReasonSlot
          {
            IReasonSlot existingSlot =
              new ReasonSlot (machine1, R (1, 5));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = attended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnanswered, 10.0, true, false);
            daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
          }
          // Existing MachineActivitySummary
          {
            IMachineActivitySummary summary;
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
              T (0).Date,
                                                                       attended, inactive);
            summary.Time = TimeSpan.FromHours (4);
            daoFactory.MachineActivitySummaryDAO.MakePersistent (summary);
          }
          // Existing ReasonSummary
          {
            IReasonSummary summary;
            summary = new ReasonSummary (machine1,
              T (0).Date,
                                          null,
                                          attended, reasonUnanswered);
            summary.Time = TimeSpan.FromHours (4);
            summary.Number = 1;
            daoFactory.ReasonSummaryDAO.MakePersistent (summary);
          }
          // MachineStatus
          {
            IMachineStatus machineStatus =
              new MachineStatus (machine1);
            machineStatus.CncMachineMode = inactive;
            machineStatus.MachineMode = inactive;
            machineStatus.MachineObservationState = attended;
            machineStatus.ManualActivity = false;
            machineStatus.Reason = reasonUnanswered;
            machineStatus.ReasonSlotEnd = T (5);
            daoFactory.MachineStatusDAO.MakePersistent (machineStatus);
          }

          // New association 1 -> Test
          {
            ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
              .InsertAutoReason (machine1, R (1, 2), reasonCoffee, 100.0, "", ",Test+", false, null);
          }

          { // First run: until +oo
            AnalysisUnitTests.RunFirst ();
            DAOFactory.EmptyAccumulators ();

            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (1), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (5)));
            });
          }
          { // 2rd run: until T(3)
            AnalysisUnitTests.RunMakeAnalysis ();
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
            DAOFactory.EmptyAccumulators ();

            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (2), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (3)));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (3)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (5)));
            });
          }

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        DynamicEndExtension.Reset ();
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    class AutoReasonCoffee : IAutoReasonExtension
    {
      public double? ManualScore
      {
        get {
          return 200;
        }
      }

      public bool UniqueInstance
      {
        get {
          return true;
        }
      }

      public IMonitoredMachine Machine => throw new NotImplementedException ();

      public IReason Reason => throw new NotImplementedException ();

      public double ReasonScore => throw new NotImplementedException ();

      public bool CanOverride (IReasonSlot reasonSlot)
      {
        return true;
      }

      public bool Initialize (IMonitoredMachine machine, Lemoine.Threading.IChecked caller)
      {
        return true;
      }

      public bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
      {
        return true;
      }

      public bool IsValidMatch (IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, double score)
      {
        return reason.Id == 28;
      }

      public void RunOnce ()
      {
        throw new NotImplementedException ();
      }
    }

    class AutoReasonTelevision : IAutoReasonExtension
    {
      public double? ManualScore
      {
        get {
          return 200;
        }
      }

      public bool UniqueInstance
      {
        get {
          return true;
        }
      }

      public IMonitoredMachine Machine => throw new NotImplementedException ();

      public IReason Reason => throw new NotImplementedException ();

      public double ReasonScore => throw new NotImplementedException ();

      public bool CanOverride (IReasonSlot reasonSlot)
      {
        return true;
      }

      public bool Initialize (IMonitoredMachine machine, Lemoine.Threading.IChecked caller)
      {
        return true;
      }

      public bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
      {
        return true;
      }

      public bool IsValidMatch (IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, double score)
      {
        return reason.Id == 30;
      }

      public void RunOnce ()
      {
        throw new NotImplementedException ();
      }
    }

    /// <summary>
    /// Test the dynamic end
    /// </summary>
    [Test]
    public void TestDifferentAutoReasonsSamePeriod ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
      Lemoine.Extensions.ExtensionManager.Add (typeof (AutoReasonCoffee));
      Lemoine.Extensions.ExtensionManager.Add (typeof (AutoReasonTelevision));

      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          ISession session = NHibernateHelper.GetCurrentSession ();
          // Reference data
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);

          IMachineObservationState unattended =
            daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Unattended);
          IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
          IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
          IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
          IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
          IReason reasonCoffee = daoFactory.ReasonDAO.FindById (28);
          IReason reasonTelevision = daoFactory.ReasonDAO.FindById (30);
          IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
          IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
          IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);

          // Existing ReasonSlot
          {
            IReasonSlot existingSlot =
              new ReasonSlot (machine1, R (1, 5));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = attended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnanswered, 10.0, true, false);
            ((ReasonSlot)existingSlot).Consolidate (null, null);
            daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
          }
          // Existing MachineActivitySummary
          {
            IMachineActivitySummary summary;
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
              T (0).Date,
                                                                       attended, inactive);
            summary.Time = TimeSpan.FromHours (4);
            daoFactory.MachineActivitySummaryDAO.MakePersistent (summary);
          }
          // Existing ReasonSummary
          {
            IReasonSummary summary;
            summary = new ReasonSummary (machine1,
              T (0).Date,
                                          null,
                                          attended, reasonUnanswered);
            summary.Time = TimeSpan.FromHours (4);
            summary.Number = 1;
            daoFactory.ReasonSummaryDAO.MakePersistent (summary);
          }
          // MachineStatus
          {
            IMachineStatus machineStatus =
              new MachineStatus (machine1);
            machineStatus.CncMachineMode = inactive;
            machineStatus.MachineMode = inactive;
            machineStatus.MachineObservationState = attended;
            machineStatus.ManualActivity = false;
            machineStatus.Reason = reasonUnanswered;
            machineStatus.ReasonSlotEnd = T (5);
            daoFactory.MachineStatusDAO.MakePersistent (machineStatus);
          }
          ModelDAOHelper.DAOFactory.FlushData ();

          // New association 1 -> 3: Coffee
          {
            ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
              .InsertAutoReason (machine1, R (1, 3), reasonCoffee, 110.0, "", "", false, null);
          }

          {
            ModelDAOHelper.DAOFactory.FlushData ();
            AnalysisUnitTests.RunMakeAnalysis ();
            DAOFactory.EmptyAccumulators ();
            ModelDAOHelper.DAOFactory.FlushData ();
          }

          {
            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (2), "Number of reason slots");
          }

          // New association 2 -> 4: Television
          {
            ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
              .InsertAutoReason (machine1, R (2, 4), reasonTelevision, 120, "", null, false, null);
          }

          {
            AnalysisUnitTests.RunMakeAnalysis ();
            DAOFactory.EmptyAccumulators ();
            ModelDAOHelper.DAOFactory.FlushData ();
          }

          {
            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (3), "Number of reason slots");
          }

          {
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None);
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None);
          }

          {
            IList<ReasonSlot> slots =
              session.CreateCriteria<ReasonSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ReasonSlot> ();
            Assert.That (slots, Has.Count.EqualTo (3), "Number of reason slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonCoffee));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (2)));
              Assert.That (slots[i].AutoReasonNumber, Is.EqualTo (1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonTelevision));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (2)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (4)));
              Assert.That (slots[i].ReasonSource.HasFlag (ReasonSource.UnsafeAutoReasonNumber), Is.True);
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (slots[i].Machine, Is.EqualTo (machine1));
              Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
              Assert.That (slots[i].MachineObservationState, Is.EqualTo (attended));
              Assert.That (slots[i].Reason, Is.EqualTo (reasonUnanswered));
              Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (4)));
              Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (5)));
              Assert.That (slots[i].AutoReasonNumber, Is.EqualTo (0));
            });
            ++i;
          }
        }
        finally {
          transaction.Rollback ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
        }
      }
    }

    class DynamicStartExtension
        : Lemoine.UnitTests.WithHourTimeStamp
        , Lemoine.Extensions.Business.DynamicTimes.IDynamicTimeExtension
    {
      static int s_step = 0;

      public DynamicStartExtension ()
        : base (UtcDateTime.From (2018, 10, 01, 08, 00, 00))
      {
      }

      public bool Initialize (IMachine machine, string parameter)
      {
        this.Machine = machine;
        return true;
      }

      public IMachine Machine
      {
        get; set;
      }

      public string Name
      {
        get {
          return "Test";
        }
      }

      public bool UniqueInstance
      {
        get {
          return true;
        }
      }

      public bool IsApplicable ()
      {
        return true;
      }

      public DynamicTimeApplicableStatus IsApplicableAt (DateTime at)
      {
        throw new NotImplementedException ();
      }

      public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
      {
        var step = s_step++;
        switch (step) {
          case 0:
            return this.CreatePending ();
          case 1:
            return this.CreateWithHint (R (1));
          default:
            return this.CreateFinal (T (1));
        }
      }

      public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
      {
        return TimeSpan.FromTicks (0);
      }

      public static void Reset ()
      {
        s_step = 0;
      }
    }

    // Note: disconnect the dynamic start for the moment
    /*
        /// <summary>
        /// Test the dynamic start
        /// </summary>
        [Test]
        public void TestDynamicStart ()
        {
          Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
          Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicStartExtension));

          try {
            IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
            using (IDAOSession daoSession = daoFactory.OpenSession ())
            using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
              ISession session = NHibernateHelper.GetCurrentSession ();
              // Reference data
              IUser user1 = daoFactory.UserDAO.FindById (1);
              IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
              IMachineObservationState attended =
                daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);

              IMachineObservationState unattended =
                daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Unattended);
              IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
              IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
              IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
              IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
              IReason reasonCoffee = daoFactory.ReasonDAO.FindById (28);
              IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
              IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
              IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);

              // Existing ReasonSlot
              {
                IReasonSlot existingSlot =
                  new ReasonSlot (machine1, R (1, 3));
                existingSlot.MachineMode = inactive;
                existingSlot.MachineObservationState = attended;
                ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnanswered, 10.0, true, false);
                daoFactory.ReasonSlotDAO.MakePersistent (existingSlot);
              }
              // Existing MachineActivitySummary
              {
                IMachineActivitySummary summary;
                summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                  T (0).Date,
                                                                           attended, inactive);
                summary.Time = TimeSpan.FromHours (2);
                daoFactory.MachineActivitySummaryDAO.MakePersistent (summary);
              }
              // Existing ReasonSummary
              {
                IReasonSummary summary;
                summary = new ReasonSummary (machine1,
                  T (0).Date,
                                              null,
                                              attended, reasonUnanswered);
                summary.Time = TimeSpan.FromHours (2);
                summary.Number = 1;
                daoFactory.ReasonSummaryDAO.MakePersistent (summary);
              }
              // MachineStatus
              {
                IMachineStatus machineStatus =
                  new MachineStatus (machine1);
                machineStatus.CncMachineMode = inactive;
                machineStatus.MachineMode = inactive;
                machineStatus.MachineObservationState = attended;
                machineStatus.ManualActivity = false;
                machineStatus.Reason = reasonUnanswered;
                machineStatus.ReasonSlotEnd = T (3);
                daoFactory.MachineStatusDAO.MakePersistent (machineStatus);
              }

              // New association 4 -> Test
              ReasonMachineAssociation association;
              {
                association =
                  new ReasonMachineAssociation ();
                association.XmlSerializationMachine = (Machine)machine1;
                association.SetManualReason (reasonCoffee);
                association.DateTime = T (2);
                association.Begin = T (2);
                association.End = T(3);
                association.Dynamic = "Test,";
                association = (ReasonMachineAssociation)daoFactory.ReasonMachineAssociationDAO.MakePersistent (association);
              }

              { // First run: nothing is done, pending
                AnalysisUnitTests.RunFirst ();
                DAOFactory.EmptyAccumulators ();

                IList<ReasonSlot> slots =
                  session.CreateCriteria<ReasonSlot> ()
                  .Add (Expression.Eq ("Machine", machine1))
                  .AddOrder (Order.Asc ("DateTimeRange"))
                  .List<ReasonSlot> ();
                Assert.AreEqual (1, slots.Count, "Number of reason slots");
                int i = 0;
                Assert.AreEqual (machine1, slots[i].Machine);
                Assert.AreEqual (inactive, slots[i].MachineMode);
                Assert.AreEqual (attended, slots[i].MachineObservationState);
                Assert.AreEqual (reasonUnanswered, slots[i].Reason);
                Assert.AreEqual (T (1), slots[i].BeginDateTime.Value);
                Assert.AreEqual (T (3), slots[i].EndDateTime.Value);
              }
              { // 2nd run: pending
                AnalysisUnitTests.RunFirst ();
                DAOFactory.EmptyAccumulators ();

                IList<ReasonSlot> slots =
                  session.CreateCriteria<ReasonSlot> ()
                  .Add (Expression.Eq ("Machine", machine1))
                  .AddOrder (Order.Asc ("DateTimeRange"))
                  .List<ReasonSlot> ();
                Assert.AreEqual (1, slots.Count, "Number of reason slots");
                int i = 0;
                Assert.AreEqual (machine1, slots[i].Machine);
                Assert.AreEqual (inactive, slots[i].MachineMode);
                Assert.AreEqual (attended, slots[i].MachineObservationState);
                Assert.AreEqual (reasonUnanswered, slots[i].Reason);
                Assert.AreEqual (T (1), slots[i].BeginDateTime.Value);
                Assert.AreEqual (T (3), slots[i].EndDateTime.Value);
              }
              { // 3rd run: until T(1)-T(3)
                AnalysisUnitTests.RunMakeAnalysis ();
                DAOFactory.EmptyAccumulators ();

                IList<ReasonSlot> slots =
                  session.CreateCriteria<ReasonSlot> ()
                  .Add (Expression.Eq ("Machine", machine1))
                  .AddOrder (Order.Asc ("DateTimeRange"))
                  .List<ReasonSlot> ();
                Assert.AreEqual (1, slots.Count, "Number of reason slots");
                int i = 0;
                Assert.AreEqual (machine1, slots[i].Machine);
                Assert.AreEqual (inactive, slots[i].MachineMode);
                Assert.AreEqual (attended, slots[i].MachineObservationState);
                Assert.AreEqual (reasonCoffee, slots[i].Reason);
                Assert.AreEqual (T (1), slots[i].BeginDateTime.Value);
                Assert.AreEqual (T (3), slots[i].EndDateTime.Value);
              }

              transaction.Rollback ();
            }
          }
          finally {
            Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
            DynamicEndExtension.Reset ();
          }
        }
        */
  }
}
