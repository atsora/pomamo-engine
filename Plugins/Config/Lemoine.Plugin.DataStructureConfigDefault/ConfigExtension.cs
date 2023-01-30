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

namespace Lemoine.Plugin.DataStructureConfigDefault
{
  public class ConfigExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IConfigExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConfigExtension).FullName);

    public double Priority => 10.0;

    object Get (string key)
    {
      if (key.Equals (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ComponentFromOperationOnly))) {
        return true; // Default value. You can override it in database
      }
      else if (key.Equals (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.IntermediateWorkPieceOperationIsSimpleOperation))) {
        return true; // Default value. You can override it in database
      }
      else if (key.Equals (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.SinglePath))) {
        return true; // Default value. You can override it in database
      }
      else if (key.Equals (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueComponentFromLine))) {
        return true; // Default value. You can override it in database
      }
      else if (key.Equals (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueComponentFromOperation))) {
        return true; // Default value. You can override it in database
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
      return true;
    }
  }
}
