// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
  /// Description of OperationDetectionExtension.
  /// </summary>
  public class OperationDetectionExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , Lemoine.Extensions.Analysis.IOperationDetectionExtension
  {
    #region Members
    bool m_initialized = false;
    bool m_active = false;
    IMonitoredMachine m_machine;
    IMachineFilter m_machineFilter = null;
    IMachineStateTemplate m_setupMachineStateTemplate = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (OperationDetectionExtension).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public OperationDetectionExtension ()
      : base (new ConfigurationLoader ())
    {
    }
    #endregion // Constructors

    #region Getters / Setters
    #endregion // Getters / Setters

    #region IOperationDetectionExtension implementation
    public bool Initialize(IMonitoredMachine machine)
    {
      return true; // The initialization is done later by AddOperation
    }

    public bool IsPreviousOperationSlotRequired()
    {
      return true;
    }

    public void AddOperation(Lemoine.Model.IMonitoredMachine machine, Lemoine.Model.IOperation operation,
                             Lemoine.Model.UtcDateTimeRange range, Lemoine.Model.LowerBound<DateTime> effectiveBegin,
                             Lemoine.Model.IOperationSlot previousOperationSlot)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != operation);
      
      if (false == m_initialized) {
        InitializeMachine (machine);
      }
      
      if (!m_active) { // Not active
        return;
      }
      
      Debug.Assert (machine.Equals (m_machine));

      if (null == m_setupMachineStateTemplate) {
        log.ErrorFormat ("AddOperation: " +
                         "the Setup Machine State Template is null " +
                         "=> return");
        return;
      }
      
      if ( (null != previousOperationSlot) && (object.Equals (previousOperationSlot.Operation, operation))) {
        // Same operation => do nothing
        return;
      }
      
      if (!effectiveBegin.HasValue) {
        log.ErrorFormat ("AddOperation: " +
                         "unexpected effective begin (-oo) " +
                         "=> return");
        return;
      }
      
      Debug.Assert (effectiveBegin.Equals (range.Lower)); // New operation => no auto-operation
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginTransaction ("SetupSwitcher.AddOperation"))
        {
          IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine, m_setupMachineStateTemplate,
                                                    new UtcDateTimeRange (range.Lower));
          association.Option = AssociationOption.Detected;
          ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
            .MakePersistent (association);
          transaction.Commit ();
        }
      }
    }

    void InitializeMachine (IMonitoredMachine machine)
    {
      if (m_initialized) { // Already initialized
        return;
      }
      
      Configuration configuration;
      if (false == LoadConfiguration (out configuration)) {
        log.WarnFormat ("Initialize: " +
                        "the configuration is not valid, skip this instance");
        m_setupMachineStateTemplate = null;
        m_initialized = true;
        return;
      }
      
      // Initialize m_setupMachineStateTemplateId
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("SetupSwitcher.Initialize"))
        {
          int machineFilterId = configuration.MachineFilterId;
          if (0 != machineFilterId) {
            m_machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
              .FindById (machineFilterId);
            if (null == m_machineFilter) {
              log.ErrorFormat ("Initialize: " +
                               "machine filter id {0} does not exist",
                               machineFilterId);
              m_active = false;
              return;
            }
          }

          m_setupMachineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindById (configuration.SetupMachineStateTemplateId);
          if (null == m_setupMachineStateTemplate) {
            log.ErrorFormat ("Initialize: " +
                             "no machine state template found for ID={0}",
                             configuration.SetupMachineStateTemplateId);
          }
          
          m_machine = machine;
          // Note: machineFilter.IsMatch requires it is done in the same session 
          if ( (null != m_machineFilter) && !m_machineFilter.IsMatch (machine)) {
            m_active = false;
          }
          else {
            m_active = true;
          }
        } // Transaction
      } // Session
      
      m_initialized = true;
    }
    
    #endregion
  }
}
