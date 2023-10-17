// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Config;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.ReportCategory
{
  public class InstallationExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IInstallationExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (InstallationExtension).FullName);

    static readonly string REPORTS_DIRECTORY_KEY = "ReportsDirectory";
    static readonly string REPORTS_DIRECTORY_DEFAULT = "";

    Configuration m_configuration;
    string m_configurationFilePath;

    bool Initialize ()
    {
      if (null == m_configuration) {
        if (!LoadConfiguration (out m_configuration)) {
          log.Error ("Initialize: LoadConfiguration error");
          return false;
        }
      }

      var reportsDirectory = Lemoine.Info.ConfigSet
        .LoadAndGet (REPORTS_DIRECTORY_KEY, REPORTS_DIRECTORY_DEFAULT);
      if (string.IsNullOrEmpty (reportsDirectory)) {
        var pfrDataDirectory = Lemoine.Info.PulseInfo.PfrDataDir;
        if (!Directory.Exists (pfrDataDirectory)) {
          log.Error ($"Initialize: directory {pfrDataDirectory} does not exist => return false");
          return false;
        }
        reportsDirectory = Path.Combine (pfrDataDirectory, "report_templates");
      }

      m_configurationFilePath = Path.Combine (reportsDirectory, m_configuration.ReportCategoriesFileName);

      return true;
    }

    public double Priority => 0.0;

    public bool CheckConfig ()
    {
      return Task.Run (CheckConfigAsync).Result;
    }

    public async Task<bool> CheckConfigAsync ()
    {
      try {
        if (!Initialize ()) {
          log.Error ("CheckConfigAsync: Initialized failed");
          return false;
        }

        if (m_configuration.UniqueCategory) {
          await RemoveInstructionLineAsync (m_configurationFilePath);
        }
        await SetInstructionLineAsync (m_configurationFilePath);
        return true;
      }
      catch (Exception ex) {
        log.Error ("CheckConfigAsync: exception", ex);
        return false;
      }
    }

    public bool RemoveConfig ()
    {
      return Task.Run (RemoveConfigAsync).Result;
    }

    public async Task<bool> RemoveConfigAsync ()
    {
      try {
        if (!Initialize ()) {
          log.Error ("RemoveConfigAsync: Initialized failed");
          return false;
        }

        await RemoveInstructionLineAsync (m_configurationFilePath);
        return true;
      }
      catch (Exception ex) {
        log.Error ("RemoveConfigAsync: exception", ex);
        return false;
      }
    }

    async Task SetInstructionLineAsync (string path)
    {
      try {
        string[] lines;
        if (File.Exists (path)) {
          lines = File.ReadAllLines (path);
        }
        else {
          lines = new string[0];
          string directoryPath = Path.GetDirectoryName (path);
          if (!Directory.Exists (directoryPath)) {
            Directory.CreateDirectory (directoryPath);
          }
        }

        var line = $"{m_configuration.ReportName}={m_configuration.Category}";

        if (lines.Any (l => l.StartsWith (line))) {
          log.Debug ($"SetInstructionLineAsync: {path} already contains {line}");
          return;
        }

        using (var file = File.AppendText (path)) {
          if (lines.Any () && !string.IsNullOrEmpty (lines.Last ())) {
            log.Debug ($"SetInstructionLinAsync: the last line was not empty");
            await file.WriteLineAsync ();
          }
          await file.WriteLineAsync (line);
          if (!string.IsNullOrEmpty (m_configuration.Comment)) {
            await file.WriteLineAsync ($"// {line} {m_configuration.Comment}");
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"SetInstructionLineAsync: exception when trying to add {m_configuration.ReportName}={m_configuration.Category} into {path}", ex);
        throw;
      }
    }

    async Task RemoveInstructionLineAsync (string path)
    {
      string[] lines;
      if (File.Exists (path)) {
        lines = File.ReadAllLines (path);
      }
      else {
        lines = new string[0];
      }

      var linePrefix = m_configuration.UniqueCategory
        ? $"{m_configuration.ReportName}="
        : $"{m_configuration.ReportName}={m_configuration.Category}";

      if (!lines.Any (l => l.StartsWith (linePrefix))) {
        log.Debug ($"RemoveInstructionLineAsync: {path} does not contain {linePrefix}");
        return;
      }

      using (var file = File.CreateText (path)) {
        foreach (var line in lines) {
          if (!line.StartsWith (linePrefix)
            && !line.StartsWith ($"// {linePrefix}")) {
            await file.WriteLineAsync (line);
          }
        }
      }
    }
  }
}
