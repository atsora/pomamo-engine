// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

#if !NET40

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;
using Lemoine.FileRepository;
using Lemoine.Info;
using Lemoine.Info.ConfigReader;
using Lemoine.ModelDAO.Interfaces;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// ApplicationInitializer for the connector
  /// </summary>
  public class ApplicationInitializerConnector : IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplicationInitializerConnector).FullName);

    readonly IConnectionInitializer m_connectionInitializer;
    readonly IFileRepoClientFactory m_fileRepoClientFactory;

    /// <summary>
    /// Constructor
    /// </summary>
    public ApplicationInitializerConnector (IConnectionInitializer connectionInitializer, IFileRepoClientFactory fileRepoClientFactory)
    {
      m_connectionInitializer = connectionInitializer;
      m_fileRepoClientFactory = fileRepoClientFactory;
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// 
    /// Not implemented
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      try {
        m_connectionInitializer.CreateAndInitializeConnection (cancellationToken);
      }
      catch (Exception ex) {
        log.Error ($"IniatializeApplication: connection initialization failed but continue", ex);
      }

      if (cancellationToken.IsCancellationRequested) {
        return;
      }

      m_fileRepoClientFactory.InitializeFileRepoClient (cancellationToken);
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      try {
        await m_connectionInitializer.CreateAndInitializeConnectionAsync (cancellationToken: cancellationToken);
      }
      catch (Exception ex) {
        log.Error ($"InitializeApplicationAsync: connection initialization failed but continue", ex);
      }

      if (cancellationToken.IsCancellationRequested) {
        return;
      }

      m_fileRepoClientFactory.InitializeFileRepoClient (cancellationToken);
    }
  }
}

#endif // !NET40
