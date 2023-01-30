// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NET6_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Core.Hosting.ApplicationInitializer
{
  /// <summary>
  /// BasicApplicationInitializer
  /// </summary>
  public class ApplicationInitializerWithAssemblyLoader : IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplicationInitializerWithAssemblyLoader).FullName);

    readonly IAssemblyLoader m_assemblyLoader;

    /// <summary>
    /// Constructor
    /// </summary>
    public ApplicationInitializerWithAssemblyLoader (IAssemblyLoader assemblyLoader)
    {
      m_assemblyLoader = assemblyLoader;
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      try {
        Lemoine.Core.Plugin.AssemblyLoaderProvider.AssemblyLoader = m_assemblyLoader;

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
    public Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      try {
        Lemoine.Core.Plugin.AssemblyLoaderProvider.AssemblyLoader = m_assemblyLoader;

        cancellationToken.ThrowIfCancellationRequested ();
      }
      catch (Exception ex) {
        log.Error ("InitializeApplicationAsync: exception", ex);
        throw;
      }

      return Task.CompletedTask;
    }
  }
}

#endif // NET6_0_OR_GREATER
