// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Plugin.AutoReasonBetweenComponents;
using Lemoine.GDBPersistentClasses;
using Pulse.Extensions.Extension;
using Pulse.Extensions.Database;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonBetweenComponents
  /// </summary>
  [TestFixture]
  public class AutoReasonBetweenComponents_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonBetweenComponents_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonBetweenComponents_UnitTest () : base (DateTime.Today.AddHours (-12))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }

    /// <summary>
    /// Test case with three cycles close to each other
    /// * component 1 from T1 to T4
    /// * T4-T6 : no slot
    /// * component 2 from T6 to T7
    /// * T7-T9 : slot with a component null
    /// * component 1 from T9 to T12
    /// Result:
    /// * autoreason applied on T4-T6
    /// * autoreason applied on T7-T9
    /// </summary>
    [Test]
    public void BetweenComponents_ReasonsApplied ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine, components
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            var component1 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (1);
            Assert.NotNull (component1);
            var component2 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (2);
            Assert.NotNull (component2);

            // Operation slots
            CreateOperationSlot (machine, 1, 4, component1);
            CreateOperationSlot (machine, 6, 7, component2);
            CreateOperationSlot (machine, 7, 9, null);
            CreateOperationSlot (machine, 9, 12, component1);

            // Plugin
            var autoReason = GetAutoReasonExtension (machine);
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that autoreasons appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO().FindAll ();
              Assert.AreEqual (2, reasonAssociations.Count, "wrong number of auto reason created");

              var reasonAssociation = reasonAssociations[0];
              Assert.AreEqual (T (4), reasonAssociation.Begin.Value, "wrong start 1");
              Assert.AreEqual (T (6), reasonAssociation.End.Value, "wrong end 1");

              reasonAssociation = reasonAssociations[1];
              Assert.AreEqual (T (7), reasonAssociation.Begin.Value, "wrong start 2");
              Assert.AreEqual (T (9), reasonAssociation.End.Value, "wrong end 2");
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case with three cycles close to each other
    /// * component 1 from T1 to T4
    /// * component 2 from T4 to T7
    /// * T7-T9 : slot with a component null
    /// * component 2 from T9 to T12
    /// Result:
    /// * no autoreasons applied
    /// </summary>
    [Test]
    public void BetweenComponents_ReasonsNotApplied ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine, components
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            var component1 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (1);
            Assert.NotNull (component1);
            var component2 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (2);
            Assert.NotNull (component2);

            // Operation slots
            CreateOperationSlot (machine, 1, 4, component1);
            CreateOperationSlot (machine, 4, 7, component2);
            CreateOperationSlot (machine, 7, 9, null);
            CreateOperationSlot (machine, 9, 12, component2);

            // Plugin
            var autoReason = GetAutoReasonExtension (machine);
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that autoreasons appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO().FindAll ();
              Assert.AreEqual (0, reasonAssociations.Count, "wrong number of auto reason created");
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    AutoReasonExtension GetAutoReasonExtension (IMonitoredMachine machine)
    {
      var plugin = new Lemoine.Plugin.AutoReasonBetweenComponents.Plugin ();
      plugin.Install (0);
      var autoReason = new AutoReasonExtension ();
      autoReason.SetTestConfiguration (
  "{ \"ReasonScore\": 65.0," +
  "\"ExtendedReasonScore\": 50.0," +
  "\"ManualScore\": 100.0}"
);
      autoReason.Initialize (machine, null);

      return autoReason;
    }

    void CreateOperationSlot (IMachine machine, int start, int end, IComponent component)
    {
      var opSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot (machine, null, component, null, null, null, null, null, R(start, end));
      ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (opSlot);
    }

    [SetUp]
    public void Setup ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (OperationDetectionStatusExtension));
    }

    [TearDown]
    public void TearDown ()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
    }

    public class OperationDetectionStatusExtension
      : Lemoine.Extensions.NotConfigurableExtension
      , IOperationDetectionStatusExtension
    {
      ILog log = LogManager.GetLogger (typeof (OperationDetectionStatusExtension).FullName);

      public int OperationDetectionStatusPriority
      {
        get
        {
          return 1;
        }
      }

      public DateTime? GetOperationDetectionDateTime ()
      {
        return DateTime.UtcNow;
      }

      public bool Initialize (IMachine machine)
      {
        return true;
      }
    }
  }
}
