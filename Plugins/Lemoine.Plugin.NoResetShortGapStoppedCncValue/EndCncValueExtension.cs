// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.NoResetShortGapStoppedCncValue
{
  /// <summary>
  /// Description of ResetCncValueExtension.
  /// </summary>
  public class StopCncValueExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , Lemoine.Extensions.Cnc.IEndCncValueExtension
  {
    TimeSpan m_maxGap;
    IField m_field;
    IMachineModule m_machineModule;

    static readonly ILog log = LogManager.GetLogger(typeof (StopCncValueExtension).FullName);
    
    #region IResetCncValueExtension implementation
    public bool Initialize(Lemoine.Model.IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);
      
      m_machineModule = machineModule;

      Configuration configuration;
      if (!LoadConfiguration (out configuration)) {
        log.Warn ("Initialize: the configuration is not valid, skip this instance");
        return false;
      }
      
      m_maxGap = configuration.MaxGap;
      
      // Initialize m_setupMachineStateTemplateId
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("SetupSwitcher.Initialize"))
        {
          int machineFilterId = configuration.MachineFilterId;
          if (0 != machineFilterId) {
            var machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
              .FindById (machineFilterId);
            if (null == machineFilter) {
              log.Error ($"Initialize: machine filter id {machineFilterId} does not exist");
              return false;
            }
            // Note: machineFilter.IsMatch requires it is done in the same session
            if ( (null != machineFilter) && !machineFilter.IsMatch (machineModule.MonitoredMachine)) {
              return false;
            }
          }
          
          int fieldId = configuration.FieldId;
          Debug.Assert (0 != fieldId);
          m_field = ModelDAOHelper.DAOFactory.FieldDAO
            .FindById (fieldId);
          if (null == m_field) {
            log.Error ($"Initialize: field id {fieldId} does not exist");
            return false;
          }
        } // Transaction
      } // Session

      return true;
    }
    
    public bool? IsEndCncValueRequested (string fieldCode, DateTime previousDateTime, DateTime newDateTime, bool previousStopped)
    {
      if (!previousStopped) {
        return null;
      }
      
      if (!string.Equals (m_field.Code, fieldCode, StringComparison.InvariantCultureIgnoreCase)) {
        // Field does not match
        return null;
      }
      
      if (newDateTime < previousDateTime) {
        log.Error ($"IsResetCncValueRequired: newDateTime {newDateTime} before previousDateTime {previousDateTime}");
        return null;
      }
      
      TimeSpan gap = newDateTime.Subtract (previousDateTime);
      if (gap < m_maxGap) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsResetCncValueRequired: Gap {gap} is lower than {m_maxGap} => return false");
        }
        return false;
      }
      
      return null;
    }
    #endregion // IResetCncValueExtension implementation
  }
}
