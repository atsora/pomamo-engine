// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
  /// ApplicationInitializer for cnc acquisition
  /// </summary>
  public class ApplicationInitializerCncAcquisition : IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplicationInitializerCncAcquisition).FullName);

    static readonly int MAX_CONNECTION_ATTEMPTS = 1;

    readonly IConnectionInitializer m_connectionInitializer;
    readonly IExtensionsLoader m_extensionsLoader;
    readonly IFileRepoClientFactory m_fileRepoClientFactory;
    readonly IApplicationNameProvider m_applicationNameProvider;

    /// <summary>
    /// Constructor
    /// </summary>
    public ApplicationInitializerCncAcquisition (IConnectionInitializer connectionInitializer, IExtensionsLoader extensionsLoader, IFileRepoClientFactory fileRepoClientFactory, IApplicationNameProvider applicationNameProvider)
    {
      m_connectionInitializer = connectionInitializer;
      m_fileRepoClientFactory = fileRepoClientFactory;
      m_extensionsLoader = extensionsLoader;
      m_applicationNameProvider = applicationNameProvider;
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
      var applicationName = m_applicationNameProvider.ApplicationName;

      bool connectionSuccess;
      try {
        m_connectionInitializer.CreateAndInitializeConnection (MAX_CONNECTION_ATTEMPTS, cancellationToken: cancellationToken);
        connectionSuccess = true;
      }
      catch (Exception ex) {
        log.Error ($"IniatializeApplication: connection initialization failed but continue", ex);
        connectionSuccess = false;
      }

      log.Info ("InitializeApplication: add the ModelDAO config reader");
      // To be able to read net.mail.from and some other config values from the database
      var modelDaoConfigReader = new Lemoine.ModelDAO.Info.ModelDAOConfigReader (false);
      if (connectionSuccess) {
        try {
          modelDaoConfigReader.Initialize (cancellationToken);
        }
        catch (Exception ex) {
          log.Error ($"InitializeApplication: Initialize of ModelDAOConfigReader in exception, skip it", ex);
        }
      }
      Lemoine.Info.ConfigSet.AddConfigReader (new PersistentCacheConfigReader (modelDaoConfigReader, $"{applicationName}.modeldaoconfig.cache"));

      if (cancellationToken.IsCancellationRequested) {
        return;
      }

      log.Info ("InitializeApplication: add the config readers from extensions");
      bool extensionsLoadSuccess;
      try {
        m_extensionsLoader.LoadExtensions ();
        extensionsLoadSuccess = true;
      }
      catch (Exception ex) {
        log.Error ($"InitializeApplication: LoadExtensions failed but continue", ex);
        extensionsLoadSuccess = false;
      }
      var configReaderFromExtensions = new Lemoine.Business.Config.ConfigReaderFromExtensions (false);
      if (extensionsLoadSuccess) {
        try {
          configReaderFromExtensions.Initialize ();
        }
        catch (Exception ex) {
          log.Error ($"InitializeApplication: Initialize of ConfigReaderFromExtensions failed", ex);
        }
      }
      Lemoine.Info.ConfigSet.AddConfigReader (new PersistentCacheConfigReader (configReaderFromExtensions, $"{applicationName}.configfromextensions.cache"));

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
      var applicationName = m_applicationNameProvider.ApplicationName;

      bool connectionSuccess;
      try {
        await m_connectionInitializer.CreateAndInitializeConnectionAsync (MAX_CONNECTION_ATTEMPTS, cancellationToken: cancellationToken);
        connectionSuccess = true;
      }
      catch (Exception ex) {
        log.Error ($"InitializeApplicationAsync: connection initialization failed but continue", ex);
        connectionSuccess = false;
      }

      log.Info ("InitializeApplication: add the ModelDAO config reader");
      // To be able to read net.mail.from and some other config values from the database
      var modelDaoConfigReader = new Lemoine.ModelDAO.Info.ModelDAOConfigReader (false);
      if (connectionSuccess) {
        try {
          modelDaoConfigReader.Initialize (cancellationToken);
        }
        catch (Exception ex) {
          log.Error ($"InitializeApplicationAsync: Initialize of ModelDAOConfigReader in exception, skip it", ex);
        }
      }
      Lemoine.Info.ConfigSet.AddConfigReader (new PersistentCacheConfigReader (modelDaoConfigReader, $"{applicationName}.modeldaoconfig.cache"));

      if (cancellationToken.IsCancellationRequested) {
        return;
      }

      log.Info ("InitializeApplicationAsync: add the config readers from extensions");
      bool extensionsLoadSuccess;
      try {
        await m_extensionsLoader.LoadExtensionsAsync (cancellationToken);
        extensionsLoadSuccess = true;
      }
      catch (Exception ex) {
        log.Error ($"InitializeApplicationAsync: LoadExtensionsAsync failed but continue", ex);
        extensionsLoadSuccess = false;
      }
      var configReaderFromExtensions = new Lemoine.Business.Config.ConfigReaderFromExtensions (false);
      if (extensionsLoadSuccess) {
        try {
          configReaderFromExtensions.Initialize ();
        }
        catch (Exception ex) {
          log.Error ($"InitializeApplicationAsync: Initialize of ConfigReaderFromExtensions failed", ex);
        }
      }
      Lemoine.Info.ConfigSet.AddConfigReader (new PersistentCacheConfigReader (configReaderFromExtensions, $"{applicationName}.configfromextensions.cache"));

      if (cancellationToken.IsCancellationRequested) {
        return;
      }

      m_fileRepoClientFactory.InitializeFileRepoClient (cancellationToken);
    }
  }
}

#endif // !NET40
