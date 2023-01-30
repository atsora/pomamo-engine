// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.Configuration.GuiBuilder
{
  /// <summary>
  /// Interface for a PluginConf data type
  /// </summary>
  public interface IPluginConfDataType
  {
    /// <summary>
    /// Optional value
    /// 
    /// Not necessarily used by all the configuration items
    /// </summary>
    bool Optional { get; set; }

    /// <summary>
    /// Multiple values are allowed
    /// 
    /// Not necessarily used by all the configuration items
    /// </summary>
    bool Multiple { get; set; }

    /// <summary>
    /// Optional control parameters
    /// 
    /// Not necessarily used by all the configuration items
    /// </summary>
    string Parameters { get; set; }

    /// <summary>
    /// Return the action to create the user control
    /// </summary>
    Action<IPluginConfig, string, string> AddControl { get; }

    /// <summary>
    /// Return the configuration reader for the user control data
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="property"></param>
    /// <returns></returns>
    Func<TConfiguration, object> GetConfigurationReader<TConfiguration> (System.Reflection.PropertyInfo property);

    /// <summary>
    /// Return the configuration setter from the user control data
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="property"></param>
    /// <returns></returns>
    Action<TConfiguration, object> GetConfigurationSetter<TConfiguration> (System.Reflection.PropertyInfo property);
  }
}
