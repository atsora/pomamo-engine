// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Lemoine.Core.Extensions.Logging;
using Lemoine.Core.Extensions.Web;
using Lemoine.Core.Log;
using Microsoft.Extensions.DependencyInjection;

namespace Lem_CncCoreService.Asp
{
  public static class WebMiddlewareServices
  {
    static readonly ILog log = LogManager.GetLogger (typeof (WebMiddlewareServices).FullName);

    /// <summary>
    /// Note: IServiceAssembliesResolver must be already set in the services
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureServices (IServiceCollection services)
    {
      services.AddSingleton<Lemoine.Core.Log.ILogFactory> ((IServiceProvider sp) => Lemoine.Core.Log.LogManager.LoggerFactory);
      // Note: this requires Microsoft.Extensions.Logging to be installed
      services.AddLogging (loggingBuilder => {
        loggingBuilder.AddLemoineLog ();
      });

      services.AddTransient<ResponseWriter> ();
    }
  }
}
