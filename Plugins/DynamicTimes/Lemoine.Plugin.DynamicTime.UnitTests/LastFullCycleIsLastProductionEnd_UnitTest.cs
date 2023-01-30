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
using Lemoine.Extensions.ExtensionsProvider;
using Pulse.Extensions.Extension;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  /// <summary>
  /// Description of SetupSwitcher_UnitTest.
  /// </summary>
  [TestFixture]
  [NonParallelizable] // Because of the use of static class CycleDetectionStatusExtension
  public class LastFullCycleIsLastProductionEnd_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (LastFullCycleIsLastProductionEnd_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public LastFullCycleIsLastProductionEnd_UnitTest ()
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
          var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
          var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);

          extensionsProvider.Add (typeof (CycleDetectionStatusExtension));

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

          var extension = new Lemoine.Plugin.CycleIsProduction
            .LastProductionEnd ();
          extension.SetTestConfiguration (@"
{
}
");
          var initializeResult = extension.Initialize (machine, null);
          Assert.IsTrue (initializeResult);

          CycleDetectionStatusExtension.SetCycleDetectionDateTime (T (100));

          CheckNoData (extension, T (80));

          CreateCycle (machine, operationSlot, R (10, 20));
          CheckNotApplicable (extension, T(15));
          CheckFinal (extension, T (80), T (20));
          CheckNoData (extension, T (80), R (70));
          CheckFinal (extension, T (80), T (20), R (19));

          CheckPending (extension, T (180));
        }
        finally {
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (null);
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    void CreateCycle (IMachine machine, IOperationSlot operationSlot, UtcDateTimeRange range)
    {
      IOperationCycle operationCycle1 = ModelDAOHelper.ModelFactory
        .CreateOperationCycle (machine);
      operationCycle1.OperationSlot = operationSlot;
      operationCycle1.SetRealBegin (range.Lower.Value);
      operationCycle1.SetRealEnd (range.Upper.Value);
      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle1);
    }

    void CheckPending (IDynamicTimeExtension extension, DateTime dateTime)
    {
      var response = extension.Get (dateTime, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.IsFalse (response.Hint.Lower.HasValue);
      Assert.IsFalse (response.Hint.Upper.HasValue);
      Assert.IsFalse (response.Final.HasValue);
    }

    void CheckAfter (IDynamicTimeExtension extension, DateTime dateTime, DateTime after)
    {
      {
        var response = extension.Get (dateTime, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Hint.Lower.HasValue);
        Assert.AreEqual (after, response.Hint.Lower.Value);
        Assert.IsFalse (response.Final.HasValue);
      }
      {
        var response = extension.Get (dateTime, new UtcDateTimeRange (after), new UtcDateTimeRange ("(,)"));
        Assert.IsFalse (response.Hint.Lower.HasValue);
        Assert.IsFalse (response.Final.HasValue);
      }
    }

    void CheckFinal (IDynamicTimeExtension extension, DateTime dateTime, DateTime final)
    {
      {
        var response = extension.Get (dateTime, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      {
        var response = extension.Get (dateTime, new UtcDateTimeRange (final.Subtract (TimeSpan.FromSeconds (1))), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
    }

    void CheckNoData (IDynamicTimeExtension extension, DateTime dateTime)
    {
      var response = extension.Get (dateTime, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.IsTrue (response.NoData);
    }


    void CheckNotApplicable (IDynamicTimeExtension extension, DateTime dateTime)
    {
      var response = extension.Get (dateTime, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.IsTrue (response.NotApplicable);
    }

    void CheckNoData (IDynamicTimeExtension extension, DateTime at, UtcDateTimeRange limit)
    {
      var response = extension.Get (at, R (0), limit);
      Assert.IsTrue (response.NoData || (response.Final.HasValue && !limit.ContainsElement (response.Final.Value)));
    }

    void CheckFinal (IDynamicTimeExtension extension, DateTime at, DateTime final, UtcDateTimeRange limit)
    {
      {
        var response = extension.Get (at, R (0), limit);
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      if (T (1) < final) {
        var response = extension.Get (at, R (1), limit);
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      {
        var response = extension.Get (at, new UtcDateTimeRange (final.Subtract (TimeSpan.FromSeconds (1))), limit);
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
    }
  }
}
