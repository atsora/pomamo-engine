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

namespace Lemoine.Plugin.AlertConfigFile
{
  public class AlertConfigExtension
    : Lemoine.Extensions.UniqueInstanceConfigurableExtension<Configuration>
    , IAlertConfigExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (AlertConfigExtension).FullName);

    readonly AlertConfig m_alertConfig = new AlertConfig ();

    public bool Initialize ()
    {
      var configurations = LoadConfigurations ();
      var filePaths = configurations
        .Distinct ()
        .Select (c => AlertConfig.GetFilePath (c.Directory, c.File))
        .Distinct ();
      var result = false;
      foreach (var path in filePaths) {
        if (System.IO.File.Exists (path)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Initialize: add {path}");
          }
          m_alertConfig.AddConfigFile (path);
          result = true;
        }
        else {
          log.Error ($"Initialize: {path} does not exist, skip it");
        }
      }
      return result;
    }

    public IEnumerable<IListener> Listeners => m_alertConfig.Listeners;

    public IEnumerable<TriggeredAction> TriggeredActions => m_alertConfig.TriggeredActions;
  }
}
