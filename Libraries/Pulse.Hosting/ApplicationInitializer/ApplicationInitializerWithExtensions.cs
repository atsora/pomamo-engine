// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
using Lemoine.ModelDAO.Interfaces;

namespace Pulse.Hosting.ApplicationInitializer
{
  /// <summary>
  /// Initialize the application to connect to the database and initialize the extensions
  /// 
  /// For a computer that is not lctr
  /// </summary>
  public class ApplicationInitializerWithExtensions : IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplicationInitializerWithExtensions).FullName);

    readonly IAssemblyLoader m_assemblyLoader;
    readonly IConnectionInitializer m_connectionInitializer;
    readonly IExtensionsLoader m_extensionsLoader;

    /// <summary>
    /// Constructor
    /// </summary>
    public ApplicationInitializerWithExtensions (IAssemblyLoader assemblyLoader, IConnectionInitializer connectionInitializer, IExtensionsLoader extensionsLoader)
    {
      m_assemblyLoader = assemblyLoader;
      m_connectionInitializer = connectionInitializer;
      m_extensionsLoader = extensionsLoader;
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      try {
        Lemoine.Core.Plugin.AssemblyLoaderProvider.AssemblyLoader = m_assemblyLoader;
        try {
          m_connectionInitializer.Initialize (cancellationToken: cancellationToken);
        }
        catch (Exception ex1) {
          log.Error ("InitializeApplication: connection initialization failed, exit", ex1);
          throw;
        }

        cancellationToken.ThrowIfCancellationRequested ();

        // To be able to read configurations in the database
        Lemoine.Info.ConfigSet.AddConfigReader (new Lemoine.ModelDAO.Info.ModelDAOConfigReader (true));

        cancellationToken.ThrowIfCancellationRequested ();

        log.Info ("InitializeApplication: load the plugins");
        m_extensionsLoader.LoadExtensions ();

        cancellationToken.ThrowIfCancellationRequested ();

        // Note: add the config readers after the extensions are activated
        log.Info ("InitializeApplication: add the extensions config readers");
        Lemoine.Info.ConfigSet.AddConfigReader (new Lemoine.Business.Config.ConfigReaderFromExtensions ());

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
    public async Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      try {
        Lemoine.Core.Plugin.AssemblyLoaderProvider.AssemblyLoader = m_assemblyLoader;
        try {
          await m_connectionInitializer.InitializeAsync (cancellationToken: cancellationToken);
        }
        catch (Exception ex1) {
          log.Error ("InitializeApplicationAsync: connection initialization failed, exit", ex1);
          throw;
        }

        cancellationToken.ThrowIfCancellationRequested ();

        // To be able to read configurations in the database
        Lemoine.Info.ConfigSet.AddConfigReader (new Lemoine.ModelDAO.Info.ModelDAOConfigReader (true));

        cancellationToken.ThrowIfCancellationRequested ();

        log.Info ("InitializeApplicationAsync: load the plugins");
        await m_extensionsLoader.LoadExtensionsAsync ();

        cancellationToken.ThrowIfCancellationRequested ();

        // Note: add the config readers after the extensions are activated
        log.Info ("InitializeApplicationAsync: add the extensions config readers");
        Lemoine.Info.ConfigSet.AddConfigReader (new Lemoine.Business.Config.ConfigReaderFromExtensions ());

        cancellationToken.ThrowIfCancellationRequested ();
      }
      catch (Exception ex) {
        log.Error ("InitializeApplicationAsync: exception", ex);
        throw;
      }
    }
  }
}
