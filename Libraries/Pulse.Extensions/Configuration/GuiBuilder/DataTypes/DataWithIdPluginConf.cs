// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Collections;
using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.Configuration.GuiBuilder.DataTypes
{
  /// <summary>
  /// Plugin conf to consider a data ID as the configuration key
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="ID"></typeparam>
  public abstract class DataWithIdPluginConf<T, ID>
    : DataPluginConf<T, ID>
    , IPluginConfDataType
    where T: IDataWithId<ID>, IDisplayable
  {
    /// <summary>
    /// To override: get the configuration key from a data item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override ID GetConfigurationKey (T item)
    {
      return item.Id;
    }

    /// <summary>
    /// To override: does an item match a configuration key
    /// </summary>
    /// <param name="item"></param>
    /// <param name="configurationKey"></param>
    /// <returns></returns>
    protected override bool TestConfigurationKey (T item, ID configurationKey)
    {
      if (null == item) {
        // TODO: Optional ?
        return object.Equals (default (ID), configurationKey);
      }
      else { // null != item
        return object.Equals (item.Id, configurationKey);
      }
    }
  }
}
