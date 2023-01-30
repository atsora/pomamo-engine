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
  public class NextCycleIsProductionStart_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (NextCycleIsProductionStart_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NextCycleIsProductionStart_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test ()
    {
      var cycleStart = T (1);

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
            .NextProductionStart ();
          extension.SetTestConfiguration (@"
{
  ""MaximumDelay"": ""0:05:00""
}
");
          var initializeResult = extension.Initialize (machine, null);
          Assert.IsTrue (initializeResult);

          {
            var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
            Assert.IsFalse (response.Hint.Lower.HasValue);
            Assert.IsFalse (response.Hint.Upper.HasValue);
            Assert.IsFalse (response.Final.HasValue);
          }

          var cycle = StartCycle (machine, operationSlot, cycleStart);
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (T (2));
          {
            var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
            Assert.IsTrue (response.Final.HasValue);
            Assert.AreEqual (cycleStart, response.Final.Value);
          }

          CycleDetectionStatusExtension.SetCycleDetectionDateTime (T (3));
          CheckNotApplicable (extension, T (2));

          cycle.SetRealEnd (T (3)); // Cycle 1 - 3
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (cycle);
          CheckNotApplicable (extension, T (2));
        }
        finally {
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (null);
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
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

    void CheckNotApplicable (IDynamicTimeExtension extension, DateTime dateTime)
    {
      var response = extension.Get (dateTime, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.IsTrue (response.NotApplicable);
    }
  }
}
