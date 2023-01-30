// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Business.Config
{
  /// <summary>
  /// Check there is no config update after a specific delay and only if the file repository is available
  /// 
  /// To restart the service only when the file repository is available to be able to load new packages for example
  /// </summary>
  public class ConfigUpdateCheckerWithAvailableFileRepository : IConfigUpdateChecker
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConfigUpdateCheckerWithAvailableFileRepository).FullName);

    static readonly string CONFIG_UPDATE_CHECKER_DELAY_KEY = "ConfigUpdateChecker.Delay";
    static readonly TimeSpan CONFIG_UPDATE_CHECKER_DELAY_DEFAULT = TimeSpan.FromSeconds (5);

    readonly ConfigUpdateChecker m_baseChecker;
    bool m_configUpdate = false;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ConfigUpdateCheckerWithAvailableFileRepository ()
    {
      var delay = Lemoine.Info.ConfigSet.LoadAndGet (CONFIG_UPDATE_CHECKER_DELAY_KEY, CONFIG_UPDATE_CHECKER_DELAY_DEFAULT);
      m_baseChecker = new ConfigUpdateChecker (delay);
    }
    #endregion // Constructors

    public void Initialize ()
    {
      m_baseChecker.Initialize ();
    }

    /// <summary>
    /// Check there was no update of the config
    /// 
    /// true is returned in case the condition could not be checked
    /// </summary>
    /// <returns></returns>
    public bool Check ()
    {
      if (m_configUpdate) {
        return !IsFileRepoAvailable ();
      }
      else { // !m_configUpdate
        var baseResult = m_baseChecker.Check ();
        if (log.IsDebugEnabled) {
          log.Debug ($"Check: base result is {baseResult}");
        }
        if (baseResult) {
          return true;
        }
        else { // !baseResult
          m_configUpdate = true;
          return !IsFileRepoAvailable ();
        }
      }
    }

    bool IsFileRepoAvailable ()
    {
      var testFileRepository = FileRepository.FileRepoClient.Test ();
      log.Info ($"IsFileRepoAvailable: test is {testFileRepository}");
      return testFileRepository;
    }

    public bool CheckNoConfigUpdate ()
    {
      return m_baseChecker.CheckNoConfigUpdate ();
    }
  }
}
