// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Threading;
using CommandLine;
using CommandLine.Text;
using Lemoine.Core.Log;
using Lemoine.FileRepository;

namespace Lem_FileRepoClient.Console
{
  class Program
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Program).FullName);

    /// <summary>
    /// Program entry point.
    /// </summary>
    public static void Main (string[] args)
    {
      try {
        var result = CommandLine.Parser.Default.ParseArguments<Options> (args);

        result.WithNotParsed<Options> (errors =>
        {
          Environment.ExitCode = 1;
          return;
        });

        result.WithParsed<Options> (opt =>
        {
          var parameters = opt.Parameters;
          if (null != parameters) {
            Lemoine.Info.ConfigSet.AddCommandLineParameters (parameters);
          }
          LogManager.AddLog4net ();

          InitializeFileRepoClient (opt);

          var nspace = opt.NSpace;
          var path = opt.Path;
          var s = Lemoine.FileRepository.FileRepoClient.GetString (nspace, path);
          System.Console.Out.WriteLine (s);
        });
      }
      catch (Exception ex) {
        log.Error ("Main: exception raised", ex);
        System.Console.Error.WriteLine ($"Exception {ex} raised");
        Environment.Exit (1);
      }
    }

    static void InitializeFileRepoClient (Options options)
    {
      if (null != Lemoine.FileRepository.FileRepoClient.Implementation) { // already set
        return;
      }

      if (!string.IsNullOrEmpty (options.FileRepoClientWebUrl)) {
        Lemoine.FileRepository.FileRepoClient.Implementation = new FileRepoClientWeb (options.FileRepoClientWebUrl);
      }
      else if (Lemoine.Info.PulseInfo.UseSharedDirectory) {
        string sharedDirectoryPath = Lemoine.Info.PulseInfo.SharedDirectoryPath;
        Lemoine.FileRepository.FileRepoClient.Implementation =
          new Lemoine.FileRepository.FileRepoClientSharedDir (sharedDirectoryPath);
      }
#if NET48
      else if (Lemoine.Info.PulseInfo.UseFileRepositoryCorba) {
        Lemoine.FileRepository.FileRepoClient.Implementation
          = new Lemoine.FileRepository.Corba.FileRepoClientCorba ();
      }
#endif // NET48
      else if (Lemoine.Info.PulseInfo.UseFileRepositoryWeb) {
        Lemoine.FileRepository.FileRepoClient.Implementation
          = new Lemoine.FileRepository.FileRepoClientWeb ();
      }
      else if (Lemoine.Info.PulseInfo.UseFileRepositoryMulti) { // Multi
        var multi = Lemoine.FileRepository.FileRepoClientMulti.CreateFromSharedDirectoryWeb (CancellationToken.None);
#if NET48
        multi.Add (new Lemoine.FileRepository.Corba.FileRepoClientCorba ());
#endif // NET48
        Lemoine.FileRepository.FileRepoClient.Implementation = multi;
      }
      else { // Default: pfr data dir for the analysis service
        var pfrDataDir = Lemoine.Info.PulseInfo.PfrDataDir;
        if (string.IsNullOrEmpty (pfrDataDir)) {
          log.FatalFormat ("InitializeFileRepoClient: unexpected invalid pfr data dir {0}", pfrDataDir);
          Lemoine.FileRepository.FileRepoClient.Implementation =
            new Lemoine.FileRepository.FileRepoClientDummy ();
        }
        else {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("InitializeFileRepoClient: consider pfr data dir {0}", pfrDataDir);
          }
          Lemoine.FileRepository.FileRepoClient.Implementation =
            new Lemoine.FileRepository.FileRepoClientDirectory (pfrDataDir);
        }
      }
      // If the FileRepository implementation is not valid, just write a log.
      // It may be successful later, and ForceSynchronization will manage the case
      // when lctr is available later
      if (!Lemoine.FileRepository.FileRepoClient.Implementation.Test ()) {
        log.ErrorFormat ("InitializeFileRepoClient: active file repository implementation is not valid right now but it may be ok later");
      }
    }

  }
}
