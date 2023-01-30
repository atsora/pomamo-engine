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
using Lemoine.Extensions.ExtensionsProvider;
using Pulse.Extensions;
using Lemoine.Extensions.Plugin;

namespace Lemoine.Plugin.Groups.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class GroupStoppedMachines_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (GroupStoppedMachines_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public GroupStoppedMachines_UnitTest ()
    {

    }

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
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.Web);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new DummyPluginsLoader ();
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);

          Lemoine.Extensions.Package.PackageFile.InstallOrUpgradeJsonString (
            @"
{
  ""Identifier"": ""GroupStoppedMachines_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""GroupStoppedMachines"",
      ""Instances"": [
        {
          ""Name"": ""All"",
          ""Parameters"": {
            ""GroupCategoryName"": ""Stopped machines"",
            ""GroupCategorySortPriority"": 20.0,
            ""GroupCategoryPrefix"": ""SM"",
            ""MinimumStopDuration"": ""0:15:00"",
            ""GroupCategorySortPriority"": 0,
            ""GroupCategoryReference"": ""All""
          }
        }
      ]
    }
  ]
}
", true, true);
          var extensionsLoader = new Lemoine.Extensions.ExtensionsLoaderLctr (extensionsProvider) {
            ForceNewExtensionsProvider = true
          };
          extensionsLoader.LoadExtensions ();

          // Machine modes
          var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
          var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);

          // Create a machine that will be stopped
          var machine1 = CreateMachine ("unit test for stopped machine group - machine 1");
          var initialMachines = new List<IMachine> ();
          initialMachines.Add (machine1);
          DateTime now = DateTime.UtcNow;
          AddFact (machine1, now.Subtract (TimeSpan.FromMinutes (20)), now, inactive);

          // Create a machine that is stopped but not for long
          var machine2 = CreateMachine ("unit test for stopped machine group - machine 2");
          initialMachines.Add (machine2);
          AddFact (machine2, now.Subtract (TimeSpan.FromMinutes (20)), now.Subtract (TimeSpan.FromMinutes (10)), active);
          AddFact (machine2, now.Subtract (TimeSpan.FromMinutes (10)), now, inactive);

          // Create a machine that is not stopped
          var machine3 = CreateMachine ("unit test for stopped machine group - machine 3");
          initialMachines.Add (machine3);
          AddFact (machine3, now.Subtract (TimeSpan.FromMinutes (20)), now, active);

          // Load the group
          var request = new Lemoine.Business.Machine.GroupFromId ("SM");
          var group = Lemoine.Business.ServiceProvider.Get (request);
          Assert.IsNotNull (group, "Group not found");

          // Get stopped machines
          var machines = group.GetMachines (initialMachines);
          Assert.AreEqual (1, machines.Count ());

          // Machine 1 is stopped
          foreach (var machine in machines) {
            Assert.AreEqual (machine.Name, "unit test for stopped machine group - machine 1");
            break;
          }
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          transaction.Rollback ();
        }
      }
    }

    IMonitoredMachine CreateMachine (string machineName)
    {
      var machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine ();
      machine.MonitoringType = ModelDAOHelper.DAOFactory.MachineMonitoringTypeDAO.FindById ((int)MachineMonitoringTypeId.Monitored);
      machine.Name = machineName;
      ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
      return machine;
    }

    void AddFact (IMonitoredMachine machine, DateTime start, DateTime end, IMachineMode machineMode)
    {
      IFact fact = ModelDAOHelper.ModelFactory.CreateFact (machine, start, end, machineMode);
      ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
    }
  }
}
