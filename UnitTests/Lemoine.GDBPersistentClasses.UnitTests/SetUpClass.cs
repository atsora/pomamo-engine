// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using NUnit.Framework;
using Lemoine.Core.Log;
using Lemoine.I18N;
using System.Reflection;
using Lemoine.ModelDAO.Interfaces;
using Pulse.Extensions;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Extensions;
using Lemoine.Extensions.Plugin;
using Pulse.Database.ConnectionInitializer;
using Lemoine.Extensions.PluginFilter;
using Lemoine.GDBMigration;
using Lemoine.Info.ConfigReader.TargetSpecific;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// SetUpClass:
  /// <item>make log4net work with nunit</item>
  /// <item>Active the DaySlot cache</item>
  /// <item>Set the right DSNName</item>
  /// </summary>
  [SetUpFixture]
  public class SetUpClass
  {
    string m_previousDSNName;
    
    [OneTimeSetUp]
    public void SetUpInitialize()
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());

      // Take care the log4net.config file is added to the deployment files of the testconfig
      var log4netConfigurationPath = System.IO.Path.Combine (TestContext.CurrentContext.TestDirectory, "pulselog.log4net");
      LogManager.ClearLoggerFactories ();
      LogManager.AddLog4net (log4netConfigurationPath);

      // Activate DaySlotCache
      DaySlotCache.Activate ();

      // Set the right DSNName
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");

      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.IntermediateWorkPieceSummary.NHibernateExtension));

      // Select the right implementation of ModelDAO
      // Use a direct connection to the database
      var migrationHelper = new MigrationHelper ();
      var databaseConnectionStatusProvider = new Pulse.Database.ConnectionInitializer.ConnectionInitializer ("Lemoine.GDBPersistentClasses.UnitTests", migrationHelper) {
        KillOrphanedConnectionsFirst = true
      };
      var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
      var pluginFilter = new NoPluginFilter ();
      var pluginsLoader = new PluginsLoader (assemblyLoader);
      var nhibernatePluginsLoader = new NHibernatePluginsLoader (assemblyLoader);
      var extensionsProvider = new ExtensionsProvider (databaseConnectionStatusProvider, pluginFilter, Lemoine.Extensions.Analysis.ExtensionInterfaceProvider.GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
      Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);
      var connectionInitializer = new ConnectionInitializerWithAdditionalNHibernateExtensions (databaseConnectionStatusProvider, extensionsProvider);
      connectionInitializer.Initialize ();

      Lemoine.Info.ConfigSet.ResetConfigReader ();
      Lemoine.Info.ConfigSet.AddConfigReader (new Lemoine.ModelDAO.Info.ModelDAOConfigReader ());

      {
        var multiCatalog = new MultiCatalog ();
        multiCatalog.Add (new StorageCatalog (new Lemoine.ModelDAO.TranslationDAOCatalog (),
                                              new TextFileCatalog ("WebServiceI18N",
                                                                   Lemoine.Info.PulseInfo.LocalConfigurationDirectory)));
        PulseCatalog.Implementation = new CachedCatalog (multiCatalog);
      }
    }

    [OneTimeTearDown]
    public void SetUpTearDown()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();

      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }
      
      Lemoine.Info.ConfigSet.ResetConfigReader ();
    }
  }

}
