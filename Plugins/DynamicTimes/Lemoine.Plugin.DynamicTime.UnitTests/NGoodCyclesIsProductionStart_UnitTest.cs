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
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Extensions.Plugin;
using Pulse.Extensions.Extension;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  /// <summary>
  /// Description of SetupSwitcher_UnitTest.
  /// </summary>
  public class NGoodCyclesIsProductionStart_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (NGoodCyclesIsProductionStart_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NGoodCyclesIsProductionStart_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
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
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          IOperation operation = ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (1); // MachiningDuration: 3600s=60min, no loading duration
          operation.LoadingDuration = TimeSpan.FromMinutes (10);
          ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);

          // Existing operation slots and machine state templates
          {
            var association = ModelDAOHelper.ModelFactory
              .CreateOperationMachineAssociation (machine, R (0, null));
            association.Operation = operation;
            association.Apply ();
          }
          ModelDAOHelper.DAOFactory.Flush ();
          IOperationSlot operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindOverlapsRange (machine, R (0, null))
            .First ();

          Lemoine.Extensions.Package.PackageFile
            .InstallOrUpgradeJsonString (@"
{
  ""Identifier"": ""NGoodCyclesIsProduction_UnitTest"",
  ""Name"": ""UnitTest"",
  ""Description"": """",
  ""Tags"": [],
  ""Version"": 1,
  ""Plugins"": [
    {
      ""Name"": ""NGoodCyclesIsProduction"",
      ""Instances"": [
        {
          ""Name"": ""Test"",
          ""Parameters"": {
  ""NumberOfGoodCycles"": 2,
  ""MaxMachiningDurationMultiplicator"": 1.1,
  ""MaxLoadingDurationMultiplicator"": 1.5
          }
        }
      ]
    }
  ]
}
", true, true);
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
          var pluginsLoader = new PluginsLoader (assemblyLoader);
          var nhibernatePluginsLoader = new DummyPluginsLoader ();
          var extensionsProvider = new ExtensionsProvider (ModelDAOHelper.DAOFactory, pluginFilter, Pulse.Extensions.Business.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);

          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          var configuration = @"
{
  ""NumberOfGoodCycles"": 2,
  ""MaxMachiningDurationMultiplicator"": 1.1,
  ""MaxLoadingDurationMultiplicator"": 1.5
}
";
          var extension = new Lemoine.Plugin.NGoodCyclesIsProduction
            .NextProductionStart ();
          extension.SetTestConfiguration (configuration);
          var initializeResult = extension.Initialize (machine, null);
          Assert.That (initializeResult, Is.True);

          CheckAfter (extension, T (0));

          var operationCycle1 = StartCycle (machine, operationSlot, T (0));
          CheckAfter (extension, T (0));
          StopCycle (operationCycle1, T (90)); // Too Long
          CheckAfter (extension, T (90));

          var operationCycle2 = StartCycle (machine, operationSlot, T (100));
          CheckAfter (extension, T (100));
          StopCycle (operationCycle2, T (161)); // ok
          CheckAfter (extension, T (100));

          // Between cycle too long
          var operationCycle3 = StartCycle (machine, operationSlot, T (200));
          CheckAfter (extension, T (200));
          StopCycle (operationCycle3, T (259)); // ok
          CheckAfter (extension, T (200));

          var operationCycle4 = StartCycle (machine, operationSlot, T (270));
          CheckAfter (extension, T (200));
          StopCycle (operationCycle4, T (332)); // ok
          CheckFinal (extension, T (200));

          CheckNoData (extension, R (0, 95));
          CheckNoData (extension, R (0, 150));
          CheckFinal (extension, T (200), R (0, 201));
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

    IOperationCycle StartCycle (IMachine machine, IOperationSlot operationSlot, DateTime start)
    {
      IOperationCycle operationCycle = ModelDAOHelper.ModelFactory
        .CreateOperationCycle (machine);
      operationCycle.OperationSlot = operationSlot;
      operationCycle.SetRealBegin (start);
      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle);
      ModelDAOHelper.DAOFactory.Flush ();
      return operationCycle;
    }

    void StopCycle (IOperationCycle operationCycle, DateTime dateTime)
    {
      operationCycle.SetRealEnd (dateTime);
      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle);
      ModelDAOHelper.DAOFactory.Flush ();
    }
  }
}
