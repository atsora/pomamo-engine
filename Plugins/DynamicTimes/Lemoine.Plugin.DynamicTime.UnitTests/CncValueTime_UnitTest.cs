// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

// Note: the tests are working with visual studio
// but not with nunit-console-runner
// The error is:
/*
Lemoine.Business.DynamicTimes.NoDynamicTime : No DynamicTime with name CncValueTimeAfterEnd
   at Lemoine.Business.DynamicTimes.DynamicTime.GetDynamicTime(String nameWithoutParameter, String parameter, IMachine machine, DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit, Boolean notApplicableExpected) in C:\Devel\pulse\pulsedotnet\Libraries\Pulse.Business\DynamicTimes\DynamicTime.cs:line 317
 */
// No idea why
//#define ENABLE_CNCVALUETIME_UNITTEST

using System;
using System.Collections.Generic;
using System.Linq;

using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.DynamicTimes;
using Pulse.Extensions;
using Lemoine.Extensions;
using Lemoine.Extensions.ExtensionsProvider;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  /// <summary>
  /// Description of SetupSwitcher_UnitTest.
  /// </summary>
  public class CncValueTime_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (NextCncValue_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncValueTime_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test 
    /// </summary>
#if ENABLE_CNCVALUETIME_UNITTEST
    [Test]
#endif
    public void TestBeforeEnd1 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue ("Extensions.AlternativePluginsDirectories", TestContext.CurrentContext.TestDirectory);

          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          IMachineModule machineModule = machine.MainMachineModule;
          IField field = ModelDAOHelper.DAOFactory.FieldDAO
            .FindById ((int)FieldId.DryRun);

          {
            Lemoine.Extensions.Package.PackageFile
              .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""CncValueTimeBeforeEnd_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""CncValueTime"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""NamePrefix"": ""CncValueTime"",
  ""FieldId"": 118,
  ""LambdaCondition"": ""(x) => !((bool)x)""
          }
        }
      ]
    }
  ]
}
", true, true);
            Lemoine.Extensions.ExtensionManager.Activate (false);
            Lemoine.Extensions.ExtensionManager.Load ();

            {
              var checker = new DynamicEndChecker ("CncValueTimeBeforeEnd", machine, T (0));

              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-5, -4), true);
              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-4, -3), false);
              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-2, -1), true);
              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-1, 1), true);
              checker.CheckFinal (T (-3));
            }

            {
              var checker = new DynamicEndChecker ("CncValueTimeBeforeEnd(2)", machine, T (0));
              checker.CheckFinal (T (-3));
            }
          }
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

#if ENABLE_CNCVALUETIME_UNITTEST
    [Test]
#endif
    public void TestBeforeEndNotApplicable ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue ("Extensions.AlternativePluginsDirectories", TestContext.CurrentContext.TestDirectory);

          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          IMachineModule machineModule = machine.MainMachineModule;
          IField field = ModelDAOHelper.DAOFactory.FieldDAO
            .FindById ((int)FieldId.DryRun);

          {
            Lemoine.Extensions.Package.PackageFile
              .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""CncValueTimeBeforeEnd_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""CncValueTime"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""NamePrefix"": ""CncValueTime"",
  ""FieldId"": 118,
  ""LambdaCondition"": ""(x) => !((bool)x)""
          }
        }
      ]
    }
  ]
}
", true, true);
            Lemoine.Extensions.ExtensionManager.Activate (false);
            Lemoine.Extensions.ExtensionManager.Load ();

            {
              var checker = new DynamicEndChecker ("CncValueTimeBeforeEnd", machine, T (0));

              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-5, -4), true);
              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-1, 1), false);
              checker.CheckNotApplicable ();
            }
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
    /// Test 
    /// </summary>
#if ENABLE_CNCVALUETIME_UNITTEST
    [Test]
