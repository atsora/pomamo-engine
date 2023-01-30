// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Config;
using Lemoine.Extensions.Configuration;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.ReserveCapacityAlert
{
  public class ConfigExtension
    : Lemoine.Extensions.UniqueInstanceConfigurableExtension<Configuration>
    , IConfigExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConfigExtension).FullName);

    IEnumerable<Configuration> m_configurations = new List<Configuration> ();
    public double Priority => 100.0;
    public string m_groupList = null;
    public int m_threshold = 0;

    object Get (string key)
    {
      if (key.Equals ("ReserveCapacityAlert.GroupList")) {
        // if (ConfigEMailExtension)
        return m_groupList;
      }
      else if (key.Equals ("ReserveCapacityAlert.Threshold")) {
        return m_threshold;
      }
      else {
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
      var configurations = LoadConfigurations ();
      if (!configurations.Any ()) {
        log.ErrorFormat ("Initialize: no valid configuration, skip this instance");
        return false;
      }
      var configuration = configurations.First ();
      m_groupList = configuration.Groups;
      m_threshold = configuration.Threshold;
      return true;
    }
  }
}
