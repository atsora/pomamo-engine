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
using Lemoine.Extensions.Business.DynamicTimes;
using Pulse.Extensions;
using Lemoine.Extensions;
using Lemoine.Extensions.ExtensionsProvider;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  class DynamicEndExtensionRedirect
    : Lemoine.UnitTests.WithMinuteTimeStamp
    , Lemoine.Extensions.Business.DynamicTimes.IDynamicTimeExtension
  {
    static int s_step = 0;

    public static void ResetStep ()
    {
      s_step = 0;
    }

    public DynamicEndExtensionRedirect ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    {
    }

    public bool Initialize (IMachine machine, string parameter)
    {
      this.Machine = machine;
      return true;
    }

    public IMachine Machine
    {
      get; set;
    }

    public string Name
    {
      get
      {
        return "Redirect";
      }
    }

    public bool UniqueInstance
    {
      get
      {
        return true;
      }
    }

    public bool IsApplicable ()
    {
      return true;
    }

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime at)
    {
      throw new NotImplementedException ();
    }

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      var step = s_step++;
      return step switch {
        0 => this.CreateWithHint (R (13)),
        _ => this.CreateFinal (T (15)),
      };
    }

    public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
    {
      return TimeSpan.FromTicks (0);
    }

    public static void Reset ()
    {
      s_step = 0;
    }
  }


  class DynamicEndExtensionAfter
    : Lemoine.UnitTests.WithMinuteTimeStamp
    , Lemoine.Extensions.Business.DynamicTimes.IDynamicTimeExtension
  {
    static int s_step = 0;

    public static void ResetStep ()
    {
      s_step = 0;
    }

    public DynamicEndExtensionAfter ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    {
    }

    public bool Initialize (IMachine machine, string parameter)
    {
      this.Machine = machine;
      return true;
    }

    public IMachine Machine
    {
      get; set;
    }

    public string Name
    {
      get
      {
        return "After";
      }
    }

    public bool UniqueInstance
    {
      get
      {
        return true;
      }
    }

    public bool IsApplicable ()
    {
      return true;
    }

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime at)
    {
      throw new NotImplementedException ();
    }

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      if (T (2) <= dateTime) {
        return this.CreateNotApplicable ();
      }
      else {
        var step = s_step++;
        return step switch {
          0 => this.CreateWithHint (R (1)),
          _ => this.CreateFinal (T (2)),
        };
      }
    }

    public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
    {
      return TimeSpan.FromTicks (0);
    }

    public static void Reset ()
    {
      s_step = 0;
    }
  }


  /// <summary>
  /// 
  /// </summary>
  public class AfterDynamicTime_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (AfterDynamicTime_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AfterDynamicTime_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test1 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        session.ForceUniqueSession ();
        try {
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

          Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtensionRedirect));
          Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtensionAfter));

          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);

          Lemoine.Extensions.Package.PackageFile
            .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""AfterDynamicTime_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""AfterDynamicTime"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""Name"": ""A"",
  ""RedirectName"": ""Redirect"",
  ""AfterName"": ""After""
          }
        }
      ]
    }
  ]
}
", true, true);
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          CheckPending ("A", machine, T (0));
          CheckWithHint ("A", machine, T (0), R (13));
          CheckFinal ("A", machine, T (0), T (15));
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
          DynamicEndExtensionAfter.ResetStep ();
          DynamicEndExtensionRedirect.ResetStep ();
        }
      }
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestAfterNotApplicableOk ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        session.ForceUniqueSession ();
        try {
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

          Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtensionRedirect));
          Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtensionAfter));

          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);

          Lemoine.Extensions.Package.PackageFile
            .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""AfterDynamicTime_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""AfterDynamicTime"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""Name"": ""A"",
  ""RedirectName"": ""Redirect"",
  ""AfterName"": ""After""
          }
        }
      ]
    }
  ]
}
", true, true);
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          CheckWithHint ("A", machine, T (10), R (13));
          CheckFinal ("A", machine, T (10), T (15));
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
          DynamicEndExtensionAfter.ResetStep ();
          DynamicEndExtensionRedirect.ResetStep ();
        }
      }
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestAfterNotApplicableKo ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        session.ForceUniqueSession ();
        try {
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

          Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtensionRedirect));
          Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtensionAfter));

          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);

          Lemoine.Extensions.Package.PackageFile
            .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""AfterDynamicTime_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""AfterDynamicTime"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""Name"": ""A"",
  ""RedirectName"": ""Redirect"",
  ""AfterName"": ""After"",
  ""AfterNotApplicableOk"": false
          }
        }
      ]
    }
  ]
}
", true, true);
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          CheckNotApplicable ("A", machine, T (10));
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
          DynamicEndExtensionAfter.ResetStep ();
          DynamicEndExtensionRedirect.ResetStep ();
        }
      }
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestAfterMaxDuration ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        session.ForceUniqueSession ();
        try {
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

          Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtensionRedirect));
          Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtensionAfter));

          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);

          Lemoine.Extensions.Package.PackageFile
            .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""AfterDynamicTime_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""AfterDynamicTime"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""Name"": ""A"",
  ""RedirectName"": ""Redirect"",
  ""AfterName"": ""After"",
  ""AfterMaxDuration"": ""0:00:01""
          }
        }
      ]
    }
  ]
}
", true, true);
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          CheckNotApplicable ("A", machine, T (0));
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
          DynamicEndExtensionAfter.ResetStep ();
          DynamicEndExtensionRedirect.ResetStep ();
        }
      }
    }

    void CheckPending (string name, IMachine machine, DateTime dateTime)
    {
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime);
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.False);
          Assert.That (response.NotApplicable, Is.False);
          Assert.That (response.NoData, Is.False);
          Assert.That (response.Hint, Is.EqualTo (new UtcDateTimeRange ("(,)")));
        });
      }
    }

    void CheckWithHint (string name, IMachine machine, DateTime dateTime, UtcDateTimeRange hint)
    {
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime);
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.False);
          Assert.That (response.NotApplicable, Is.False);
          Assert.That (response.NoData, Is.False);
          Assert.That (response.Hint, Is.EqualTo (hint));
        });
      }
    }

    void CheckFinal (string name, IMachine machine, DateTime dateTime, DateTime final)
    {
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime);
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime, new UtcDateTimeRange (final.Subtract (TimeSpan.FromSeconds (1))), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
    }

    void CheckNotApplicable (string name, IMachine machine, DateTime dateTime)
    {
      var response = Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime (name, machine, dateTime);
      Assert.Multiple (() => {
        Assert.That (response.NoData, Is.True);
        Assert.That (response.NotApplicable, Is.True);
      });
    }
  }
}
