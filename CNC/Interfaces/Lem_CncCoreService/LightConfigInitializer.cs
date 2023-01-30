// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;
using Lemoine.Hosting.AsyncInitialization;
using Lemoine.I18N;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.ModelDAO.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lem_CncCoreService
{
  /// <summary>
  /// Light config initializer with no database access
  /// </summary>
  public class LightConfigInitializer : IAsyncInitializer
  {
    readonly ILog log = LogManager.GetLogger<FullConfigInitializer> ();

    readonly IConfiguration m_configuration;
    public LightConfigInitializer (IConfiguration configuration)
    {
      m_configuration = configuration;
    }

    public async Task InitializeAsync ()
    {
      log.Info ("InitializeAsync: add the .NET Core configurations");
      await Task.Run (() => Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader (m_configuration)));
    }
  }
}
