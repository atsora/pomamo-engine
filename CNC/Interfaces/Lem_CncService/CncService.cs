// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;

using Lemoine.CncEngine;
using Lemoine.Threading;
using Lemoine.Core.Log;
using System.Linq;
using System.Diagnostics;
#if NETSTANDARD || NET48 || NETCOREAPP
using Lemoine.Business.Config;
using Lemoine.Extensions;
using Lemoine.ModelDAO.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.Extensions.ExtensionsProvider;
using Pulse.Extensions;
#endif // NETSTANDARD || NET48 || NETCOREAPP
using Lemoine.Extensions.Interfaces;
using Lemoine.FileRepository;
using Lemoine.Core.Plugin;
using System.ServiceModel;
using Lem_CncService.Wcf;
using Lemoine.Model;
using Lemoine.Extensions.DummyImplementations;
#if !NET40
using Lemoine.GDBMigration;
using Lemoine.Core.Hosting.LctrChecker;
using Lemoine.ModelDAO.LctrChecker;
using Lemoine.Info.ConfigReader;
#endif // !NET40

namespace Lem_CncService
{
  /// <summary>
  /// New Cnc Service
  /// </summary>
  public sealed class CncService
#if NETSTANDARD || NET48 || NETCOREAPP
    : Lemoine.Threading.IThreadServiceAsync, IDisposable
#else // !(NETSTANDARD || NET48 || NETCOREAPP)
    : Lemoine.Threading.IThreadService, IDisposable
#endif // !(NETSTANDARD || NET48 || NETCOREAPP)
  {
    static readonly string LOCAL_CNC_CONFIGURATION_KEY = "Cnc.LocalCncConfiguration";
    static readonly string LOCAL_CNC_CONFIGURATION_DEFAULT = "";

    static readonly string ACTIVATE_WCF_KEY = "Cnc.Wcf.Activate";
    static readonly bool ACTIVATE_WCF_DEFAULT = true;

    static readonly string BASE_URLS_KEY = "Cnc.Wcf.BaseUrls"; // ; comma separated list
    static readonly string BASE_URLS_DEFAULT = "http://localhost:8082";

#if NETSTANDARD || NET48 || NETCOREAPP
    static readonly int MAX_CONNECTION_ATTEMPTS = 5;
#endif // NETSTANDARD || NET48 || NETCOREAPP

#region Members
    readonly string m_localCncConfiguration;
    readonly IFileRepoClientFactory m_fileRepoClientFactory;
    readonly IAssemblyLoader m_assemblyLoader;
    readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource ();
    bool m_disposed = false;
    readonly IList<Acquisition> m_threadCncAcquisitions = new List<Acquisition> ();
    readonly IList<AcquisitionProcessExecution> m_processCncAcquisitions = new List<AcquisitionProcessExecution> ();

    readonly CheckThreadsAndProcesses m_checkThreadsAndProcesses = new CheckThreadsAndProcesses ();

    readonly IList<ServiceHost> m_serviceHosts = new List<ServiceHost> ();
#endregion

    static readonly ILog log = LogManager.GetLogger (typeof (CncService).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncService ()
    {
      m_fileRepoClientFactory =
        new FileRepoClientFactoryNoCorba (DefaultFileRepoClientMethod.Multi);

      m_assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();

      m_localCncConfiguration = Lemoine.Info.ConfigSet
        .LoadAndGet (LOCAL_CNC_CONFIGURATION_KEY, LOCAL_CNC_CONFIGURATION_DEFAULT);
#if NETSTANDARD || NET48 || NETCOREAPP
      if (string.IsNullOrEmpty (m_localCncConfiguration)) {
        var configUpdateChecker = new ConfigUpdateCheckerWithAvailableFileRepository ();
        var fileRepoChecker = new FileRepoChecker ();
        m_checkThreadsAndProcesses.AddAdditionalCheckers (configUpdateChecker, fileRepoChecker);
      }
#endif // NETSTANDARD || NET48 || NETCOREAPP
    }

#region Service methods
    /// <summary>
    /// <see cref="IThreadService"/>
    /// </summary>
    public void Initialize ()
    {
#if NETSTANDARD || NET48 || NETCOREAPP
      System.Threading.Tasks.Task.Run (() => InitializeAsync (CancellationToken.None)).Wait ();
    }

    /// <summary>
    /// Use the default OnStart method
    /// </summary>
    public async System.Threading.Tasks.Task InitializeAsync (CancellationToken cancellationToken)
    {
      try {
        await InitializeThreadsAsync (cancellationToken);
      }
      catch (Exception ex) {
        log.Error ($"InitializeAsync: exception", ex);
        throw;
      }
    }

    async System.Threading.Tasks.Task InitializeThreadsAsync (CancellationToken cancellationToken)
    {
      using (var linkedCancellationTokenSource = CancellationTokenSource
        .CreateLinkedTokenSource (m_cancellationTokenSource.Token, cancellationToken)) {
#else // !(NETSTANDARD || NET48 || NETCOREAPP)
      using (var linkedCancellationTokenSource = CancellationTokenSource
        .CreateLinkedTokenSource (m_cancellationTokenSource.Token)) {
#endif // NETSTANDARD || NET48 || NETCOREAPP
        var linkedToken = linkedCancellationTokenSource.Token;

#if NET40
        var cncEngineConfig = new CncEngineConfig40 ();
#else // !NET40
        var cncEngineConfig = new Lemoine.CncEngine.CncEngineConfig (false, true) {
          ConsoleProgramName = "Lem_CncConsole",
        };
#endif // !NET40
#if NETSTANDARD || NET48 || NETCOREAPP
        var applicationName = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Name;
        var migrationHelper = new MigrationHelper ();
        var connectionInitializer = new Pulse.Database.ConnectionInitializer.ConnectionInitializer (applicationName, migrationHelper) {
          KillOrphanedConnectionsFirst = true
        };
        var pluginFilter = new PluginFilterFromFlag (PluginFlag.Cnc);
        var pluginsLoader = new PluginsLoader (m_assemblyLoader);
        var nhibernatePluginsLoader = new DummyPluginsLoader ();
        var extensionsProvider = new ExtensionsProvider (connectionInitializer, pluginFilter, GetInterfaceProviders (), pluginsLoader, nhibernatePluginsLoader);
        Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider);
        Lemoine.Core.Plugin.AssemblyLoaderProvider.AssemblyLoader = m_assemblyLoader; // For MultiConfigurableCncDataQueue
        var lctrChecker = new CheckDatabaseLctrChecker (new DatabaseLctrChecker (), connectionInitializer);

        IExtensionsLoader extensionsLoader = new ExtensionsLoader (m_fileRepoClientFactory, extensionsProvider, lctrChecker, connectionInitializer); // LoadExtensionsAsync is delayed (in Lemoine.Cnc.FileRepoFactory)

        if (string.IsNullOrEmpty (m_localCncConfiguration)) {
          log.Info ("InitializeThreadsAsync: initialize the database");
          // Use a direct connection to the database
          // For the moment, I doubt the cnc service needs the NHibernate extensions.
          // If needed in the future, use ConnectionInitializerWithNHibernateExtensions instead
          bool connectionSuccess;
          try {
            await connectionInitializer.CreateAndInitializeConnectionAsync (MAX_CONNECTION_ATTEMPTS, linkedToken); // Only 5 attempts, about 10s
            connectionSuccess = true;
          }
          catch (Exception ex) {
            log.Error ($"InitializeThreadsAsync: connection initialization failed but continue", ex);
            connectionSuccess = false;
          }

          log.Info ("InitializeThreadsAsync: add the ModelDAO config reader");
          // To be able to read net.mail.from and some other config values from the database
          var modelDaoConfigReader = new Lemoine.ModelDAO.Info.ModelDAOConfigReader (false);
          if (connectionSuccess) {
            try {
              modelDaoConfigReader.Initialize (linkedToken);
            }
            catch (Exception ex) {
              log.Error ($"InitializeThreadsAsync: Initialize of ModelDAOConfigReader in exception, skip it", ex);
            }
          }
          Lemoine.Info.ConfigSet.AddConfigReader (new PersistentCacheConfigReader (modelDaoConfigReader, "cncservice.modaldaoconfig.cache"));

          if (linkedToken.IsCancellationRequested) {
            return;
          }

          log.Info ("InitializeThreadsAsync: add the config readers from extensions");
          bool extensionsLoadSuccess;
          try {
            await extensionsLoader.LoadExtensionsAsync (linkedToken);
            extensionsLoadSuccess = true;
          }
          catch (Exception ex) {
            log.Error ($"InitializeThreadsAsync: LoadExtensionsAsync failed but continue", ex);
            extensionsLoadSuccess = false;
          }
          var configReaderFromExtensions = new Lemoine.Business.Config.ConfigReaderFromExtensions (false);
          if (extensionsLoadSuccess) {
            try {
              configReaderFromExtensions.Initialize ();
            }
            catch (Exception ex) {
              log.Error ($"InitializeThreadsAsync: Initialize of ConfigReaderFromExtensions failed", ex);
            }
          }
          Lemoine.Info.ConfigSet.AddConfigReader (new PersistentCacheConfigReader (configReaderFromExtensions, "cncservice.configfromextensions.cache"));

          if (linkedToken.IsCancellationRequested) {
            return;
          }

          m_fileRepoClientFactory.InitializeFileRepoClient (linkedToken);
          if (log.IsDebugEnabled) {
            log.Debug ($"InitializeThreadsAsync: InitializeFileRepoClient completed");
          }

          if (linkedToken.IsCancellationRequested) {
            return;
          }

          m_checkThreadsAndProcesses.InitializeAdditionalCheckers (linkedToken);
          if (log.IsDebugEnabled) {
            log.Debug ($"InitializeThreadsAsync: InitializeAdditionalCheckers completed");
          }
        }
        // else no database access or FileRepo implementation is required

        if (linkedToken.IsCancellationRequested) {
          return;
        }
#endif // NETSTANDARD || NET48 || NETCOREAPP

        IAcquisitionSet acquisitionListBuilder;
        IAcquisitionFinder acquisitionFinder;
#if NETSTANDARD || NET48 || NETCOREAPP
        if (string.IsNullOrEmpty (m_localCncConfiguration)) {
          var cncAcquisitionInitializer = new Lemoine.CncEngine.LpostCncAcquisitionInitializer<Lemoine.GDBPersistentClasses.MachineModule> (cncEngineConfig);

          // TODO: really manage the distant resources ?
          cncAcquisitionInitializer.CopyDistantResources (linkedToken);
          if (log.IsDebugEnabled) {
            log.Debug ($"InitializeThreadsAsync: CopyDistantResources completed");
          }

          // - Get the number of free licenses for CNC output modules
          //   Do it here to be sure the instance is created in the parent thread
          int freeLicenses = cncAcquisitionInitializer.GetFreeLicenses ();
          if (log.IsDebugEnabled) {
            log.Debug ($"InitializeThreadsAsync: freeLicenses={freeLicenses}");
          }

          // - Get the list of cncAcquisitions
          acquisitionListBuilder = new AcquisitionsFromCncAcquisitions (cncEngineConfig, extensionsLoader, cncAcquisitionInitializer, m_assemblyLoader, m_fileRepoClientFactory);
          // Note: ToList () to initialize first all the threads before starting them,
          //       else there are some CORBA problems
          //       while trying to download the configuration files

          acquisitionFinder = new AcquisitionFinderById ();
        }
        else {
          acquisitionListBuilder = new AcquisitionsFromLocalFile (cncEngineConfig, m_localCncConfiguration, m_assemblyLoader, m_fileRepoClientFactory, extensionsLoader);

          acquisitionFinder = new AcquisitionFinderUnique ();
        }
#else // !(NETSTANDARD || NET48 || NETCOREAPP)
        acquisitionListBuilder = new AcquisitionsFromLocalFile (cncEngineConfig, m_localCncConfiguration, m_assemblyLoader, m_fileRepoClientFactory, new ExtensionsLoaderDummy ());
        acquisitionFinder = new AcquisitionFinderUnique ();
#endif // !(NETSTANDARD || NET48 || NETCOREAPP)

        if (linkedToken.IsCancellationRequested) {
          return;
        }

        if (log.IsDebugEnabled) {
          log.Debug ($"InitializeThreadsAsync: about to get acquisitions");
        }
        var acquisitions = acquisitionListBuilder.GetAcquisitions (linkedToken);
        if (log.IsDebugEnabled) {
          log.Debug ($"InitializeThreadsAsync: got {acquisitions.Count ()} acquisitions");
        }
        foreach (var acquisition in acquisitions) {
          if (linkedToken.IsCancellationRequested) {
            return;
          }
          if (log.IsDebugEnabled) {
            log.Debug ($"InitializeThreadsAsync: about to start {acquisition}");
          }
          acquisition.StartThreadOrProcess (linkedToken);
        }

        foreach (var acquisition in acquisitions) {
          if (log.IsDebugEnabled) {
            log.Debug ($"InitializeThreadsAsync: about to add {acquisition} to thread or process checker");
          }
          acquisition.AddToThreadsAndProcessesChecker (m_checkThreadsAndProcesses);
        }
        m_checkThreadsAndProcesses.Start (linkedToken);
        if (log.IsDebugEnabled) {
          log.Debug ("InitializeThreadsAsync: check threads and processes started");
        }

        var activateWcf = Lemoine.Info.ConfigSet.LoadAndGet (ACTIVATE_WCF_KEY, ACTIVATE_WCF_DEFAULT);
        if (activateWcf) {
          if (log.IsDebugEnabled) {
            log.Debug ($"InitializeThreadsAsync: create the WCF services");
          }
          CreateWcfServices (acquisitionListBuilder, acquisitionFinder);
        }
      }
    }

#if NETSTANDARD || NET48 || NETCOREAPP
    static IEnumerable<IExtensionInterfaceProvider> GetInterfaceProviders () => Lemoine.Extensions.Cnc.ExtensionInterfaceProvider.GetInterfaceProviders ().Union (new List<IExtensionInterfaceProvider> { new Pulse.Extensions.Database.ExtensionInterfaceProvider () });
#endif

    void CreateWcfServices (IAcquisitionSet acquisitionSet, IAcquisitionFinder acquisitionFinder)
    {
      var baseUrls = Lemoine.Info.ConfigSet.LoadAndGet (BASE_URLS_KEY, BASE_URLS_DEFAULT);

      try {
        var service = new WcfService (acquisitionSet, acquisitionFinder);
        var baseUris = baseUrls.Split (new char[] { ';' })
          .Select (x => new Uri (x))
          .ToArray ();
        var host = new ServiceHost (service, baseUris);
        host.Open ();
        m_serviceHosts.Add (host);
      }
      catch (Exception ex) {
        log.Error ("CreateWcfServices: exception in starting the wcf service", ex);
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Service.Service"/>
    /// </summary>
    public void OnStart (params string[] args)
    {
      this.Initialize ();
    }

    void CloseServiceHosts ()
    {
      foreach (var serviceHost in m_serviceHosts) {
        try {
          serviceHost.Close ();
        }
        catch (Exception ex) {
          log.Error ($"OnStop: closing {serviceHost} failed", ex);
        }
      }
    }

    /// <summary>
    /// Run on service stop.
    /// </summary>
    public void OnStop ()
    {
      LogManager.SetApplicationStopping ();

      m_cancellationTokenSource?.Cancel ();

      CloseServiceHosts ();

      const int timeout = 100;

      // - Stop the checking thread
      m_checkThreadsAndProcesses?.Abort ();

      // - Stop the running Lem_CncConsole processes
      foreach (AcquisitionProcessExecution processCncAcquisition in m_processCncAcquisitions) {
        processCncAcquisition.Abort ();
      }
      // - Cancel the threads
      foreach (Acquisition threadCncAcquisition in m_threadCncAcquisitions) {
        threadCncAcquisition?.Cancel ();
      }

      // - Stop the threads
      foreach (Acquisition threadCncAcquisition in m_threadCncAcquisitions) {
        try {
          threadCncAcquisition.Abort ();
        }
        catch (Exception ex) {
          threadCncAcquisition.GetLogger ().Error ("Aborting thread of cncAcquisition failed", ex);
        }
      }
      // Really wait before all threads finish
      foreach (Acquisition threadCncAcquisition in m_threadCncAcquisitions) {
        var thread = threadCncAcquisition.Thread;
        if ((null != thread) && (thread.IsAlive)) {
          threadCncAcquisition.GetLogger ().Warn ("wait thread of cncAcquisition finishes");
          try {
            thread.Join (timeout);
          }
          catch (Exception ex) {
            threadCncAcquisition.GetLogger ().Error ("Joining thread of cncAcquisition failed", ex);
          }
        }
      }
    }
#endregion

#region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    ~CncService () => Dispose (false);

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    public void Dispose ()
    {
      Dispose (true);
      GC.SuppressFinalize (this);
    }

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    /// <param name="disposing"></param>
    void Dispose (bool disposing)
    {
      if (m_disposed) {
        return;
      }

      if (disposing) {
        CloseServiceHosts ();
        m_checkThreadsAndProcesses?.Dispose ();
        m_cancellationTokenSource?.Dispose ();
      }

      m_disposed = true;
    }
#endregion // IDisposable implementation
  }
}
