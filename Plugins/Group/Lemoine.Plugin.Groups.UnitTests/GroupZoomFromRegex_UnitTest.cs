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
  public class GroupZoomFromRegex_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (GroupZoomFromRegex_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public GroupZoomFromRegex_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestNoVirtual ()
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
      ""Name"": ""GroupCompany"",
      ""Instances"": [
        {
          ""Name"": """",
          ""Parameters"": {
            ""GroupCategoryName"": ""Company"",
            ""GroupCategoryPrefix"": ""C"",
            ""GroupCategorySortPriority"": 10.1
          }
        }
      ]
    },
    {
      ""Name"": ""GroupDepartment"",
      ""Instances"": [
        {
          ""Name"": """",
          ""Parameters"": {
            ""GroupCategoryName"": ""Department"",
            ""GroupCategoryPrefix"": ""D"",
            ""GroupCategorySortPriority"": 10.1
          }
        }
      ]
    },
    {
      ""Name"": ""GroupMachineCategory"",
      ""Instances"": [
        {
          ""Name"": """",
          ""Parameters"": {
            ""GroupCategoryName"": ""MachineCategory"",
            ""GroupCategoryPrefix"": ""MC"",
            ""GroupCategorySortPriority"": 10.1
          }
        }
      ]
    },
    {
      ""Name"": ""GroupZoomFromRegex"",
      ""Instances"": [
        {
          ""Name"": ""CD"",
          ""Parameters"": {
            ""ParentRegex"": ""C\\d+"",
            ""ChildrenRegex"": ""({ParentGroupId}_)?D\\d+""
          }
        },
        {
          ""Name"": ""CDMC"",
          ""Parameters"": {
            ""ParentRegex"": ""C\\d+_D\\d+"",
            ""ChildrenRegex"": ""((C\\d+_)?D\\d+_)?MC\\d+""
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

          var machineCategory = ModelDAOHelper.ModelFactory.CreateMachineCategory ();
          machineCategory.Name = "MachineCategory";
          ModelDAOHelper.DAOFactory.MachineCategoryDAO.MakePersistent (machineCategory);
          var machine1 = ModelDAOHelper.DAOFactory.MachineDAO
            .FindById (1);
          machine1.Category = machineCategory;
          ModelDAOHelper.DAOFactory.MachineDAO.MakePersistent (machine1);
          ModelDAOHelper.DAOFactory.Flush ();

          {
            var request = new Lemoine.Business.Machine.GroupZoomIn ("C1");
            var response = Business.ServiceProvider.Get (request);
            Assert.That (response.Children.Count (), Is.EqualTo (2));
            Assert.IsFalse (response.Dynamic.Value);
            Assert.IsTrue (response.Children.Contains ("C1_D1"));
            Assert.IsTrue (response.Children.Contains ("C1_D2"));
          }
          {
            var request = new Lemoine.Business.Machine.GroupZoomOut ("C1_D1");
            var response = Business.ServiceProvider.Get (request);
            Assert.IsFalse (response.Dynamic.Value);
            Assert.That (response.Parent, Is.EqualTo ("C1"));
          }
          {
            var request = new Lemoine.Business.Machine.GroupZoomIn ("C1_D1");
            var response = Business.ServiceProvider.Get (request);
            Assert.That (response.Children.Count (), Is.EqualTo (1));
            Assert.IsFalse (response.Dynamic.Value);
            Assert.IsTrue (response.Children.First ().StartsWith ("MC"));
          }
          {
            var request = new Lemoine.Business.Machine.GroupZoomOut ("MC" + machineCategory.Id);
            var response = Business.ServiceProvider.Get (request);
            Assert.IsFalse (response.Dynamic.Value);
            Assert.That (response.Parent, Is.EqualTo ("C1_D1"));
          }
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestIncludeVirtualChildren ()
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
      ""Name"": ""GroupCompany"",
      ""Instances"": [
        {
          ""Name"": """",
          ""Parameters"": {
            ""GroupCategoryName"": ""Company"",
            ""GroupCategoryPrefix"": ""C"",
            ""GroupCategorySortPriority"": 10.1
          }
        }
      ]
    },
    {
      ""Name"": ""GroupDepartment"",
      ""Instances"": [
        {
          ""Name"": """",
          ""Parameters"": {
            ""GroupCategoryName"": ""Department"",
            ""GroupCategoryPrefix"": ""D"",
            ""GroupCategorySortPriority"": 10.1
          }
        }
      ]
    },
    {
      ""Name"": ""GroupMachineCategory"",
      ""Instances"": [
        {
          ""Name"": """",
          ""Parameters"": {
            ""GroupCategoryName"": ""MachineCategory"",
            ""GroupCategoryPrefix"": ""MC"",
            ""GroupCategorySortPriority"": 10.1
          }
        }
      ]
    },
    {
      ""Name"": ""GroupZoomFromRegex"",
      ""Instances"": [
        {
          ""Name"": ""CD"",
          ""Parameters"": {
            ""ParentRegex"": ""C\\d+"",
            ""ChildrenRegex"": ""{ParentGroupId}_D\\d+"",
            ""IncludeVirtualChildren"": true
          }
        },
        {
          ""Name"": ""CDMC"",
          ""Parameters"": {
            ""ParentRegex"": ""C\\d+_D\\d+"",
            ""ChildrenRegex"": ""{ParentGroupId}_MC\\d+"",
            ""IncludeVirtualChildren"": true
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

          var machineCategory = ModelDAOHelper.ModelFactory.CreateMachineCategory ();
          machineCategory.Name = "MachineCategory";
          ModelDAOHelper.DAOFactory.MachineCategoryDAO.MakePersistent (machineCategory);
          var machine1 = ModelDAOHelper.DAOFactory.MachineDAO
            .FindById (1);
          machine1.Category = machineCategory;
          ModelDAOHelper.DAOFactory.MachineDAO.MakePersistent (machine1);
          ModelDAOHelper.DAOFactory.Flush ();

          {
            var request = new Lemoine.Business.Machine.GroupZoomIn ("C1");
            var response = Business.ServiceProvider.Get (request);
            Assert.That (response.Children.Count (), Is.EqualTo (2));
            Assert.IsFalse (response.Dynamic.Value);
            Assert.IsTrue (response.Children.Contains ("C1_D1"));
            Assert.IsTrue (response.Children.Contains ("C1_D2"));
          }
          {
            var request = new Lemoine.Business.Machine.GroupZoomOut ("C1_D1");
            var response = Business.ServiceProvider.Get (request);
            Assert.IsFalse (response.Dynamic.Value);
            Assert.That (response.Parent, Is.EqualTo ("C1"));
          }
          {
            var request = new Lemoine.Business.Machine.GroupZoomIn ("C1_D1");
            var response = Business.ServiceProvider.Get (request);
            Assert.That (response.Children.Count (), Is.EqualTo (1));
            Assert.IsFalse (response.Dynamic.Value);
            Assert.IsTrue (response.Children.First ().StartsWith ("C1_D1_MC"));
          }
          {
            var request = new Lemoine.Business.Machine.GroupZoomOut ("C1_D1_MC" + machineCategory.Id);
            var response = Business.ServiceProvider.Get (request);
            Assert.IsFalse (response.Dynamic.Value);
            Assert.That (response.Parent, Is.EqualTo ("C1_D1"));
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
