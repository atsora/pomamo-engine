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
  /// File repo client factory to be used on Lctr to use the local files without considering any specific configuration
  /// </summary>
  public class FileRepoClientFactoryLctr : Lemoine.FileRepository.IFileRepoClientFactory
  {
    ILog log = LogManager.GetLogger<FileRepoClientFactoryNoCorba> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public FileRepoClientFactoryLctr ()
    {
    }

    /// <summary>
    /// <see cref="IFileRepoClientFactory"/>
    /// </summary>
    /// <returns></returns>
    public virtual IFileRepoClient CreateFileRepoClient (CancellationToken cancellationToken)
    {
      return CreateFromPfrDataDir (cancellationToken);
    }

    IFileRepoClient CreateFromPfrDataDir (CancellationToken cancellationToken)
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
