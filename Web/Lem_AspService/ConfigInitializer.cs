// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Hosting.AsyncInitialization;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;
using Lemoine.I18N;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.ModelDAO.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lem_AspService
{
  public class ConfigInitializer : IAsyncInitializer
  {
    readonly ILog log = LogManager.GetLogger<ConfigInitializer> ();

    static readonly int MAX_CONNECTION_ATTEMPTS = 5200;

    readonly IConfiguration m_configuration;
    readonly IConnectionInitializer m_connectionInitializer;
    readonly IExtensionsLoader m_extensionsLoader;
    public ConfigInitializer (IConfiguration configuration, IConnectionInitializer connectionInitializer, IExtensionsLoader extensionsLoader)
    {
      m_configuration = configuration;
      m_connectionInitializer = connectionInitializer;
      m_extensionsLoader = extensionsLoader;
    }

    public async Task InitializeAsync ()
    {
      log.Info ("InitializeAsync: add the .NET Core configurations");
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader (m_configuration));

      log.Info ("InitializeAsync: add the ModelDAO config reader");
      // To be able to read net.mail.from and some other config values from the database
      bool connectionSuccess;
      try {
        await m_connectionInitializer.CreateAndInitializeConnectionAsync (MAX_CONNECTION_ATTEMPTS); // 5200 connections, about 4 hours (for maintenance)
        connectionSuccess = true;
      }
      catch (Exception ex) {
        log.Error ($"InitializeAsync: connection initialization failed, throw", ex);
        connectionSuccess = false;
        throw;
      }
      try {
        Lemoine.Info.ConfigSet.AddConfigReader (new Lemoine.ModelDAO.Info.ModelDAOConfigReader (connectionSuccess));
      }
      catch (Exception ex) {
        log.Error ($"InitializeAsync: AddConfigReader ModelDAOConfigReader failed", ex);
      }
      try {
        Lemoine.Info.ConfigSet.AddConfigReader (new Lemoine.ModelDAO.Info.ApplicationStateConfigReader ("user.", connectionSuccess));
      }
      catch (Exception ex) {
        log.Error ($"InitializeAsync: AddConfigReader ApplicationStateConfigReader failed", ex);
      }

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
      try {
        Lemoine.Info.ConfigSet.AddConfigReader (new Lemoine.Business.Config.ConfigReaderFromExtensions (extensionsLoadSuccess));
      }
      catch (Exception ex) {
        log.Error ($"InitializeAsync: AddConfigReader ConfigReaderFromExtensions failed", ex);
      }
    }
  }
}
