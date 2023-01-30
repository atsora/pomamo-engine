// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.GDBPersistentClasses;
using Lemoine.Plugin.AutoReasonStopBetweenCycles;
using System.Linq;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonStopBetweenCycles
  /// </summary>
  [TestFixture]
  public class AutoReasonStopBetweenCycles_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonStopBetweenCycles_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonStopBetweenCycles_UnitTest () : base (DateTime.Today.AddHours (-12))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }

    /// <summary>
    /// Test case with three cycles close to each other
    /// * a first full cycle from T1 to T5
    /// * a second full cycle from T10 and no end
    /// * a third full cycle from T17 to T20
    /// => scenario made in Lemoine.AutoReason.UnitTests.DynamicCycleEndExtension
    /// Configuration
    /// * extended period start at 3
    /// </summary>
    [Test]
    public void StopBetweenCycles_ShortStop ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            // Plugin
            var autoReason = GetAutoReasonExtension (machine, 3);

            // FIRST FULL CYCLE

            // Operation cycles
            CreateOperationCycle (1, 5, machine);
            CreateOperationCycle (10, null, machine);
            CreateOperationCycle (17, 20, machine);

            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that autoreasons appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ()
                .OrderBy (x => x.Range.Upper).OrderBy (x => x.Range.Lower).ToList ();
              Assert.AreEqual (4, reasonAssociations.Count, "wrong number of auto reason created");

              var reasonAssociation = reasonAssociations[0]; // Short
              Assert.AreEqual (T (5), reasonAssociation.Begin.Value, "wrong start 1");
              Assert.AreEqual (T (5).AddSeconds (3), reasonAssociation.End.Value, "wrong end 1");
              Assert.AreEqual ("ReasonStopBetweenCycles", reasonAssociation.Reason.TranslationKey, "wrong reason 1");

              reasonAssociation = reasonAssociations[1]; // Long
              Assert.AreEqual (T (5), reasonAssociation.Begin.Value, "wrong start 2");
              Assert.AreEqual (T (5).AddDays (1), reasonAssociation.End.Value, "wrong end 2");
              Assert.AreEqual ("ReasonStopBetweenCyclesExtended", reasonAssociation.Reason.TranslationKey, "wrong reason 2");

              reasonAssociation = reasonAssociations[2]; // Short
              Assert.AreEqual (T (20), reasonAssociation.Begin.Value, "wrong start 3");
              Assert.AreEqual (T (20).AddSeconds (3), reasonAssociation.End.Value, "wrong end 3");
              Assert.AreEqual ("ReasonStopBetweenCycles", reasonAssociation.Reason.TranslationKey, "wrong reason 3");

              reasonAssociation = reasonAssociations[3]; // Long
              Assert.AreEqual (T (20), reasonAssociation.Begin.Value, "wrong start 4");
              Assert.AreEqual (T (20).AddDays (1), reasonAssociation.End.Value, "wrong end 4");
              Assert.AreEqual ("ReasonStopBetweenCyclesExtended", reasonAssociation.Reason.TranslationKey, "wrong reason 4");
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    AutoReasonExtension GetAutoReasonExtension (IMonitoredMachine machine, int extendedPeriodLimit)
    {
      string limitText = "";
      if (extendedPeriodLimit > 0) {
        var duration = TimeSpan.FromSeconds (extendedPeriodLimit);
        limitText = string.Format ("{0:D2}:{1:D2}:{2:D2}", duration.Hours, duration.Minutes, duration.Seconds);
      }

      var plugin = new Lemoine.Plugin.AutoReasonStopBetweenCycles.Plugin ();
      plugin.Install (0);
      var autoReason = new AutoReasonExtension ();
      autoReason.SetTestConfiguration (
  "{ \"ReasonScore\": 65.0," +
  "\"ExtendedReasonScore\": 50.0," +
  "\"ManualScore\": 100.0," +
  "\"ExtendedPeriod\": \"" + limitText + "\"}"
);
      autoReason.Initialize (machine, null);

      return autoReason;
    }

    void CreateReasonSlot (int start, int end, IMachine machine, IMachineMode machineMode, IMachineObservationState mos, IReason reason)
    {
      var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (start, end));
      reasonSlot.MachineMode = machineMode;
      reasonSlot.MachineObservationState = mos;
      ((ReasonSlot)reasonSlot).SetDefaultReason (reason, 10.0, true, true);
      ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
    }

    void CreateOperationCycle (int start, int? end, IMonitoredMachine machine)
    {
      var oc = ModelDAOHelper.ModelFactory.CreateOperationCycle (machine);
      oc.Begin = T (start);
      if (end.HasValue) {
        oc.SetRealEnd (T (end.Value));
      }

      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (oc);
    }
    
    [SetUp]
    public void Setup ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
      Lemoine.Extensions.ExtensionManager.Add (typeof (DynamicCycleEndExtension));
    }

    [TearDown]
    public void TearDown ()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
      DynamicCycleEndExtension.Reset ();
    }
  }
}
