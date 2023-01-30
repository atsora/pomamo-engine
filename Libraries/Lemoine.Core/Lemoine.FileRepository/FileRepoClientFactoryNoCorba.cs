// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.FileRepository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// Default file repo client method to use in case no specific method is set in configuration
  /// </summary>
  public enum DefaultFileRepoClientMethod
  {
    /// <summary>
    /// Use the PFR data directory directly
    /// 
    /// To be used in a service that is set on lctr
    /// </summary>
    PfrDataDir = 1,
    /// <summary>
    /// Use a multi file repo client (that tries different methods)
    /// </summary>
    Multi = 2,
  }

  /// <summary>
  /// File repo client factory with no Corba support
  /// </summary>
  public class FileRepoClientFactoryNoCorba : Lemoine.FileRepository.IFileRepoClientFactory
  {
    ILog log = LogManager.GetLogger<FileRepoClientFactoryNoCorba> ();

    readonly Func<DefaultFileRepoClientMethod> m_getDefault;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="defaultMethod"></param>
    public FileRepoClientFactoryNoCorba (DefaultFileRepoClientMethod defaultMethod = DefaultFileRepoClientMethod.Multi)
    {
      m_getDefault = () => defaultMethod;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="defaultMethod"></param>
    public FileRepoClientFactoryNoCorba (Func<DefaultFileRepoClientMethod> defaultMethod)
    {
      m_getDefault = defaultMethod;
    }

    /// <summary>
    /// <see cref="IFileRepoClientFactory"/>
    /// </summary>
    /// <returns></returns>
    public virtual IFileRepoClient CreateFileRepoClient (CancellationToken cancellationToken)
    {
      var defaultMethod = m_getDefault ();
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
      else if (Lemoine.Info.PulseInfo.UseFileRepositoryWeb) {
        return new Lemoine.FileRepository.FileRepoClientWeb ();
      }
      else if (Lemoine.Info.PulseInfo.UseFileRepositoryMulti) {
        return CreateMultiAndLog (cancellationToken);
      }
      else { // Default
        switch (defaultMethod) {
        case DefaultFileRepoClientMethod.PfrDataDir:
          return CreateFromPfrDataDir ();
        case DefaultFileRepoClientMethod.Multi:
          return CreateMultiAndLog (cancellationToken);
        default:
          log.Fatal ($"CreateFileRepoClient: unexpected default method {defaultMethod}");
          throw new InvalidOperationException ("Unknown default method");
        }
      }
    }

    /// <summary>
    /// Create a multi file repo client
    /// </summary>
    protected virtual FileRepoClientMulti CreateMulti (CancellationToken cancellationToken = default)
    {
      return FileRepoClientMulti.CreateFromSharedDirectoryWeb (cancellationToken);
    }

    /// <summary>
    /// Create a multi file repo client and log if it is valid
    /// </summary>
    protected FileRepoClientMulti CreateMultiAndLog (CancellationToken cancellationToken)
    {
      var multiClient = CreateMulti (cancellationToken);
      if (0 == multiClient.Count) {
        log.Fatal ($"CreateMulti: multi-client with no implementation");
      }
      else if (multiClient.Count <= 1) {
        log.Warn ($"CreateMulti: multi-client with a unique implementation (corba)");
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"CreateMulti: multi-client with {multiClient.Count} implementations");
        }
      }
      return multiClient;
    }

    IFileRepoClient CreateFromPfrDataDir ()
    {
      var pfrDataDir = Lemoine.Info.PulseInfo.PfrDataDir;
      if (IsPfrdataDirectoryValid (pfrDataDir)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"CreateFromPfrDataDir: consider pfr data dir {pfrDataDir}");
        }
        return new FileRepoClientDirectory (pfrDataDir);
      }
      else {
        log.Fatal ($"CreateFromPfrDataDir: unexpected invalid pfr data dir {pfrDataDir}");
        return new FileRepoClientDummy ();
      }
    }

    bool IsPfrdataDirectoryValid (string pfrDataDirectory)
    {
      if (string.IsNullOrEmpty (pfrDataDirectory)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("IsPfrdataDirectoryValid: empty or null");
        }
        return false;
      }
      if (!Directory.Exists (pfrDataDirectory)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsPfrdataDirectoryValid: directory {pfrDataDirectory} does not exist => return false");
        }
        return false;
      }
      else { // Check there is at least one CNC configuration file in it
        var cncConfigsDirectory = Path.Combine (pfrDataDirectory, "cncconfigs");
        if (!Directory.Exists (cncConfigsDirectory)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsPfrdataDirectoryValid: directory {cncConfigsDirectory} does not exist => return false");
          }
          return false;
        }
        else {
          return true;
        }
      }
    }
  }
}
