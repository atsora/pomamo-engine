// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
    static readonly int OTHER_MACHINE_ID = 1;
    static readonly int OPERATION_ID = 1; // MachiningDuration: 3600s=60min, no loading duration
    static readonly int SETUP_MACHINE_STATE_TEMPLATE_ID = 7;
    static readonly int PRODUCTION_MACHINE_STATE_TEMPLATE_ID = 9;
    static readonly int UNKNOWN_MACHINE_FILTER_ID = 999999;

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

    /// <summary>
    /// With the default configuration, one good cycle is enough:
    /// this is the historical behaviour
    /// </summary>
    [Test]
    public void TestOneGoodCycleByDefault ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operationSlot = InitializeSlots (machine, GetOperation (),
            GetMachineStateTemplate (SETUP_MACHINE_STATE_TEMPLATE_ID));
          var production = GetMachineStateTemplate (PRODUCTION_MACHINE_STATE_TEMPLATE_ID);

          var extension = CreateExtension (machine, GetConfiguration ());

          extension.StopCycle (AddFullCycle (machine, operationSlot, 10, 70)); // 60 min <= 72

          var associations = GetAssociations ();
          Assert.That (associations, Has.Count.EqualTo (1));
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
    /// When 2 good cycles are required, the switch only happens on the second one,
    /// and it starts at the begin of the first cycle of the serie
    /// </summary>
    [Test]
    public void TestTwoGoodCyclesRequired ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operationSlot = InitializeSlots (machine, GetOperation (),
            GetMachineStateTemplate (SETUP_MACHINE_STATE_TEMPLATE_ID));
          var production = GetMachineStateTemplate (PRODUCTION_MACHINE_STATE_TEMPLATE_ID);

          var extension = CreateExtension (machine, GetConfiguration (numberOfGoodCycles: 2));

          extension.StopCycle (AddFullCycle (machine, operationSlot, 10, 70)); // good
          Assert.That (GetAssociations (), Is.Empty, "one good cycle is not enough");

          extension.StopCycle (AddFullCycle (machine, operationSlot, 80, 140)); // good

          var associations = GetAssociations ();
          Assert.That (associations, Has.Count.EqualTo (1));
          Assert.Multiple (() => {
            Assert.That (associations[0].MachineStateTemplate, Is.EqualTo (production));
            Assert.That (associations[0].Begin.Value, Is.EqualTo (T (10)),
              "the switch starts at the begin of the first good cycle of the serie");
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// A bad cycle breaks the serie of consecutive good cycles
    /// </summary>
    [Test]
    public void TestBadCycleBreaksTheSerie ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operationSlot = InitializeSlots (machine, GetOperation (),
            GetMachineStateTemplate (SETUP_MACHINE_STATE_TEMPLATE_ID));

          var extension = CreateExtension (machine, GetConfiguration (numberOfGoodCycles: 2));

          extension.StopCycle (AddFullCycle (machine, operationSlot, 10, 70)); // good
          extension.StopCycle (AddFullCycle (machine, operationSlot, 80, 180)); // 100 min > 72: bad
          Assert.That (GetAssociations (), Is.Empty, "the serie is broken by the bad cycle");

          extension.StopCycle (AddFullCycle (machine, operationSlot, 190, 250)); // good
          Assert.That (GetAssociations (), Is.Empty, "only one good cycle since the bad one");

          extension.StopCycle (AddFullCycle (machine, operationSlot, 260, 320)); // good

          var associations = GetAssociations ();
          Assert.That (associations, Has.Count.EqualTo (1));
          Assert.That (associations[0].Begin.Value, Is.EqualTo (T (190)),
            "the serie restarts after the bad cycle");
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// A configuration that does not set the number of good cycles keeps the historical
    /// behaviour (1), and a number below 1 is rejected
    /// </summary>
    [Test]
    public void TestNumberOfGoodCyclesConfiguration ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var loader = new Lemoine.Extensions.Configuration.ConfigurationLoader<Configuration> ();

          { // An existing configuration, saved before the parameter was added
            var configuration = loader.LoadConfiguration ($$"""
{
  "SetupMachineStateTemplateIds": [ {{SETUP_MACHINE_STATE_TEMPLATE_ID}} ],
  "ProductionMachineStateTemplateId": {{PRODUCTION_MACHINE_STATE_TEMPLATE_ID}},
  "CycleDurationPercentageTrigger": 120,
  "BetweenCyclesDurationPercentageTrigger": 0
}
""");
            Assert.That (configuration.NumberOfGoodCycles, Is.EqualTo (1));
            Assert.That (configuration.IsValid (out var errors), Is.True);
          }

          { // An explicit number of good cycles
            var configuration = loader.LoadConfiguration ($$"""
{
  "SetupMachineStateTemplateIds": [ {{SETUP_MACHINE_STATE_TEMPLATE_ID}} ],
  "ProductionMachineStateTemplateId": {{PRODUCTION_MACHINE_STATE_TEMPLATE_ID}},
  "CycleDurationPercentageTrigger": 120,
  "BetweenCyclesDurationPercentageTrigger": 0,
  "NumberOfGoodCycles": 3
}
""");
            Assert.That (configuration.NumberOfGoodCycles, Is.EqualTo (3));
            Assert.That (configuration.IsValid (out var errors), Is.True);
          }

          { // An invalid number of good cycles
            var configuration = new Configuration {
              ProductionMachineStateTemplateId = PRODUCTION_MACHINE_STATE_TEMPLATE_ID,
              NumberOfGoodCycles = 0
            };
            var valid = configuration.IsValid (out var errors);
            Assert.Multiple (() => {
              Assert.That (valid, Is.False);
              Assert.That (errors, Has.Exactly (1).Items);
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// The switch happens when the machine matches the configured machine filter
    /// </summary>
    [Test]
    public void TestMachineFilterMatch ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operationSlot = InitializeSlots (machine, GetOperation (),
            GetMachineStateTemplate (SETUP_MACHINE_STATE_TEMPLATE_ID));
          var machineFilter = CreateMachineFilter (machine);

          var extension = CreateExtension (machine,
            GetConfiguration (machineFilterId: machineFilter.Id));
          extension.StopCycle (AddFullCycle (machine, operationSlot, 10, 70));

          Assert.That (GetAssociations (), Has.Count.EqualTo (1));
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Nothing happens when the machine does not match the configured machine filter
    /// </summary>
    [Test]
    public void TestMachineFilterNoMatch ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var otherMachine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (OTHER_MACHINE_ID);
          var operationSlot = InitializeSlots (machine, GetOperation (),
            GetMachineStateTemplate (SETUP_MACHINE_STATE_TEMPLATE_ID));
          var machineFilter = CreateMachineFilter (otherMachine);

          var extension = CreateExtension (machine,
            GetConfiguration (machineFilterId: machineFilter.Id));
          extension.StopCycle (AddFullCycle (machine, operationSlot, 10, 70));

          Assert.That (GetAssociations (), Is.Empty);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Nothing happens when the configured machine filter does not exist
    /// </summary>
    [Test]
    public void TestUnknownMachineFilter ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operationSlot = InitializeSlots (machine, GetOperation (),
            GetMachineStateTemplate (SETUP_MACHINE_STATE_TEMPLATE_ID));

          var extension = CreateExtension (machine,
            GetConfiguration (machineFilterId: UNKNOWN_MACHINE_FILTER_ID));
          extension.StopCycle (AddFullCycle (machine, operationSlot, 10, 70));

          Assert.That (GetAssociations (), Is.Empty);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// The pallet changing duration of the machine is used as the standard between
    /// cycles duration, and it has the priority on the loading/unloading durations
    /// </summary>
    [Test]
    public void TestPalletChangingDurationAsBetweenDuration ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          machine.PalletChangingDuration = TimeSpan.FromMinutes (10);
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);

          var operation = GetOperation ();
          // Much longer than the pallet changing duration, to check it is not used
          operation.LoadingDuration = TimeSpan.FromMinutes (100);
          ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);

          var operationSlot = InitializeSlots (machine, operation,
            GetMachineStateTemplate (SETUP_MACHINE_STATE_TEMPLATE_ID));

          // 110 % of 10 min => a gap of up to 11 min is good
          var extension = CreateExtension (machine,
            GetConfiguration (betweenCyclesDurationPercentageTrigger: 110));

          var cycle1 = AddFullCycle (machine, operationSlot, 10, 70);
          extension.StopCycle (cycle1); // the between duration must be considered => nothing yet
          Assert.That (GetAssociations (), Is.Empty);

          var cycle2 = AddFullCycle (machine, operationSlot, 85, 145); // gap of 15 min: too long
          extension.CreateBetweenCycle (ModelDAOHelper.ModelFactory
            .CreateBetweenCycles (cycle1, cycle2));
          Assert.That (GetAssociations (), Is.Empty, "a 15 min gap exceeds 11 min");

          var cycle3 = AddFullCycle (machine, operationSlot, 156, 216); // gap of 11 min: good
          extension.CreateBetweenCycle (ModelDAOHelper.ModelFactory
            .CreateBetweenCycles (cycle2, cycle3));

          var associations = GetAssociations ();
          Assert.That (associations, Has.Count.EqualTo (1), "an 11 min gap is within 11 min");
          Assert.That (associations[0].Begin.Value, Is.EqualTo (T (85)));
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Once the switch to production is done, it is not done a second time
    /// until the observation state slots are flagged as modified
    /// </summary>
    [Test]
    public void TestNoSwitchTwiceUntilSlotChange ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operationSlot = InitializeSlots (machine, GetOperation (),
            GetMachineStateTemplate (SETUP_MACHINE_STATE_TEMPLATE_ID));

          var extension = CreateExtension (machine, GetConfiguration ());

          extension.StopCycle (AddFullCycle (machine, operationSlot, 10, 70));
          Assert.That (GetAssociations (), Has.Count.EqualTo (1));

          extension.StopCycle (AddFullCycle (machine, operationSlot, 80, 140));
          Assert.That (GetAssociations (), Has.Count.EqualTo (1),
            "the pending change inhibits any new switch");

          // Flag the observation state slots as modified
          var slotExtension = new SlotExtension ();
          slotExtension.AddSlot (ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindAll (machine).Last ());

          extension.StopCycle (AddFullCycle (machine, operationSlot, 150, 210));
          var associations = GetAssociations ();
          Assert.That (associations, Has.Count.EqualTo (2));
          Assert.That (associations[1].Begin.Value, Is.EqualTo (T (150)));
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// The notifier must not keep the extensions of the obsolete activity analyses alive:
    /// a new instance registers itself on every analysis creation
    /// </summary>
    [Test]
    public void TestListenersAreNotLeaked ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operationSlot = InitializeSlots (machine, GetOperation (),
            GetMachineStateTemplate (SETUP_MACHINE_STATE_TEMPLATE_ID));
          var slot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.FindAll (machine).Last ();

          RegisterTransientExtensions (machine, 50);
          // Note: the count is taken after the registration and not before, because
          //       AddListener also purges, so listeners left by the previous tests
          //       may have been removed in the meantime
          var afterRegistration = ObservationStateSlotChangeNotifier.ListenerCount;
          Assert.That (afterRegistration, Is.GreaterThanOrEqualTo (50),
            "the extensions did register themselves");

          CollectGarbage ();
          // The purge is done while notifying
          ObservationStateSlotChangeNotifier.NotifyChanges (slot);

          Assert.That (ObservationStateSlotChangeNotifier.ListenerCount,
            Is.LessThanOrEqualTo (afterRegistration - 50),
            "the extensions that are not referenced any more must have been purged");

          // A listener that is still referenced keeps being notified
          var liveExtension = CreateExtension (machine, GetConfiguration ());
          CollectGarbage ();
          KeepAlive (liveExtension);
          Assert.That (ObservationStateSlotChangeNotifier.ListenerCount,
            Is.GreaterThan (0), "a referenced listener must not be purged");

          liveExtension.StopCycle (AddFullCycle (machine, operationSlot, 10, 70));
          Assert.That (GetAssociations (), Has.Count.EqualTo (1),
            "the live extension is still functional");
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Register extensions and drop every reference on them.
    ///
    /// This must be a separate method, else the local variables may be kept alive
    /// until the end of the calling method in a debug build
    /// </summary>
    [MethodImpl (MethodImplOptions.NoInlining)]
    void RegisterTransientExtensions (IMonitoredMachine machine, int count)
    {
      for (int i = 0; i < count; ++i) {
        var extension = new OperationCycleDetectionExtension ();
        extension.Initialize (machine);
      }
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    void KeepAlive (object o)
    {
      GC.KeepAlive (o);
    }

    static void CollectGarbage ()
    {
      GC.Collect ();
      GC.WaitForPendingFinalizers ();
      GC.Collect ();
    }

    /// <summary>
    /// Build a Json configuration
    /// </summary>
    string GetConfiguration (int numberOfGoodCycles = 1,
                             int betweenCyclesDurationPercentageTrigger = 0,
                             int? machineFilterId = null)
    {
      var machineFilterProperty = machineFilterId.HasValue
        ? $@", ""MachineFilterId"": {machineFilterId.Value}"
        : "";
      return $@"{{
  ""SetupMachineStateTemplateIds"": [ {SETUP_MACHINE_STATE_TEMPLATE_ID} ],
  ""ProductionMachineStateTemplateId"": {PRODUCTION_MACHINE_STATE_TEMPLATE_ID},
  ""CycleDurationPercentageTrigger"": 120,
  ""BetweenCyclesDurationPercentageTrigger"": {betweenCyclesDurationPercentageTrigger},
  ""NumberOfGoodCycles"": {numberOfGoodCycles}{machineFilterProperty}
}}";
    }

    OperationCycleDetectionExtension CreateExtension (IMonitoredMachine machine, string configuration)
    {
      var extension = new OperationCycleDetectionExtension ();
      extension.SetTestConfiguration (configuration);
      extension.Initialize (machine);
      extension.DetectionProcessStart ();
      return extension;
    }

    /// <summary>
    /// Create a full operation cycle, from T(begin) to T(end)
    /// </summary>
    IOperationCycle AddFullCycle (IMonitoredMachine machine, IOperationSlot operationSlot,
                                  double begin, double end)
    {
      var operationCycle = ModelDAOHelper.ModelFactory.CreateOperationCycle (machine);
      operationCycle.OperationSlot = operationSlot;
      operationCycle.Begin = T (begin);
      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle);
      operationCycle.SetRealEnd (T (end));
      return operationCycle;
    }

    IMachineFilter CreateMachineFilter (IMachine machine)
    {
      var machineFilter = ModelDAOHelper.ModelFactory
        .CreateMachineFilter ("ProductionSwitcherTest", MachineFilterInitialSet.None);
      machineFilter.Items.Add (ModelDAOHelper.ModelFactory
        .CreateMachineFilterItem (machine, MachineFilterRule.Add));
      ModelDAOHelper.DAOFactory.MachineFilterDAO.MakePersistent (machineFilter);
      ModelDAOHelper.DAOFactory.Flush ();
      return machineFilter;
    }

    IList<IMachineStateTemplateAssociation> GetAssociations ()
    {
      ModelDAOHelper.DAOFactory.Flush ();
      return ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
        .FindAll ()
        .OrderBy (a => a.Begin)
        .ToList ();
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
