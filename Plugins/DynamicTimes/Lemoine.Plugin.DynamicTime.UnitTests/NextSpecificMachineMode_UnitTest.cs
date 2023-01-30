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
using Pulse.Extensions;
using Lemoine.Extensions;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Extensions.Plugin;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class NextSpecificMachineMode_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (NextSpecificMachineMode_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NextSpecificMachineMode_UnitTest ()
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
            .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""NextSpecificMachineMode_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""NextSpecificMachineMode"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""Name"": ""A"",
  ""EndMachineModeIds"": [10],
  ""CancelMachineModeIds"": [2],
  ""StartUpMachineModeIds"": [2, 8]
          }
        }
      ]
    }
  ]
}
", true, true);
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          { // Check final
            var checker = new DynamicEndChecker ("A", machine, T (0));

            checker.CheckPending ();

            AddFact (machine, R (1, 2), machining);
            checker.CheckAfter (T (2));

            AddFact (machine, R (3, 4), autoFeed);
            checker.CheckAfter (T (4));

            AddFact (machine, R (5, 6), inactive);
            checker.CheckAfter (T (6));

            AddFact (machine, R (7, 8), error);
            checker.CheckFinal (T (7));
          }

          { // Check cancel
            var checker = new DynamicEndChecker ("A", machine, T (10));

            checker.CheckPending ();

            AddFact (machine, R (11, 12), machining);
            checker.CheckAfter (T (12));

            AddFact (machine, R (13, 14), autoFeed);
            checker.CheckAfter (T (14));

            AddFact (machine, R (15, 16), inactive);
            checker.CheckAfter (T (16));

            AddFact (machine, R (17, 18), machining);
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
