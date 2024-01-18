// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.Extensions.Business.DynamicTimes;
using log4net;
using Pulse.Extensions;
using Lemoine.Extensions;
using Lemoine.Extensions.ExtensionsProvider;
using Pulse.Extensions.Extension;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  /// <summary>
  /// Description of SetupSwitcher_UnitTest.
  /// </summary>
  public class ActivityIsProductionNextProductionEnd_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (ActivityIsProductionNextProductionEnd_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ActivityIsProductionNextProductionEnd_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestActivityIsProductionNextProductionEnd1 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

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

          {
            var extension = new Lemoine.Plugin.ActivityIsProduction
              .NextProductionEnd ();
            extension.SetTestConfiguration (@"
{
  ""Manual"": false,
  ""NorManualNorAuto"": true
}
");
            var initializeResult = extension.Initialize (machine, "");
            Assert.That (initializeResult, Is.True);

            CheckPending (extension, T (0));

            AddFact (machine, R (0, 1), active);
            CheckAfter (extension, T (0), T (1));

            AddFact (machine, R (1, 2), machining);
            CheckAfter (extension, T (0), T (2));

            AddFact (machine, R (2, 3), manualActive);
            CheckFinal (extension, T (0), T (2));

            CheckNoData (extension, T (0), R (0, 1));
            CheckNoData (extension, T (0), R (0, 2));
            CheckFinal (extension, T (0), T (2), R (0, 2, "[]"));
            CheckFinal (extension, T (0), T (2), R (0, 3));
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
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestActivityIsProductionNextProductionEndGapAtLimit ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

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

          {
            var extension = new Lemoine.Plugin.ActivityIsProduction
              .NextProductionEnd ();
            extension.SetTestConfiguration (@"
{
  ""Manual"": false,
  ""NorManualNorAuto"": true
}
");
            var initializeResult = extension.Initialize (machine, "");
            Assert.That (initializeResult, Is.True);

            AddFact (machine, R (0, 1), active);
            AddFact (machine, R (1, 2), machining);
            AddFact (machine, R (3, 4), manualActive);
            CheckFinal (extension, T (0), T (2));

            CheckNoData (extension, T (0), R (0, 1));
            CheckNoData (extension, T (0), R (0, 2));
            CheckFinal (extension, T (0), T (2), R (0, 2, "[]"));
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
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestActivityIsProductionNextProductionEndInitialGap ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

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

          {
            var extension = new Lemoine.Plugin.ActivityIsProduction
              .NextProductionEnd ();
            extension.SetTestConfiguration (@"
{
  ""Manual"": false,
  ""NorManualNorAuto"": true
}
");
            var initializeResult = extension.Initialize (machine, "");
            Assert.That (initializeResult, Is.True);

            CheckPending (extension, T (0));

            AddFact (machine, R (1, 2), active);
            CheckNoData (extension, T (0));
            CheckNoData (extension, T (0), R (0, 1));
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
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestActivityIsProductionNextProductionEndGap ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

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

          {
            var extension = new Lemoine.Plugin.ActivityIsProduction
              .NextProductionEnd ();
            extension.SetTestConfiguration (@"
{
  ""Manual"": false,
  ""NorManualNorAuto"": true
}
");
            var initializeResult = extension.Initialize (machine, "");
            Assert.That (initializeResult, Is.True);

            CheckPending (extension, T (0));

            AddFact (machine, R (0, 1), active);
            CheckAfter (extension, T (0), T (1));

            AddFact (machine, R (2, 3), machining);
            CheckFinal (extension, T (0), T (1));

            CheckNoData (extension, T (0), R (0, 1));
            CheckFinal (extension, T (0), T (1), R (0, 2));
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
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestActivityIsProductionNextProductionEndNoProduction ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

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

          {
            var extension = new Lemoine.Plugin.ActivityIsProduction
              .NextProductionEnd ();
            extension.SetTestConfiguration (@"
{
  ""Manual"": false,
  ""NorManualNorAuto"": true
}
");
            var initializeResult = extension.Initialize (machine, "");
            Assert.That (initializeResult, Is.True);

            CheckPending (extension, T (0));

            AddFact (machine, R (0, 1), inactive);
            CheckNoData (extension, T (0));

            CheckNoData (extension, T (0), R (0, 1));
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
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestActivityIsProductionNextProductionEnd1Short ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
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

          {
            var extension = new Lemoine.Plugin.ActivityIsProduction
              .NextProductionEnd ();
            extension.SetTestConfiguration (@"
{
  ""Manual"": false,
  ""NorManualNorAuto"": true,
  ""IgnoreShortMachineModeIds"": [1, 4],
  ""IgnoreShortMaximumDuration"": ""0:02:00""
}
");
            var initializeResult = extension.Initialize (machine, "");
            Assert.That (initializeResult, Is.True);

            CheckPending (extension, T (0));

            AddFact (machine, R (0, 1), active);
            CheckAfter (extension, T (0), T (1));

            AddFact (machine, R (1, 2), machining);
            CheckAfter (extension, T (0), T (2));

            AddFact (machine, R (2, 3), manualActive);
            CheckAfter (extension, T (0), T (2));

            CheckNoData (extension, T (0), R (0, 1));
            CheckNoData (extension, T (0), R (0, 2));
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
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestActivityIsProductionNextProductionEndGapAtLimitShort ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
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
          IMachineMode noData = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.NoData);

          {
            var extension = new Lemoine.Plugin.ActivityIsProduction
              .NextProductionEnd ();
            extension.SetTestConfiguration (@"
{
  ""Manual"": false,
  ""NorManualNorAuto"": true,
  ""IgnoreShortMachineModeIds"": [1, 8],
  ""IgnoreShortMaximumDuration"": ""0:02:00""
}
");
            var initializeResult = extension.Initialize (machine, "");
            Assert.That (initializeResult, Is.True);

            AddFact (machine, R (0, 1), active);
            AddFact (machine, R (1, 2), machining);
            AddFact (machine, R (3, 4), manualActive);
            CheckFinal (extension, T (0), T (2));

            CheckNoData (extension, T (0), R (0, 1));
            CheckNoData (extension, T (0), R (0, 2));
            CheckFinal (extension, T (0), T (2), R (0, 2, "[]"));
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
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestActivityIsProductionNextProductionEndInitialGapShort ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
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

          {
            var extension = new Lemoine.Plugin.ActivityIsProduction
              .NextProductionEnd ();
            extension.SetTestConfiguration (@"
{
  ""Manual"": false,
  ""NorManualNorAuto"": true,
  ""IgnoreShortMachineModeIds"": [1, 8],
  ""IgnoreShortMaximumDuration"": ""0:02:00""
}
");
            var initializeResult = extension.Initialize (machine, "");
            Assert.That (initializeResult, Is.True);

            CheckPending (extension, T (0));

            AddFact (machine, R (1, 2), active);
            CheckAfter (extension, T (0), T (2));
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
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestActivityIsProductionNextProductionEndGapShort ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
          IMachineMode inactive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Inactive);
          IMachineMode hold = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Hold);
          IMachineMode active = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Active);
          IMachineMode manualActive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.ManualActive);
          IMachineMode autoFeed = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.AutoFeed);
          IMachineMode machining = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Machining);

          {
            var extension = new Lemoine.Plugin.ActivityIsProduction
              .NextProductionEnd ();
            extension.SetTestConfiguration (@"
{
  ""Manual"": false,
  ""NorManualNorAuto"": true,
  ""IgnoreShortMachineModeIds"": [1, 8],
  ""IgnoreShortMaximumDuration"": ""0:04:00""
}
");
            var initializeResult = extension.Initialize (machine, "");
            Assert.That (initializeResult, Is.True);

            CheckPending (extension, T (0));

            AddFact (machine, R (0, 1), active);
            CheckAfter (extension, T (0), T (1));

            AddFact (machine, R (2, 3), machining);
            CheckAfter (extension, T (0), T (3));

            AddFact (machine, R (3, 4), inactive);
            CheckAfter (extension, T (0), T (3));

            AddFact (machine, R (4, 5), hold);
            CheckAfter (extension, T (0), T (3));

            AddFact (machine, R (6, 7), machining);
            CheckAfter (extension, T (0), T (7));

            AddFact (machine, R (7, 8), hold);
            CheckAfter (extension, T (0), T (7));

            AddFact (machine, R (8, 15), inactive);
            CheckFinal (extension, T (0), T (7));

            CheckNoData (extension, T (0), R (0, 1));
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
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestActivityIsProductionNextProductionEndNoProductionShort ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
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

          {
            var extension = new Lemoine.Plugin.ActivityIsProduction
              .NextProductionEnd ();
            extension.SetTestConfiguration (@"
{
  ""Manual"": false,
  ""NorManualNorAuto"": true,
  ""IgnoreShortMachineModeIds"": [1, 8],
  ""IgnoreShortMaximumDuration"": ""0:02:00""
}
");
            var initializeResult = extension.Initialize (machine, "");
            Assert.That (initializeResult, Is.True);

            CheckPending (extension, T (0));

            AddFact (machine, R (0, 1), inactive);
            CheckPending (extension, T (0));

            AddFact (machine, R (1, 5), inactive);
            CheckNoData (extension, T (0));

            CheckNoData (extension, T (0), R (0, 1));
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


    void CheckPending (IDynamicTimeExtension extension, DateTime dateTime)
    {
      var response = extension.Get (dateTime, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.Multiple (() => {
        Assert.That (response.Hint.Lower.HasValue, Is.False);
        Assert.That (response.Hint.Upper.HasValue, Is.False);
        Assert.That (response.Final.HasValue, Is.False);
      });
    }

    void CheckAfter (IDynamicTimeExtension extension, DateTime dateTime, DateTime after)
    {
      {
        var response = extension.Get (dateTime, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
      {
        var response = extension.Get (dateTime, new UtcDateTimeRange (after), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
    }

    void CheckFinal (IDynamicTimeExtension extension, DateTime dateTime, DateTime final)
    {
      {
        var response = extension.Get (dateTime, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      {
        var response = extension.Get (dateTime, new UtcDateTimeRange (final.Subtract (TimeSpan.FromSeconds (1))), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
    }

    void CheckNoData (IDynamicTimeExtension extension, DateTime dateTime)
    {
      var response = extension.Get (dateTime, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.That (response.NoData, Is.True);
    }

    void CheckNoData (IDynamicTimeExtension extension, DateTime at, UtcDateTimeRange limit)
    {
      var response = extension.Get (at, R (0), limit);
      Assert.That (response.NoData, Is.True);
    }

    void CheckFinal (IDynamicTimeExtension extension, DateTime at, DateTime final, UtcDateTimeRange limit)
    {
      {
        var response = extension.Get (at, R (0), limit);
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      if (T (1) < final) {
        var response = extension.Get (at, R (1), limit);
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      {
        var response = extension.Get (at, new UtcDateTimeRange (final), limit);
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
    }
  }
}
