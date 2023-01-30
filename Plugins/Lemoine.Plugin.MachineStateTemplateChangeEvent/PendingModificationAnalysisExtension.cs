// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.MachineStateTemplateChangeEvent
{
  /// <summary>
  /// Description of ModificationExtension.
  /// </summary>
  public class PendingModificationAnalysisExtension: Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , Lemoine.Extensions.Analysis.IPendingModificationAnalysisExtension
  {
    #region Members
    bool m_initializedConfiguration = false;
    bool m_active = false;
    Configuration m_configuration = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (PendingModificationAnalysisExtension).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region IPendingModificationAnalysisExtension implementation
    public void Initialize (IMachine machine)
    {
      if (null == machine) {
        m_active = false;
        m_initializedConfiguration = true;
      }
      else {
        InitializeConfiguration (machine);
      }
    }


    public void BeforeMakeAnalysis (IModification modification)
    {
    }


    public void AfterMakeAnalysis (IModification modification, bool completed)
    {
      Debug.Assert (null != modification);

      log.DebugFormat ("AfterMakeAnalysis: id={0} type={1} completed={2} analysisStatus={3}",
        ((Lemoine.Collections.IDataWithId<long>)modification).Id,
        modification.GetType (),
        completed,
        modification.AnalysisStatus);

      if (completed && (modification is IMachineStateTemplateAssociation) && (null == modification.Parent)) {
        if (modification.AnalysisStatus.IsCompletedSuccessfully ()) {
          GenerateEvent (modification);
        }
        else if (modification.AnalysisStatus.IsInProgress ()) {
          log.DebugFormat ("AfterMakeAnalysis: modification with a Pending sub-modification");
          // This is managed later, with NotifyAllSubModificationsCompleted
        }
        else {
          log.WarnFormat ("AfterMakeAnalysis: modification id={0} ended in error",
                          ((Lemoine.Collections.IDataWithId<long>)modification).Id);
        }
      }
    }

    public void NotifyAllSubModificationsCompleted (IModification modification)
    {
      Debug.Assert (null != modification);

      if ( (modification is IMachineStateTemplateAssociation) && (null == modification.Parent)) {
        Debug.Assert (modification.AnalysisStatus.IsCompletedSuccessfully ());
        log.DebugFormat ("NotifyAllSubModificationsCompleted: modification with sub-modifications just flagged as completed");
        GenerateEvent (modification); // Probably a sub-modification is Pending, for example LinkOperation
      }
    }

    void GenerateEvent (IModification modification)
    {
      var machineStateTemplateAssociation = modification as IMachineStateTemplateAssociation;
      var machine = machineStateTemplateAssociation.Machine;

      InitializeConfiguration (machine); // In case of PendingGlobalMachineModification, make it lazy

      log.DebugFormat ("GenerateEvent: active={0}",
        m_active);

      if (m_active) {
        var machineStateTemplate = machineStateTemplateAssociation.MachineStateTemplate;
        if ((0 < m_configuration.NewMachineStateTemplateId)
            && (m_configuration.NewMachineStateTemplateId != machineStateTemplate.Id)) {
          log.DebugFormat ("GenerateEvent: " +
                           "the machine state template does not match the configuration, skip it");
          return;
        }

        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginTransaction ("Plugin.MachineStateTemplateChangeEvent.CreateEvent")) {
            IEventLevel level = ModelDAOHelper.DAOFactory.EventLevelDAO.FindById (m_configuration.EventLevelId);
            if (null == level) {
              log.ErrorFormat ("GenerateEvent: " +
                               "level with id {0} is not defined");
              return;
            }
            log.DebugFormat ("GenerateEvent: create the event");
            var ev = new EventMachineStateTemplateChange (level, modification.DateTime, machine, machineStateTemplate,
                                                          machineStateTemplateAssociation.Range);
            (new EventMachineStateTemplateChangeDAO ()).MakePersistent (ev);
            transaction.Commit ();
          }
        }

      }
    }


    public void MakeAnalysisException (IModification modification, Exception ex)
    {
    }
    #endregion // IPendingModificationAnalysisExtension implementation

    #region Methods
    void InitializeConfiguration (IMachine machine)
    {
      Debug.Assert (null != machine);

      if (m_initializedConfiguration) { // Already initialized
        return;
      }

      if (!LoadConfiguration (out m_configuration)) {
        log.WarnFormat ("InitializeConfiguration: " +
                        "the configuration is not valid, skip this instance");
        m_active = false;
        m_initializedConfiguration = true;
        m_configuration = null;
        return;
      }

      IMachineFilter machineFilter = null;
      if (0 < m_configuration.MachineFilterId) { // Machine filter
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (var transaction = session.BeginReadOnlyTransaction ("MachineStateTemplateChangeEvent.InitializeConfiguration")) {
          int machineFilterId = m_configuration.MachineFilterId;
          if (0 != machineFilterId) {
            machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
              .FindById (machineFilterId);
            if (null == machineFilter) {
              log.ErrorFormat ("Initialize: " +
                               "machine filter id {0} does not exist",
                               machineFilterId);
              m_active = false;
              m_initializedConfiguration = true;
              return;
            }
          }
        }
      }

      m_active = true;
      m_initializedConfiguration = true;
      return;
    }
    #endregion // Methods
  }
}
