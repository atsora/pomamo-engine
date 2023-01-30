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

namespace Lemoine.Plugin.ResetProductionState
{
  public class ProductionStateExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IProductionStateExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionStateExtension).FullName);

    Configuration m_configuration;

    double m_score;

    public double Score => m_score;

    public bool IsRequired (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange)
    {
      return !reasonSlotChange.IsEmpty ();
    }

    public bool ConsolidateProductionStateRate (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange)
    {
      newReasonSlot.ProductionState = null;
      newReasonSlot.ProductionRate = null;
      return true;
    }

    public bool Initialize (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

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

      return true;
    }


  }
}
