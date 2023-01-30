// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Business;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace Lemoine.Plugin.PerfRecorderSimpleLog
{
  public class PerfRecorderExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IPerfRecorderExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (PerfRecorderExtension).FullName);

    Configuration m_configuration;
    Regex m_regex = null;

    public bool Initialize ()
    {
      if (false == LoadConfiguration (out m_configuration)) {
        log.ErrorFormat ("Initialize: load configuration returned false");
        return false;
      }
      if (!string.IsNullOrEmpty (m_configuration.Regex)) {
        m_regex = new Regex (m_configuration.Regex);
      }
      return true;
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      return Task.CompletedTask;
    }

    public void Record (string key, TimeSpan duration)
    {
      if (null != m_regex) {
        if (!m_regex.IsMatch (key)) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("Record: key {0} does not match regex {1}", key, m_regex);
          }
          return;
        }
      }
      var prefix = m_configuration.LogPrefix ?? "";
      var perfLog = LogManager.GetLogger (prefix + key);
      var message = "Performance time=" + duration;
      if (m_configuration.Fatal <= duration) {
        perfLog.Fatal (message);
      }
      else if (m_configuration.Error <= duration) {
        perfLog.Error (message);
      }
      else if (m_configuration.Warn <= duration) {
        perfLog.Warn (message);
      }
      else if (m_configuration.Info <= duration) {
        perfLog.Info (message);
      }
      else {
        perfLog.Debug (message);
      }
    }
  }
}
