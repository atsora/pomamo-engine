/* -*- c# -*- ******************************************************************
 * Copyright (c) Nicolas Relange. All Rights Reserved.
 */

using CommandLine;
using Lemoine.Core.Log;
using System;

namespace Lem_ConfigPassword.Console
{
  internal class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    static void Main (string[] args)
    {
//      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());

      Options? options = null;
      try
      {
        var result = CommandLine.Parser.Default.ParseArguments<Options> (args);

        result.WithNotParsed<Options> (errors =>
        {
          Environment.ExitCode = 1;
          options = null;
        });

        result.WithParsed<Options> (opt =>
        {
          options = opt;
        });
      }
      catch (Exception ex)
      {
        System.Console.Error.WriteLine ($"Exception {ex} raised");
        Environment.Exit (1);
        return;
      }

      if (options is null)
      {
        System.Console.Error.WriteLine ($"Invalid arguments");
        Environment.Exit (1);
        return;
      }

      var parameters = options.Parameters;
      if (null != parameters)
      {
        Lemoine.Info.ConfigSet.AddCommandLineParameters (parameters);
      }
      LogManager.AddLog4net (traceEnabled: options.TraceEnabled);

      if (options.CheckAdmin) {
        var result = Lemoine.Info.PasswordManager.IsAdmin (options.Password);
        System.Console.WriteLine ($"CheckAdmin({options.Password}) = {result}");
      }

      if (options.Hash) {
        var hash = Lemoine.Model.Password.Hash (options.Password, options.Salt);
        System.Console.WriteLine ($"Hash({options.Password}) = {hash}");
      }
    }

  }
}
