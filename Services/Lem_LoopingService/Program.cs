// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.Service;
#if NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2)
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
#endif // NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2)
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lem_LoopingService
{
  static class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program));

    /// <summary>
    /// Service name
    /// </summary>
#if NET40 || NET45 || NET48
    internal
#endif // NET40 || NET45 || NET48
    static readonly string SERVICE_NAME = "Lemoine Looping";

#if NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1)
    static readonly string USE_WORKER_KEY = "Service.UseWorker";
#if NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2)
    static readonly bool USE_WORKER_DEFAULT = true;
#else  // !(NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2))
    static readonly bool USE_WORKER_DEFAULT = false;
#endif // NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2)
#endif // NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1)

    /// <summary>
    /// Program entry point
    /// </summary>
#if NETCOREAPP
    static async Task Main (string[] args)
#else // !NETCOREAPP
    static void Main (string[] args)
#endif // NETCOREAPP
    {
      try {
        Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());
        ServiceOptions options = ServiceOptions.Parse (args);
        LogManager.AddLog4net ();
        bool interactive = options.Debug;
        if (options.Install) {
#if NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1)
          await Task.Run (ThreadServiceBase.Install);
#else // !(NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1))
          ThreadServiceBase.Install ();
#endif // NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1)
          return;
        }
        else if (options.Remove) {
#if NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1)
          await Task.Run (ThreadServiceBase.Remove);
#else // !(NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1))
          ThreadServiceBase.Remove ();
#endif // NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1)
          return;
        }

#if NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1)
        var useWorker = Lemoine.Info.ConfigSet
          .LoadAndGet (USE_WORKER_KEY, USE_WORKER_DEFAULT);
        if (useWorker || !RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
          var builder = Worker.CreateHostBuilder (args, options);
          if (options.Interactive) {
#if NETCOREAPP
            await builder.RunConsoleAsync ();
#else // !NETCOREAPP
            builder.RunConsole ();
#endif // NETCOREAPP
          }
          else { // Or builder.RunAsServiceAsync () ?
#if NETCOREAPP
            await builder.Build ().RunAsync ();
#else // !NETCOREAPP
            builder.Build ().Run ();
#endif // NETCOREAPP
          }
        }
        else {
          var service = new ThreadServiceBase (new LoopingService (), SERVICE_NAME, options, args);
          service.Run ();
        }
#else // !(NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1))
        var service = new ThreadServiceBase (new  LoopingService (), SERVICE_NAME, options, args);
#if NETCOREAPP
        await Task.Run (service.Run);
#else // !NETCOREAPP
        service.Run ();
#endif // NETCOREAPP
#endif // NETCOREAPP && !(NETCOREAPP1_0 || NETCOREAPP1_1)
      }
      catch (Exception) {
        Environment.Exit (1);
      }
    }
  }
}
