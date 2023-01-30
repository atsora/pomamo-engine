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

namespace Lemoine.Plugin.OperationSlotByMachineShift
{
  public class ConfigExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IConfigExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConfigExtension).FullName);

    public double Priority => 100.0; // AnalysisConfigProductionShop: 20.0

    public T Get<T> (string key)
    {
      var v = Get (key);
      if (v is T) {
        return (T)v;
      }
      else {
        log.Error ($"Get: invalid type for {v} VS {typeof (T)}");
        throw new InvalidCastException ();
      }
    }

    object Get (string key)
    {
      if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationSlotSplitOption))) {
        return (int)OperationSlotSplitOption.ByMachineShift;
      }
      else {
        throw new Lemoine.Info.ConfigKeyNotFoundException (key);
      }
    }

    public bool Initialize ()
    {
      return true;
    }
  }
}
