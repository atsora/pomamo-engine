// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Plugin.ProductionSwitcher;
using NUnit.Framework;
using Lemoine.Core.Log;
using Pulse.Extensions.Extension;

namespace Lemoine.Plugins.UnitTests
{
  /// <summary>
  /// Description of SetupSwitcher_UnitTest.
  /// </summary>
  public class ProductionSwitcher_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger(typeof (ProductionSwitcher_UnitTest).FullName);
    
    /// <summary>
    /// Constructor
    /// </summary>
    public ProductionSwitcher_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }
    
    /// <summary>
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestOperationCycleDetectionExtension1 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        // Reference data
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (2);
        IOperation operation = ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1); // MachiningDuration: 3600s=60min, no loading duration
        IMachineStateTemplate production = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById (9);
        IMachineStateTemplate setup = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById (7);
        
        // Existing operation slots and machine state templates
        {
          var association = ModelDAOHelper.ModelFactory
            .CreateOperationMachineAssociation (machine, R(0, null));
          association.Operation = operation;
          association.Apply ();
        }
        {
          var association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine, setup, T(0));
          association.Apply ();
        }
        ModelDAOHelper.DAOFactory.Flush ();
        IOperationSlot operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindOverlapsRange (machine, R(0, null))
          .First ();
        
        {
          OperationCycleDetectionExtension extension = new OperationCycleDetectionExtension ();
          extension.SetTestConfiguration (@"<?xml version=""1.0"" encoding=""UTF-8""?>
<properties>
  <property>
    <key>SetupMachineStateTemplateId</key>
    <value>7</value>
  </property>
  <property>
    <key>ProductionMachineStateTemplateId</key>
    <value>9</value>
  </property>
  <property>
    <key>CycleDurationPercentageTrigger</key>
    <value>120</value>
  </property>
  <property>
    <key>BetweenCyclesDurationPercentageTrigger</key>
    <value>0</value>
  </property>
</properties>
");
          extension.Initialize (machine);
          extension.DetectionProcessStart ();
          IOperationCycle operationCycle1 = ModelDAOHelper.ModelFactory
            .CreateOperationCycle (machine);
          operationCycle1.OperationSlot = operationSlot;
          operationCycle1.Begin = T(10);
          extension.StartCycle (operationCycle1);
          operationCycle1.SetRealEnd (T(90));
          extension.StopCycle (operationCycle1); // Long cycle: 80 minutes > 72
          {
            IList<IMachineStateTemplateAssociation> associations = ModelDAOHelper.DAOFactory
              .MachineStateTemplateAssociationDAO.FindAll ();
            Assert.AreEqual (0, associations.Count);
          }
          
          IOperationCycle operationCycle2 = ModelDAOHelper.ModelFactory
            .CreateOperationCycle (machine);
          operationCycle2.OperationSlot = operationSlot;
          operationCycle2.Begin = T(100);
          extension.StartCycle (operationCycle2);
          operationCycle2.SetRealEnd (T(170));
          extension.StopCycle (operationCycle2); // Short cycle: 70 minutes < 72
          { // Check the new machine state template
            IMachineStateTemplateAssociation newAssociation = ModelDAOHelper.DAOFactory
              .MachineStateTemplateAssociationDAO.FindAll ().First ();
            Assert.AreEqual (production, newAssociation.MachineStateTemplate);
            Assert.AreEqual (T(100), newAssociation.Begin.Value);
          }
          
          // From now, any change is inhibited
          IOperationCycle operationCycle3 = ModelDAOHelper.ModelFactory
            .CreateOperationCycle (machine);
          operationCycle3.OperationSlot = operationSlot;
          operationCycle3.Begin = T(200);
          extension.StartCycle (operationCycle3);
          operationCycle3.SetRealEnd (T(270));
          extension.StopCycle (operationCycle3); // Short cycle: 70 minutes < 72
          {
            IList<IMachineStateTemplateAssociation> associations = ModelDAOHelper.DAOFactory
              .MachineStateTemplateAssociationDAO.FindAll ();
            Assert.AreEqual (1, associations.Count);
          }
          
          // Until an error is detected...
          extension.DetectionProcessError (machine.MainMachineModule, new Exception ());
          extension.StartCycle (operationCycle3);
          extension.StopCycle (operationCycle3); // Short cycle: 70 minutes < 72
          {
            IList<IMachineStateTemplateAssociation> associations = ModelDAOHelper.DAOFactory
              .MachineStateTemplateAssociationDAO.FindAll ();
            Assert.AreEqual (2, associations.Count);
            Assert.AreEqual (production, associations[1].MachineStateTemplate);
            Assert.AreEqual (T(200), associations[1].Begin.Value);
          }
          
          // From now any change is inhibited
          IOperationCycle operationCycle4 = ModelDAOHelper.ModelFactory
            .CreateOperationCycle (machine);
          operationCycle4.OperationSlot = operationSlot;
          operationCycle4.Begin = T(300);
          extension.StartCycle (operationCycle4);
          operationCycle4.SetRealEnd (T(370));
          extension.StopCycle (operationCycle4); // Short cycle: 70 minutes < 72
          {
            IList<IMachineStateTemplateAssociation> associations = ModelDAOHelper.DAOFactory
              .MachineStateTemplateAssociationDAO.FindAll ();
            Assert.AreEqual (2, associations.Count);
          }
          
          // Until the observation state slots were flagged as modified
          {
            var slotExtension = new SlotExtension ();
            IObservationStateSlot slot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
              .FindAll (machine).Last ();
            slotExtension.AddSlot (slot);
          }
          extension.StartCycle (operationCycle4);
          extension.StopCycle (operationCycle4); // Short cycle: 70 minutes < 72
          {
            IList<IMachineStateTemplateAssociation> associations = ModelDAOHelper.DAOFactory
              .MachineStateTemplateAssociationDAO.FindAll ();
            Assert.AreEqual (3, associations.Count);
            Assert.AreEqual (production, associations[2].MachineStateTemplate);
            Assert.AreEqual (T(300), associations[2].Begin.Value);
          }
        }
        
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestOperationCycleDetectionExtension2 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        // Reference data
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (2);
        IOperation operation = ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1); // MachiningDuration: 3600s=60min, no loading duration
        operation.LoadingDuration = TimeSpan.FromMinutes (10);
        ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);
        IMachineStateTemplate production = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById (9);
        IMachineStateTemplate setup = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById (7);
        
        // Existing operation slots and machine state templates
        {
          var association = ModelDAOHelper.ModelFactory
            .CreateOperationMachineAssociation (machine, R(0, null));
          association.Operation = operation;
          association.Apply ();
        }
        {
          var association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine, setup, T(0));
          association.Apply ();
        }
        ModelDAOHelper.DAOFactory.Flush ();
        IOperationSlot operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindOverlapsRange (machine, R(0, null))
          .First ();
        
        {
          OperationCycleDetectionExtension extension = new OperationCycleDetectionExtension ();
          extension.SetTestConfiguration (@"<?xml version=""1.0"" encoding=""UTF-8""?>
<properties>
  <property>
    <key>SetupMachineStateTemplateId</key>
    <value>7</value>
  </property>
  <property>
    <key>ProductionMachineStateTemplateId</key>
    <value>9</value>
  </property>
  <property>
    <key>CycleDurationPercentageTrigger</key>
    <value>120</value>
  </property>
  <property>
    <key>BetweenCyclesDurationPercentageTrigger</key>
    <value>110</value>
  </property>
</properties>
");
          extension.Initialize (machine);
          extension.DetectionProcessStart ();
          IOperationCycle operationCycle1 = ModelDAOHelper.ModelFactory
            .CreateOperationCycle (machine);
          operationCycle1.OperationSlot = operationSlot;
          operationCycle1.Begin = T(10);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle1);
          extension.StartCycle (operationCycle1);
          operationCycle1.SetRealEnd (T(90));
          extension.StopCycle (operationCycle1); // Long cycle: 80 minutes > 72
          {
            IList<IMachineStateTemplateAssociation> associations = ModelDAOHelper.DAOFactory
              .MachineStateTemplateAssociationDAO.FindAll ();
            Assert.AreEqual (0, associations.Count);
          }
          
          IOperationCycle operationCycle2 = ModelDAOHelper.ModelFactory
            .CreateOperationCycle (machine);
          operationCycle2.OperationSlot = operationSlot;
          operationCycle2.Begin = T(100);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle2);
          extension.StartCycle (operationCycle2);
          
          IBetweenCycles betweenCycles1 = ModelDAOHelper.ModelFactory
            .CreateBetweenCycles (operationCycle1, operationCycle2);
          extension.CreateBetweenCycle (betweenCycles1); // 11 minutes: good !
          
          operationCycle2.SetRealEnd (T(170));
          extension.StopCycle (operationCycle2); // Short cycle: 70 minutes < 72
          {
            IList<IMachineStateTemplateAssociation> associations = ModelDAOHelper.DAOFactory
              .MachineStateTemplateAssociationDAO.FindAll ();
            Assert.AreEqual (0, associations.Count);
          }
          
          IOperationCycle operationCycle3 = ModelDAOHelper.ModelFactory
            .CreateOperationCycle (machine);
          operationCycle3.OperationSlot = operationSlot;
          operationCycle3.Begin = T(200);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle3);
          extension.StartCycle (operationCycle3);
          
          IBetweenCycles betweenCycles2 = ModelDAOHelper.ModelFactory
            .CreateBetweenCycles (operationCycle2, operationCycle3); // 30 minutes: bad !
          extension.CreateBetweenCycle (betweenCycles2);
          {
            IList<IMachineStateTemplateAssociation> associations = ModelDAOHelper.DAOFactory
              .MachineStateTemplateAssociationDAO.FindAll ();
            Assert.AreEqual (0, associations.Count);
          }
          
          operationCycle3.SetRealEnd (T(260));
          extension.StopCycle (operationCycle3); // Short cycle: 60 minutes < 72
          {
            IList<IMachineStateTemplateAssociation> associations = ModelDAOHelper.DAOFactory
              .MachineStateTemplateAssociationDAO.FindAll ();
            Assert.AreEqual (0, associations.Count);
          }
          
          IOperationCycle operationCycle4 = ModelDAOHelper.ModelFactory
            .CreateOperationCycle (machine);
          operationCycle4.OperationSlot = operationSlot;
          operationCycle4.Begin = T(270);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle4);
          extension.StartCycle (operationCycle4);
          
          IBetweenCycles betweenCycles3 = ModelDAOHelper.ModelFactory
            .CreateBetweenCycles (operationCycle3, operationCycle4); // 10 minutes: good !
          extension.CreateBetweenCycle (betweenCycles3);
          { // Check the new machine state template
            IMachineStateTemplateAssociation newAssociation = ModelDAOHelper.DAOFactory
              .MachineStateTemplateAssociationDAO.FindAll ().First ();
            Assert.AreEqual (production, newAssociation.MachineStateTemplate);
            Assert.AreEqual (T(200), newAssociation.Begin.Value);
          }
        }
        
        transaction.Rollback ();
      }
    }
  }
}
