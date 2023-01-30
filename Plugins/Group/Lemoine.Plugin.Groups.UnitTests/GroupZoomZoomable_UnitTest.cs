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
using Lemoine.Extensions.Plugin;
using Lemoine.Extensions.ExtensionsProvider;

namespace Lemoine.Plugin.Groups.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class GroupZoomZoomable_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (GroupZoomZoomable_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public GroupZoomZoomable_UnitTest ()
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
  ""Identifier"": ""GroupFromId_UnitTest"",
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
      ""Name"": ""GroupConcurrentGroups"",
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
    },
    {
      ""Name"": ""GroupZoomZoomable"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
            ""ParentRegex"": ""T\\w+""
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

          {
            var request = new Lemoine.Business.Machine.GroupZoomIn ("Test");
            var response = Business.ServiceProvider.Get (request);
            Assert.AreEqual (2, response.Children.Count ());
            Assert.IsFalse (response.Dynamic.Value);
            Assert.IsTrue (response.Children.Contains ("1"));
            Assert.IsTrue (response.Children.Contains ("2"));
          }
          {
            var request = new Lemoine.Business.Machine.GroupZoomOut ("1");
            var response = Business.ServiceProvider.Get (request);
            Assert.IsFalse (response.Dynamic.Value);
            Assert.AreEqual ("Test", response.Parent);
          }
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          transaction.Rollback ();
        }
      }
    }
  }
}
