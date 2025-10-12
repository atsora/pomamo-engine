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
using Lemoine.Extensions.Plugin;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  class DynamicEndExtensionB
    : Lemoine.UnitTests.WithMinuteTimeStamp
    , Lemoine.Extensions.Business.DynamicTimes.IDynamicTimeExtension
  {
    static int s_step = 0;

    public DynamicEndExtensionB ()
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
      get {
        return "B";
      }
    }

    public bool UniqueInstance
    {
      get {
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
        _ => this.CreateFinal (T (13)),
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


  /// <summary>
  /// 
  /// </summary>
  public class ApplicableAtMachineStateShift_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplicableAtMachineStateShift_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ApplicableAtMachineStateShift_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        session.ForceUniqueSession ();
        try {
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new DummyPluginsLoader ();
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);

          Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicEndExtensionB));

          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);
          var breakTime = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Break);
          var unattended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Unattended);
          IShift shift1 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
          IShift shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);

          {
            var association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine, attended, R (0, 1));
            association.Shift = shift2;
            association.Apply ();
          }
          {
            var association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine, attended, R (10, 13));
            association.Shift = shift1;
            association.Apply ();
          }
          {
            var association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine, breakTime, R (11, 12));
            association.Shift = shift1;
            association.Apply ();
          }

          ModelDAOHelper.DAOFactory.Flush ();
          var observationStateSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindAll (machine);
          var observationStateSlotAt0 = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindAt (machine, T (0));
          var observationStateSlotAtAsync0 = System.Threading.Tasks.Task.Run (async () => await ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindAtAsync (machine, T (0))).GetAwaiter ().GetResult ();

          Lemoine.Extensions.Package.PackageFile
            .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""ApplicableAtMachineStateShift_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""ApplicableAtMachineStateShift"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""Name"": ""A"",
  ""RedirectName"": ""B""
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
          CheckNotApplicable ("A", machine, T (1));
          CheckNotApplicable ("A", machine, T (2));
          CheckFinal ("A", machine, T (10), T (13));
          CheckFinal ("A", machine, T (11), T (13));
          CheckFinal ("A", machine, T (12), T (13));
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
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
