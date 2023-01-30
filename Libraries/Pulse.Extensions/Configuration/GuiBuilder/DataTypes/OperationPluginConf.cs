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
  /// Plugin conf to consider the operation ID as the configuration key
  /// </summary>
  public class OperationPluginConf
    : DataPluginConf<IOperation, int>
    , IPluginConfDataType
  {
    /// <summary>
    /// <see cref="DataPluginConf{T, TConfigurationKey}"/>
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<IOperation> FindAll ()
    {
      return ModelDAO.ModelDAOHelper.DAOFactory.OperationDAO
        .FindAll ();
    }

    /// <summary>
    /// <see cref="DataPluginConf{T, TConfigurationKey}"/>
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override int GetConfigurationKey (IOperation item)
    {
      return ((IDataWithId)item).Id;
    }

    /// <summary>
    /// <see cref="DataPluginConf{T, TConfigurationKey}"/>
    /// </summary>
    /// <param name="item"></param>
    /// <param name="configurationKey"></param>
    /// <returns></returns>
    protected override bool TestConfigurationKey (IOperation item, int configurationKey)
    {
      if (null == item) {
        // TODO: Optional ?
        return 0 == configurationKey;
      }
      else { // null != item
        return ((IDataWithId)item).Id == configurationKey;
      }
    }
  }
}
