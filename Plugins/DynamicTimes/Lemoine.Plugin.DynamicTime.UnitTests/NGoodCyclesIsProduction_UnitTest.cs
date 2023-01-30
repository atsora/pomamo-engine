// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Extensions.Database;
using Pulse.Extensions.Database;
using Pulse.Extensions;
using Lemoine.Extensions;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Extensions.Plugin;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  /// <summary>
  /// Description of SetupSwitcher_UnitTest.
  /// </summary>
  public class NGoodCyclesIsProduction_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    class CycleDetectionStatusExtension
      : ICycleDetectionStatusExtension
      , IOperationDetectionStatusExtension
    {
      public bool UniqueInstance
      {
        get {
          return true;
        }
      }

      public int CycleDetectionStatusPriority
      {
        get { return 1; }
      }

      public int OperationDetectionStatusPriority
      {
        get { return 1; }
      }

      public DateTime? GetCycleDetectionDateTime ()
      {
        return new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc)
          .AddMinutes (100);
      }

      public DateTime? GetOperationDetectionDateTime ()
      {
        return new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc)
          .AddMinutes (100);
      }

      public bool Initialize (IMachine machine)
      {
        return true;
      }
    }

    readonly ILog log = LogManager.GetLogger (typeof (LastFullCycleIsLastProductionEnd_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NGoodCyclesIsProduction_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test1Cycle ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        session.ForceUniqueSession ();
        using (IDAOTransaction transaction = session.BeginTransaction ()) {
          try {
            Lemoine.Info.ConfigSet.ForceValue ("NGoodCyclesIsProduction.LastProductionEnd.ApplicableTimeSpan", TimeSpan.FromDays (10 * 365));
            Lemoine.Info.ConfigSet.ForceValue ("NGoodCyclesIsProduction.NextProductionEnd.ApplicableTimeSpan", TimeSpan.FromDays (10 * 365));
            Lemoine.Info.ConfigSet.ForceValue ("NGoodCyclesIsProduction.NextProductionStart.ApplicableTimeSpan", TimeSpan.FromDays (10 * 365));

            IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
              .FindById (2);
            IOperation operation = ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (1); // MachiningDuration: 3600s=60min, no loading duration
            operation.LoadingDuration = TimeSpan.FromMinutes (10);
            ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);

            // Existing operation slots and machine state templates
            {
              var association = ModelDAOHelper.ModelFactory
                .CreateOperationMachineAssociation (machine, R (-10, null));
              association.Operation = operation;
              association.Apply ();
            }
            ModelDAOHelper.DAOFactory.Flush ();
            IOperationSlot operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindOverlapsRange (machine, R (0, null))
              .First ();

            var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
            var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
            var pluginsLoader = new PluginsLoader (assemblyLoader);
            var nhibernatePluginsLoader = new DummyPluginsLoader ();
            var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
            Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);

            Lemoine.Extensions.ExtensionManager.Add (typeof (CycleDetectionStatusExtension));

            Lemoine.Extensions.Package.PackageFile
              .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""NGoodCyclesIsProduction_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""NGoodCyclesIsProduction"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""NumberOfGoodCycles"": 1,
  ""MaxMachiningDurationMultiplicator"": 1.0,
  ""MaxLoadingDurationMultiplicator"": 1.0
          }
        }
      ]
    }
  ]
}
", true, true);
            Lemoine.Extensions.ExtensionManager.Activate (false);
            Lemoine.Extensions.ExtensionManager.Load ();

            CheckNotApplicable ("LastProductionEnd", machine, T (80));
            CheckNotApplicable ("NextProductionStart", machine, T (0));
            CheckNotApplicable ("NextProductionEnd", machine, T (80));

            CreateCycle (machine, null, R (-200, -100));
            CheckNoData ("LastProductionEnd", machine, T (-300));
            CheckAfter ("NextProductionStart", machine, T (0), T (100));
            CheckAfter ("NextProductionEnd", machine, T (80), T (100));

            CreateCycle (machine, operationSlot, R (10, 20));
            CheckFinal ("LastProductionEnd", machine, T (80), T (20));
            CheckFinal ("NextProductionStart", machine, T (0), T (10));
            CheckNoData ("NextProductionEnd", machine, T (-10));
            CheckAfter ("NextProductionEnd", machine, T (0), T (100));

            CheckPending ("LastProductionEnd", machine, T (180));
          }
          finally {
            Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
            Lemoine.Info.ConfigSet.ResetForceValues ();
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test2GoodCycles ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        session.ForceUniqueSession ();
        using (var transaction = session.BeginTransaction ()) {
          try {
            Lemoine.Info.ConfigSet.ForceValue ("NGoodCyclesIsProduction.LastProductionEnd.ApplicableTimeSpan", TimeSpan.FromDays (10 * 365));
            Lemoine.Info.ConfigSet.ForceValue ("NGoodCyclesIsProduction.NextProductionEnd.ApplicableTimeSpan", TimeSpan.FromDays (10 * 365));
            Lemoine.Info.ConfigSet.ForceValue ("NGoodCyclesIsProduction.NextProductionStart.ApplicableTimeSpan", TimeSpan.FromDays (10 * 365));

            IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
              .FindById (2);
            IOperation operation = ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (1);
            operation.MachiningDuration = TimeSpan.FromMinutes (12);
            operation.LoadingDuration = TimeSpan.FromMinutes (10);
            ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);

            // Existing operation slots and machine state templates
            {
              var association = ModelDAOHelper.ModelFactory
                .CreateOperationMachineAssociation (machine, R (0, null));
              association.Operation = operation;
              association.Apply ();
            }
            ModelDAOHelper.DAOFactory.Flush ();
            IOperationSlot operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindOverlapsRange (machine, R (0, null))
              .First ();

            var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
            var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
            var pluginsLoader = new PluginsLoader (assemblyLoader);
            var nhibernatePluginsLoader = new DummyPluginsLoader ();
            var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
            Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);

            Lemoine.Extensions.ExtensionManager.Add (typeof (CycleDetectionStatusExtension));

            Lemoine.Extensions.Package.PackageFile
              .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""NGoodCyclesIsProduction_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""NGoodCyclesIsProduction"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""NumberOfGoodCycles"": 2,
  ""MaxMachiningDurationMultiplicator"": 1.0,
  ""MaxLoadingDurationMultiplicator"": 1.0
          }
        }
      ]
    }
  ]
}
", true, true);
            Lemoine.Extensions.ExtensionManager.Activate (false);
            Lemoine.Extensions.ExtensionManager.Load ();

            CheckNotApplicable ("LastProductionEnd", machine, T (80));
            CheckNotApplicable ("NextProductionStart", machine, T (0));
            CheckNotApplicable ("NextProductionEnd", machine, T (80));

            CreateCycle (machine, null, R (-200, -100));
            CheckNoData ("LastProductionEnd", machine, T (-300));
            CheckAfter ("NextProductionStart", machine, T (0), T (100));
            CheckAfter ("NextProductionEnd", machine, T (0), T (100));

            CreateCycle (machine, operationSlot, R (10, 20));
            CheckNoData ("LastProductionEnd", machine, T (80));
            CheckAfter ("NextProductionStart", machine, T (0), T (10));
            CheckAfter ("NextProductionEnd", machine, T (0), T (100));

            CreateCycle (machine, operationSlot, R (21, 31));
            CheckFinal ("LastProductionEnd", machine, T (80), T (31));
            CheckFinal ("NextProductionStart", machine, T (0), T (10));
            CheckAfter ("NextProductionEnd", machine, T (0), T (100));

            CreateCycle (machine, operationSlot, R (50, 70));
            CheckFinal ("LastProductionEnd", machine, T (80), T (31));
            CheckFinal ("NextProductionStart", machine, T (0), T (10));
            CheckAfter ("NextProductionStart", machine, T (21), T (70));
            CheckFinal ("NextProductionEnd", machine, T (0), T (31));
            CheckFinal ("NextProductionEnd", machine, T (21), T (31));

            CreateCycle (machine, operationSlot, R (71, 72));
            CheckFinal ("LastProductionEnd", machine, T (80), T (31));
            CheckNoData ("LastProductionEnd", machine, T (80), R (32));
            CheckFinal ("LastProductionEnd", machine, T (80), T (31), R (30));
            CheckAfter ("NextProductionStart", machine, T (32), T (71));
            CheckFinal ("NextProductionEnd", machine, T (21), T (31));
            CheckNoData ("NextProductionEnd", machine, T (21), R (21, 30));
            CheckFinal ("NextProductionEnd", machine, T (21), T (31), R (21, 31, "[]"));
            CheckFinal ("NextProductionEnd", machine, T (21), T (31), R (21, 32));

            CheckPending ("LastProductionEnd", machine, T (180));
          }
          finally {
            Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
            Lemoine.Info.ConfigSet.ResetForceValues ();
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestOperationChange ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue ("NGoodCyclesIsProduction.LastProductionEnd.ApplicableTimeSpan", TimeSpan.FromDays (10 * 365));
          Lemoine.Info.ConfigSet.ForceValue ("NGoodCyclesIsProduction.NextProductionEnd.ApplicableTimeSpan", TimeSpan.FromDays (10 * 365));
          Lemoine.Info.ConfigSet.ForceValue ("NGoodCyclesIsProduction.NextProductionStart.ApplicableTimeSpan", TimeSpan.FromDays (10 * 365));

          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          IOperation operation = ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1);
          IOperation operation2 = ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (2);
          operation.MachiningDuration = TimeSpan.FromMinutes (12);
          operation.LoadingDuration = TimeSpan.FromMinutes (10);
          ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);

          // Existing operation slots and machine state templates
          {
            var association = ModelDAOHelper.ModelFactory
              .CreateOperationMachineAssociation (machine, R (0, 20));
            association.Operation = operation;
            association.Apply ();
          }
          {
            var association = ModelDAOHelper.ModelFactory
              .CreateOperationMachineAssociation (machine, R (20, null));
            association.Operation = operation2;
            association.Apply ();
          }
          ModelDAOHelper.DAOFactory.Flush ();
          var operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindOverlapsRange (machine, R (0, null));
          var operationSlot = operationSlots.First ();
          var operationSlot2 = operationSlots.Last ();

          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new DummyPluginsLoader ();
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);

          Lemoine.Extensions.ExtensionManager.Add (typeof (CycleDetectionStatusExtension));

          Lemoine.Extensions.Package.PackageFile
            .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""NGoodCyclesIsProduction_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""NGoodCyclesIsProduction"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""NumberOfGoodCycles"": 2,
  ""MaxMachiningDurationMultiplicator"": 1.0,
  ""MaxLoadingDurationMultiplicator"": 1.0
          }
        }
      ]
    }
  ]
}
", true, true);
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          CheckNotApplicable ("LastProductionEnd", machine, T (80));
          CheckNotApplicable ("NextProductionStart", machine, T (0));
          CheckNotApplicable ("NextProductionEnd", machine, T (80));

          CreateCycle (machine, null, R (-200, -100));
          CheckNoData ("LastProductionEnd", machine, T (-300));
          CheckAfter ("NextProductionStart", machine, T (0), T (100));
          CheckAfter ("NextProductionEnd", machine, T (0), T (100));

          CreateCycle (machine, operationSlot, R (10, 20));
          CheckNoData ("LastProductionEnd", machine, T (80));
          CheckAfter ("NextProductionStart", machine, T (0), T (10));
          CheckAfter ("NextProductionEnd", machine, T (0), T (100));
          CheckAfter ("NextProductionEnd", machine, T (10), T (100));

          CreateCycle (machine, operationSlot2, R (21, 31));
          CheckNoData ("LastProductionEnd", machine, T (80));
          CheckAfter ("NextProductionStart", machine, T (0), T (21));
          CheckNoData ("NextProductionEnd", machine, T (0));

          CreateCycle (machine, operationSlot2, R (32, 42));
          CheckFinal ("LastProductionEnd", machine, T (80), T (42));
          CheckFinal ("NextProductionStart", machine, T (21), T (21));
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    void CreateCycle (IMachine machine, IOperationSlot operationSlot, UtcDateTimeRange range)
    {
      IOperationCycle operationCycle1 = ModelDAOHelper.ModelFactory
        .CreateOperationCycle (machine);
      operationCycle1.OperationSlot = operationSlot;
      operationCycle1.SetRealBegin (range.Lower.Value);
      operationCycle1.SetRealEnd (range.Upper.Value);
      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle1);
    }

    void CheckPending (string name, IMachine machine, DateTime dateTime)
    {
      var response = Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime (name, machine, dateTime);
      Assert.IsFalse (response.Hint.Lower.HasValue);
      Assert.IsFalse (response.Hint.Upper.HasValue);
      Assert.IsFalse (response.Final.HasValue);
    }

    void CheckAfter (string name, IMachine machine, DateTime dateTime, DateTime after)
    {
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime);
        Assert.IsTrue (response.Hint.Lower.HasValue);
        Assert.AreEqual (after, response.Hint.Lower.Value);
        Assert.IsFalse (response.Final.HasValue);
      }
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime, new UtcDateTimeRange (after), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (!response.Hint.Lower.HasValue || response.Hint.Lower.Equals (after)
          || response.Hint.Lower.Equals (T (100))); // T (100) is the cycle detection time
        Assert.IsFalse (response.Final.HasValue);
      }
    }

    void CheckFinal (string name, IMachine machine, DateTime dateTime, DateTime final)
    {
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime);
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime, new UtcDateTimeRange (final.Subtract (TimeSpan.FromSeconds (1))), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
    }

    void CheckNoData (string name, IMachine machine, DateTime dateTime)
    {
      var response = Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime (name, machine, dateTime);
      Assert.IsTrue (response.NoData);
    }

    void CheckNotApplicable (string name, IMachine machine, DateTime dateTime)
    {
      var response = Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime (name, machine, dateTime);
      Assert.IsTrue (response.NotApplicable);
    }

    void CheckNoData (string name, IMachine machine, DateTime at, UtcDateTimeRange limit)
    {
      var response = Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime (name, machine, at, R (-100), limit);
      Assert.IsTrue (response.NoData || (response.Final.HasValue && !limit.ContainsElement (response.Final.Value)));
    }

    void CheckFinal (string name, IMachine machine, DateTime at, DateTime final, UtcDateTimeRange limit)
    {
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, at, R (-100), limit);
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      if (T (1) < final) {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, at, R (1), limit);
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, at, new UtcDateTimeRange (final.Subtract (TimeSpan.FromSeconds (1))), limit);
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
    }
  }
}
