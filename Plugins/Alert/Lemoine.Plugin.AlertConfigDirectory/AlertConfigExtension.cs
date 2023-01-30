// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Alert;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.AlertConfigDirectory
{
  public class AlertConfigExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IAlertConfigExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (AlertConfigExtension).FullName);

    static readonly string CONFIG_DIRECTORY_KEY = "Alert.ConfigDirectory";
    static readonly string CONFIG_DIRECTORY_DEFAULT = "alert.d";
    // Note: Os specific default values for Alert.ConfigDirectory
    //       are defined in Lemoine.Core.TargetSpecific

    AlertConfig m_alertConfig;

    public bool Initialize ()
    {
      if (!LoadConfiguration (out var configuration)) {
        log.Error ($"Initialize: configuration is not valid => return false");
        return false;
      }

      var directory = configuration.Directory;
      if (string.IsNullOrEmpty (directory)) {
        directory = Lemoine.Info.ConfigSet.LoadAndGet (CONFIG_DIRECTORY_KEY, CONFIG_DIRECTORY_DEFAULT);
      }
      if (string.IsNullOrEmpty (directory)) {
        log.Error ($"Initialize: directory is null or empty => return false");
        return false;
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"Initialize: consider directory {directory}");
      }

      m_alertConfig = new AlertConfig (directory);
      return m_alertConfig.Listeners.Any () || m_alertConfig.TriggeredActions.Any ();
    }

    public IEnumerable<IListener> Listeners => m_alertConfig.Listeners;

    public IEnumerable<TriggeredAction> TriggeredActions => m_alertConfig.TriggeredActions;
  }
}
