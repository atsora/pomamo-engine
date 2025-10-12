// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lemoine.Conversion;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Config;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.WebAppConfig
{
  public class InstallationExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IInstallationExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (InstallationExtension).FullName);

    readonly string WWWROOT_KEY = "wwwroot";

    readonly string WWWROOT_DEFAULT_WIN = @"C:\inetpub\wwwroot";
    readonly string WWWROOT_DEFAULT_LINUX = "/var/www";

    Configuration m_configuration;
    Regex m_includeRegex;

    bool Initialize ()
    {
      if (null == m_configuration) {
        if (!LoadConfiguration (out m_configuration)) {
          log.Error ("Initialize: LoadConfiguration error");
          return false;
        }
      }
      m_includeRegex = new Regex ($"({m_configuration.RelativePath.Replace (".", "\\.")}\\?v=\\d+\\.\\d+\\.\\d+\\.\\d+)(-[-:TZ0-9]+)?\"", RegexOptions.Compiled);

      return true;
    }

    public double Priority => 0.0;

    public bool CheckConfig ()
    {
      return Task.Run (CheckConfigAsync).GetAwaiter ().GetResult ();
    }

    public async Task<bool> CheckConfigAsync ()
    {
      try {
        if (!Initialize ()) {
          log.Error ("CheckConfigAsync: Initialized failed");
          return false;
        }

        List<Task> tasks = new List<Task> ();
        foreach (var path in GetAbsolutePaths ()) {
          tasks.Add (SetInstructionLineAsync (path));
        }
        await Task.WhenAll (tasks);
        await UpdateHtmls ();
        return true;
      }
      catch (Exception ex) {
        log.Error ("CheckConfigAsync: exception", ex);
        return false;
      }
    }

    public bool RemoveConfig ()
    {
      return Task.Run (RemoveConfigAsync).GetAwaiter ().GetResult ();
    }

    public async Task<bool> RemoveConfigAsync ()
    {
      try {
        if (!Initialize ()) {
          log.Error ("RemoveConfigAsync: Initialized failed");
          return false;
        }

        List<Task> tasks = new List<Task> ();
        foreach (var path in GetAbsolutePaths ()) {
          tasks.Add (RemoveInstructionLineAsync (path));
        }
        await Task.WhenAll (tasks);
        await UpdateHtmls ();
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

        if (lines.Any (l => l.StartsWith (m_configuration.InstructionLine))) {
          log.Debug ($"SetInstructionLineAsync: {path} already contains {m_configuration.InstructionLine}");
          return;
        }

        var line = m_configuration.InstructionLine;
        if (!string.IsNullOrEmpty (m_configuration.Comment)) {
          line += $" // {m_configuration.Comment}";
        }
        using (var file = File.AppendText (path)) {
          if (lines.Any () && !string.IsNullOrEmpty (lines.Last ())) {
            log.Debug ($"SetInstructionLinAsync: the last line was not empty");
            await file.WriteLineAsync ();
          }
          await file.WriteLineAsync (line);
        }
      }
      catch (Exception ex) {
        log.Error ($"SetInstructionLineAsync: exception when trying to add {m_configuration.InstructionLine} into {path}", ex);
        throw;
      }
    }

    async Task RemoveInstructionLineAsync (string path)
    {
      if (!File.Exists (path)) {
        if (log.IsWarnEnabled) {
          log.Warn ($"RemoveInstrucitonLineAsync: file {path} does not exist => do nothing");
        }
        return;
      }

      string[] lines = File.ReadAllLines (path);

      if (!lines.Any (l => l.StartsWith (m_configuration.InstructionLine))) {
        log.Debug ($"RemoveInstructionLineAsync: {path} does not contain {m_configuration.InstructionLine}");
        return;
      }

      using (var file = File.CreateText (path)) {
        foreach (var line in lines) {
          if (!line.StartsWith (m_configuration.InstructionLine)) {
            await file.WriteLineAsync (line);
          }
        }
      }
    }

    IEnumerable<string> GetRootDirectories ()
    {
      var wwwroot = Lemoine.Info.ConfigSet
        .LoadAndGet (WWWROOT_KEY, RuntimeInformation.IsOSPlatform (OSPlatform.Linux) ? WWWROOT_DEFAULT_LINUX : WWWROOT_DEFAULT_WIN);
      var directories = Directory.GetDirectories (wwwroot);
      foreach (var directory in directories) {
        var directoryInfo = new DirectoryInfo (directory);
        var directoryName = directoryInfo.Name;
        if (IsDirectoryRootNameMatch (directoryName)) {
          yield return directory;
        }
      }
    }

    IEnumerable<string> GetAbsolutePaths ()
    {
      foreach (var rootDirectory in GetRootDirectories ()) {
        var path = Path.Combine (rootDirectory, m_configuration.RelativePath);
        yield return path;
      }
    }

    bool IsDirectoryRootNameMatch (string directoryName)
    {
      if (string.IsNullOrEmpty (m_configuration.RootDirectoryNames)) {
        return true;
      }
      if (m_configuration.RootDirectoryNames.Equals ("*")) {
        return true;
      }
      var rootDirectoryNames = m_configuration.RootDirectoryNames.Split (new char[] { ',' });
      return rootDirectoryNames.Any (d => d.Equals (directoryName, StringComparison.InvariantCultureIgnoreCase));
    }

    IEnumerable<string> GetHtmlPaths ()
    {
      foreach (var rootDirectory in GetRootDirectories ()) {
        var directoryInfo = new DirectoryInfo (rootDirectory);
        var htmlPages = directoryInfo.GetFiles (m_configuration.HtmlPattern, SearchOption.TopDirectoryOnly);
        foreach (var htmlPage in htmlPages) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetHtmlPaths: return {htmlPage.FullName}");
          }
          yield return htmlPage.FullName;
        }
      }
    }

    async Task UpdateHtmls ()
    {
      List<Task> tasks = new List<Task> ();
      foreach (var path in GetHtmlPaths ()) {
        tasks.Add (UpdateHtml (path));
      }
      await Task.WhenAll (tasks);
    }

    bool IsUpdateRequired (string line, DateTime newTime)
    {
      var match = m_includeRegex.Match (line);
      if (match.Success) {
        var newValue = $"{match.Groups[1].Value}-{newTime.ToIsoString ()}\"";
        return !string.Equals (match.Value, newValue);
      }
      else {
        return false;
      }
    }


    async Task UpdateHtml (string path)
    {
      if (!File.Exists (path)) {
        log.Error ($"UpdateHtml: file {path} does not exist, unexpected");
        return;
      }

      try {
        string[] lines = File.ReadAllLines (path);

        var newTime = DateTime.UtcNow;
        if (!lines.Any (l => IsUpdateRequired (l, newTime))) {
          if (log.IsDebugEnabled) {
            log.Debug ($"UpdateHtml: no update is required for {path} => return");
          }
          return;
        }

        var bakPath = path + ".bakConfigUpdate";
        File.Copy (path, bakPath, true);
        try {
          using (var file = File.CreateText (path)) {
            foreach (var line in lines) {
              var match = m_includeRegex.Match (line);
              if (match.Success) {
                var newLine = line.Replace (match.Value, $"{match.Groups[1].Value}-{newTime.ToIsoString ()}\"");
                if (log.IsDebugEnabled) {
                  log.Debug ($"UpdateHtml: replace {line} by {newLine}, match={match.Value}");
                }
                await file.WriteLineAsync (newLine);
              }
              else {
                await file.WriteLineAsync (line);
              }
            }
          }
        }
        catch (Exception ex1) {
          log.Error ($"UpdateHtml: update failed, roll back {path}", ex1);
          File.Copy (bakPath, path, true);
          throw;
        }

        try {
          File.Delete (bakPath);
        }
        catch (Exception ex1) {
          log.Error ($"UpdateHtml: error while deleting backup file {bakPath}", ex1);
        }
      }
      catch (Exception ex) {
        log.Error ($"UpdateHtml: exception when trying to update {path} for include of {m_configuration.RelativePath}", ex);
        throw;
      }
    }
  }
}

