// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.FileRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Lem_AspService.Services
{
  /// <summary>
  /// File repo client factory with no Corba support
  /// </summary>
  public class FileRepoClientFactory : Lemoine.FileRepository.IFileRepoClientFactory
  {
    ILog log = LogManager.GetLogger<FileRepoClientFactory> ();

    readonly HttpClient m_httpClient;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpClient"></param>
    public FileRepoClientFactory (HttpClient httpClient)
    {
      m_httpClient = httpClient;
    }

    public IFileRepoClient CreateFileRepoClient (CancellationToken cancellationToken)
    {
      if (Lemoine.Info.PulseInfo.UseSharedDirectory) {
        string sharedDirectoryPath = Lemoine.Info.PulseInfo.SharedDirectoryPath;
        if (log.IsDebugEnabled) {
          log.Debug ($"CreateFileRepoClient: use shared directory with directory {sharedDirectoryPath}");
        }
        return new Lemoine.FileRepository.FileRepoClientSharedDir (sharedDirectoryPath);
      }
      else if (Lemoine.Info.PulseInfo.UseFileRepositoryCorba) {
        log.Error ("CreateFileRepoClient: Corba not supported although requested");
        throw new InvalidOperationException ("No Corba support");
      }
      else {
        var multiClient = new Lemoine.FileRepository.FileRepoClientMulti ();
        { // Pfr data dir
          var pfrDataDir = Lemoine.Info.PulseInfo.PfrDataDir;
          if (!string.IsNullOrEmpty (pfrDataDir)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"InitializeFileRepoClient: add FileRepoClient with PFR data directory {pfrDataDir}");
            }
            multiClient.Add (new Lemoine.FileRepository.FileRepoClientDirectory (pfrDataDir));
          }
        }
        { // FileRepoSharedDir
          var sharedDirectoryPath = Lemoine.Info.PulseInfo.SharedDirectoryPath;
          if (!string.IsNullOrEmpty (sharedDirectoryPath)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"InitializeFileRepoClient: add FileRepoClient with shared directory {sharedDirectoryPath}");
            }
            multiClient.Add (new Lemoine.FileRepository.FileRepoClientSharedDir (sharedDirectoryPath));
          }
        }
        { // FileRepoWeb with the main web service URL
          var mainWebServiceUrl = Lemoine.Info.PulseInfo.MainWebServiceUrl;
          if (!string.IsNullOrEmpty (mainWebServiceUrl)) {
            var ipAddresses = Lemoine.Info.ComputerInfo.GetIPAddresses ();
            var computerNames = Lemoine.Info.ComputerInfo.GetNames ();
            if (ipAddresses.Any (i => mainWebServiceUrl.Contains (i))
              || computerNames.Any (c => mainWebServiceUrl.Contains (c))) {
              log.ErrorFormat ("InitializeFileRepoClient: the main web service url {0} points to the local computer, which is not correct, it should not be defined", mainWebServiceUrl);
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"InitializeFileRepoClient: add WebFileRepo {mainWebServiceUrl}");
              }
              multiClient.Add (new Lemoine.FileRepository.FileRepoClientWeb (m_httpClient, mainWebServiceUrl));
            }
          }
        }
        if (0 == multiClient.Count) {
          log.Fatal ($"InitializeFileRepoClient: multi-client with no implementation");
        }
        else if (multiClient.Count <= 1) {
          log.Warn ($"InitializeFileRepoClient: multi-client with a unique implementation (corba)");
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"InitializeFileRepoClient: multi-client with {multiClient.Count} implementations");
          }
        }
        return multiClient;
      }
    }
  }
}
