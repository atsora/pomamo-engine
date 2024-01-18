// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.GDBPersistentClasses;
using Lemoine.Plugin.AutoReasonStopDuringMachiningSequence;
using Lemoine.Core.Log;
using System.Linq;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonStopDuringMachiningSequence
  /// </summary>
  [TestFixture]
  public class AutoReasonStopDuringMachiningSequence_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonStopDuringMachiningSequence_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonStopDuringMachiningSequence_UnitTest () : base (DateTime.Today.AddMonths (-1))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }

    /// <summary>
    /// Test case with several sequence slots
    /// * Pallet change from T1 to T6
    /// * Machining from T7 to T8
    /// * Pallet change from T9 to T10
    /// * Machining from T10 to T12
    /// Configuration: no regex for filtering the sequence name
    /// Associated reason slots "stop during machining":
    /// => from T7 to no end
    /// => from T10 to no end
    /// </summary>
    [Test]
    public void MachiningSequence_StopsDetected ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);

            // Associated machine module
            IMachineModule mamo = null;
            if (machine.MachineModules != null) {
              foreach (var elt in machine.MachineModules) {
                mamo = elt;
              }
            }

            Assert.That (mamo, Is.Not.Null);

            // Get an operation
            var operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (1);

            // Create sequences
            CreateSequenceSlot (1, 6, mamo, SequenceKind.AutoPalletChange, operation, "");
            CreateSequenceSlot (7, 8, mamo, SequenceKind.Machining, operation, "");
            CreateSequenceSlot (9, 10, mamo, SequenceKind.AutoPalletChange, operation, "");
            CreateSequenceSlot (10, 12, mamo, SequenceKind.Machining, operation, "");

            // Plugin
            var autoReason = GetAutoReasonExtension (machine, "");
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that autoreasons appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ()
                .OrderBy (x => x.Range.Lower).ToList ();
              Assert.That (reasonAssociations, Has.Count.EqualTo (2), "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (7)), "wrong start for the first reason");
              Assert.IsFalse (reasonAssociation.End.HasValue, "the first reason should have no end");
              reasonAssociation = reasonAssociations[1];
              Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (10)), "wrong start for the second reason");
              Assert.IsFalse (reasonAssociation.End.HasValue, "the second reason should have no end");
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case with several sequence slots
    /// * Pallet change from T1 to T6
    /// * Machining from T7 to T8, sequence name is "a_234_n"
    /// * Pallet change from T9 to T10
    /// * Machining from T10 to T12, sequence name is "a_234_p"
    /// Configuration: regex "_[0-9]{3}_n" for filtering the sequence name
    /// Associated reason slots "stop during machining":
    /// => from T7 to no end
    /// </summary>
    [Test]
    public void MachiningSequence_FilteringRegex ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);

            // Associated machine module
            IMachineModule mamo = null;
            if (machine.MachineModules != null) {
              foreach (var elt in machine.MachineModules) {
                mamo = elt;
              }
            }

            Assert.That (mamo, Is.Not.Null);

            // Get an operation
            var operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (1);

            // Create sequences
            CreateSequenceSlot (1, 6, mamo, SequenceKind.AutoPalletChange, operation, "");
            CreateSequenceSlot (7, 8, mamo, SequenceKind.Machining, operation, "a_234_n");
            CreateSequenceSlot (9, 10, mamo, SequenceKind.AutoPalletChange, operation, "");
            CreateSequenceSlot (10, 12, mamo, SequenceKind.Machining, operation, "a_234_p");

            // Plugin
            var autoReason = GetAutoReasonExtension (machine, "_[0-9]{3}_n");
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that autoreasons appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ()
                .OrderBy (x => x.Range.Lower).ToList ();
              Assert.That (reasonAssociations, Has.Count.EqualTo (1), "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (7)), "wrong start for the first reason");
              Assert.IsFalse (reasonAssociation.End.HasValue, "the first reason should have no end");
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    AutoReasonExtension GetAutoReasonExtension (IMonitoredMachine machine, string regex)
    {
      var plugin = new Lemoine.Plugin.AutoReasonStopDuringMachiningSequence.Plugin ();
      plugin.Install (0);
      var autoReason = new AutoReasonExtension ();
      autoReason.SetTestConfiguration (
        "{ \"ReasonScore\": 60.0," +
        "\"ManualScore\": 100.0," +
        "\"SequenceNameRegex\":\"" + regex + "\"}"
      );

      // Initialize the date
      string dateTimeKey = autoReason.GetKey ("DateTime");
      var autoReasonState = ModelDAOHelper.ModelFactory.CreateAutoReasonState (machine, autoReason.GetKey ("DateTime"));
      autoReasonState.Value = T (-1);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.MakePersistent (autoReasonState);

      autoReason.Initialize (machine, null);
      return autoReason;
    }

    void CreateSequenceSlot (int start, int end, IMachineModule machineModule, SequenceKind kind, IOperation operation, string sequenceName)
    {
      // Default sequence name
      if (sequenceName == "") {
        sequenceName = string.Format ("sequence-{0}-{1}", start, end);
      }

      // First create a path
      var path = ModelDAOHelper.ModelFactory.CreatePath ();
      path.Operation = operation;
      path.Number = 1234897862 + start;
      ModelDAOHelper.DAOFactory.PathDAO.MakePersistent (path);

      // Then create a sequence
      var sequence = ModelDAOHelper.ModelFactory.CreateSequence (sequenceName);
      sequence.Kind = kind;
      sequence.Path = path;
      ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent (sequence);

      // Finally create a sequence slot
      var sequenceSlot = ModelDAOHelper.ModelFactory.CreateSequenceSlot (machineModule, sequence, R (start, end));
      ModelDAOHelper.DAOFactory.SequenceSlotDAO.MakePersistent (sequenceSlot);
    }
  }
}
