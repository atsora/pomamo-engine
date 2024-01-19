// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.GDBPersistentClasses;
using Lemoine.Plugin.AutoReasonStopSequence;
using Lemoine.Core.Log;
using System.Linq;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonStopSequence
  /// </summary>
  [TestFixture]
  public class AutoReasonStopSequence_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonShiftEnd_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonStopSequence_UnitTest () : base (DateTime.Today.AddHours (-1))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }

    /// <summary>
    /// Test case with several sequence slots
    /// * Machining from T1 to T6
    /// * Stop from T7 to T8
    /// * Machining from T9 to T10
    /// * Stop from T10 to T12
    /// Associated reason slots "stop sequence"
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
            CreateSequenceSlot (1, 6, mamo, SequenceKind.Machining, operation, string.Format ("sequence-{0}-{1}", 1, 6));
            CreateSequenceSlot (7, 8, mamo, SequenceKind.Stop, operation, string.Format ("sequence-{0}-{1}", 7, 8));
            CreateSequenceSlot (9, 10, mamo, SequenceKind.Machining, operation, string.Format ("sequence-{0}-{1}", 9, 10));
            CreateSequenceSlot (10, 12, mamo, SequenceKind.Stop, operation, string.Format ("sequence-{0}-{1}", 10, 12));
            
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
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (7)), "wrong start for the first reason");
                Assert.That (reasonAssociation.End.HasValue, Is.False, "the first reason should have no end");
              });
              reasonAssociation = reasonAssociations[1];
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (10)), "wrong start for the second reason");
                Assert.That (reasonAssociation.End.HasValue, Is.False, "the second reason should have no end");
              });
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case with several sequence slots with different names and with a filter
    /// * Machining from T1 to T6
    /// * Stop from T7 to T8, called "this"
    /// * Machining from T9 to T10
    /// * Stop from T10 to T12, called "that"
    /// Associated reason slots "stop sequence", the filter being "hi"
    /// => from T7 to no end
    /// </summary>
    [Test]
    public void StopSequence_StopsDetectedWithFilter ()
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
            var dateTime = T (0);
            var sequenceSlots = ModelDAOHelper.DAOFactory.SequenceSlotDAO.FindAllEndFrom (mamo, dateTime);
            CreateSequenceSlot (1, 6, mamo, SequenceKind.Machining, operation, string.Format ("sequence-{0}-{1}", 1, 6));
            CreateSequenceSlot (7, 8, mamo, SequenceKind.Stop, operation, "this");
            CreateSequenceSlot (9, 10, mamo, SequenceKind.Machining, operation, string.Format ("sequence-{0}-{1}", 9, 10));
            CreateSequenceSlot (10, 12, mamo, SequenceKind.Stop, operation, "that");
            Assert.That (ModelDAOHelper.DAOFactory.SequenceSlotDAO.FindAllEndFrom (mamo, dateTime), Has.Count.EqualTo (sequenceSlots.Count + 4));

            // Plugin
            var autoReason = GetAutoReasonExtension (machine, "hi");
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that autoreasons appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ();
              Assert.That (reasonAssociations, Has.Count.EqualTo (1), "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (7)), "wrong start for the first reason");
                Assert.That (reasonAssociation.End.HasValue, Is.False, "the first reason should have no end");
              });
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    AutoReasonExtension GetAutoReasonExtension (IMonitoredMachine machine, string filter)
    {
      var plugin = new Lemoine.Plugin.AutoReasonStopSequence.Plugin ();
      plugin.Install (0);
      var autoReason = new AutoReasonExtension ();
      autoReason.SetTestConfiguration (
        "{ \"ReasonScore\": 60.0," +
        "\"ManualScore\": 100.0," +
        "\"Filter\":\"" + filter + "\"}"
      );

      // Initialize the date
      string dateTimeKey = autoReason.GetKey ("DateTime");
      var autoReasonState = ModelDAOHelper.DAOFactory.AutoReasonStateDAO.GetAutoReasonState (machine, autoReason.GetKey ("DateTime")) ??
        ModelDAOHelper.ModelFactory.CreateAutoReasonState (machine, autoReason.GetKey ("DateTime"));
      autoReasonState.Value = T (-1);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.MakePersistent (autoReasonState);

      autoReason.Initialize (machine, null);
      return autoReason;
    }

    void CreateSequenceSlot (int start, int end, IMachineModule machineModule, SequenceKind kind, IOperation operation, string sequenceName)
    {
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
