// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Extensions.Database;

namespace Lemoine.Analysis.UnitTests
{
  /// <summary>
  /// Unit tests for the class DefaultReasonUndefined.
  /// </summary>
  [TestFixture]
  public class DefaultReasonWithDuration_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DefaultReasonWithDuration_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DefaultReasonWithDuration_UnitTest ()
      : base (Lemoine.UnitTests.UtcDateTime.From (2018, 01, 01))
    {
    }

    /// <summary>
    /// Test the method TryResetReason
    /// </summary>
    [Test]
    public void TestTryResetReason ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Reference data
            // - Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);
            // - Machine modes / Machine observation states
            var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
            var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
            var autoNullOverride = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.AutoNullOverride);
            var reasonUndefined = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Undefined);
            var reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Motion);
            var reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Unanswered);
            var reasonShort = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Short);

            // Plugin
            var reasonExtension = new Lemoine.Plugin.DefaultReasonWithDurationConfig.DefaultReasonWithDuration ();
            reasonExtension.Initialize (machine);

            { // Motion
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 3));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = attended;

              reasonExtension.TryResetReason (ref reasonSlot);
              Assert.Multiple (() => {
                Assert.That (reasonSlot.Reason, Is.EqualTo (reasonMotion));
                Assert.That (reasonSlot.ReasonScore, Is.EqualTo (90.0));
              });
            }

            { // AutoNullOverride / Long
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 3600));
              reasonSlot.MachineMode = autoNullOverride;
              reasonSlot.MachineObservationState = attended;

              reasonExtension.TryResetReason (ref reasonSlot);
              Assert.Multiple (() => {
                Assert.That (reasonSlot.Reason, Is.EqualTo (reasonUnanswered));
                Assert.That (reasonSlot.ReasonScore, Is.EqualTo (10.0));
              });
            }

            { // AutoNullOverride / Short
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 3));
              reasonSlot.MachineMode = autoNullOverride;
              reasonSlot.MachineObservationState = attended;

              reasonExtension.TryResetReason (ref reasonSlot);
              Assert.Multiple (() => {
                Assert.That (reasonSlot.Reason, Is.EqualTo (reasonShort));
                Assert.That (reasonSlot.ReasonScore, Is.EqualTo (10.0));
              });
            }
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }
  }
}
