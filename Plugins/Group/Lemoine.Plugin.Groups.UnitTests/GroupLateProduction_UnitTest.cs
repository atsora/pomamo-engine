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
using Lemoine.Business;
using Lemoine.Extensions.ExtensionsProvider;
using Pulse.Extensions;
using Lemoine.Extensions.Plugin;

namespace Lemoine.Plugin.Groups.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class GroupLateProduction_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (GroupLateProduction_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public GroupLateProduction_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {

        // Create 3 machines
        var initialMachines = new List<IMonitoredMachine> ();
        initialMachines.Add (CreateMachine ("unit test for late production - machine 1"));
        initialMachines.Add (CreateMachine ("unit test for late production - machine 2"));
        initialMachines.Add (CreateMachine ("unit test for late production - machine 3"));

        // Load the group
        Lemoine.Extensions.Package.PackageFile.InstallOrUpgradeJsonString (
@"{
  ""Identifier"": ""GroupLateProduction_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""GroupLateProduction"",
      ""Instances"": [
        {
          ""Name"": ""All"",
          ""Parameters"": {
            ""GroupCategoryName"": ""Machines with late production"",
            ""GroupCategoryPrefix"": ""LP""
          }
        }
      ]
    }
  ]
}", true, true);
        var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
        var pluginFilter = new PluginFilterFromFlag (PluginFlag.Web);
        var pluginsLoader = new PluginsLoader (assemblyLoader);
        var nhibernatePluginsLoader = new DummyPluginsLoader ();
        var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
        var extensionsLoader = new Lemoine.Extensions.ExtensionsLoaderLctr (extensionsProvider);
        extensionsLoader.LoadExtensions ();

        var request = new Lemoine.Business.Machine.GroupFromId ("LP");
        var group = Lemoine.Business.ServiceProvider.Get (request);
        Assert.IsNotNull (group, "Group not found");

        // Use a custom service provider to simulate a production
        using (new CustomServiceProvider (new GroupLateProduction_TestServiceProvider())) {
          try {
            // Get the number of machines
            var machines = group.GetMachines (initialMachines);
            Assert.AreEqual (1, machines.Count ());

            // Machine 1 is late
            foreach (var machine in machines) {
              Assert.AreEqual (machine.Name, "unit test for late production - machine 1");
              break;
            }
          }
          finally {
            Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
            transaction.Rollback ();
          }
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
  }
}
