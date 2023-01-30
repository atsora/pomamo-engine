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
  /// 
  /// </summary>
  [TestFixture]
  [NonParallelizable] // Because of the use of static class CycleDetectionStatusExtension
  public class CycleIsProductionNextProductionEnd_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (NextCycleIsProductionStart_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CycleIsProductionNextProductionEnd_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestCycleIsProductionNextProductionEndFullCycle ()
    {
      var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
      var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
      Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
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
            .NextProductionEnd ();
          extension.SetTestConfiguration (@"
{
  ""MaximumDelay"": ""0:05:00""
}
");
          var initializeResult = extension.Initialize (machine, null);
          Assert.IsTrue (initializeResult);

          CheckPending (extension, T (0));

          CreateFullCycle (machine, operationSlot, T (0), T (1));
          CheckFinal (extension, T (0), T (1));
          CheckNoData (extension, T (0), R (0, 1));
          CheckFinal (extension, T (0), T (1), R (0, 1, "[]"));
          CheckFinal (extension, T (0), T (1), R (0, 2));

          CheckPending (extension, T (2));

          StartCycle (machine, operationSlot, T (3));
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (T (3));
          CheckNotApplicable (extension, T (2));
          CheckPending (extension, T (3));
        }
        finally {
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (null);
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    void StartCycle (IMachine machine, IOperationSlot operationSlot, DateTime start)
    {
      IOperationCycle operationCycle1 = ModelDAOHelper.ModelFactory
        .CreateOperationCycle (machine);
      operationCycle1.OperationSlot = operationSlot;
      operationCycle1.SetRealBegin (start);
      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle1);
    }

    void CreateFullCycle (IMachine machine, IOperationSlot operationSlot, DateTime start, DateTime end)
    {
      IOperationCycle operationCycle1 = ModelDAOHelper.ModelFactory
        .CreateOperationCycle (machine);
      operationCycle1.OperationSlot = operationSlot;
      operationCycle1.SetRealBegin (start);
      operationCycle1.SetRealEnd (end);
      operationCycle1.Full = true;
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
        Assert.IsTrue (response.Hint.Lower.HasValue);
        Assert.AreEqual (after, response.Hint.Lower.Value);
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
        var response = extension.Get (at, new UtcDateTimeRange (final), limit);
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
    }
  }
}
