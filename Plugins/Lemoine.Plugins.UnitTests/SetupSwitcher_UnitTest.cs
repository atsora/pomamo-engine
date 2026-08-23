// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Plugin.SetupSwitcher;
using NUnit.Framework;
using Lemoine.Core.Log;
using Pulse.Extensions.Extension;

namespace Lemoine.Plugins.UnitTests
{
  /// <summary>
  /// Unit tests of the SetupSwitcher plugin.
  ///
  /// The plugin must switch the machine state template to a set-up one
  /// as soon as the detected operation changes.
  /// </summary>
  public class SetupSwitcher_UnitTest
    : Lemoine.UnitTests.WithDayTimeStamp
  {
    static readonly int MACHINE_ID = 2;
    static readonly int OTHER_MACHINE_ID = 1;
    static readonly int OLD_OPERATION_ID = 1;
    static readonly int NEW_OPERATION_ID = 2;
    static readonly int SETUP_MACHINE_STATE_TEMPLATE_ID = 7;
    static readonly int PRODUCTION_MACHINE_STATE_TEMPLATE_ID = 9;
    static readonly int UNKNOWN_MACHINE_STATE_TEMPLATE_ID = 999999;
    static readonly int UNKNOWN_MACHINE_FILTER_ID = 999999;

    readonly ILog log = LogManager.GetLogger (typeof (SetupSwitcher_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public SetupSwitcher_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// A new operation triggers a switch to the set-up machine state template,
    /// while the same operation does not
    /// </summary>
    [Test]
    public void TestOperationDetectionExtension ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var oldOperation = GetOperation (OLD_OPERATION_ID);
          var newOperation = GetOperation (NEW_OPERATION_ID);
          var setup = GetMachineStateTemplate (SETUP_MACHINE_STATE_TEMPLATE_ID);

          var previousOperationSlot = InitializeOperationAndMachineStateTemplate (machine, oldOperation);

          var extension = CreateExtension (SETUP_MACHINE_STATE_TEMPLATE_ID);

          extension.AddOperation (machine, oldOperation, R (1, null), T (1), previousOperationSlot);
          Assert.That (GetAssociations (), Is.Empty, "no set-up association for the same operation");

          extension.AddOperation (machine, newOperation, R (2, null), T (2), previousOperationSlot);

          var associations = GetAssociations ();
          Assert.That (associations, Has.Count.EqualTo (1));
          Assert.Multiple (() => {
            Assert.That (associations[0].MachineStateTemplate, Is.EqualTo (setup));
            Assert.That (associations[0].Machine, Is.EqualTo (machine));
            Assert.That (associations[0].Begin.Value, Is.EqualTo (T (2)));
            Assert.That (associations[0].End.HasValue, Is.False);
            Assert.That (associations[0].Option, Is.EqualTo (AssociationOption.Detected));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// No set-up is triggered when the detected operation is the same
    /// as the operation of the previous operation slot
    /// </summary>
    [Test]
    public void TestSameOperationNoSwitch ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operation = GetOperation (OLD_OPERATION_ID);

          var previousOperationSlot = InitializeOperationAndMachineStateTemplate (machine, operation);

          var extension = CreateExtension (SETUP_MACHINE_STATE_TEMPLATE_ID);
          extension.AddOperation (machine, operation, R (1, null), T (1), previousOperationSlot);
          extension.AddOperation (machine, operation, R (2, null), T (2), previousOperationSlot);

          Assert.That (GetAssociations (), Is.Empty);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// A set-up is triggered when there is no previous operation slot at all
    /// </summary>
    [Test]
    public void TestNoPreviousOperationSlot ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operation = GetOperation (NEW_OPERATION_ID);
          var setup = GetMachineStateTemplate (SETUP_MACHINE_STATE_TEMPLATE_ID);

          var extension = CreateExtension (SETUP_MACHINE_STATE_TEMPLATE_ID);
          extension.AddOperation (machine, operation, R (3, null), T (3), null);

          var associations = GetAssociations ();
          Assert.That (associations, Has.Count.EqualTo (1));
          Assert.Multiple (() => {
            Assert.That (associations[0].MachineStateTemplate, Is.EqualTo (setup));
            Assert.That (associations[0].Begin.Value, Is.EqualTo (T (3)));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Nothing is done and no exception is raised when the effective begin is -oo
    /// </summary>
    [Test]
    public void TestNoEffectiveBegin ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operation = GetOperation (NEW_OPERATION_ID);

          var noLowerBound = new LowerBound<DateTime> (null);
          var extension = CreateExtension (SETUP_MACHINE_STATE_TEMPLATE_ID);
          extension.AddOperation (machine, operation,
            new UtcDateTimeRange (noLowerBound, T (4)), noLowerBound, null);

          Assert.That (GetAssociations (), Is.Empty);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// A set-up is triggered when the machine matches the configured machine filter
    /// </summary>
    [Test]
    public void TestMachineFilterMatch ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operation = GetOperation (NEW_OPERATION_ID);
          var machineFilter = CreateMachineFilter (machine);

          var extension = CreateExtension (SETUP_MACHINE_STATE_TEMPLATE_ID, machineFilter.Id);
          extension.AddOperation (machine, operation, R (3, null), T (3), null);

          Assert.That (GetAssociations (), Has.Count.EqualTo (1));
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// No set-up is triggered when the machine does not match the configured machine filter
    /// </summary>
    [Test]
    public void TestMachineFilterNoMatch ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var otherMachine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (OTHER_MACHINE_ID);
          var operation = GetOperation (NEW_OPERATION_ID);
          var machineFilter = CreateMachineFilter (otherMachine);

          var extension = CreateExtension (SETUP_MACHINE_STATE_TEMPLATE_ID, machineFilter.Id);
          extension.AddOperation (machine, operation, R (3, null), T (3), null);

          Assert.That (GetAssociations (), Is.Empty);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Nothing is done when the configured machine filter does not exist
    /// </summary>
    [Test]
    public void TestUnknownMachineFilter ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operation = GetOperation (NEW_OPERATION_ID);

          var extension = CreateExtension (SETUP_MACHINE_STATE_TEMPLATE_ID, UNKNOWN_MACHINE_FILTER_ID);
          extension.AddOperation (machine, operation, R (3, null), T (3), null);
          // A second call must not trigger a new initialization either
          extension.AddOperation (machine, operation, R (4, null), T (4), null);

          Assert.That (GetAssociations (), Is.Empty);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Nothing is done when the configured set-up machine state template does not exist
    /// </summary>
    [Test]
    public void TestUnknownSetupMachineStateTemplate ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operation = GetOperation (NEW_OPERATION_ID);

          var extension = CreateExtension (UNKNOWN_MACHINE_STATE_TEMPLATE_ID);
          extension.AddOperation (machine, operation, R (3, null), T (3), null);

          Assert.That (GetAssociations (), Is.Empty);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Nothing is done when no set-up machine state template is configured
    /// </summary>
    [Test]
    public void TestNoSetupMachineStateTemplateConfigured ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operation = GetOperation (NEW_OPERATION_ID);

          var extension = new OperationDetectionExtension ();
          extension.SetTestConfiguration ("{}");
          extension.AddOperation (machine, operation, R (3, null), T (3), null);

          Assert.That (GetAssociations (), Is.Empty);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// The legacy XML configuration format is not supported any more:
    /// the instance is skipped instead of raising an exception to the caller
    /// </summary>
    [Test]
    public void TestXmlConfigurationIsRejected ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var operation = GetOperation (NEW_OPERATION_ID);

          var xmlConfiguration = $"""
<?xml version="1.0" encoding="UTF-8"?>
<properties>
  <property>
    <key>SetupMachineStateTemplateId</key>
    <value>{SETUP_MACHINE_STATE_TEMPLATE_ID}</value>
  </property>
</properties>
""";

          // The loader rejects the XML format explicitly (and logs it as fatal)
          var loader = new Lemoine.Extensions.Configuration.ConfigurationLoader<Configuration> ();
          Assert.Throws<ArgumentException> (() => loader.LoadConfiguration (xmlConfiguration));

          // ... and the extension skips the instance instead of propagating the exception
          var extension = new OperationDetectionExtension ();
          extension.SetTestConfiguration (xmlConfiguration);
          Assert.DoesNotThrow (() => extension.AddOperation (machine, operation, R (3, null), T (3), null));
          Assert.That (GetAssociations (), Is.Empty);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Null or empty parameters are loaded like an empty Json configuration,
    /// and not like an invalid configuration
    /// </summary>
    [Test]
    public void TestEmptyConfigurationIsLoadedLikeEmptyJson ()
    {
      var loader = new Lemoine.Extensions.Configuration.ConfigurationLoader<Configuration> ();

      var reference = loader.LoadConfiguration ("{}");
      Assert.That (reference, Is.Not.Null);

      foreach (var parameters in new string[] { null, "" }) {
        var configuration = loader.LoadConfiguration (parameters);
        Assert.That (configuration, Is.Not.Null, $"configuration for [{parameters ?? "null"}]");
        Assert.Multiple (() => {
          Assert.That (configuration.SetupMachineStateTemplateId,
            Is.EqualTo (reference.SetupMachineStateTemplateId));
          Assert.That (configuration.MachineFilterId,
            Is.EqualTo (reference.MachineFilterId));
        });
      }
    }

    /// <summary>
    /// The previous operation slot is required by this extension
    /// </summary>
    [Test]
    public void TestIsPreviousOperationSlotRequired ()
    {
      var extension = new OperationDetectionExtension ();
      Assert.That (extension.IsPreviousOperationSlotRequired (), Is.True);
    }

    /// <summary>
    /// Initialize returns true whatever the machine is, and rejects a null machine
    /// </summary>
    [Test]
    public void TestInitialize ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var extension = new OperationDetectionExtension ();
          Assert.Multiple (() => {
            Assert.That (extension.Initialize (machine), Is.True);
            Assert.Throws<ArgumentNullException> (() => extension.Initialize (null));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// StopOperation does not create any machine state template association
    /// </summary>
    [Test]
    public void TestStopOperation ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();

          var extension = CreateExtension (SETUP_MACHINE_STATE_TEMPLATE_ID);
          extension.StopOperation (machine, T (3));

          Assert.That (GetAssociations (), Is.Empty);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the validity check of the configuration
    /// </summary>
    [Test]
    public void TestConfigurationIsValid ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = GetMachine ();
          var machineFilter = CreateMachineFilter (machine);

          { // Valid: no machine filter
            var configuration = new Configuration {
              SetupMachineStateTemplateId = SETUP_MACHINE_STATE_TEMPLATE_ID
            };
            var valid = configuration.IsValid (out var errors);
            Assert.Multiple (() => {
              Assert.That (valid, Is.True);
              Assert.That (errors, Is.Empty);
            });
          }

          { // Valid: with a machine filter
            var configuration = new Configuration {
              SetupMachineStateTemplateId = SETUP_MACHINE_STATE_TEMPLATE_ID,
              MachineFilterId = machineFilter.Id
            };
            Assert.That (configuration.IsValid (out var errors), Is.True);
          }

          { // Invalid: no set-up machine state template
            var configuration = new Configuration ();
            var valid = configuration.IsValid (out var errors);
            Assert.Multiple (() => {
              Assert.That (valid, Is.False);
              Assert.That (errors, Has.Exactly (1).Items);
            });
          }

          { // Invalid: unknown set-up machine state template
            var configuration = new Configuration {
              SetupMachineStateTemplateId = UNKNOWN_MACHINE_STATE_TEMPLATE_ID
            };
            var valid = configuration.IsValid (out var errors);
            Assert.Multiple (() => {
              Assert.That (valid, Is.False);
              Assert.That (errors, Has.Exactly (1).Items);
            });
          }

          { // Invalid: unknown machine filter
            var configuration = new Configuration {
              SetupMachineStateTemplateId = SETUP_MACHINE_STATE_TEMPLATE_ID,
              MachineFilterId = UNKNOWN_MACHINE_FILTER_ID
            };
            var valid = configuration.IsValid (out var errors);
            Assert.Multiple (() => {
              Assert.That (valid, Is.False);
              Assert.That (errors, Has.Exactly (1).Items);
            });
          }

          { // Invalid: both the machine filter and the machine state template are unknown
            var configuration = new Configuration {
              SetupMachineStateTemplateId = UNKNOWN_MACHINE_STATE_TEMPLATE_ID,
              MachineFilterId = UNKNOWN_MACHINE_FILTER_ID
            };
            var valid = configuration.IsValid (out var errors);
            Assert.Multiple (() => {
              Assert.That (valid, Is.False);
              Assert.That (errors, Has.Exactly (2).Items);
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    IMonitoredMachine GetMachine ()
    {
      var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
        .FindById (MACHINE_ID);
      Assert.That (machine, Is.Not.Null);
      return machine;
    }

    IOperation GetOperation (int operationId)
    {
      var operation = ModelDAOHelper.DAOFactory.OperationDAO
        .FindById (operationId);
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
    /// Create an extension with a JSON configuration
    /// </summary>
    OperationDetectionExtension CreateExtension (int setupMachineStateTemplateId, int? machineFilterId = null)
    {
      var extension = new OperationDetectionExtension ();
      var machineFilterProperty = machineFilterId.HasValue
        ? $@", ""MachineFilterId"": {machineFilterId.Value}"
        : "";
      extension.SetTestConfiguration ($@"{{ ""SetupMachineStateTemplateId"": {setupMachineStateTemplateId}{machineFilterProperty} }}");
      return extension;
    }

    /// <summary>
    /// Set an operation and a production machine state template from T(0),
    /// then return the associated operation slot
    /// </summary>
    IOperationSlot InitializeOperationAndMachineStateTemplate (IMonitoredMachine machine, IOperation operation)
    {
      {
        var association = ModelDAOHelper.ModelFactory
          .CreateOperationMachineAssociation (machine, R (0, null));
        association.Operation = operation;
        association.Apply ();
      }
      {
        var production = GetMachineStateTemplate (PRODUCTION_MACHINE_STATE_TEMPLATE_ID);
        var association = ModelDAOHelper.ModelFactory
          .CreateMachineStateTemplateAssociation (machine, production, T (0));
        association.Apply ();
      }
      ModelDAOHelper.DAOFactory.Flush ();

      return ModelDAOHelper.DAOFactory.OperationSlotDAO
        .FindOverlapsRange (machine, R (0, null))
        .First ();
    }

    IMachineFilter CreateMachineFilter (IMachine machine)
    {
      var machineFilter = ModelDAOHelper.ModelFactory
        .CreateMachineFilter ("SetupSwitcherTest", MachineFilterInitialSet.None);
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
  }
}
