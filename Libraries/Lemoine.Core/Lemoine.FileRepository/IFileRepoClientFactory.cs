// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// Interface for IFileRepoClient factory
  /// </summary>
  public interface IFileRepoClientFactory
  {
    /// <summary>
    /// Create a FileRepoClient
    /// </summary>
    /// <returns></returns>
    IFileRepoClient CreateFileRepoClient (CancellationToken cancellationToken);
  }

  /// <summary>
  /// Extensions to interface IFileRepoClientFactory
  /// </summary>
  public static class FileRepoClientFactoryExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FileRepoClientFactoryExtensions).FullName);

    /// <summary>
    /// Get the file repo client from Lemoine.FileRepository
    /// 
    /// If it has not been initialized yet, initialize it
    /// </summary>
    /// <param name="fileRepoClientFactory"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static IFileRepoClient GetFileRepositoryClient (this IFileRepoClientFactory fileRepoClientFactory, CancellationToken? cancellationToken = null)
    {
      if (null == Lemoine.FileRepository.FileRepoClient.Implementation) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetFileRepositoryClient: not initialized, initialize it first");
        }
        fileRepoClientFactory.InitializeFileRepoClient (cancellationToken);
      }

      return Lemoine.FileRepository.FileRepoClient.Implementation;
    }

    /// <summary>
    /// Initialize a file repo client from a <see cref="IFileRepoClientFactory"/>
    /// if it has not already been initialized
    /// </summary>
    /// <param name="fileRepoClientFactory"></param>
    /// <param name="cancellationToken"></param>
    public static void InitializeFileRepoClient (this IFileRepoClientFactory fileRepoClientFactory, CancellationToken? cancellationToken = null)
    {
      if (null != Lemoine.FileRepository.FileRepoClient.Implementation) { // already set
        log.Warn ("InitializeFileRepoClient: already initialized");
        return;
      }
      else {
        Lemoine.FileRepository.FileRepoClient.Implementation = fileRepoClientFactory.CreateFileRepoClient (cancellationToken ?? CancellationToken.None);

        // If the FileRepository implementation is not valid, just write a log.
        // It may be successful later, and ForceSynchronization will manage the case
        // when lctr is available later
        try {
          if (!Lemoine.FileRepository.FileRepoClient.Implementation.Test ()) {
            log.Error ("InitializeFileRepoClient: active file repository implementation is not valid right now but it may be ok later");
          }
        }
        catch (Exception ex) {
          log.Fatal ("InitializeFileRepoClient: test of active file repository implementation returned an exception. In case of a temporary problem, an exception should not be raised, and false returned instead", ex);
        }
      }
    }
  }
}