#endif
    public void TestBeforeStart1 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue ("Extensions.AlternativePluginsDirectories", TestContext.CurrentContext.TestDirectory);

          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          IMachineModule machineModule = machine.MainMachineModule;
          IField field = ModelDAOHelper.DAOFactory.FieldDAO
            .FindById ((int)FieldId.DryRun);

          {
            Lemoine.Extensions.Package.PackageFile
              .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""CncValueTimeBeforeStart_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""CncValueTime"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""NamePrefix"": ""CncValueTime"",
  ""FieldId"": 118,
  ""LambdaCondition"": ""(x) => !((bool)x)""
          }
        }
      ]
    }
  ]
}
", true, true);
            Lemoine.Extensions.ExtensionManager.Activate (false);
            Lemoine.Extensions.ExtensionManager.Load ();

            {
              var checker = new DynamicEndChecker ("CncValueTimeBeforeStart", machine, T (0));

              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-6, -5), true);
              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-5, -4), false);
              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-4, -3), false);
              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-2, -1), true);
              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-1, 1), true);
              checker.CheckFinal (T (-5));
            }

            {
              var checker = new DynamicEndChecker ("CncValueTimeBeforeStart(2)", machine, T (0));
              checker.CheckFinal (T (-5));
            }
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
    /// Test 
    /// </summary>
#if ENABLE_CNCVALUETIME_UNITTEST
    [Test]
#endif
    public void TestBeforeStart2 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue ("Extensions.AlternativePluginsDirectories", TestContext.CurrentContext.TestDirectory);

          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          IMachineModule machineModule = machine.MainMachineModule;
          IField field = ModelDAOHelper.DAOFactory.FieldDAO
            .FindById ((int)FieldId.DryRun);

          {
            Lemoine.Extensions.Package.PackageFile
              .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""CncValueTimeBeforeStart_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""CncValueTime"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""NamePrefix"": ""CncValueTime"",
  ""FieldId"": 118,
  ""LambdaCondition"": ""(x) => !((bool)x)""
          }
        }
      ]
    }
  ]
}
", true, true);
            Lemoine.Extensions.ExtensionManager.Activate (false);
            Lemoine.Extensions.ExtensionManager.Load ();

            {
              var checker = new DynamicEndChecker ("CncValueTimeBeforeStart", machine, T (0));

              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-6, -5), true);
              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-2, -1), false);
              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-1, 1), false);
              checker.CheckFinal (T (-2));
            }

            {
              var checker = new DynamicEndChecker ("CncValueTimeBeforeStart(2)", machine, T (0));
              checker.CheckFinal (T (-2));
            }
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
    /// Test 
    /// </summary>
#if ENABLE_CNCVALUETIME_UNITTEST
    [Test]
#endif
    public void TestAfterStartEnd1 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue ("Extensions.AlternativePluginsDirectories", TestContext.CurrentContext.TestDirectory);

          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          IMachineModule machineModule = machine.MainMachineModule;
          IField field = ModelDAOHelper.DAOFactory.FieldDAO
            .FindById ((int)FieldId.DryRun);

          {
            Lemoine.Extensions.Package.PackageFile
              .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""CncValueTimeBeforeEnd_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""CncValueTime"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""NamePrefix"": ""CncValueTime"",
  ""FieldId"": 118,
  ""LambdaCondition"": ""(x) => !((bool)x)""
          }
        }
      ]
    }
  ]
}
", true, true);
            Lemoine.Extensions.ExtensionManager.Activate (false);
            Lemoine.Extensions.ExtensionManager.Load ();

            {
              var checker = new DynamicEndChecker ("CncValueTimeAfterStart", machine, T (0));

              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-2, -1), true);
              checker.CheckPending ();

              AddCncValue (machineModule, field, R (0, 1), true);
              checker.CheckAfter (T (1));

              AddCncValue (machineModule, field, R (2, 3), false);
              checker.CheckFinal (T (2));
            }

            {
              var checker = new DynamicEndChecker ("CncValueTimeAfterStart(2)", machine, T (0));
              checker.CheckFinal (T (2));
            }
          }
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

#if ENABLE_CNCVALUETIME_UNITTEST
    [Test]
