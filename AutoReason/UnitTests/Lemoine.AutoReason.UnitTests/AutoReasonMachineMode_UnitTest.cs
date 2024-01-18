// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Plugin.AutoReasonMachineMode;
using Lemoine.GDBPersistentClasses;
using System.Linq;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonMachineMode
  /// </summary>
  [TestFixture]
  public class AutoReasonMachineMode_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonMachineMode_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonMachineMode_UnitTest () : base (DateTime.Today.AddDays (-1))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }

    /// <summary>
    /// Test case with a series of fact, each one have a specified machine mode
    /// * from T0 to T8: Active
    /// * from T8 to T9: M0
    /// * from T9 to T15: M0
    /// * from T16 to T20: M0
    /// * from T21 to T25: Active
    /// * from T25 to T26: M0
    /// * from T26 to T30: Off
    /// * from T32 to T34: Active
    /// 
    /// Configuration of the plugin
    /// * MachineModeId is 21 (Active)
    /// * DynamicEnd is NextMachineMode
    /// 
    /// Result => two auto reasons generated, starting at T8 and T25
    /// </summary>
    [Test]
    public void MachineMode_M0_detection ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);

            // Machine modes
            var mmActive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var mmM0 = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.M0);
            var mmOff = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Off);

            // Translation key / value
            var translationKey = "AutoReasonMachineModeM0";
            var translationValue = "Auto reason M0";

            // Create facts
            CreateFact (machine, mmActive, 0, 8);
            CreateFact (machine, mmM0, 8, 9);
            CreateFact (machine, mmM0, 9, 15);
            CreateFact (machine, mmM0, 16, 20);
            CreateFact (machine, mmActive, 21, 25);
            CreateFact (machine, mmM0, 25, 26);
            CreateFact (machine, mmOff, 26, 30);
            CreateFact (machine, mmActive, 32, 34);
            

            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine, 21, ",NextMachineMode", translationKey, translationValue);
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that 2 autoreasons appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ()
                .OrderBy (x => x.Range.Lower).ToList ();
              Assert.That (reasonAssociations, Has.Count.EqualTo (2), "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (8)), "wrong start 1");
                Assert.That (reasonAssociation.DynamicEnd, Is.EqualTo ("NextMachineMode"), "wrong dynamic end 1");
                Assert.That (reasonAssociation.Reason.TranslationKey, Is.EqualTo (translationKey), "wrong translation key 1");
              });
              reasonAssociation = reasonAssociations[1];
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (25)), "wrong start 2");
                Assert.That (reasonAssociation.DynamicEnd, Is.EqualTo ("NextMachineMode"), "wrong dynamic end 2");
                Assert.That (reasonAssociation.Reason.TranslationKey, Is.EqualTo (translationKey), "wrong translation key 2");
              });
            }

            // Test that the translation value is set correctly
            var translation = ModelDAOHelper.DAOFactory.TranslationDAO.Find ("", translationKey);
            Assert.That (translation, Is.Not.Null);


          } finally {
            transaction.Rollback ();
          }
        }
      }
    }
    
    /// <summary>
    /// Test case for the recognition of a parent machine mode
    /// * from T0 to T8: Active
    /// * from T8 to T9: M0
    /// * from T9 to T15: M1
    /// * from T16 to T20: M60
    /// * from T21 to T25: Active
    /// * from T25 to T26: MStop (parent of M0, M1, M60)
    /// * from T26 to T30: Off
    /// * from T32 to T34: Active
    /// 
    /// Configuration of the plugin
    /// * MachineModeId is 39 (MStop: parent of M0, M1, M60)
    /// * DynamicEnd is NextMachineMode
    /// 
    /// Result => four auto reasons generated, starting at T8, T9, T16 and T25
    /// </summary>
    [Test]
    public void MachineMode_parent_detection ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);

            // Machine modes
            var mmActive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);
            var mmM0 = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.M0);
            var mmM1 = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.M1);
            var mmM60 = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.M60);
            var mmMStop = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.MStop);
            var mmOff = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Off);

            // Translation key / value
            var translationKey = "AutoReasonMachineModeM0M1M60";
            var translationValue = "Auto reason M0 / M1 / M60 (programmed stops)";

            // Create facts
            CreateFact (machine, mmActive, 0, 8);
            CreateFact (machine, mmM0, 8, 9);
            CreateFact (machine, mmM1, 9, 15);
            CreateFact (machine, mmM60, 16, 20);
            CreateFact (machine, mmActive, 21, 25);
            CreateFact (machine, mmMStop, 25, 26);
            CreateFact (machine, mmOff, 26, 30);
            CreateFact (machine, mmActive, 32, 34);


            // FIRST RUN, INITIALIZING THE PLUGIN ON A FIRST SHIFT

            // Plugin
            var autoReason = GetAutoReasonExtension (machine, 39, ",NextMachineMode", translationKey, translationValue);
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();
            autoReason.RunOnce ();

            // Check that 4 autoreasons appeared, each one with a specific detail
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ()
                .OrderBy (x => x.Range.Lower).ToList ();
              Assert.That (reasonAssociations, Has.Count.EqualTo (4), "wrong number of auto reason created");
              var reasonAssociation = reasonAssociations[0];
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (8)), "wrong start 1");
                Assert.That (reasonAssociation.DynamicEnd, Is.EqualTo ("NextMachineMode"), "wrong dynamic end 1");
                Assert.That (reasonAssociation.Reason.TranslationKey, Is.EqualTo (translationKey), "wrong translation key 1");
                Assert.That (mmM0.Display, Is.EqualTo (reasonAssociation.ReasonDetails));
              });
              reasonAssociation = reasonAssociations[1];
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (9)), "wrong start 2");
                Assert.That (mmM1.Display, Is.EqualTo (reasonAssociation.ReasonDetails));
              });
              reasonAssociation = reasonAssociations[2];
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (16)), "wrong start 3");
                Assert.That (mmM60.Display, Is.EqualTo (reasonAssociation.ReasonDetails));
              });
              reasonAssociation = reasonAssociations[3];
              Assert.Multiple (() => {
                Assert.That (reasonAssociation.Begin.Value, Is.EqualTo (T (25)), "wrong start 4");
                Assert.That (mmMStop.Display, Is.EqualTo (reasonAssociation.ReasonDetails));
              });
            }

            // Test that the translation value is set correctly
            var translation = ModelDAOHelper.DAOFactory.TranslationDAO.Find ("", translationKey);
            Assert.That (translation, Is.Not.Null);


          } finally {
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

    AutoReasonExtension GetAutoReasonExtension (IMonitoredMachine machine, int machineModeId, string dynamicEnd, string translationKey, string translationValue)
    {
      var plugin = new Lemoine.Plugin.AutoReasonMachineMode.Plugin ();
      plugin.Install (0);
      var autoReason = new AutoReasonExtension ();
      autoReason.SetTestConfiguration (
  "{ \"ReasonScore\": 60.0," +
  "\"ManualScore\": 100.0," +
  "\"DynamicEnd\": \"" + dynamicEnd + "\"," +
  "\"DefaultReasonTranslationKey\": \"" + translationKey + "\"," +
  "\"DefaultReasonTranslationValue\": \"" + translationValue + "\"," +
  "\"MachineModeId\": " + machineModeId + "}"
);

      // Initialize the date
      string dateTimeKey = autoReason.GetKey ("DateTime");
      var autoReasonState = ModelDAOHelper.ModelFactory.CreateAutoReasonState (machine, autoReason.GetKey ("DateTime"));
      autoReasonState.Value = T (-1);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.MakePersistent (autoReasonState);

      autoReason.Initialize (machine, null);
      return autoReason;
    }
  }
}
