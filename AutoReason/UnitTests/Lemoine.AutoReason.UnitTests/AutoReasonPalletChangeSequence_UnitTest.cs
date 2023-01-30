// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.GDBPersistentClasses;
using Lemoine.Plugin.AutoReasonPalletChangeSequence;
using Lemoine.Core.Log;
using System.Linq;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonPalletChangeSequence
  /// </summary>
  [TestFixture]
  public class AutoReasonPalletChangeSequence_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonPalletChangeSequence_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonPalletChangeSequence_UnitTest () : base (DateTime.Today.AddMonths (-1))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }

    /// <summary>
    /// Test case with several sequence slots
    /// * Machining from T1 to T6
    /// * Pallet change from T7 to T8
    /// * Machining from T9 to T10
    /// * Pallet change from T10 to T12
    /// Associated reason slots "pallet change sequence"
    /// => from T7 to no end
    /// => from T10 to no end
    /// </summary>
    [Test]
    public void StopSequence_StopsDetected ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            // Associated machine module
            IMachineModule mamo = null;
            if (machine.MachineModules != null) {
              foreach (var elt in machine.MachineModules) {
                mamo = elt;
              }
            }

            Assert.NotNull (mamo);

            // Get an operation
            var operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (1);
            
            // Create sequences
            CreateSequenceSlot (1, 6, mamo, SequenceKind.Machining, operation);
            CreateSequenceSlot (7, 8, mamo, SequenceKind.AutoPalletChange, operation);
            CreateSequenceSlot (9, 10, mamo, SequenceKind.Machining, operation);
            CreateSequenceSlot (10, 12, mamo, SequenceKind.AutoPalletChange, operation);
            
            // Plugin
            var autoReason = GetAutoReasonExtension (machine);
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that autoreasons appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ()
                .OrderBy (x => x.Range.Lower).ToList ();
              Assert.AreEqual (2, reasonAssociations.Count, "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.AreEqual (T (7), reasonAssociation.Begin.Value, "wrong start for the first reason");
              Assert.IsFalse (reasonAssociation.End.HasValue, "the first reason should have no end");
              reasonAssociation = reasonAssociations[1];
              Assert.AreEqual (T (10), reasonAssociation.Begin.Value, "wrong start for the second reason");
              Assert.IsFalse (reasonAssociation.End.HasValue, "the second reason should have no end");
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    AutoReasonExtension GetAutoReasonExtension (IMonitoredMachine machine)
    {
      var plugin = new Lemoine.Plugin.AutoReasonPalletChangeSequence.Plugin ();
      plugin.Install (0);
      var autoReason = new AutoReasonExtension ();
      autoReason.SetTestConfiguration (
        "{ \"ReasonScore\": 60.0," +
        "\"ManualScore\": 100.0}"
      );

      // Initialize the date
      string dateTimeKey = autoReason.GetKey ("DateTime");
      var autoReasonState = ModelDAOHelper.ModelFactory.CreateAutoReasonState (machine, autoReason.GetKey ("DateTime"));
      autoReasonState.Value = T (-1);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.MakePersistent (autoReasonState);

      autoReason.Initialize (machine, null);
      return autoReason;
    }

    void CreateSequenceSlot (int start, int end, IMachineModule machineModule, SequenceKind kind, IOperation operation)
    {
      // First create a path
      var path = ModelDAOHelper.ModelFactory.CreatePath ();
      path.Operation = operation;
      path.Number = 1234897862 + start;
      ModelDAOHelper.DAOFactory.PathDAO.MakePersistent (path);

      // Then create a sequence
      var sequence = ModelDAOHelper.ModelFactory.CreateSequence (string.Format ("sequence-{0}-{1}", start, end));
      sequence.Kind = kind;
      sequence.Path = path;
      ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent (sequence);

      // Finally create a sequence slot
      var sequenceSlot = ModelDAOHelper.ModelFactory.CreateSequenceSlot (machineModule, sequence, R (start, end));
      ModelDAOHelper.DAOFactory.SequenceSlotDAO.MakePersistent (sequenceSlot);
    }
  }
}
