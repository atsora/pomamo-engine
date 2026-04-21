// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.Core.Log;
using Pulse.Extensions;
using Lemoine.Extensions;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Extensions.Plugin;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class PreviousSpecificMachineMode_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (PreviousSpecificMachineMode_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public PreviousSpecificMachineMode_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new DummyPluginsLoader ();
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
          IMachineMode inactive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Inactive);
          IMachineMode active = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Active);
          IMachineMode manualActive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.ManualActive);
          IMachineMode autoFeed = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.AutoFeed);
          IMachineMode machining = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Machining);
          IMachineMode error = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Error);

          Lemoine.Extensions.Package.PackageFile
            .InstallOrUpgradeJsonString ("""
{
  "Identifier": "PreviousSpecificMachineMode_UnitTest",
  "Name": "UnitTest",
  "Description": "",
  "Tags": [],
  "Version": 1,
  "Plugins": [
    {
      "Name": "PreviousSpecificMachineMode",
      "Instances": [
        {
          "Name": "Test",
          "Parameters": {
  "Name": "A",
  "EndMachineModeIds": [10],
  "CancelMachineModeIds": [2],
  "StartUpMachineModeIds": [2, 8]
          }
        }
      ]
    }
  ]
}
""", true, true);
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          { // Check final
            var checker = new DynamicEndChecker ("A", machine, T (0));

            checker.CheckPending ();

            AddFact (machine, R (-18, -17), machining);
            AddFact (machine, R (-16, -15), inactive);
            AddFact (machine, R (-15, -13), autoFeed);
            AddFact (machine, R (-12, -11), machining);
            AddFact (machine, R (-8, -7), error);
            AddFact (machine, R (-6, -5), inactive);
            AddFact (machine, R (-4, -3), autoFeed);
            AddFact (machine, R (-2, -1), machining);
            checker.CheckPending ();

            AddFact (machine, R (1, 2), machining);

            checker.CheckFinal (T (-7));
          }

          { // Check cancel
            var checker = new DynamicEndChecker ("A", machine, T (-10));
            checker.CheckNoData ();
          }
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestMaxDuration ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new DummyPluginsLoader ();
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
          IMachineMode inactive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Inactive);
          IMachineMode active = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Active);
          IMachineMode manualActive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.ManualActive);
          IMachineMode autoFeed = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.AutoFeed);
          IMachineMode machining = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Machining);
          IMachineMode error = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Error);

          Lemoine.Extensions.Package.PackageFile
              .InstallOrUpgradeJsonString ("""
{
  "Identifier": "PreviousSpecificMachineMode_UnitTest",
  "Name": "UnitTestMaxDuration",
  "Description": "",
  "Tags": [],
  "Version": 1,
  "Plugins": [
    {
      "Name": "PreviousSpecificMachineMode",
      "Instances": [
        {
          "Name": "Test",
          "Parameters": {
  "Name": "A",
  "EndMachineModeIds": [10],
  "CancelMachineModeIds": [2],
  "StartUpMachineModeIds": [2, 8],
  "MaxDuration": "0:05:00"
          }
        }
      ]
    }
  ]
}
""", true, true);
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          { // Check no data
            var checker = new DynamicEndChecker ("A", machine, T (0));

            checker.CheckPending ();

            AddFact (machine, R (-18, -17), machining);
            AddFact (machine, R (-16, -15), inactive);
            AddFact (machine, R (-15, -13), autoFeed);
            AddFact (machine, R (-12, -11), machining);
            AddFact (machine, R (-8, -7), error);
            AddFact (machine, R (-6, -5), inactive);
            AddFact (machine, R (-4, -3), autoFeed);
            AddFact (machine, R (-2, -1), machining);
            checker.CheckPending ();

            AddFact (machine, R (1, 2), machining);

            checker.CheckNoData ();
          }
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    void AddFact (IMonitoredMachine machine, UtcDateTimeRange range, IMachineMode machineMode)
    {
      IFact fact = ModelDAOHelper.ModelFactory.CreateFact (machine, range.Lower.Value, range.Upper.Value, machineMode);
      ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
    }
  }
}
