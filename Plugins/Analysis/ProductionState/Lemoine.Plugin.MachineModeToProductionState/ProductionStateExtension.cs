// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Database;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.MachineModeToProductionState
{
  public class ProductionStateExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IProductionStateExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionStateExtension).FullName);

    Configuration m_configuration;

    IMonitoredMachine m_machine;
    double m_score;

    IProductionState m_productionState;
    string m_defaultProductionStateTranslationKey;
    string m_defaultProductionStateTranslationValue;
    string m_defaultProductionStateColor;
    double? m_productionRate;

    IMachineMode m_machineMode;
    TimeSpan? m_maxDuration;

    public double Score => m_score;

    public bool IsRequired (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange)
    {
      return reasonSlotChange.HasFlag (ReasonSlotChange.Requested) || reasonSlotChange.HasFlag (ReasonSlotChange.NewActivity) || reasonSlotChange.HasFlag (ReasonSlotChange.MachineMode) || (m_maxDuration.HasValue && reasonSlotChange.HasFlag (ReasonSlotChange.Period));
    }

    public bool ConsolidateProductionStateRate (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange)
    {
      Debug.Assert (null != m_machineMode);
      Debug.Assert (null != m_productionState);

      if (m_maxDuration.HasValue && newReasonSlot.Duration.HasValue && (m_maxDuration.Value < newReasonSlot.Duration.Value)) {
        return false;
      }

      var isDescendantOrSelfOfRequest = new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (m_machineMode, newReasonSlot.MachineMode);
      var isDescendantOrSelf = Lemoine.Business.ServiceProvider.Get (isDescendantOrSelfOfRequest);
      if (!isDescendantOrSelf) {
        return false;
      }

      newReasonSlot.ProductionState = m_productionState;
      newReasonSlot.ProductionRate = m_productionRate;
      return true;
    }

    public bool Initialize (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      m_machine = machine;

      if (!LoadConfiguration (out m_configuration)) {
        log.Warn ($"Initialize: the configuration is not valid");
        return false;
      }

      return Initialize (m_configuration);
    }

    /// <summary>
    /// Initialize (configuration part)
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected virtual bool Initialize (Configuration configuration)
    {
      m_score = configuration.Score;
      m_productionRate = configuration.ProductionRate;
      m_maxDuration = configuration.MaxDuration;

      if (!string.IsNullOrEmpty (configuration.DefaultProductionStateTranslationKey)) {
        Debug.Assert (!string.IsNullOrEmpty (configuration.DefaultProductionStateTranslationValue));
        m_defaultProductionStateTranslationKey = configuration.DefaultProductionStateTranslationKey;
        m_defaultProductionStateTranslationValue = configuration.DefaultProductionStateTranslationValue;
        m_defaultProductionStateColor = configuration.DefaultProductionStateColor;
      }

      if ((0 == configuration.ProductionStateId) && string.IsNullOrEmpty (m_defaultProductionStateTranslationKey)) {
        log.Error ($"Initialize: no production state {configuration.ProductionStateId} or {m_defaultProductionStateTranslationKey} was set => return false");
        return false;
      }

      if (!configuration.CheckMachineFilter (m_machine)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Initialize: machine {m_machine} does not match machine filter {configuration.MachineFilterId} => return false");
        }
        return false;
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int productionStateId = configuration.ProductionStateId;
        if (0 == productionStateId) {
          Debug.Assert (!string.IsNullOrEmpty (m_defaultProductionStateTranslationKey)); // See above
          m_productionState = ConfigRequests.AddProductionState (m_defaultProductionStateTranslationKey,
            m_defaultProductionStateTranslationValue, m_defaultProductionStateColor);
        }
        else { // 0 != productionStateId
          m_productionState = ModelDAOHelper.DAOFactory.ProductionStateDAO
            .FindById (productionStateId);
        }
        if (m_productionState is null) {
          log.Error ($"Initialize: production state {productionStateId} or {m_defaultProductionStateTranslationKey} could not be loaded");
          return false;
        }
        else {
          if (log.IsDebugEnabled) {
            log.DebugFormat ($"Initialize: successfully loaded production state {m_defaultProductionStateTranslationKey}: id {m_productionState.Id}");
          }
        }

        m_machineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
          .FindById (configuration.MachineModeId);
        if (m_machineMode is null) {
          log.Error ($"Initialize: machine mode {configuration.MachineModeId} could not be loaded");
          return false;
        }
      } // session

      return true;
    }


  }
}
