// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

#if !NET40

using Lemoine.Core.Log;
using Lemoine.Core.Extensions.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Service
{
  /// <summary>
  /// Extensions of <see cref="IHostBuilder"/>
  /// </summary>
  public static class HostBuilderExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (HostBuilderExtensions).FullName);

    /// <summary>
    /// Set the AppConfiguration for a Pomamo Service
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureServiceAppConfiguration (this IHostBuilder hostBuilder, ServiceOptions options)
    {
      return hostBuilder.ConfigureConsoleAppConfiguration (options);
    }
  }
}

#endif // !NET40
