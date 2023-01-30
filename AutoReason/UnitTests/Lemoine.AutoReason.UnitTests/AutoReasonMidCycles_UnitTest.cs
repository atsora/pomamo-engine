// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Plugin.AutoReasonMidCycle;
using Lemoine.GDBPersistentClasses;
using System.Linq;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonMidCycle
  /// </summary>
  [TestFixture]
  public class AutoReasonMidCycle_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonMidCycle_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonMidCycle_UnitTest () : base (DateTime.Today.AddHours(-12))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }

    /// <summary>
    /// Test case with two full cycles
    /// * from T0 to T10
    /// * from T20 to T30
    /// => auto reason "inside cycles" applied from T0 and from T20
    /// </summary>
    [Test]
    public void MidCycles_SimpleDetection ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FULL CYCLE

            // Initialize an operation cycle in the past
            CreateOperationCycle (-20, -10, machine);

            // Plugin
            var autoReason = GetAutoReasonExtension (machine);
            autoReason.RunOnce ();

            // OTHER RUNS

            // First operation cycle from T0 to T10
            CreateOperationCycle (0, 10, machine);
            autoReason.RunOnce ();

            // Second operation cycle from T20 to T30
            CreateOperationCycle (20, 30, machine);
            autoReason.RunOnce ();

            // Check that an autoreason appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ()
                .OrderBy (x => x.Range.Lower).ToList ();
              Assert.AreEqual (3, reasonAssociations.Count, "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.AreEqual (T (-20), reasonAssociation.Begin.Value, "wrong start 1");
              Assert.IsFalse (reasonAssociation.End.HasValue, "wrong end 1");
              reasonAssociation = reasonAssociations[1];
              Assert.AreEqual (T (0), reasonAssociation.Begin.Value, "wrong start 2");
              Assert.IsFalse (reasonAssociation.End.HasValue, "wrong end 2");
              reasonAssociation = reasonAssociations[2];
              Assert.AreEqual (T (20), reasonAssociation.Begin.Value, "wrong start 3");
              Assert.IsFalse (reasonAssociation.End.HasValue, "wrong end 3");
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    AutoReasonExtension GetAutoReasonExtension (IMonitoredMachine machine)
    {
      var plugin = new Lemoine.Plugin.AutoReasonMidCycle.Plugin ();
      plugin.Install (0);
      var autoReason = new AutoReasonExtension ();
      autoReason.SetTestConfiguration ("{ \"ReasonScore\": 65.0}");
      autoReason.Initialize (machine, null);
      autoReason.TestMode = true;

      return autoReason;
    }

    void CreateOperationCycle (int start, int end, IMonitoredMachine machine)
    {
      var oc = ModelDAOHelper.ModelFactory.CreateOperationCycle (machine);
      oc.Begin = T (start);
      oc.SetRealEnd (T (end));
      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (oc);
    }
  }
}
