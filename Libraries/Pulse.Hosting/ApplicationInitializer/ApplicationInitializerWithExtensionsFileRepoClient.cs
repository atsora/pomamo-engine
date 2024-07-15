// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.Extensions.Interfaces;
using Lemoine.FileRepository;
using Lemoine.ModelDAO.Interfaces;

namespace Pulse.Hosting.ApplicationInitializer
{
  /// <summary>
  /// Initialize the application to connect to the database, initialize the extensions
  /// and the file repo client
  /// 
  /// For a computer that is not lctr
  /// </summary>
  public class ApplicationInitializerWithExtensionsFileRepoClient : ApplicationInitializerWithExtensions
    , IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplicationInitializerWithExtensionsFileRepoClient).FullName);

    readonly IFileRepoClientFactory m_fileRepoClientFactory;

    /// <summary>
    /// Constructor
    /// </summary>
    public ApplicationInitializerWithExtensionsFileRepoClient (IAssemblyLoader assemblyLoader, IConnectionInitializer connectionInitializer, IExtensionsLoader extensionsLoader, IFileRepoClientFactory fileRepoClientFactory)
      : base (assemblyLoader, connectionInitializer, extensionsLoader)
    {
      m_fileRepoClientFactory = fileRepoClientFactory;
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    public override void InitializeApplication (CancellationToken cancellationToken = default)
    {
      try {
        base.InitializeApplication (cancellationToken);
        cancellationToken.ThrowIfCancellationRequested ();

        m_fileRepoClientFactory.InitializeFileRepoClient (cancellationToken);
        cancellationToken.ThrowIfCancellationRequested ();
      }
      catch (Exception ex) {
        log.Error ("InitializeApplication: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async override Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      try {
        await base.InitializeApplicationAsync (cancellationToken);
        cancellationToken.ThrowIfCancellationRequested ();

        m_fileRepoClientFactory.InitializeFileRepoClient (cancellationToken);
        cancellationToken.ThrowIfCancellationRequested ();
      }
      catch (Exception ex) {
        log.Error ("InitializeApplicationAsync: exception", ex);
        throw;
      }
    }
  }
}
