// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Linq;
using Lemoine.ModelDAO;
using Pulse.Web.Reason;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Extensions.Plugin;
using Pulse.Extensions;
using System.Data.Odbc;

namespace Pulse.Web.UnitTests.Reason
{
  /// <summary>
  /// Unit tests for the class ReasonSelectionService
  /// </summary>
  [TestFixture]
  public class ReasonSelection_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReasonSelection_UnitTest).FullName);

    Pulse.Web.Reason.ReasonSelectionService m_service;

    [Test]
    public void TestReasonSelection ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var request = new ReasonSelectionRequestDTO ();
          request.MachineId = 1;
          request.Range = "[2011-11-22T08:30:00Z,2011-11-22T08:30:00Z]";

          var response = m_service.GetWithoutCache (request) as IList<ReasonSelectionResponseDTO>;

          Assert.That (response, Is.Not.Null);
          Assert.That (response, Has.Count.EqualTo (9));

          var reason1 = response.First (item => (16 == item.Id));
          Assert.Multiple (() => {
            Assert.That (reason1.Id, Is.EqualTo (16));
            Assert.That (reason1.ReasonGroupId, Is.EqualTo (17));
            Assert.That (reason1.ReasonGroupDisplay, Is.EqualTo ("Mounted new workpiece"));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    [Test]
    public void TestReasonSelectionWithReasonData ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager.Add<Lemoine.Plugin.TestReasonData.ReasonSelectionExtension> ();
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          var request = new ReasonSelectionRequestDTO ();
          request.MachineId = 1;
          request.Range = "[2011-11-22T08:30:00Z,2011-11-22T08:30:00Z]";

          var response = m_service.GetWithoutCache (request) as IList<ReasonSelectionResponseDTO>;

          Assert.That (response, Is.Not.Null);
          Assert.That (response, Has.Count.EqualTo (11));

          var withDataSelections = response.Where (item => (5 == item.Id) && (item.Data != null));
          Assert.Multiple (() => {
            Assert.That (withDataSelections.ToList (), Has.Count.EqualTo (2));
            Assert.That (withDataSelections.Select (x => x.Display), Has.One.With.EqualTo ("Test 1"));
            Assert.That (withDataSelections.Select (x => x.Data), Has.All.With.ContainKey ("Test"));
            Assert.That (withDataSelections.Select (x => x.Data), Has.One.With.ContainValue (1.0));
            Assert.That (withDataSelections.Select (x => x.Data), Has.One.With.ContainValue (2.0));
          });
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          transaction.Rollback ();
        }
      }
    }

    [Test]
    public void TestReasonSelectionTycos ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();

      // To initialize the ODBC drivers that could be used in plugins
      using (var connection = new OdbcConnection ("")) {
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Extensions.Package.PackageFile
            .InstallOrUpgradeJsonString ("""
            {
              "Identifier": "TestReasonSelectionTycos",
              "Name": "UnitTest",
              "Description": "",
              "Tags": [],
              "Version": 1,
              "Plugins": [
                {
                  "Name": "Tycos.MaintenanceWO",
                  "Instances": [
                  {
                    "Name": "Test",
                    "Parameters": {
                      "ReasonId": 1,
                      "DsnName": "TycosAdmin2000",
                      "Password": "engineer99"
                    }
                  }
                  ]
                }
              ]
            }
            """, true, true);
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (Lemoine.Model.PluginFlag.Web);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Web.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, new DummyPluginsLoader ());
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          var machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (1);
          machine.ExternalCode = "CHETO GUNDRILL #1";
          ModelDAOHelper.DAOFactory.MachineDAO.MakePersistent (machine);

          var request = new ReasonSelectionRequestDTO ();
          request.MachineId = 1;
          request.Range = "[2011-11-22T08:30:00Z,2011-11-22T08:30:00Z]";

          var response = m_service.GetWithoutCache (request) as IList<ReasonSelectionResponseDTO>;

          Assert.That (response, Is.Not.Null);
          Assert.That (response, Has.Count.EqualTo (12));

          var withDataSelections = response.Where (item => (1 == item.Id) && (item.Data != null));
          Assert.Multiple (() => {
            Assert.That (withDataSelections.ToList (), Has.Count.EqualTo (3));
            Assert.That (withDataSelections.Select (x => x.Display), Has.One.With.EqualTo ("36063: Gundrill toolchanger#2 was damaged during the tool change operation."));
            Assert.That (withDataSelections.Select (x => x.Data), Has.All.With.ContainKey ("mwo"));
          });
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          transaction.Rollback ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        }
      }
    }

    [OneTimeSetUp]
    public void Init ()
    {
      m_service = new ReasonSelectionService ();

      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
    }
  }
}
