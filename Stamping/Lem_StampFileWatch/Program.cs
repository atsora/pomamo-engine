// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader.TargetSpecific;
using CommandLine;


namespace Lemoine.Stamping.Lem_StampFileWatch
{
  static class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    static void Main (string[] args)
    {
      try {
        Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());

        LogManager.AddLog4net ();

        var stampFileWatcher = new StampFileWatch ();
        stampFileWatcher.Initialize ();
      }

      catch (Exception) {
        log.Fatal ("Lem_StampFileWatch: exit requested. Skip");
        Environment.Exit (1);
      }
    }
  }
}
