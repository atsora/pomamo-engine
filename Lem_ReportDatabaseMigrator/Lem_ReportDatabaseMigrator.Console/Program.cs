// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using CommandLine;
using Lemoine.ReportDatabaseMigrator;
using Lemoine.Core.Log;
using CommandLine.Text;
using Lemoine.Info.ConfigReader.TargetSpecific;

namespace Lem_ReportDatabaseMigrator.Console
{
  class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    /// <summary>
    /// Program entry point.
    /// </summary>
    public static void Main (string[] args)
    {
      System.Console.WriteLine ("Start Report Database Migrator...");

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

          string LogFileName;
          if (opt.LogFile.CompareTo ("") != 0) {
            LogFileName = opt.LogFile;
          }
          else {
            LogFileName = "ReportDataMigrationLogFile.txt";
          }

          TextWriter outfile = null;
          try {
            if (!File.Exists (LogFileName)) {
              outfile = new StreamWriter (LogFileName);
            }
            else {
              outfile = File.AppendText (LogFileName);
            }
            outfile.WriteLine ("");
            outfile.WriteLine ("{0} Lem_ReportDatabaseMigrator ", DateTime.Now.ToString ());

            ReportDatabaseMigrator reportMigrator; // = new ReportDatabaseMigrator ();
            try {
              if (opt.DSN.CompareTo ("") != 0) {
                outfile.WriteLine ("Lem_ReportDatabaseMigrator DSN = {0}", opt.DSN);
                reportMigrator = new ReportDatabaseMigrator (opt.DSN);
              }
              else {
                outfile.WriteLine ("Lem_ReportDatabaseMigrator default DSN");
                reportMigrator = new ReportDatabaseMigrator ();
              }
            }
            catch (Exception ex) {
              System.Console.WriteLine ("Exception reportMigrator can not set DSN {0}", ex);
              outfile.WriteLine ("Exception reportMigrator can not set DSN {0}", ex);

              Environment.ExitCode = 1;
              return;
            }
            string query;
            if (opt.SQLfile.CompareTo ("") == 0) {
              if (opt.SQLquery.CompareTo ("") == 0) {
                System.Console.WriteLine ("WARNING ! Please, write a SQL query or file");
                outfile.WriteLine ("WARNING : NO SQL query nor file");
                //outfile.Close();
                //Environment.Exit (0);
                Environment.ExitCode = 0;
                return;
              }
              else {
                query = opt.SQLquery;
                outfile.WriteLine ("Start...query {0}", query);
              }
            }
            else {
              string fileName = opt.SQLfile;
              //string fileName = "reportRole.sql";
              //string fileName = "reportViews.sql";
              //string fileName = "reportRights.sql";
              using (TextReader reader = File.OpenText (fileName)) {
                query = reader.ReadToEnd ();
              }
              outfile.WriteLine ("Start... file {0}", fileName);
            }

            try {
              reportMigrator.ExecuteSQLstring (query);
              System.Console.WriteLine ("...done");
              outfile.WriteLine ("...done");

            }
            catch (System.Data.Odbc.OdbcException ex) {
              System.Console.WriteLine ("Exception ODBC ExecuteSQLstring {0}", ex);
              outfile.WriteLine ("Exception ODBC ExecuteSQLstring {0}", ex);
              //outfile.Close();
              //Environment.Exit (1);
              Environment.ExitCode = 1;
              return;
            }
            catch (Exception ex) {
              System.Console.WriteLine ("Exception ExecuteSQLstring {0}", ex);
              outfile.WriteLine ("Exception ExecuteSQLstring {0}", ex);
              //outfile.Close();
              //Environment.Exit (1);
              Environment.ExitCode = 1;
              return;
            }
          }
          catch (Exception) {  // File
            System.Console.WriteLine ("WARNING ! No log file");
            //Environment.Exit (1);
            Environment.ExitCode = 1;
            return;
          }
          finally {
            outfile.Close ();
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
