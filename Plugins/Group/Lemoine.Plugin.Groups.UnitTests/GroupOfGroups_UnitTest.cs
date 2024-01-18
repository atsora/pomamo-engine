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
  public class GroupOfGroups_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (GroupOfGroups_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public GroupOfGroups_UnitTest ()
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
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.Web);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new DummyPluginsLoader ();
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);

          Lemoine.Extensions.Package.PackageFile
            .InstallOrUpgradeJsonString (
            @"
{
  ""Identifier"": ""GroupOfGroups_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""GroupSingleMachine"",
      ""Instances"": [
        {
          ""Name"": ""Default"",
          ""Parameters"": {
            ""GroupCategoryName"": """",
            ""GroupCategorySortPriority"": 0,
            ""GroupCategoryPrefix"": """"
          }
        }
      ]
    },
    {
      ""Name"": ""GroupOfGroups"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
            ""GroupCategoryName"": """",
            ""GroupCategorySortPriority"": 0,
            ""GroupCategoryReference"": ""Test"",
            ""GroupId"": ""Test"",
            ""GroupName"": ""Test"",
            ""GroupSortPriority"": 0,
            ""GroupIds"": [ ""1"", ""2"" ]
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

          var groupId = "Test";
          var request = new Lemoine.Business.Machine.GroupFromId (groupId);
          var group = Lemoine.Business.ServiceProvider.Get (request);
          Assert.IsFalse (group.Dynamic);
          var machines = group.GetMachines ();
          Assert.That (machines.Count (), Is.EqualTo (2));
          Assert.IsTrue (machines.Any (m => m.Id == 1));
          Assert.IsTrue (machines.Any (m => m.Id == 2));
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          transaction.Rollback ();
        }
      }
    }
  }
}
