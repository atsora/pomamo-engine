// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Hosting.AsyncInitialization;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pulse.Hosting;

namespace Lem_AspService
{
  public class Program
  {
    static readonly string URLS_KEY = "AspService.Urls";
    static readonly string URLS_DEFAULT = ""; // if empty, use the urls in command line or in appsettings.json or the default one

    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    public static async Task Main (string[] args)
    {
      try {
        Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());
        ServiceOptions options = ServiceOptions.Parse (args);
        LogManager.AddLog4net ();
        bool interactive = options.Debug;

        var builder = CreateHostBuilder (args, options);
        var host = builder.Build ();
        await host.InitAsync ();
        await host.RunAsync ();
      }
      catch (Exception ex) {
        log.Error ("Main: Exception raised", ex);
        System.Console.Error.WriteLine ($"Exception {ex} raised");
        Environment.Exit (1);
        return;
      }
    }

    public static IHostBuilder CreateHostBuilder (string[] args, ServiceOptions options)
    {
      var defaultBuilder = Host
        .CreateDefaultBuilder (args);

      if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        defaultBuilder = defaultBuilder
          .UseWindowsService ();
      }
      else if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
        defaultBuilder = defaultBuilder
          .UseSystemd ();
      }

      return defaultBuilder
        .ConfigurePulseServiceAppConfiguration (options)
        .ConfigureWebHostDefaults (webBuilder =>
        {
          var urls = Lemoine.Info.ConfigSet.LoadAndGet (URLS_KEY, URLS_DEFAULT);
          if (!string.IsNullOrEmpty (urls)) {
            webBuilder.UseUrls (urls);
          }
          webBuilder.UseStartup<Startup> ();
        });
    }
  }
}
