// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Config;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.AnalysisStateMachineLiveOperationStatus
{
  public class ConfigExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IConfigExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConfigExtension).FullName);

    Configuration m_configuration;

    public double Priority => m_configuration?.ConfigPriority ?? 50.0;

    object Get (string key)
    {
      switch (key) {
      case "Analysis.Activity.MaxTime":
        return m_configuration.MaxTime;
      case "Analysis.Activity.MachineStateTemplates.MaxTime":
        return m_configuration.MachineStateTemplatesMaxTime;
      case "Analysis.Activity.PendingModifications.MaxTime":
        return m_configuration.PendingModificationsMaxTime;
      case "Analysis.Activity.Facts.MaxNumber":
        return m_configuration.FactNumber;
      case "Analysis.Activity.ProcessingReasonSlots.MaxTime":
        return m_configuration.ProcessingReasonSlotsMaxTime;
      case "Analysis.Activity.Detection.MaxTime":
        return m_configuration.DetectionMaxTime;
      case "Analysis.Activity.AutoSequences.MaxTime":
        return m_configuration.AutoSequencesMaxTime;
      case "Analysis.AutoModificationPriority":
        return m_configuration.AutoModificationPriority;
      default:
        throw new Lemoine.Info.ConfigKeyNotFoundException (key);
      }
    }

    public T Get<T> (string key)
    {
      var v = Get (key);
      if (v is T t) {
        return t;
      }
      else {
        log.Error ($"Get: invalid type for {key}: {v} VS {typeof (T)}");
        throw new InvalidCastException ();
      }
    }

    public bool Initialize ()
    {
      if (false == LoadConfiguration (out m_configuration)) {
        log.Error ("Initialize: load configuration returned false");
        return false;
      }

      return true;
    }
  }
}
