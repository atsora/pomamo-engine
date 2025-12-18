// Copyright (C) 2025 Atsora Solutions
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
  /// Plugin conf to consider the reason group ID as the configuration key
  /// </summary>
  public class ReasonGroupPluginConf
    : DataPluginConf<IReasonGroup, int>
    , IPluginConfDataType
  {
    /// <summary>
    /// <see cref="DataPluginConf{T, TConfigurationKey}"/>
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<IReasonGroup> FindAll ()
    {
      return ModelDAO.ModelDAOHelper.DAOFactory.ReasonGroupDAO
        .FindAll ();
    }

    /// <summary>
    /// <see cref="DataPluginConf{T, TConfigurationKey}"/>
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override int GetConfigurationKey (IReasonGroup item)
    {
      return item.Id;
    }

    /// <summary>
    /// <see cref="DataPluginConf{T, TConfigurationKey}"/>
    /// </summary>
    /// <param name="item"></param>
    /// <param name="configurationKey"></param>
    /// <returns></returns>
    protected override bool TestConfigurationKey (IReasonGroup item, int configurationKey)
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
