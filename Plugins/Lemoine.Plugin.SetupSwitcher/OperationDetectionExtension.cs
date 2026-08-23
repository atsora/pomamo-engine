// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024-2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.SetupSwitcher
{
  /// <summary>
  /// Operation detection extension that switches the machine state template
  /// to a set-up one as soon as a new operation is detected.
  /// </summary>
  public class OperationDetectionExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , Lemoine.Extensions.Analysis.IOperationDetectionExtension
  {
    bool m_initialized = false;
    bool m_active = false;
    IMonitoredMachine m_machine = null;
    IMachineStateTemplate m_setupMachineStateTemplate = null;

    ILog log = LogManager.GetLogger (typeof (OperationDetectionExtension).FullName);

    /// <summary>
    /// <see cref="Lemoine.Extensions.Analysis.IAnalysisExtension"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public bool Initialize (IMonitoredMachine machine)
    {
      if (null == machine) {
        log.Fatal ("Initialize: machine is null");
        throw new ArgumentNullException (nameof (machine));
      }

      m_machine = machine;
      log = LogManager.GetLogger ($"{typeof (OperationDetectionExtension).FullName}.{machine.Id}");

      // Note: the configuration and the machine filter are checked later by InitializeMachine,
      //       so that the database is not requested here
      return true;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Analysis.IOperationDetectionExtension"/>
    /// </summary>
    /// <returns></returns>
    public bool IsPreviousOperationSlotRequired ()
    {
      return true;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Analysis.IOperationDetectionExtension"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="operation">not null</param>
    /// <param name="range"></param>
    /// <param name="effectiveBegin"></param>
    /// <param name="previousOperationSlot">nullable</param>
    public void AddOperation (IMonitoredMachine machine, IOperation operation,
                              UtcDateTimeRange range, LowerBound<DateTime> effectiveBegin,
                              IOperationSlot previousOperationSlot)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != operation);

      InitializeMachine (machine);

      if (!m_active) { // Not active
        return;
      }

      Debug.Assert (machine.Equals (m_machine));
      Debug.Assert (null != m_setupMachineStateTemplate);

      if ((null != previousOperationSlot)
        && object.Equals (previousOperationSlot.Operation, operation)) {
        // Same operation => do nothing
        if (log.IsDebugEnabled) {
          log.Debug ("AddOperation: same operation as in the previous operation slot => do nothing");
        }
        return;
      }

      if (!effectiveBegin.HasValue) {
        log.Error ("AddOperation: unexpected effective begin (-oo) => return");
        return;
      }

      Debug.Assert (effectiveBegin.Equals (range.Lower)); // New operation => no auto-operation

      if (log.IsDebugEnabled) {
        log.Debug ($"AddOperation: switch to the set-up machine state template {m_setupMachineStateTemplate.Id} from {effectiveBegin}");
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("SetupSwitcher.AddOperation")) {
          IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine, m_setupMachineStateTemplate,
                                                    new UtcDateTimeRange (effectiveBegin));
          association.Option = AssociationOption.Detected;
          ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
            .MakePersistent (association);
          transaction.Commit ();
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Analysis.IOperationDetectionExtension"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime"></param>
    public void StopOperation (IMonitoredMachine machine, DateTime dateTime)
    {
      // No new operation => nothing to do
    }

    /// <summary>
    /// Load the configuration and the associated set-up machine state template.
    /// 
    /// This is done only once: whatever the result is, the instance is flagged as initialized
    /// so that the database is not requested again and again.
    /// </summary>
    /// <param name="machine">not null</param>
    void InitializeMachine (IMonitoredMachine machine)
    {
      if (m_initialized) { // Already initialized
        return;
      }

      // Note: whatever happens below, the initialization is not attempted a second time
      m_initialized = true;
      m_active = false;
      m_setupMachineStateTemplate = null;
      m_machine = machine;

      Configuration configuration;
      if (!LoadConfiguration (out configuration)) {
        log.Warn ("InitializeMachine: the configuration is not valid, skip this instance");
        return;
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("SetupSwitcher.Initialize")) {
          int machineFilterId = configuration.MachineFilterId;
          if (0 != machineFilterId) {
            var machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
              .FindById (machineFilterId);
            if (null == machineFilter) {
              log.Error ($"InitializeMachine: machine filter id {machineFilterId} does not exist => skip this instance");
              return;
            }
            // Note: machineFilter.IsMatch requires it is done in the same session
            if (!machineFilter.IsMatch (machine)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"InitializeMachine: machine {machine.Id} does not match the machine filter {machineFilterId} => skip this instance");
              }
              return;
            }
          }

          m_setupMachineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindById (configuration.SetupMachineStateTemplateId);
          if (null == m_setupMachineStateTemplate) {
            log.Error ($"InitializeMachine: no machine state template found for id {configuration.SetupMachineStateTemplateId} => skip this instance");
            return;
          }

          m_active = true;
        } // Transaction
      } // Session
    }
  }
}
