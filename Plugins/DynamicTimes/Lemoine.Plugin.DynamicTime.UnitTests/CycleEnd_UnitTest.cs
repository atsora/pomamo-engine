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
using Lemoine.Extensions;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Extensions.Database;
using Pulse.Extensions.Database;
using Lemoine.Extensions.ExtensionsProvider;
using Pulse.Extensions;
using Pulse.Database.ConnectionInitializer;
using Lemoine.GDBMigration;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  /// <summary>
  /// Description of SetupSwitcher_UnitTest.
  /// </summary>
  public class CycleEnd_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    class CycleDetectionStatusExtension
      : ICycleDetectionStatusExtension
      , IOperationDetectionStatusExtension
    {
      public bool UniqueInstance
      {
        get
        {
          return true;
        }
      }

      public int CycleDetectionStatusPriority
      {
        get { return 1; }
      }

      public int OperationDetectionStatusPriority
      {
        get { return 1; }
      }

      public DateTime? GetCycleDetectionDateTime ()
      {
        return new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc)
          .AddMinutes (100);
      }

      public DateTime? GetOperationDetectionDateTime ()
      {
        return new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc)
          .AddMinutes (100);
      }

      public bool Initialize (IMachine machine)
      {
        return true;
      }
    }

    readonly ILog log = LogManager.GetLogger (typeof (CycleEnd_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CycleEnd_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test ()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
      var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
      var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
      Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);

     extensionsProvider.Add (typeof (CycleDetectionStatusExtension));

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

          var extension = new Lemoine.Plugin.DynamicTimesCycle
            .CycleEnd ();
          var initializeResult = extension.Initialize (machine, null);
          Assert.That (initializeResult, Is.True);

          CheckNoData (extension, T (0));
          CheckPending (extension, T (200));

          var cycle1 = StartCycle (machine, operationSlot, T (1));
          CheckNoData (extension, T (0));
          CheckPending (extension, T (1));
          CheckPending (extension, T (100));
          CheckPending (extension, T (200));

          EndCycle (cycle1, T (2));
          CheckNoData (extension, T (0));
          CheckFinal (extension, T (1), T (2));
          CheckNoData (extension, T (1), R (1, 2));
          CheckFinal (extension, T (1), T (2), R (1, 2, "[]"));
          CheckFinal (extension, T (1), T (2), R (1, 3));
          // CheckFinal (extension, T (2), T (2)); // cycle1 is considered finishing strictly before T(2)
          CheckNoData (extension, T (3));

          var cycle2 = StartCycle (machine, operationSlot, T (10));
          CheckNoData (extension, T (0));
          CheckFinal (extension, T (1), T (2));
          CheckNoData (extension, T (1), R (1, 2));
          CheckFinal (extension, T (1), T (2), R (1, 2, "[]"));
          CheckFinal (extension, T (1), T (2), R (1, 3));
          CheckNoData (extension, T (3));
          CheckPending (extension, T (10));

          SetEstimatedEnd (cycle2, T (20));
          CheckNoData (extension, T (0));
          CheckFinal (extension, T (1), T (2));
          CheckNoData (extension, T (1), R (1, 2));
          CheckFinal (extension, T (1), T (2), R (1, 2, "[]"));
          CheckFinal (extension, T (1), T (2), R (1, 3));
          CheckNoData (extension, T (3));
          CheckPending (extension, T (10));

          var cycle3 = StartCycle (machine, operationSlot, T (20));
          CheckNoData (extension, T (0));
          CheckFinal (extension, T (1), T (2));
          CheckNoData (extension, T (1), R (1, 2));
          CheckFinal (extension, T (1), T (2), R (1, 2, "[]"));
          CheckFinal (extension, T (1), T (2), R (1, 3));
          CheckNoData (extension, T (3));
          CheckFinal (extension, T (10), T (20));
          CheckNoData (extension, T (10), R (10, 20));
          CheckFinal (extension, T (10), T (20), R (10, 20, "[]"));
          CheckFinal (extension, T (10), T (20), R (10, 30));
          CheckFinal (extension, T (19), T (20));
          CheckNoData (extension, T (19), R (19, 20));
          CheckFinal (extension, T (19), T (20), R (19, 20, "[]"));
          CheckFinal (extension, T (19), T (20), R (19, 30));
          CheckPending (extension, T (20));

          SetEstimatedEnd (cycle3, T (30));
          CheckNoData (extension, T (0));
          CheckFinal (extension, T (1), T (2));
          CheckNoData (extension, T (3));
          CheckFinal (extension, T (10), T (20));
          CheckFinal (extension, T (19), T (20));
          CheckPending (extension, T (20));

          var cycle4 = CreateCycleEstimatedStart (machine, operationSlot, T (30), T (150));
          CheckPending (extension, T (20));
          CheckFinal (extension, T (120), T (150));
          CheckNoData (extension, T (120), R (120, 150));
          CheckFinal (extension, T (120), T (150), R (120, 150, "[]"));
          CheckFinal (extension, T (120), T (150), R (120, 160));

          Merge (cycle3, cycle4);
          CheckFinal (extension, T (20), T (150));
          CheckNoData (extension, T (20), R (20, 150));
          CheckFinal (extension, T (20), T (150), R (20, 150, "[]"));
          CheckFinal (extension, T (20), T (150), R (20, 160));
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    IOperationCycle StartCycle (IMachine machine, IOperationSlot operationSlot, DateTime start)
    {
      IOperationCycle operationCycle1 = ModelDAOHelper.ModelFactory
        .CreateOperationCycle (machine);
      operationCycle1.OperationSlot = operationSlot;
      operationCycle1.SetRealBegin (start);
      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle1);
      return operationCycle1;
    }

    IOperationCycle CreateCycleEstimatedStart (IMachine machine, IOperationSlot operationSlot, DateTime start, DateTime end)
    {
      IOperationCycle operationCycle1 = ModelDAOHelper.ModelFactory
        .CreateOperationCycle (machine);
      operationCycle1.OperationSlot = operationSlot;
      operationCycle1.SetEstimatedBegin (start);
      operationCycle1.SetRealEnd (end);
      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle1);
      return operationCycle1;
    }

    IOperationCycle Merge (IOperationCycle operationCycle1, IOperationCycle operationCycle2)
    {
      var end2 = operationCycle2.End;
      var status2 = operationCycle2.Status;

      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakeTransient (operationCycle2);

      if (end2.HasValue) {
        if (status2.HasFlag (OperationCycleStatus.EndEstimated)) {
          operationCycle1.SetEstimatedEnd (end2);
        }
        else {
          operationCycle1.SetRealEnd (end2.Value);
        }
      }
      else {
        operationCycle1.SetEstimatedEnd (null);
      }
      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle1);

      return operationCycle1;
    }

    void EndCycle (IOperationCycle cycle, DateTime end)
    {
      cycle.SetRealEnd (end);
      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (cycle);
    }

    void SetEstimatedEnd (IOperationCycle cycle, DateTime end)
    {
      cycle.SetEstimatedEnd (end);
      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (cycle);
    }

    void CheckNoData (IDynamicTimeExtension extension, DateTime at)
    {
      var response = extension.Get (at, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.That (response.NoData, Is.True);
    }

    void CheckPending (IDynamicTimeExtension extension, DateTime at)
    {
      var response = extension.Get (at, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.Multiple (() => {
        Assert.That (response.Hint.Lower.HasValue, Is.False);
        Assert.That (response.Hint.Upper.HasValue, Is.False);
        Assert.That (response.Final.HasValue, Is.False);
      });
    }

    void CheckAfter (IDynamicTimeExtension extension, DateTime at, DateTime after)
    {
      {
        var response = extension.Get (at, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
      {
        var response = extension.Get (at, new UtcDateTimeRange (after), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.False);
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
    }

    void CheckFinal (IDynamicTimeExtension extension, DateTime at, DateTime final)
    {
      {
        var response = extension.Get (at, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      {
        var response = extension.Get (at, new UtcDateTimeRange (final.Subtract (TimeSpan.FromSeconds (1))), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
    }

    void CheckNoData (IDynamicTimeExtension extension, DateTime at, UtcDateTimeRange limit)
    {
      var response = extension.Get (at, R (0), limit);
      Assert.That (response.NoData || (response.Final.HasValue && !limit.ContainsElement (response.Final.Value)), Is.True);
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
