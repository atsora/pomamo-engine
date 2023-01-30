// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.Analysis.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class DefaultReasonMinimalConfig_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DefaultReasonMinimalConfig_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DefaultReasonMinimalConfig_UnitTest ()
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
            Assert.NotNull (machine);
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
            var reasonExtension = new Lemoine.Plugin.DefaultReasonMinimalConfig.DefaultReasonMinimalConfig ();
            var initializationResult = reasonExtension.Initialize (machine, new [] { new Lemoine.Plugin.DefaultReasonMinimalConfig.Configuration {
              InactiveLongDefaultReasonTranslationKey = "ReasonUnanswered",
              InactiveLongDefaultReasonTranslationValue = "Unanswered",
            } });
            Assert.IsTrue (initializationResult);

            { // Motion
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 3));
              reasonSlot.MachineMode = active;
              reasonSlot.MachineObservationState = attended;

              reasonExtension.TryResetReason (ref reasonSlot);
              Assert.AreEqual (reasonMotion, reasonSlot.Reason);
              Assert.AreEqual (2.0, reasonSlot.ReasonScore);
            }

            { // AutoNullOverride / Long
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 3600));
              reasonSlot.MachineMode = autoNullOverride;
              reasonSlot.MachineObservationState = attended;

              reasonExtension.TryResetReason (ref reasonSlot);
              Assert.AreEqual (reasonUnanswered, reasonSlot.Reason);
              Assert.AreEqual (2.0, reasonSlot.ReasonScore);
            }

            { // AutoNullOverride / Short
              var reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, R (1, 3));
              reasonSlot.MachineMode = autoNullOverride;
              reasonSlot.MachineObservationState = attended;

              reasonExtension.TryResetReason (ref reasonSlot);
              Assert.AreEqual (reasonShort, reasonSlot.Reason);
              Assert.AreEqual (2.0, reasonSlot.ReasonScore);
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
