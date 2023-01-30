// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;
using Lemoine.Hosting.AsyncInitialization;
using Lemoine.I18N;
using Lemoine.Info.ConfigReader;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.ModelDAO.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lem_CncCoreService
{
  /// <summary>
  /// Config initializer, including using the database
  /// </summary>
  public class FullConfigInitializer : IAsyncInitializer
  {
    readonly ILog log = LogManager.GetLogger<FullConfigInitializer> ();

    static readonly int MAX_CONNECTION_ATTEMPTS = 5;

    readonly IConfiguration m_configuration;
    readonly IConnectionInitializer m_connectionInitializer;
    readonly IExtensionsLoader m_extensionsLoader;
    public FullConfigInitializer (IConfiguration configuration, IConnectionInitializer connectionInitializer, IExtensionsLoader extensionsLoader)
    {
      m_configuration = configuration;
      m_connectionInitializer = connectionInitializer;
      m_extensionsLoader = extensionsLoader;
    }

    public async Task InitializeAsync ()
    {
      log.Info ("InitializeAsync: add the .NET Core configurations");
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader (m_configuration));

      log.Info ("InitializeAsync: initialize the database");
      bool connectionSuccess;
      try {
        await m_connectionInitializer.CreateAndInitializeConnectionAsync (MAX_CONNECTION_ATTEMPTS); // Only 5 attempts, about 10s
        connectionSuccess = true;
      }
      catch (Exception ex) {
        log.Error ($"InitializeAsync: connection initialization failed but continue", ex);
        connectionSuccess = false;
      }

      log.Info ("InitializeAsync: add the ModelDAO config reader");
      // To be able to read net.mail.from and some other config values from the database
      var modelDaoConfigReader = new Lemoine.ModelDAO.Info.ModelDAOConfigReader (false);
      if (connectionSuccess) {
        try {
          modelDaoConfigReader.Initialize ();
        }
        catch (Exception ex) {
          log.Error ($"InitializeAsync: Initialize of ModelDAOConfigReader in exception, skip it", ex);
        }
      }
      Lemoine.Info.ConfigSet.AddConfigReader (new PersistentCacheConfigReader (modelDaoConfigReader, "cnccoreservice.modeldaoconfig.cache"));

      log.Info ("InitializeAsync: add the config readers from extensions");
      bool extensionsLoadSuccess;
      try {
        await m_extensionsLoader.LoadExtensionsAsync ();
        extensionsLoadSuccess = true;
      }
      catch (Exception ex) {
        log.Error ($"InitializeAsync: LoadExtensionsAsync failed but continue", ex);
        extensionsLoadSuccess = false;
      }
      var configReaderFromExtensions = new Lemoine.Business.Config.ConfigReaderFromExtensions (false);
      if (extensionsLoadSuccess) {
        try {
          configReaderFromExtensions.Initialize ();
        }
        catch (Exception ex) {
          log.Error ($"InitializeAsync: Initialize of ConfigReaderFromExtensions failed", ex);
        }
      }
      Lemoine.Info.ConfigSet.AddConfigReader (new PersistentCacheConfigReader (configReaderFromExtensions, "cnccoreservice.configfromextensions.cache"));
    }
  }
}
