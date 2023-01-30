// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Plugin.AutoReasonLongIdleSameMachineMode;
using Lemoine.GDBPersistentClasses;
using System.Linq;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonLongIdleSameMachineMode
  /// </summary>
  [TestFixture]
  public class AutoReasonLongIdleSameMachineMode_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonLongIdleSameMachineMode_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonLongIdleSameMachineMode_UnitTest () : base (DateTime.Today.AddDays (-1))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }

    /// <summary>
    /// Test case with a series of fact, each one have a specified machine mode
    /// * from T0 to T8: Active
    /// * from T8 to T16: M0
    /// * from T16 to T19: Off
    /// 
    /// Configuration of the plugin
    /// * DynamicEnd is NextMachineMode
    /// * Minimum duration is 4
    /// 
    /// Result => 1 auto reason generated, starting at T8
    /// </summary>
    [Test]
    public void MachineMode_simple_detection ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            // Machine modes
            var mmActive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var mmM0 = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.M0);
            var mmOff = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Off);

            // Translation key
            var translationKey = "LongIdleSameMachineMode";

            // Create facts
            CreateFact (machine, mmActive, 0, 8);
            CreateFact (machine, mmM0, 8, 16);
            CreateFact (machine, mmOff, 16, 19);
            

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine, 4, 3, ",NextMachineMode");
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that 1 autoreason appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ().OrderBy (x => x.Range.Lower).ToList ();
              Assert.AreEqual (1, reasonAssociations.Count, "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.AreEqual (T (8), reasonAssociation.Begin.Value, "wrong start 1");
              Assert.AreEqual ("NextMachineMode", reasonAssociation.DynamicEnd, "wrong dynamic end 1");
              Assert.AreEqual (translationKey, reasonAssociation.Reason.TranslationKey, "wrong translation key 1");
            }

            // Test that the translation value is set correctly
            var translation = ModelDAOHelper.DAOFactory.TranslationDAO.Find ("", translationKey);
            Assert.IsNotNull (translation);

          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case with a series of fact, each one have a specified machine mode
    /// * from T0 to T8: Active
    /// * from T8 to T9: M0
    /// * from T9 to T11: M0
    /// * from T11 to T13: M0
    /// * from T13 to T15: Off
    /// * from T15 to T18: Off
    /// 
    /// Configuration of the plugin
    /// * DynamicEnd is NextMachineMode
    /// * Minimum duration is 4
    /// 
    /// Result => 2 auto reasons generated, starting at T8 and T13
    /// </summary>
    [Test]
    public void MachineMode_fragmented_detection ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            // Machine modes
            var mmActive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var mmM0 = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.M0);
            var mmOff = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Off);

            // Translation key
            var translationKey = "LongIdleSameMachineMode";

            // Create facts
            CreateFact (machine, mmActive, 0, 8);
            CreateFact (machine, mmM0, 8, 9);
            CreateFact (machine, mmM0, 9, 11);
            CreateFact (machine, mmM0, 11, 13);
            CreateFact (machine, mmOff, 13, 15);
            CreateFact (machine, mmOff, 15, 18);


            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine, 4, 3, ",NextMachineMode");
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that 1 autoreason appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ().OrderBy (x => x.Range.Lower).ToList ();
              Assert.AreEqual (2, reasonAssociations.Count, "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.AreEqual (T (8), reasonAssociation.Begin.Value, "wrong start 1");
              Assert.AreEqual ("NextMachineMode", reasonAssociation.DynamicEnd, "wrong dynamic end 1");
              Assert.AreEqual (translationKey, reasonAssociation.Reason.TranslationKey, "wrong translation key 1");
              reasonAssociation = reasonAssociations[1];
              Assert.AreEqual (T (13), reasonAssociation.Begin.Value, "wrong start 2");
            }

            // Test that the translation value is set correctly
            var translation = ModelDAOHelper.DAOFactory.TranslationDAO.Find ("", translationKey);
            Assert.IsNotNull (translation);

          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case with a series of fact, each one have a specified machine mode
    /// * from T0 to T8: Active
    /// * from T8 to T10: initial gap
    /// * from T10 to T12: M0
    /// * from T12 to T15: M0
    /// * from T15 to T20: Active
    /// 
    /// Configuration of the plugin
    /// * DynamicEnd is NextMachineMode
    /// * Minimum duration is 4
    /// * Max gap duration is 3
    /// 
    /// Result => 1 auto reason generated, starting at T10 after the gap
    /// </summary>
    [Test]
    public void MachineMode_detection_with_initial_gap ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            // Machine modes
            var mmActive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var mmM0 = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.M0);

            // Translation key
            var translationKey = "LongIdleSameMachineMode";

            // Create facts
            CreateFact (machine, mmActive, 0, 8);
            CreateFact (machine, mmM0, 10, 12);
            CreateFact (machine, mmM0, 12, 15);
            CreateFact (machine, mmActive, 15, 18);


            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine, 4, 3, ",NextMachineMode");
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that 1 autoreason appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ().OrderBy (x => x.Range.Lower).ToList ();
              Assert.AreEqual (1, reasonAssociations.Count, "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.AreEqual (T (10), reasonAssociation.Begin.Value, "wrong start 1");
              Assert.AreEqual ("NextMachineMode", reasonAssociation.DynamicEnd, "wrong dynamic end 1");
              Assert.AreEqual (translationKey, reasonAssociation.Reason.TranslationKey, "wrong translation key 1");
            }

            // Test that the translation value is set correctly
            var translation = ModelDAOHelper.DAOFactory.TranslationDAO.Find ("", translationKey);
            Assert.IsNotNull (translation);

          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case with a series of fact, each one have a specified machine mode
    /// * from T0 to T8: Active
    /// * from T8 to T12: M0
    /// * from T12 to T14: small gap inside
    /// * from T14 to T15: M0
    /// * from T15 to T20: big gap inside
    /// * from T20 to T22: M0
    /// * from T22 to T30: M0
    /// 
    /// Configuration of the plugin
    /// * DynamicEnd is NextMachineMode
    /// * Minimum duration is 4
    /// * Max gap duration is 3
    /// 
    /// Result => 2 auto reasons generated, starting at T8 and T20
    /// </summary>
    [Test]
    public void MachineMode_detection_with_gaps_inside ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            // Machine modes
            var mmActive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var mmM0 = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.M0);

            // Translation key
            var translationKey = "LongIdleSameMachineMode";

            // Create facts
            CreateFact (machine, mmActive, 0, 8);
            CreateFact (machine, mmM0, 8, 12);
            CreateFact (machine, mmM0, 14, 15);
            CreateFact (machine, mmM0, 20, 22);
            CreateFact (machine, mmM0, 22, 30);


            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine, 4, 3, ",NextMachineMode");
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that 2 autoreasons appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ().OrderBy (x => x.Range.Lower).ToList ();
              Assert.AreEqual (2, reasonAssociations.Count, "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.AreEqual (T (8), reasonAssociation.Begin.Value, "wrong start 1");
              Assert.AreEqual ("NextMachineMode", reasonAssociation.DynamicEnd, "wrong dynamic end 1");
              Assert.AreEqual (translationKey, reasonAssociation.Reason.TranslationKey, "wrong translation key 1");
              reasonAssociation = reasonAssociations[1];
              Assert.AreEqual (T (20), reasonAssociation.Begin.Value, "wrong start 2");
            }

            // Test that the translation value is set correctly
            var translation = ModelDAOHelper.DAOFactory.TranslationDAO.Find ("", translationKey);
            Assert.IsNotNull (translation);

          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case with a series of fact, each one have a specified machine mode
    /// * from T0 to T8: Active
    /// * from T8 to T11: M0
    /// * from T11 to T13: final gap that can be attached
    /// * from T13 to T15: Off
    /// * from T15 to T20: final gap that cannot be attached
    /// * from T20 to T22: Active
    /// 
    /// Configuration of the plugin
    /// * DynamicEnd is NextMachineMode
    /// * Minimum duration is 4
    /// * Max gap duration is 3
    /// 
    /// Result => 1 auto reason generated, starting at T8
    /// </summary>
    [Test]
    public void MachineMode_detection_with_final_gaps ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);

            // Machine modes
            var mmActive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var mmM0 = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.M0);
            var mmOff = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Off);

            // Translation key
            var translationKey = "LongIdleSameMachineMode";

            // Create facts
            CreateFact (machine, mmActive, 0, 8);
            CreateFact (machine, mmM0, 8, 11);
            CreateFact (machine, mmOff, 13, 15);
            CreateFact (machine, mmActive, 20, 22);

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine, 4, 3, ",NextMachineMode");
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that 1 autoreason appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ().OrderBy (x => x.Range.Lower).ToList ();
              Assert.AreEqual (2, reasonAssociations.Count, "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.AreEqual (T (8), reasonAssociation.Begin.Value, "wrong start 1");
              Assert.AreEqual ("NextMachineMode", reasonAssociation.DynamicEnd, "wrong dynamic end 1");
              Assert.AreEqual (translationKey, reasonAssociation.Reason.TranslationKey, "wrong translation key 1");
            }

            // Test that the translation value is set correctly
            var translation = ModelDAOHelper.DAOFactory.TranslationDAO.Find ("", translationKey);
            Assert.IsNotNull (translation);

          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    void CreateFact (IMonitoredMachine machine, IMachineMode machineMode, int start, int end)
    {
      var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, T (start), T (end), machineMode);
      ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
    }

    AutoReasonExtension GetAutoReasonExtension (IMonitoredMachine machine, int minimumDuration, int maxGap, string dynamicEnd)
    {
      var plugin = new Lemoine.Plugin.AutoReasonLongIdleSameMachineMode.Plugin ();
      plugin.Install (0);
     
      var autoReason = new AutoReasonExtension ();
      autoReason.SetTestConfiguration (
        "{ \"ReasonScore\": 60.0," +
        "\"ManualScore\": 100.0," +
        "\"DynamicEnd\": \"" + dynamicEnd + "\"," +
        "\"MaxGapDuration\": \"" + TimeSpan.FromSeconds (maxGap).ToString (@"hh\:mm\:ss") + "\"," +
        "\"MinDuration\": \"" + TimeSpan.FromSeconds(minimumDuration).ToString(@"hh\:mm\:ss") + "\" }"
      );

      // Initialize the date
      var autoReasonState = ModelDAOHelper.ModelFactory.CreateAutoReasonState (machine, autoReason.GetKey ("DateTime"));
      autoReasonState.Value = T (-1);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.MakePersistent (autoReasonState);

      autoReason.Initialize (machine, null);
      return autoReason;
    }
  }
}
