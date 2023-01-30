// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Threading;
using CommandLine;
using CommandLine.Text;
using Lemoine.Core.Log;
using Lemoine.GDBUtils;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Pulse.Database.ConnectionInitializer;

namespace Lem_DatabaseUpgrade.Console
{
  class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    /// <summary>
    /// Program entry point.
    /// </summary>
    public static void Main (string[] args)
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());

      try {
        var result = CommandLine.Parser.Default.ParseArguments<Options> (args);

        result.WithNotParsed (errors =>
        {
          Environment.ExitCode = 1;
          return;
        });

        result.WithParsed (opt =>
        {
          var parameters = opt.Parameters;
          if (null != parameters) {
            Lemoine.Info.ConfigSet.AddCommandLineParameters (parameters);
          }
          LogManager.AddLog4net ();

          var textWriter = System.Console.Out;
          var messageLoggerFactory =
            new MessageLoggerFactory (textWriter, Level.Info);
          var upgradeLogger = messageLoggerFactory.GetLogger ("");
          var connectionInitializer = new ConnectionInitializerDatabaseUpgrade ();
          var defaultValuesFactory = new Pulse.Database.DefaultValuesFactory ();
          var databaseUpgrader = DatabaseUpgrader.Create<Lemoine.GDBPersistentClasses.Machine> (connectionInitializer, defaultValuesFactory);
          var upgradeResult = databaseUpgrader.Upgrade ("Lem_DatabaseUpgrade.Console", upgradeLogger);
           Environment.ExitCode = upgradeResult ? 0 : 1;
          return;
        });
      }
      catch (Exception ex) {
        log.Error ("Main: options parsing  exception", ex);
        System.Console.Error.WriteLine ($"Options parsing exception {ex} raised");
        Environment.ExitCode = 1;
      }
    }
  }
}
