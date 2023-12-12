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

namespace Lemoine.Plugin.AnalysisStateMachineMoldShop
{
  public class ConfigExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IConfigExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConfigExtension).FullName);

    public double Priority => 30.0;

    object Get (string key)
    {
      switch (key) {
      case "Analysis.Activity.MaxTime":
        return TimeSpan.FromSeconds (40);
      case "Analysis.Activity.MachineStateTemplates.MaxTime":
        return TimeSpan.FromSeconds (100);
      case "Analysis.Activity.PendingModifications.MaxTime":
        return TimeSpan.FromSeconds (40);
      case "Analysis.Activity.Facts.MaxNumber":
        return 50;
      case "Analysis.Activity.ProcessingReasonSlots.MaxTime":
        return TimeSpan.FromSeconds (10);
      case "Analysis.Activity.ProcessingReasonSlots.MinTime":
        return TimeSpan.FromSeconds (5);
      case "Analysis.Activity.Detection.MaxTime":
        return TimeSpan.FromSeconds (40);
      case "Analysis.Activity.AutoSequences.MaxTime":
        return TimeSpan.FromSeconds (20);
      case "Analysis.Activity.AutoSequences.MinTime":
        return TimeSpan.FromSeconds (10);
      default:
        throw new Lemoine.Info.ConfigKeyNotFoundException (key);
      }
    }

    public T Get<T> (string key)
    {
      var v = Get (key);
      if (v is T) {
        return (T)v;
      }
      else {
        log.Error ($"Get: invalid type for {key}: {v} VS {typeof (T)}");
        throw new InvalidCastException ();
      }
    }

    public bool Initialize ()
    {
      return true;
    }
  }
}
