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
    bool m_initializedConfiguration = false;
    bool m_active = false;
    Configuration m_configuration = null;

    static readonly ILog log = LogManager.GetLogger (typeof (PendingModificationAnalysisExtension).FullName);

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

      if (log.IsDebugEnabled) {
        log.Debug ($"AfterMakeAnalysis: id={((Lemoine.Collections.IDataWithId<long>)modification).Id} type={modification.GetType ()} completed={completed} analysisStatus={modification.AnalysisStatus}");
      }

      if (completed && (modification is IMachineStateTemplateAssociation) && (null == modification.Parent)) {
        if (modification.AnalysisStatus.IsCompletedSuccessfully ()) {
          GenerateEvent (modification);
        }
        else if (modification.AnalysisStatus.IsInProgress ()) {
          if (log.IsDebugEnabled) {
            log.Debug ("AfterMakeAnalysis: modification with a Pending sub-modification");
          }
          // This is managed later, with NotifyAllSubModificationsCompleted
        }
        else {
          log.Warn ($"AfterMakeAnalysis: modification id={((Lemoine.Collections.IDataWithId<long>)modification).Id} ended in error");
        }
      }
    }

    public void NotifyAllSubModificationsCompleted (IModification modification)
    {
      Debug.Assert (null != modification);

      if ( (modification is IMachineStateTemplateAssociation) && (null == modification.Parent)) {
        Debug.Assert (modification.AnalysisStatus.IsCompletedSuccessfully ());
        if (log.IsDebugEnabled) {
          log.Debug ("NotifyAllSubModificationsCompleted: modification with sub-modifications just flagged as completed");
        }
        GenerateEvent (modification); // Probably a sub-modification is Pending, for example LinkOperation
      }
    }

    void GenerateEvent (IModification modification)
    {
      var machineStateTemplateAssociation = modification as IMachineStateTemplateAssociation;
      var machine = machineStateTemplateAssociation.Machine;

      InitializeConfiguration (machine); // In case of PendingGlobalMachineModification, make it lazy

      if (log.IsDebugEnabled) {
        log.Debug ($"GenerateEvent: active={m_active}");
      }

      if (m_active) {
        var machineStateTemplate = machineStateTemplateAssociation.MachineStateTemplate;
        if ((0 < m_configuration.NewMachineStateTemplateId)
            && (m_configuration.NewMachineStateTemplateId != machineStateTemplate.Id)) {
          if (log.IsDebugEnabled) {
            log.Debug ("GenerateEvent: the machine state template does not match the configuration, skip it");
          }
          return;
        }

        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginTransaction ("Plugin.MachineStateTemplateChangeEvent.CreateEvent")) {
            IEventLevel level = ModelDAOHelper.DAOFactory.EventLevelDAO.FindById (m_configuration.EventLevelId);
            if (null == level) {
              log.Error ($"GenerateEvent: level with id {m_configuration.EventLevelId} is not defined");
              return;
            }
            if (log.IsDebugEnabled) {
              log.Debug ("GenerateEvent: create the event");
            }
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

    void InitializeConfiguration (IMachine machine)
    {
      Debug.Assert (null != machine);

      if (m_initializedConfiguration) { // Already initialized
        return;
      }

      if (!LoadConfiguration (out m_configuration)) {
        log.Warn ("InitializeConfiguration: the configuration is not valid, skip this instance");
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
              log.Error ($"Initialize: machine filter id {machineFilterId} does not exist");
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
  }
}
