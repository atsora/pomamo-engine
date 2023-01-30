// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.Core.Log;
using Lemoine.GDBPersistentClasses;
using Lemoine.Collections;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class AutoReasonForward_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (AutoReasonForward_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonForward_UnitTest ()
      : base (DateTime.Today.AddDays (-1))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }


    void CreateAutoReasonState (IMonitoredMachine machine, string key, object v)
    {
      var autoReasonState = ModelDAOHelper.ModelFactory.CreateAutoReasonState (machine, key);
      autoReasonState.Value = v;
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.MakePersistent (autoReasonState);
    }

    void CheckAutoReasonState (IMonitoredMachine machine, string key, object v)
    {
      var autoReasonState = ModelDAOHelper.DAOFactory.AutoReasonStateDAO.GetAutoReasonState (machine, key);
      Assert.AreEqual (v, autoReasonState.Value);
    }

    void CheckNoReasonMachineAssociation ()
    {
      var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
        .FindAll ();
      Assert.AreEqual (0, reasonMachineAssociations.Count);
    }

    void CheckReasonMachineAssociation (IReasonMachineAssociation reasonMachineAssociation, IMonitoredMachine machine, UtcDateTimeRange range, string translationKey, string details, double score)
    {
      Assert.AreEqual (range, reasonMachineAssociation.Range);
      Assert.AreEqual (ReasonSource.Auto, reasonMachineAssociation.ReasonSource);
      Assert.AreEqual (machine, reasonMachineAssociation.Machine);
      Assert.AreEqual (translationKey, reasonMachineAssociation.Reason.TranslationKey);
      Assert.AreEqual (details, reasonMachineAssociation.ReasonDetails);
      Assert.AreEqual (score, reasonMachineAssociation.ReasonScore);
    }

    /// <summary>
    /// Test case 1
    /// 
    /// <para>
    /// Feature: create a reason machine association on a machine and test it is forwarded to the other machine
    /// </para>
    /// <para>
    /// Scenario: </para>
    /// <para>Given an auto-reason machine association on machine 1</para>
    /// <para>When the auto-reason plugin is run</para>
    /// <para>Then the auto-reason machine association from machine 1 is copied to machine 2, only the score changes</para>
    /// 
    /// <code>
    /// </code>
    /// </summary>
    [Test]
    public void TestCase1 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // References
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);
            var targetMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
            Assert.NotNull (targetMachine);
            var coffee = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (28);
            Assert.NotNull (coffee);

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonForward.Plugin ();
            plugin.Install (0);
            var autoReason = new Lemoine.Plugin.AutoReasonForward.AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""ReasonId"": 28,
  ""TargetMachineId"": 2
}
");
            bool initializeResult = autoReason.Initialize (machine, null);
            Assert.IsTrue (initializeResult);

            string lastModificationIdKey = autoReason.GetKey ("LastModificationId");
            /*
            CreateAutoReasonState (machine, lastModificationIdKey, 0);
            */

            // Initialize the data
            var reasonMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateReasonMachineAssociation (machine, R (1, 2));
            reasonMachineAssociation.SetAutoReason (coffee, 60, true, "Details");
            reasonMachineAssociation = new ReasonMachineAssociationDAO ().MakePersistent (reasonMachineAssociation);

            ModelDAOHelper.DAOFactory.Flush ();

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, lastModificationIdKey, ((IDataWithId<long>)reasonMachineAssociation).Id);
            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ()
                .Where (r => r.Machine.Id == targetMachine.Id)
                .ToList ();
              Assert.AreEqual (1, reasonMachineAssociations.Count);
              int i = 0;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                targetMachine, R (1, 2), null, "Details", 90.0);
              Assert.AreEqual (coffee, reasonMachineAssociations[i].Reason);
            }

          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    [SetUp]
    public void Setup ()
    {
    }

    [TearDown]
    public void TearDown ()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
    }
  }
}
