// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using CommandLine;
using Lemoine.GDBMigration;
using Lemoine.Core.Log;
using Migrator;
using Migrator.Framework;
using CommandLine.Text;
using Lemoine.Info.ConfigReader.TargetSpecific;

namespace Lem_GDBMigrator.Console
{
  class Program
  {
    static readonly ILog log = LogManager.GetLogger(typeof (Program).FullName);

    /// <summary>
    /// Program entry point.
    /// </summary>
    public static void Main(string[] args)
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

          MigrationHelper migrationHelper;
          var connectionString = opt.ConnectionString;
          if (!string.IsNullOrEmpty (connectionString)) {
            migrationHelper = new MigrationHelper (connectionString,
                                                   false);
          }
          else {
            migrationHelper = new MigrationHelper (false);
          }
          if (opt.Simulate) {
            migrationHelper.Simulate = opt.Simulate;
          }
          if (opt.To != int.MinValue) {
            log.DebugFormat ("Main: " +
                             "migrate to {0}",
                             opt.To);
            migrationHelper.Migrate (opt.To);
          }
          else if (opt.Upgrade) {
            log.DebugFormat ("Main: " +
                             "upgrade");
            migrationHelper.Migrate ();
          }
          else if (opt.List) {
            log.DebugFormat ("Main: " +
                             "list the migrations");
            System.Console.Out.WriteLine ("Connection string: {0}",
                                          migrationHelper.ConnectionString);
            System.Console.Out.WriteLine ();
            IList<long> appliedMigrations =
              migrationHelper.Migrator.AppliedMigrations;
            foreach (Type m in migrationHelper.Migrator.MigrationsTypes) {
              long version = MigrationLoader.GetMigrationVersion (m);
              bool applied = appliedMigrations.Contains (version);
              System.Console.Out.WriteLine ("{0} {1} {2}",
                                            applied ? "=>" : "  ",
                                            version.ToString ().PadLeft (3),
                                            StringUtils.ToHumanName (m.Name));
            }
          }
          else {
            log.Warn ("Main: no command argument");
            System.Console.Error.WriteLine ("No command argument was set");
            Environment.Exit (1);
          }
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
