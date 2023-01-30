// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Config;

namespace Lemoine.Plugin.DefaultAccumulators
{
  public class ConfigExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IConfigExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConfigExtension).FullName);

    public double Priority => 20.0;

    public object Get (string key)
    {
      switch (key) {
      case "Summary.ProductionRate.Active":
      case "Summary.ProductionState.Active":
        return true;
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
        log.Error ($"Get: invalid type for {v} VS {typeof (T)}");
        throw new InvalidCastException ();
      }
    }

    public bool Initialize ()
    {
      return true;
    }
  }
}
