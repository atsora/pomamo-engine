// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Info;
using Lemoine.Info.ConfigReader;

namespace ReportWizardCli
{
  /// <summary>
  /// Initialize the configuration with some specific configurations
  /// </summary>
  public class IniConfigInitializer : IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (IniConfigInitializer).FullName);

    readonly string m_path;

    /// <summary>
    /// Constructor
    /// </summary>
    public IniConfigInitializer (string path)
    {
      m_path = path;
    }

    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      var iniConfigReader = IniFileConfigReader.CreateFromPath (m_path, prefix: "ReportWizardCli");
      Lemoine.Info.ConfigSet.AddConfigReader (iniConfigReader);
    }

    public Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      InitializeApplication (cancellationToken);
      return Task.CompletedTask;
    }
  }
}
