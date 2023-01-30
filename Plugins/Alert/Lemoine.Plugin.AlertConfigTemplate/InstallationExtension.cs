// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Config;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.AlertConfigTemplate
{
  public class InstallationExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IInstallationExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (InstallationExtension).FullName);

    string m_templatePath;
    string m_configFilePath;

    public double Priority => 100.0;

    bool Initialize ()
    {
      if (!LoadConfiguration (out var configuration)) {
        log.Error ($"Initialize: LoadConfiguration failed");
        return false;
      }
      try {
        var directoryPath = new Lemoine.Extensions.Alert.AlertConfigDirectory (configuration.Directory).AbsolutePath;
        m_templatePath = Path.Combine (directoryPath, configuration.TemplateFileName);
        if (!File.Exists (m_templatePath)) {
          log.Error ($"Initialize: template {m_templatePath} does not exist => return false");
          return false;
        }
        m_configFilePath = Path.Combine (directoryPath, configuration.ConfigFileName);
      }
      catch (Exception ex) {
        log.Error ($"Initialize: exception => return false", ex);
        return false;
      }

      return true;
    }

    public bool CheckConfig ()
    {
      if (!Initialize ()) {
        log.Debug ($"CheckConfig: Initialize failed");
        return false;
      }

      if (!File.Exists (m_configFilePath)) {
        File.Copy (m_templatePath, m_configFilePath);
        if (log.IsDebugEnabled) {
          log.Debug ($"CheckConfig: copy of {m_templatePath} into {m_configFilePath} successful");
        }
      }
      return true;
    }

    public bool RemoveConfig ()
    {
      return true;
    }
  }
}
