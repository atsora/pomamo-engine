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
using Lemoine.ServiceTools.ServicesProviders;

namespace Lem_WatchDogService
{
  /// <summary>
  /// Initialize the configuration with some specific configurations
  /// </summary>
  public class SpecificConfigInitializer: IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (SpecificConfigInitializer).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public SpecificConfigInitializer ()
    {
    }

    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      string key;
      if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        key = ConfigServicesProviderOnWindows.ConfigKey;
      }
      else if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
        key = ConfigServicesProviderOnLinux.ConfigKey;
      }
      else {
        log.Warn ($"InitializeApplication: not supported platform");
        return;
      }

      var path = ProgramInfo.AbsolutePath + ".cfg";

      var fileConfigReader = new ListStringFileConfigReader (key, path);
      Lemoine.Info.ConfigSet.AddConfigReader (fileConfigReader);
    }

    public Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      InitializeApplication (cancellationToken);
      return Task.CompletedTask;
    }
  }
}
