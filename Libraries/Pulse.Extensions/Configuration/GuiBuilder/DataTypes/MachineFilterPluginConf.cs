// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.Configuration.GuiBuilder.DataTypes
{
  /// <summary>
  /// Plugin conf to consider the machine filter ID as the configuration key
  /// </summary>
  public class MachineFilterPluginConf
    : DataPluginConf<IMachineFilter, int>
    , IPluginConfDataType
  {
    /// <summary>
    /// <see cref="DataPluginConf{T, TConfigurationKey}"/>
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<IMachineFilter> FindAll ()
    {
      return ModelDAO.ModelDAOHelper.DAOFactory.MachineFilterDAO
        .FindAll ();
    }

    /// <summary>
    /// <see cref="DataPluginConf{T, TConfigurationKey}"/>
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override int GetConfigurationKey (IMachineFilter item)
    {
      return item.Id;
    }

    /// <summary>
    /// <see cref="DataPluginConf{T, TConfigurationKey}"/>
    /// </summary>
    /// <param name="item"></param>
    /// <param name="configurationKey"></param>
    /// <returns></returns>
    protected override bool TestConfigurationKey (IMachineFilter item, int configurationKey)
    {
      if (null == item) {
        // TODO: Optional ?
        return 0 == configurationKey;
      }
      else { // null != item
        return item.Id == configurationKey;
      }
    }
  }
}