#endif
    public void TestAfterStartNotApplicable ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue ("Extensions.AlternativePluginsDirectories", TestContext.CurrentContext.TestDirectory);

          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          IMachineModule machineModule = machine.MainMachineModule;
          IField field = ModelDAOHelper.DAOFactory.FieldDAO
            .FindById ((int)FieldId.DryRun);

          {
            Lemoine.Extensions.Package.PackageFile
              .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""CncValueTimeBeforeEnd_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""CncValueTime"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""NamePrefix"": ""CncValueTime"",
  ""FieldId"": 118,
  ""LambdaCondition"": ""(x) => !((bool)x)""
          }
        }
      ]
    }
  ]
}
", true, true);
            Lemoine.Extensions.ExtensionManager.Activate (false);
            Lemoine.Extensions.ExtensionManager.Load ();

            {
              var checker = new DynamicEndChecker ("CncValueTimeAfterStart", machine, T (0));

              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-5, -4), true);
              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-1, 1), false);
              checker.CheckNotApplicable ();
            }
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
    /// Test 
    /// </summary>
#if ENABLE_CNCVALUETIME_UNITTEST
    [Test]
#endif
    public void TestAfterEnd1 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue ("Extensions.AlternativePluginsDirectories", TestContext.CurrentContext.TestDirectory);

          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          IMachineModule machineModule = machine.MainMachineModule;
          IField field = ModelDAOHelper.DAOFactory.FieldDAO
            .FindById ((int)FieldId.DryRun);

          {
            Lemoine.Extensions.Package.PackageFile
              .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""CncValueTimeBeforeStart_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""CncValueTime"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""NamePrefix"": ""CncValueTime"",
  ""FieldId"": 118,
  ""LambdaCondition"": ""(x) => !((bool)x)""
          }
        }
      ]
    }
  ]
}
", true, true);
            Lemoine.Extensions.ExtensionManager.Activate (false);
            Lemoine.Extensions.ExtensionManager.Load ();

            {
              var checker = new DynamicEndChecker ("CncValueTimeAfterEnd", machine, T (0));

              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-2, -1), true);
              checker.CheckPending ();

              AddCncValue (machineModule, field, R (0, 1), true);
              checker.CheckAfter (T (1));

              AddCncValue (machineModule, field, R (2, 3), false);
              checker.CheckAfter (T (2));

              AddCncValue (machineModule, field, R (3, 4), false);
              checker.CheckAfter (T (3));

              AddCncValue (machineModule, field, R (4, 5), true);
              checker.CheckFinal (T (4));
            }

            {
              var checker = new DynamicEndChecker ("CncValueTimeAfterEnd(2)", machine, T (0));
              checker.CheckFinal (T (4));
            }
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
    /// Test 
    /// </summary>
#if ENABLE_CNCVALUETIME_UNITTEST
    [Test]
#endif
    public void TestAfterEnd2 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue ("Extensions.AlternativePluginsDirectories", TestContext.CurrentContext.TestDirectory);

          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          IMachineModule machineModule = machine.MainMachineModule;
          IField field = ModelDAOHelper.DAOFactory.FieldDAO
            .FindById ((int)FieldId.DryRun);

          {
            Lemoine.Extensions.Package.PackageFile
              .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""CncValueTimeBeforeStart_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""CncValueTime"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""NamePrefix"": ""CncValueTime"",
  ""FieldId"": 118,
  ""LambdaCondition"": ""(x) => !((bool)x)""
          }
        }
      ]
    }
  ]
}
", true, true);
            Lemoine.Extensions.ExtensionManager.Activate (false);
            Lemoine.Extensions.ExtensionManager.Load ();

            {
              var checker = new DynamicEndChecker ("CncValueTimeAfterEnd", machine, T (0));

              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-6, -5), true);
              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-2, -1), false);
              checker.CheckPending ();

              AddCncValue (machineModule, field, R (-1, 1), false);
              checker.CheckAfter (T (-1));

              AddCncValue (machineModule, field, R (1, 2), false);
              checker.CheckAfter (T (1));

              AddCncValue (machineModule, field, R (3, 4), true);
              checker.CheckFinal (T (2));
            }

            {
              var checker = new DynamicEndChecker ("CncValueTimeAfterEnd(2)", machine, T (0));
              checker.CheckFinal (T (2));
            }
          }
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    void AddCncValue (IMachineModule machineModule, IField field, UtcDateTimeRange range, object v)
    {
      var cncValue = ModelDAOHelper.ModelFactory
        .CreateCncValue (machineModule, field, range.Lower.Value);
      cncValue.End = range.Upper.Value;
      cncValue.Value = v;
      ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue);
    }

  }
}
