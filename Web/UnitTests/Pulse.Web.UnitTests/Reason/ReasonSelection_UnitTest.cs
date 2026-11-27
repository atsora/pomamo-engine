// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Pulse.Extensions;
using Pulse.Web.CommonResponseDTO;
using Pulse.Web.Reason;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    public async Task TestReasonSelectionAsync ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var request = new ReasonSelectionRequestDTO ();
          request.MachineId = 1;
          request.Range = "[2011-11-22T08:30:00Z,2011-11-22T08:30:00Z]";

          var response = await m_service.Get (request) as IList<ReasonSelectionResponseDTO>;

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

    /// <summary>
    /// A machine state template offered as a reason, without a color of its own:
    /// it takes the color of its reason group
    /// </summary>
    [Test]
    public async Task TestReasonSelectionMachineStateTemplateGroupColorAsync ()
    {
      var (selection, reasonGroupColor) = await GetMachineStateTemplateSelectionAsync (null);
      Assert.Multiple (() => {
        Assert.That (reasonGroupColor, Is.Not.Empty);
        Assert.That (selection.Color, Is.EqualTo (reasonGroupColor));
        Assert.That (selection.ReasonGroupColor, Is.EqualTo (reasonGroupColor));
      });
    }

    /// <summary>
    /// A machine state template offered as a reason, with a color of its own:
    /// it keeps its color
    /// </summary>
    [Test]
    public async Task TestReasonSelectionMachineStateTemplateOwnColorAsync ()
    {
      const string color = "#123456";
      var (selection, reasonGroupColor) = await GetMachineStateTemplateSelectionAsync (color);
      Assert.Multiple (() => {
        Assert.That (reasonGroupColor, Is.Not.EqualTo (color));
        Assert.That (selection.Color, Is.EqualTo (color));
        Assert.That (selection.ReasonGroupColor, Is.EqualTo (reasonGroupColor));
      });
    }

    /// <summary>
    /// The machine state template 1 (attended) offered as a reason of the group 1 on machine 1,
    /// after setting its color, in a transaction that is rolled back, and the color of the reason group 1
    /// </summary>
    async Task<(ReasonSelectionResponseDTO, string)> GetMachineStateTemplateSelectionAsync (string machineStateTemplateColor)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Extensions.Package.PackageFile.InstallOrUpgradeJsonString ($$"""
{
  "Identifier": "MachineStateTemplateReasonSelection_UnitTest",
  "Name": "UnitTest",
  "Description": "",
  "Tags": [],
  "Version": 1,
  "Plugins": [
    {
      "Name": "MachineStateTemplateReasonSelection",
      "Instances": [
        {
          "Name": "Test",
          "Parameters": {
            "ReasonGroupId": 1,
            "MachineStateTemplateIds": [ 1 ]
          }
        }
      ]
    }
  ]
}
""", true, true);
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.Web);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory, pluginFilter, Lemoine.Extensions.Analysis.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, new DummyPluginsLoader ());
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          var unattendedMachineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.FindById (2);
          var attendedMachineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.FindById (1);
          attendedMachineStateTemplate.Color = machineStateTemplateColor;
          ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.MakePersistent (attendedMachineStateTemplate);
          var nextMachineStateTemplate = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateFlow (unattendedMachineStateTemplate, attendedMachineStateTemplate);
          ModelDAOHelper.DAOFactory.MachineStateTemplateFlowDAO.MakePersistent (nextMachineStateTemplate);

          var request = new ReasonSelectionRequestDTO ();
          request.MachineId = 1;
          request.Range = "[2011-11-22T08:30:00Z,2011-11-22T08:30:00Z]";

          var response = await m_service.Get (request) as IList<ReasonSelectionResponseDTO>;

          Assert.That (response, Is.Not.Null);
          var machineStateTemplateSelections = response
            .Where (item => item.ClassificationId.StartsWith ("MST"))
            .ToList ();
          Assert.That (machineStateTemplateSelections.Select (x => x.ClassificationId), Is.EqualTo (new[] { "MST1" }));
          var reasonGroupColor = ModelDAOHelper.DAOFactory.ReasonGroupDAO.FindById (1).Color;
          return (machineStateTemplateSelections.Single (), reasonGroupColor);
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          transaction.Rollback ();
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
