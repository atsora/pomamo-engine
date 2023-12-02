// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader.TargetSpecific;
using CommandLine;
using System.Threading.Tasks;
using System.Threading;

namespace Lemoine.Stamping.Lem_NcFileWatchStamper
{
  static class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    /// <summary>
    /// Program entry point
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
#if NETCOREAPP
    static async Task Main (string[] args)
#else // !NETCOREAPP
    static async Task MainAsync (string[] args)
#endif // !NETCOREAPP
    {
      try {
        Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());

        LogManager.AddLog4net ();

        var stamper = new NcFileWatchStamper ();
        await stamper.InitializeAsync (CancellationToken.None);
      }

      catch (Exception) {
        log.Fatal ("Lem_NcFileWatchStamper: exit requested");
        Environment.Exit (1);
      }
    }
  }
}
