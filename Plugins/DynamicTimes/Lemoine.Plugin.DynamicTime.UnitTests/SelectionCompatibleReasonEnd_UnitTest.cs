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
  /// Unit tests of the SelectionCompatibleReasonEnd dynamic time
  /// </summary>
  public class SelectionCompatibleReasonEnd_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (SelectionCompatibleReasonEnd_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public SelectionCompatibleReasonEnd_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// A manual reason is not stopped by a machine mode change,
    /// but by the first machine mode that is not associated to the reason in table reasonselection
    /// </summary>
    [Test]
    public void TestSelectionCompatibleReasonEnd ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          InitializeExtensions ();

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
          IMachineMode inactive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)MachineModeId.Inactive);
          IMachineMode inactiveOn = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)MachineModeId.InactiveOn);
          IMachineMode autoInactive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)MachineModeId.AutoInactive);
          IMachineMode autoFeed = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)MachineModeId.AutoFeed);
          IMachineObservationState attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);

          var reason = CreateReason ("SelectionCompatibleReasonEndTest");
          // Note: inactiveOn and autoInactive are descendants of inactive
          AddReasonSelection (inactive, attended, reason);
          SetMachineObservationState (machine, R (-10, 100), attended);
          AddManualReason (machine, R (0), reason);

          InstallPlugin ("""
{
  "Identifier": "SelectionCompatibleReasonEnd_UnitTest",
  "Name": "UnitTest",
  "Description": "",
  "Tags": [],
  "Version": 1,
  "Plugins": [
    {
      "Name": "DynamicTimesManualReason",
      "Instances": [
        {
          "Name": "Test",
          "Parameters": {
  "NamePrefix": "Test"
          }
        }
      ]
    }
  ]
}
""");

          var checker = new DynamicEndChecker ("TestSelectionCompatibleReasonEnd", machine, T (0));

          checker.CheckPending ();

          AddFact (machine, R (0, 1), autoInactive);
          checker.CheckAfter (T (1));

          // A new machine mode, but it is still a descendant of inactive => the reason still applies
          AddFact (machine, R (1, 2), inactiveOn);
          checker.CheckAfter (T (2));

          // autoFeed is not a descendant of inactive => the reason does not apply any more
          AddFact (machine, R (2, 3), autoFeed);
          checker.CheckFinal (T (2));
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// A reason that is configured for a machine mode applies to all its descendants,
    /// even if the descendant machine mode is configured as well
    /// </summary>
    [Test]
    public void TestConfiguredDescendantMachineMode ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          InitializeExtensions ();

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
          IMachineMode inactive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)MachineModeId.Inactive);
          IMachineMode inactiveOn = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)MachineModeId.InactiveOn);
          IMachineMode autoInactive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)MachineModeId.AutoInactive);
          IMachineMode autoFeed = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)MachineModeId.AutoFeed);
          IMachineObservationState attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);

          var parentReason = CreateReason ("SelectionCompatibleParentReason");
          var childReason = CreateReason ("SelectionCompatibleChildReason");
          AddReasonSelection (inactive, attended, parentReason);
          // autoInactive has its own configuration, that does not contain parentReason
          AddReasonSelection (autoInactive, attended, childReason);
          SetMachineObservationState (machine, R (-10, 100), attended);
          AddManualReason (machine, R (0), parentReason);

          InstallPlugin ("""
{
  "Identifier": "SelectionCompatibleReasonEnd_UnitTest",
  "Name": "UnitTestConfiguredDescendantMachineMode",
  "Description": "",
  "Tags": [],
  "Version": 1,
  "Plugins": [
    {
      "Name": "DynamicTimesManualReason",
      "Instances": [
        {
          "Name": "Test",
          "Parameters": {
  "NamePrefix": "Test"
          }
        }
      ]
    }
  ]
}
""");

          AddFact (machine, R (0, 1), inactiveOn);
          AddFact (machine, R (1, 2), autoInactive);
          AddFact (machine, R (2, 3), autoFeed);

          // parentReason is configured for inactive, an ancestor of autoInactive:
          // it still applies in T(1)-T(2), although autoInactive is configured with childReason only
          var checker = new DynamicEndChecker ("TestSelectionCompatibleReasonEnd", machine, T (0));
          checker.CheckFinal (T (2));
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the dynamic time that is applied from an observation state change
    /// </summary>
    [Test]
    public void TestObservationStateChange ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          InitializeExtensions ();

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
          IMachineMode inactive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)MachineModeId.Inactive);
          IMachineMode inactiveOn = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)MachineModeId.InactiveOn);
          IMachineMode autoInactive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)MachineModeId.AutoInactive);
          IMachineMode autoFeed = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)MachineModeId.AutoFeed);
          IMachineObservationState attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);

          var reason = CreateReason ("SelectionCompatibleObservationStateChange");
          AddReasonSelection (inactive, attended, reason);
          // Two consecutive observation state slots: the observation state changes in T(2)
          SetMachineObservationState (machine, R (-10, 2), attended);
          SetMachineObservationState (machine, R (2, 100), attended);
          AddManualReason (machine, R (0), reason);

          InstallPlugin ("""
{
  "Identifier": "SelectionCompatibleReasonEnd_UnitTest",
  "Name": "UnitTestObservationStateChange",
  "Description": "",
  "Tags": [],
  "Version": 1,
  "Plugins": [
    {
      "Name": "SameMachineMode",
      "Instances": [
        {
          "Name": "Default",
          "Parameters": {}
        }
      ]
    },
    {
      "Name": "DynamicTimesManualReason",
      "Instances": [
        {
          "Name": "Stop",
          "Parameters": {
  "NamePrefix": "Stop"
          }
        },
        {
          "Name": "NextMode",
          "Parameters": {
  "NamePrefix": "NextMode",
  "ObservationStateChangeDynamicTime": "NextMachineMode"
          }
        },
        {
          "Name": "Recursive",
          "Parameters": {
  "NamePrefix": "Recursive",
  "ObservationStateChangeDynamicTime": "RecursiveSelectionCompatibleReasonEnd"
          }
        }
      ]
    }
  ]
}
""");

          AddFact (machine, R (0, 1), autoInactive);
          AddFact (machine, R (1, 2), inactiveOn);
          AddFact (machine, R (2, 3), inactiveOn);
          AddFact (machine, R (3, 4), inactive);
          AddFact (machine, R (4, 5), autoFeed);

          { // No dynamic time in configuration: stop at the observation state change
            var checker = new DynamicEndChecker ("StopSelectionCompatibleReasonEnd", machine, T (0));
            checker.CheckFinal (T (2));
          }

          { // Stop at the first machine mode change after the observation state change
            var checker = new DynamicEndChecker ("NextModeSelectionCompatibleReasonEnd", machine, T (0));
            checker.CheckFinal (T (3));
          }

          { // Same behaviour in the next observation state slot:
            // inactive is still compatible, autoFeed is not
            var checker = new DynamicEndChecker ("RecursiveSelectionCompatibleReasonEnd", machine, T (0));
            checker.CheckFinal (T (4));
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
    /// No reason is applied yet at the specified date/time:
    /// the computation is postponed, without skipping any period after it
    /// </summary>
    [Test]
    public void TestNoManualReason ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          InitializeExtensions ();

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
          IMachineMode inactive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)MachineModeId.Inactive);
          IMachineMode autoFeed = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)MachineModeId.AutoFeed);
          IMachineObservationState attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);

          var reason = CreateReason ("SelectionCompatibleNoManualReason");
          AddReasonSelection (inactive, attended, reason);
          SetMachineObservationState (machine, R (-10, 100), attended);

          InstallPlugin ("""
{
  "Identifier": "SelectionCompatibleReasonEnd_UnitTest",
  "Name": "UnitTestNoManualReason",
  "Description": "",
  "Tags": [],
  "Version": 1,
  "Plugins": [
    {
      "Name": "DynamicTimesManualReason",
      "Instances": [
        {
          "Name": "Test",
          "Parameters": {
  "NamePrefix": "Test"
          }
        }
      ]
    }
  ]
}
""");

          AddFact (machine, R (0, 1), inactive);
          AddFact (machine, R (1, 2), autoFeed);

          // No manual reason at T(0) yet: the returned hint must not skip any period after T(0)
          var response = Lemoine.Business.DynamicTimes.DynamicTime
            .GetDynamicTime ("TestSelectionCompatibleReasonEnd", machine, T (0));
          Assert.Multiple (() => {
            Assert.That (response.Final.HasValue, Is.False);
            Assert.That (response.NoData, Is.False);
            Assert.That (response.Hint.Lower.HasValue, Is.True);
            Assert.That (response.Hint.Lower.Value, Is.EqualTo (T (0)));
          });

          // Once the manual reason is applied, the dynamic end is returned
          AddManualReason (machine, R (0), reason);
          var checker = new DynamicEndChecker ("TestSelectionCompatibleReasonEnd", machine, T (0));
          checker.CheckFinal (T (1));
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    void InitializeExtensions ()
    {
      var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
      var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
      var pluginsLoader = new PluginsLoader (assemblyLoader);
      var nhibernatePluginsLoader = new DummyPluginsLoader ();
      var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter,
        Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
      Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);
    }

    void InstallPlugin (string json)
    {
      Lemoine.Extensions.Package.PackageFile
        .InstallOrUpgradeJsonString (json, true, true);
      Lemoine.Extensions.ExtensionManager.Activate (false);
      Lemoine.Extensions.ExtensionManager.Load ();
    }

    IReason CreateReason (string name)
    {
      var reasonGroup = ModelDAOHelper.ModelFactory.CreateReasonGroup ();
      reasonGroup.Name = name;
      ModelDAOHelper.DAOFactory.ReasonGroupDAO.MakePersistent (reasonGroup);
      var reason = ModelDAOHelper.ModelFactory.CreateReason (reasonGroup);
      reason.Name = name;
      ModelDAOHelper.DAOFactory.ReasonDAO.MakePersistent (reason);
      return reason;
    }

    void AddReasonSelection (IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason)
    {
      var reasonSelection = ModelDAOHelper.ModelFactory
        .CreateReasonSelection (machineMode, machineObservationState);
      reasonSelection.Reason = reason;
      reasonSelection.Selectable = true;
      ModelDAOHelper.DAOFactory.ReasonSelectionDAO.MakePersistent (reasonSelection);
    }

    /// <summary>
    /// Create an observation state slot in the specified range.
    ///
    /// Note: the unit test database may already contain some observation state slots for this machine.
    ///       They are removed first because the observation state slots must not overlap,
    ///       else FindAt does not return a unique result
    /// </summary>
    void SetMachineObservationState (IMachine machine, UtcDateTimeRange range, IMachineObservationState machineObservationState)
    {
      foreach (var existingSlot in ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
        .FindOverlapsRange (machine, range)) {
        ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakeTransient (existingSlot);
      }
      var observationStateSlot = ModelDAOHelper.ModelFactory
        .CreateObservationStateSlot (machine, range);
      observationStateSlot.MachineObservationState = machineObservationState;
      ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent (observationStateSlot);
      ModelDAOHelper.DAOFactory.Flush ();
    }

    void AddManualReason (IMachine machine, UtcDateTimeRange range, IReason reason)
    {
      var modificationId = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
        .InsertManualReason (machine, range, reason, 100.0, null, null);
      ModelDAOHelper.DAOFactory.Flush ();
      var association = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
        .FindById (modificationId, machine);
      var reasonProposal = ModelDAOHelper.ModelFactory
        .CreateReasonProposal (association, range);
      ModelDAOHelper.DAOFactory.ReasonProposalDAO.MakePersistent (reasonProposal);
      ModelDAOHelper.DAOFactory.Flush ();
    }

    void AddFact (IMonitoredMachine machine, UtcDateTimeRange range, IMachineMode machineMode)
    {
      IFact fact = ModelDAOHelper.ModelFactory.CreateFact (machine, range.Lower.Value, range.Upper.Value, machineMode);
      ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
    }
  }
}
