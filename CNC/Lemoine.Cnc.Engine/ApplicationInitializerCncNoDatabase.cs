// Copyright (C) 2009-2023 Atsora Solutions
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
using Lemoine.FileRepository;

namespace Lemoine.Cnc.Engine
{
  /// <summary>
  /// ApplicationInitializer for cnc acquisition with no database connection and a dummy file repo
  /// </summary>
  public class ApplicationInitializerCncNoDatabase : IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplicationInitializerCncNoDatabase).FullName);

    readonly IFileRepoClientFactory m_fileRepoClientFactory;

    /// <summary>
    /// Constructor
    /// </summary>
    public ApplicationInitializerCncNoDatabase (IFileRepoClientFactory fileRepoClientFactory)
    {
      m_fileRepoClientFactory =  fileRepoClientFactory;
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
    public Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      if (cancellationToken.IsCancellationRequested) {
        return Task.CompletedTask;
      }

      m_fileRepoClientFactory.InitializeFileRepoClient (cancellationToken);
      return Task.CompletedTask;
    }
  }
}

#endif // !NET40