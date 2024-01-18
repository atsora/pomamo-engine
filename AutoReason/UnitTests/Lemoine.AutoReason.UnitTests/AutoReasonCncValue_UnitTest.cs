// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.GDBPersistentClasses;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonCncValue
  /// </summary>
  [TestFixture]
  public class AutoReasonCncValue_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonCncValue_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonCncValue_UnitTest ()
      : base (DateTime.Today.AddMonths (-1))
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
      Assert.That (autoReasonState.Value, Is.EqualTo (v));
    }

    void CheckNoReasonMachineAssociation ()
    {
      var machineModifications = ModelDAOHelper.DAOFactory.MachineModificationDAO
        .FindAll ();
      Assert.That (machineModifications, Is.Empty);
    }

    void CheckReasonMachineAssociation (IReasonMachineAssociation reasonMachineAssociation, IMonitoredMachine machine, UtcDateTimeRange range, string translationKey, string details, double score)
    {
      Assert.Multiple (() => {
        Assert.That (reasonMachineAssociation.Range, Is.EqualTo (range));
        Assert.That (reasonMachineAssociation.ReasonSource, Is.EqualTo (ReasonSource.Auto));
        Assert.That (reasonMachineAssociation.Machine, Is.EqualTo (machine));
        Assert.That (reasonMachineAssociation.Reason.TranslationKey, Is.EqualTo (translationKey));
        Assert.That (reasonMachineAssociation.ReasonDetails, Is.EqualTo (details));
        Assert.That (reasonMachineAssociation.ReasonScore, Is.EqualTo (score));
      });
    }

    /// <summary>
    /// Test case 1:
    /// 
    /// <para>
    /// Feature: Apply a reason when a new cnc value matches the condition punctually
    /// </para>
    /// <para>
    /// Scenario: A cnc value for DryRun is considered. It is switched to True punctually. An auto-reason must be created</para>
    /// <para>Given a CncValue for DryRun Off between T0 and T1. The auto-reason is processed until T1</para>
    /// <para>When the CncValue for DryRun turns to On at T2</para>
    /// <para>Then an auto-reason is created at T2, and the auto-reason state is set at T3</para>
    /// 
    /// <code>
    ///     x--    (auto-reason)
    /// ----+      (Dry run)
    /// 0 1 2 3
    /// </code>
    /// </summary>
    [Test]
    public void TestCase1 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);
            var machineModule = machine.MainMachineModule;
            var field = ModelDAOHelper.DAOFactory.FieldDAO.FindById ((int)FieldId.DryRun);

            var cncValue1 = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, T (0));
            cncValue1.End = T (1);
            cncValue1.Value = false;
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue1);

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonCncValue.Plugin ();
            plugin.Install (0);
            var autoReason = new Lemoine.Plugin.AutoReasonCncValue.AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""DefaultReasonTranslationKey"": ""Test"",
  ""DefaultReasonTranslationValue"": ""Test"",
  ""LambdaCondition"": ""(x) => (bool)x"",
  ""DynamicEnd"": ""CncValueEnd"",
  ""FieldId"": 118
}
");
            bool initializeResult = autoReason.Initialize (machine, null);
            Assert.IsTrue (initializeResult);

            string dateTimeKey = autoReason.GetKey (machineModule, "DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (1));

            cncValue1.End = T (2);
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue1);
            var cncValue2 = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, T (2));
            cncValue2.End = T (2);
            cncValue2.Value = true;
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue2);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (3));
            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.That (reasonMachineAssociations, Has.Count.EqualTo (1));
              int i = 0;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (2), "Test", "True", 90.0);
            }

          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case 2:
    /// 
    /// <para>
    /// Feature: Apply a reason when a new cnc value matches the condition during one second
    /// </para>
    /// <para>
    /// Scenario: A cnc value for DryRun is considered. It is switched to True during one second (previously off). An auto-reason must be created</para>
    /// <para>Given a CncValue for DryRun Off between T0 and T2. The auto-reason is processed until T1</para>
    /// <para>When the CncValue for DryRun turns to On between T2 and T3</para>
    /// <para>Then an auto-reason is created at T2, and the auto-reason state is set at T4</para>
    /// 
    /// <code>
    ///     x--    (auto-reason)
    /// ----+++    (Dry run)
    /// 0 1 2 3
    /// </code>
    /// </summary>
    [Test]
    public void TestCase2 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);
            var machineModule = machine.MainMachineModule;
            var field = ModelDAOHelper.DAOFactory.FieldDAO.FindById ((int)FieldId.DryRun);

            var cncValue1 = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, T (0));
            cncValue1.End = T (1);
            cncValue1.Value = false;
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue1);

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonCncValue.Plugin ();
            plugin.Install (0);
            var autoReason = new Lemoine.Plugin.AutoReasonCncValue.AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""DefaultReasonTranslationKey"": ""Test"",
  ""DefaultReasonTranslationValue"": ""Test"",
  ""LambdaCondition"": ""(x) => (bool)x"",
  ""DynamicEnd"": ""CncValueEnd"",
  ""FieldId"": 118
}
");
            bool initializeResult = autoReason.Initialize (machine, null);
            Assert.IsTrue (initializeResult);

            string dateTimeKey = autoReason.GetKey (machineModule, "DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (1));

            cncValue1.End = T (2);
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue1);
            var cncValue2 = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, T (2));
            cncValue2.End = T (3);
            cncValue2.Value = true;
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue2);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (4));
            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.That (reasonMachineAssociations, Has.Count.EqualTo (1));
              int i = 0;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (2), "Test", "True", 90.0);
            }

          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case 3:
    /// 
    /// <para>
    /// Feature: Apply a reason when a new cnc value matches the condition during one second, when the cnc value starts before the auto-reason state
    /// </para>
    /// <para>
    /// Scenario: A cnc value for DryRun is considered. It is switched to True during one second (previously off) one second before the auto-reason state. An auto-reason must be created</para>
    /// <para>Given a CncValue for DryRun Off between T0 and T1. The auto-reason is processed until T2</para>
    /// <para>When the CncValue for DryRun turns to On between T2 and T3</para>
    /// <para>Then an auto-reason is created at T2, and the auto-reason state is set to T4</para>
    /// 
    /// <code>
    ///   x--    (auto-reason)
    /// --+++      (Dry run)
    /// 0 1 2 3
    /// </code>
    /// </summary>
    [Test]
    public void TestCase3 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);
            var machineModule = machine.MainMachineModule;
            var field = ModelDAOHelper.DAOFactory.FieldDAO.FindById ((int)FieldId.DryRun);

            var cncValue1 = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, T (0));
            cncValue1.End = T (1);
            cncValue1.Value = false;
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue1);

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonCncValue.Plugin ();
            plugin.Install (0);
            var autoReason = new Lemoine.Plugin.AutoReasonCncValue.AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""DefaultReasonTranslationKey"": ""Test"",
  ""DefaultReasonTranslationValue"": ""Test"",
  ""LambdaCondition"": ""(x) => (bool)x"",
  ""DynamicEnd"": ""CncValueEnd"",
  ""FieldId"": 118
}
");
            bool initializeResult = autoReason.Initialize (machine, null);
            Assert.IsTrue (initializeResult);

            string dateTimeKey = autoReason.GetKey (machineModule, "DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (2));

            cncValue1.End = T (2);
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue1);
            var cncValue2 = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, T (2));
            cncValue2.End = T (3);
            cncValue2.Value = true;
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue2);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (4));
            { // Test ReasonMachineAssociation
              var reasonMachineAssociations = new ReasonMachineAssociationDAO ()
                .FindAll ();
              Assert.That (reasonMachineAssociations, Has.Count.EqualTo (1));
              int i = 0;
              CheckReasonMachineAssociation (reasonMachineAssociations[i],
                machine, R (2), "Test", "True", 90.0);
            }

          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case 4:
    /// 
    /// <para>
    /// Feature: Do not apply a reason when a new cnc value does not match the condition
    /// </para>
    /// <para>
    /// Scenario: A cnc value for DryRun is considered. It is switched to False during one second (previously on). No new auto-reason must be created</para>
    /// <para>Given a CncValue for DryRun On between T0 and T1. The auto-reason is processed until T2</para>
    /// <para>When the CncValue for DryRun turns to Off between T2 and T3</para>
    /// <para>Then no an auto-reason is created, and the auto-reason state is set to T4</para>
    /// </summary>
    [Test]
    public void TestCase4 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);
            var machineModule = machine.MainMachineModule;
            var field = ModelDAOHelper.DAOFactory.FieldDAO.FindById ((int)FieldId.DryRun);

            var cncValue1 = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, T (0));
            cncValue1.End = T (1);
            cncValue1.Value = true;
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue1);

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonCncValue.Plugin ();
            plugin.Install (0);
            var autoReason = new Lemoine.Plugin.AutoReasonCncValue.AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""DefaultReasonTranslationKey"": ""Test"",
  ""DefaultReasonTranslationValue"": ""Test"",
  ""LambdaCondition"": ""(x) => (bool)x"",
  ""DynamicEnd"": ""CncValueEnd"",
  ""FieldId"": 118
}
");
            bool initializeResult = autoReason.Initialize (machine, null);
            Assert.IsTrue (initializeResult);

            string dateTimeKey = autoReason.GetKey (machineModule, "DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (2));

            cncValue1.End = T (2);
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue1);
            var cncValue2 = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, T (2));
            cncValue2.End = T (3);
            cncValue2.Value = false;
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue2);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (4));
            CheckNoReasonMachineAssociation ();
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case 5:
    /// 
    /// <para>
    /// Feature: Do not apply a new reason when a matching cnc value just gets longer
    /// </para>
    /// <para>
    /// Scenario: </para>
    /// <para>Given a CncValue for DryRun On between T0 and T1. The auto-reason is processed until T2</para>
    /// <para>When the CncValue for DryRun On gets longer until T3</para>
    /// <para>Then no new auto-reason is created</para>
    /// </summary>
    [Test]
    public void TestCase5 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);
            var machineModule = machine.MainMachineModule;
            var field = ModelDAOHelper.DAOFactory.FieldDAO.FindById ((int)FieldId.DryRun);

            var cncValue1 = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, T (0));
            cncValue1.End = T (1);
            cncValue1.Value = true;
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue1);

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonCncValue.Plugin ();
            plugin.Install (0);
            var autoReason = new Lemoine.Plugin.AutoReasonCncValue.AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""DefaultReasonTranslationKey"": ""Test"",
  ""DefaultReasonTranslationValue"": ""Test"",
  ""LambdaCondition"": ""(x) => (bool)x"",
  ""DynamicEnd"": ""CncValueEnd"",
  ""FieldId"": 118
}
");
            bool initializeResult = autoReason.Initialize (machine, null);
            Assert.IsTrue (initializeResult);

            string dateTimeKey = autoReason.GetKey (machineModule, "DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (2));

            cncValue1.End = T (3);
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue1);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (4));
            CheckNoReasonMachineAssociation ();
          }
          finally {
            transaction.Rollback ();
          }
        }
      }
    }

    /// <summary>
    /// Test case 6:
    /// 
    /// <para>
    /// Feature: Do not apply a new reason when a matching cnc value just gets longer when it was initially with a null duration
    /// </para>
    /// <para>
    /// Scenario: </para>
    /// <para>Given a CncValue for DryRun On at T0. The auto-reason is processed until T1</para>
    /// <para>When the CncValue for DryRun On gets longer until T3</para>
    /// <para>Then no new auto-reason is created</para>
    /// </summary>
    [Test]
    public void TestCase6 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.That (machine, Is.Not.Null);
            var machineModule = machine.MainMachineModule;
            var field = ModelDAOHelper.DAOFactory.FieldDAO.FindById ((int)FieldId.DryRun);

            var cncValue1 = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, T (0));
            cncValue1.End = T (0);
            cncValue1.Value = true;
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue1);

            // Plugin
            var plugin = new Lemoine.Plugin.AutoReasonCncValue.Plugin ();
            plugin.Install (0);
            var autoReason = new Lemoine.Plugin.AutoReasonCncValue.AutoReasonExtension ();
            autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 90.0,
  ""ManualScore"": 100.0,
  ""DefaultReasonTranslationKey"": ""Test"",
  ""DefaultReasonTranslationValue"": ""Test"",
  ""LambdaCondition"": ""(x) => (bool)x"",
  ""DynamicEnd"": ""CncValueEnd"",
  ""FieldId"": 118
}
");
            bool initializeResult = autoReason.Initialize (machine, null);
            Assert.IsTrue (initializeResult);

            string dateTimeKey = autoReason.GetKey (machineModule, "DateTime");
            CreateAutoReasonState (machine, dateTimeKey, T (1));

            cncValue1.End = T (3);
            ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue1);

            autoReason.RunOnce ();
            CheckAutoReasonState (machine, dateTimeKey, T (4));
            CheckNoReasonMachineAssociation ();
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
