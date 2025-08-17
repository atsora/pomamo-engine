// Copyright (C) 2025 Atsora Solutions

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
using Lemoine.Plugin.IfApplicableDynamicTime;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  class DynamicEndExtensionIfApplicable
    : Lemoine.UnitTests.WithMinuteTimeStamp
    , Lemoine.Extensions.Business.DynamicTimes.IDynamicTimeExtension
  {
    static int s_step = 0;

    public static void ResetStep ()
    {
      s_step = 0;
    }

    public DynamicEndExtensionIfApplicable ()
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

    public string Name => "RunIfApplicable";

    public bool UniqueInstance => true;

    public bool IsApplicable () => true;

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime at) => throw new NotImplementedException ();

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      return s_step switch {
        0 => this.CreateWithHint (R (13)),
        _ => this.CreateNotApplicable (),
      };
    }

    public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
    {
      return TimeSpan.FromTicks (0);
    }

    public static void GoToNextStep ()
    {
      s_step++;
    }

    public static void Reset ()
    {
      s_step = 0;
    }
  }

  /// <summary>
  /// Description of SetupSwitcher_UnitTest.
  /// </summary>
  public class IfApplicable_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (IfApplicable_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public IfApplicable_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test
    /// </summary>
    [Test]
    public void TestIfApplicableNextActiveMachineModeInitialGap ()
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

          Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtensionIfApplicable));
          Lemoine.Extensions.ExtensionManager.Add (typeof (IfApplicable));

          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);

          {
            var response = Lemoine.Business.DynamicTimes.DynamicTime.GetDynamicTime ("IfApplicable(RunIfApplicable)", machine, T(0));
            Assert.That (response.NoData, Is.False);
          }
          DynamicEndExtensionIfApplicable.GoToNextStep ();
          {
            var response = Lemoine.Business.DynamicTimes.DynamicTime.GetDynamicTime ("IfApplicable(RunIfApplicable)", machine, T (0));
            Assert.That (response.Final, Is.EqualTo (T (0)));
          }
          DynamicEndExtensionIfApplicable.ResetStep ();

          {
            var extension = new Lemoine.Plugin.IfApplicableDynamicTime.IfApplicable ();
            var initializeResult = extension.Initialize (machine, "RunIfApplicable");
            Assert.That (initializeResult, Is.True);

            CheckAfter (extension, T (0));
            DynamicEndExtensionIfApplicable.GoToNextStep ();
            CheckFinal (extension, T (0));
          }
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    void CheckPending (IDynamicTimeExtension extension)
    {
      var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
      Assert.Multiple (() => {
        Assert.That (response.Hint.Lower.HasValue, Is.False);
        Assert.That (response.Hint.Upper.HasValue, Is.False);
        Assert.That (response.Final.HasValue, Is.False);
      });
    }

    void CheckPending (IMachine machine)
    {
      var response = Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime ("NextActiveMachineMode", machine, T (0));
      Assert.Multiple (() => {
        Assert.That (response.Hint.Lower.HasValue, Is.False);
        Assert.That (response.Hint.Upper.HasValue, Is.False);
        Assert.That (response.Final.HasValue, Is.False);
      });
    }

    void CheckAfter (IDynamicTimeExtension extension, DateTime after)
    {
      {
        var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
      if (T (1) < after) {
        var response = extension.Get (T (0), R (1), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
      {
        var response = extension.Get (T (0), new UtcDateTimeRange (after), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
    }

    void CheckAfter (IMachine machine, DateTime after)
    {
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextActiveMachineMode", machine, T (0));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
      if (T (1) < after) {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextActiveMachineMode", machine, T (0), R (1), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextActiveMachineMode", machine, T (0), new UtcDateTimeRange (after), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
    }

    void CheckFinal (IDynamicTimeExtension extension, DateTime final)
    {
      {
        var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      if (T (1) < final) {
        var response = extension.Get (T (0), R (1), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      {
        var response = extension.Get (T (0), new UtcDateTimeRange (final), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
    }

    void CheckFinal (IMachine machine, DateTime final)
    {
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextActiveMachineMode", machine, T (0));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      if (T (1) < final) {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextActiveMachineMode", machine, T (0), R (1), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextActiveMachineMode", machine, T (0), new UtcDateTimeRange (final), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
    }

    void CheckNoData (IDynamicTimeExtension extension, UtcDateTimeRange limit)
    {
      var response = extension.Get (T (0), R (0), limit);
      Assert.That (response.NoData, Is.True);
    }

    void CheckFinal (IDynamicTimeExtension extension, DateTime final, UtcDateTimeRange limit)
    {
      {
        var response = extension.Get (T (0), R (0), limit);
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      if (T (1) < final) {
        var response = extension.Get (T (0), R (1), limit);
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      {
        var response = extension.Get (T (0), new UtcDateTimeRange (final), limit);
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
    }

    void CheckNoData (IDynamicTimeExtension extension)
    {
      var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
      Assert.That (response.NoData, Is.True);
    }

    void CheckNotApplicable (IDynamicTimeExtension extension)
    {
      var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
      Assert.That (response.NotApplicable, Is.True);
    }
  }
}
