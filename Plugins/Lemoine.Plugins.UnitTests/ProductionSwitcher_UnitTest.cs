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
    static readonly int MACHINE_ID = 2;
    static readonly int OPERATION_ID = 1; // MachiningDuration: 3600s=60min, no loading duration
    static readonly int SETUP_MACHINE_STATE_TEMPLATE_ID = 7;
    static readonly int PRODUCTION_MACHINE_STATE_TEMPLATE_ID = 9;

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
          extension.SetTestConfiguration ("""
{
  "SetupMachineStateTemplateIds": [ 7 ],
  "ProductionMachineStateTemplateId": 9,
  "CycleDurationPercentageTrigger": 120,
  "BetweenCyclesDurationPercentageTrigger": 0
}
""");
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
            Assert.That (associations, Is.Empty);
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
            Assert.Multiple (() => {
              Assert.That (newAssociation.MachineStateTemplate, Is.EqualTo (production));
              Assert.That (newAssociation.Begin.Value, Is.EqualTo (T (100)));
            });
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
            Assert.That (associations, Has.Count.EqualTo (1));
          }
          
          // Until an error is detected...
          extension.DetectionProcessError (machine.MainMachineModule, new Exception ());
          extension.StartCycle (operationCycle3);
          extension.StopCycle (operationCycle3); // Short cycle: 70 minutes < 72
          {
            IList<IMachineStateTemplateAssociation> associations = ModelDAOHelper.DAOFactory
              .MachineStateTemplateAssociationDAO.FindAll ();
            Assert.That (associations, Has.Count.EqualTo (2));
            Assert.Multiple (() => {
              Assert.That (associations[1].MachineStateTemplate, Is.EqualTo (production));
              Assert.That (associations[1].Begin.Value, Is.EqualTo (T (200)));
            });
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
            Assert.That (associations, Has.Count.EqualTo (2));
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
            Assert.That (associations, Has.Count.EqualTo (3));
            Assert.Multiple (() => {
              Assert.That (associations[2].MachineStateTemplate, Is.EqualTo (production));
              Assert.That (associations[2].Begin.Value, Is.EqualTo (T (300)));
            });
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
          extension.SetTestConfiguration ("""
{
  "SetupMachineStateTemplateIds": [ 7 ],
  "ProductionMachineStateTemplateId": 9,
  "CycleDurationPercentageTrigger": 120,
  "BetweenCyclesDurationPercentageTrigger": 110
}
""");
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
            Assert.That (associations, Is.Empty);
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
            Assert.That (associations, Is.Empty);
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
            Assert.That (associations, Is.Empty);
          }
          
          operationCycle3.SetRealEnd (T(260));
          extension.StopCycle (operationCycle3); // Short cycle: 60 minutes < 72
          {
            IList<IMachineStateTemplateAssociation> associations = ModelDAOHelper.DAOFactory
              .MachineStateTemplateAssociationDAO.FindAll ();
            Assert.That (associations, Is.Empty);
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
            Assert.Multiple (() => {
              Assert.That (newAssociation.MachineStateTemplate, Is.EqualTo (production));
              Assert.That (newAssociation.Begin.Value, Is.EqualTo (T (200)));
            });
          }
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// When no set-up machine state template is configured, all of them apply
    /// (as documented in the configuration description)
    /// </summary>
    [Test]
    public void TestEmptySetupMachineStateTemplatesMeansAll ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operation = GetOperation (); // MachiningDuration: 60 min
          var production = GetMachineStateTemplate (PRODUCTION_MACHINE_STATE_TEMPLATE_ID);
          var setup = GetMachineStateTemplate (SETUP_MACHINE_STATE_TEMPLATE_ID);

          var operationSlot = InitializeSlots (machine, operation, setup);

          var extension = new OperationCycleDetectionExtension ();
          extension.SetTestConfiguration ($$"""
{
  "SetupMachineStateTemplateIds": [],
  "ProductionMachineStateTemplateId": {{PRODUCTION_MACHINE_STATE_TEMPLATE_ID}},
  "CycleDurationPercentageTrigger": 120,
  "BetweenCyclesDurationPercentageTrigger": 0
}
""");
          extension.Initialize (machine);
          extension.DetectionProcessStart ();

          var operationCycle = ModelDAOHelper.ModelFactory.CreateOperationCycle (machine);
          operationCycle.OperationSlot = operationSlot;
          operationCycle.Begin = T (10);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle);
          extension.StartCycle (operationCycle);
          operationCycle.SetRealEnd (T (70)); // 60 min <= 60 * 1.2 => good cycle
          extension.StopCycle (operationCycle);

          var associations = ModelDAOHelper.DAOFactory
            .MachineStateTemplateAssociationDAO.FindAll ();
          Assert.That (associations, Has.Count.EqualTo (1),
            "an empty set-up list must mean that every machine state template applies");
          Assert.Multiple (() => {
            Assert.That (associations[0].MachineStateTemplate, Is.EqualTo (production));
            Assert.That (associations[0].Begin.Value, Is.EqualTo (T (10)));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// A full cycle without any begin must not make the detection fail,
    /// even when the operation has no machining duration
    /// </summary>
    [Test]
    public void TestFullCycleWithoutBegin ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operation = GetOperation ();
          operation.MachiningDuration = null; // => IsGoodCycle only relies on Full
          ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);
          var setup = GetMachineStateTemplate (SETUP_MACHINE_STATE_TEMPLATE_ID);

          var operationSlot = InitializeSlots (machine, operation, setup);

          var extension = new OperationCycleDetectionExtension ();
          extension.SetTestConfiguration ($$"""
{
  "SetupMachineStateTemplateIds": [ {{SETUP_MACHINE_STATE_TEMPLATE_ID}} ],
  "ProductionMachineStateTemplateId": {{PRODUCTION_MACHINE_STATE_TEMPLATE_ID}},
  "CycleDurationPercentageTrigger": 120,
  "BetweenCyclesDurationPercentageTrigger": 0
}
""");
          extension.Initialize (machine);
          extension.DetectionProcessStart ();

          var operationCycle = ModelDAOHelper.ModelFactory.CreateOperationCycle (machine);
          operationCycle.OperationSlot = operationSlot;
          // No begin on purpose
          operationCycle.SetRealEnd (T (70)); // sets Full, although Begin is null
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle);
          Assert.Multiple (() => {
            Assert.That (operationCycle.Full, Is.True);
            Assert.That (operationCycle.Begin.HasValue, Is.False);
          });

          Assert.DoesNotThrow (() => extension.StopCycle (operationCycle));

          // Without a begin, no start date/time can be used for the switch
          Assert.That (ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO.FindAll (),
            Is.Empty);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    IMonitoredMachine GetMachine ()
    {
      var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (MACHINE_ID);
      Assert.That (machine, Is.Not.Null);
      return machine;
    }

    IOperation GetOperation ()
    {
      var operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (OPERATION_ID);
      Assert.That (operation, Is.Not.Null);
      return operation;
    }

    IMachineStateTemplate GetMachineStateTemplate (int machineStateTemplateId)
    {
      var machineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
        .FindById (machineStateTemplateId);
      Assert.That (machineStateTemplate, Is.Not.Null);
      return machineStateTemplate;
    }

    /// <summary>
    /// Apply an operation and a machine state template from T(0),
    /// then return the associated operation slot
    /// </summary>
    IOperationSlot InitializeSlots (IMonitoredMachine machine, IOperation operation,
                                    IMachineStateTemplate machineStateTemplate)
    {
      {
        var association = ModelDAOHelper.ModelFactory
          .CreateOperationMachineAssociation (machine, R (0, null));
        association.Operation = operation;
        association.Apply ();
      }
      {
        var association = ModelDAOHelper.ModelFactory
          .CreateMachineStateTemplateAssociation (machine, machineStateTemplate, T (0));
        association.Apply ();
      }
      ModelDAOHelper.DAOFactory.Flush ();

      return ModelDAOHelper.DAOFactory.OperationSlotDAO
        .FindOverlapsRange (machine, R (0, null))
        .First ();
    }
  }
}
