// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Lemoine.DataRepository;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.ModelDAO.Interfaces;
using Lemoine.FileRepository;
using Lemoine.Core.Hosting;

namespace Lem_SynchronizationService
{
  /// <summary>
  /// Main class of service Lem_MachineAssociationSynchronizationService
  /// </summary>
  public sealed class SynchronizationService
    : Lemoine.Threading.IThreadServiceAsync, IDisposable
  {
    static readonly TimeSpan DEFAULT_EVERY = TimeSpan.FromMinutes (1);
    static readonly string CONFIGURATION_FILES_KEY = "ConfigurationFiles";

    static readonly string TEST_KEY = "Synchro.Test.Active";
    static readonly bool TEST_DEFAULT = false;

    static readonly string TEST_DATA_DIRECTORY_KEY = "Synchro.Test.Data.Directory";
    static readonly string TEST_DATA_DIRECTORY_DEFAULT = Lemoine.Info.PulseInfo.LocalConfigurationDirectory;

    static readonly string TEST_DATA_SUFFIX_KEY = "Synchro.Test.Data.Suffix";
    static readonly string TEST_DATA_SUFFIX_DEFAULT = ".testdata.xml";

    static readonly string TEST_SYNCHRO_DIRECTORY_KEY = "Synchro.Test.Synchro.Directory";
    static readonly string TEST_SYNCHRO_DIRECTORY_DEFAULT = Lemoine.Info.PulseInfo.LocalConfigurationDirectory;

    static readonly string TEST_SYNCHRO_SUFFIX_KEY = "Synchro.Test.Synchro.Suffix";
    static readonly string TEST_SYNCHRO_SUFFIX_DEFAULT = ".testsynchro.xml";

    #region Members
    readonly IApplicationInitializer m_applicationInitializer;
    readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource ();
    bool m_disposed = false;
    IDictionary<string, Synchronizer> m_synchronizers =
      new Dictionary<string, Synchronizer> (); // configuration => synchronizer
    #endregion


    static readonly ILog log = LogManager.GetLogger (typeof (SynchronizationService).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public SynchronizationService (IApplicationInitializer applicationInitializer)
    {
      Debug.Assert (null != applicationInitializer);

      m_applicationInitializer = applicationInitializer;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Use the default OnStart method
    /// </summary>
    public void Initialize ()
    {
      System.Threading.Tasks.Task.Run (() => InitializeAsync (CancellationToken.None)).Wait ();
    }

    /// <summary>
    /// Use the default OnStart method
    /// </summary>
    public async System.Threading.Tasks.Task InitializeAsync (CancellationToken cancellationToken)
    {
      await InitializeThreadsAsync (cancellationToken);
    }

    async System.Threading.Tasks.Task InitializeThreadsAsync (CancellationToken cancellationToken)
    {
      using (var linkedCancellationTokenSource = CancellationTokenSource
        .CreateLinkedTokenSource (m_cancellationTokenSource.Token, cancellationToken)) {
        var linkedToken = linkedCancellationTokenSource.Token;

        await m_applicationInitializer.InitializeApplicationAsync ();

        if (linkedToken.IsCancellationRequested) {
          // In case Stop is run before the service is fully initialized
          return;
        }

        string configurationFiles =
          Lemoine.Info.ConfigSet.Get<string> (CONFIGURATION_FILES_KEY);
        foreach (string configurationFile in configurationFiles.Split (new char[] { ',' })) {
          ODBCFactory factory =
            new ODBCFactory (XmlSourceType.URI, configurationFile, new ClassicConnectionParameters ());
          Repository repository = new Repository ();
          repository.MainFactory = factory;
          repository.CopyBuilder =
            new LemoineGDBBuilder ();

          if (linkedToken.IsCancellationRequested) {
            // In case Stop is run before the service is fully initialized
            return;
          }

          if (Lemoine.Info.ConfigSet.LoadAndGet<bool> (TEST_KEY,
            TEST_DEFAULT)) {
            var doc = factory.GetData (linkedToken);

            string dataOutputPath;
            {
              var directory = Lemoine.Info.ConfigSet.LoadAndGet<string> (TEST_DATA_DIRECTORY_KEY, TEST_DATA_DIRECTORY_DEFAULT);
              var fileName = configurationFile + Lemoine.Info.ConfigSet.LoadAndGet<string> (TEST_DATA_SUFFIX_KEY, TEST_DATA_SUFFIX_DEFAULT);
              dataOutputPath = System.IO.Path.Combine (directory, fileName);
            }
            log.InfoFormat ("InitializeThreads: Test data with file path {0}", dataOutputPath);
            doc.Save (dataOutputPath);

            string synchroOutputPath;
            {
              var directory = Lemoine.Info.ConfigSet.LoadAndGet<string> (TEST_SYNCHRO_DIRECTORY_KEY, TEST_SYNCHRO_DIRECTORY_DEFAULT);
              var fileName = configurationFile + Lemoine.Info.ConfigSet.LoadAndGet<string> (TEST_SYNCHRO_SUFFIX_KEY, TEST_SYNCHRO_SUFFIX_DEFAULT);
              synchroOutputPath = System.IO.Path.Combine (directory, fileName);
            }
            log.InfoFormat ("InitializeThreads: Test synchro with file path {0}", dataOutputPath);
            Repository xmlrep = new Repository ();
            xmlrep.MainFactory = factory;
            xmlrep.CopyBuilder = new XMLBuilder (synchroOutputPath);
            xmlrep.UpdateAndSynchronize (linkedToken);
          }
          else {
            Synchronizer synchronizer = new Synchronizer (repository);
            synchronizer.Every = GetEveryParameter (factory, "every");
            synchronizer.NoDataEvery = GetEveryParameter (factory, "nodataevery");
            m_synchronizers[configurationFile] = synchronizer;
            synchronizer.Start (linkedToken);
          }
        }

        // - Check no 'exit' was requested
        while (!linkedToken.IsCancellationRequested) {
          await System.Threading.Tasks.Task.Delay (100, linkedToken);
          if (linkedToken.IsCancellationRequested) {
            // OnStop was called, return
            log.Info ("InitializeThreads: cancellation requested (OnStop called), return");
            return;
          }
        }
      }

      log.Fatal ("InitializeThreads: exit was requested by one of the two main threads");
      try {
        foreach (var synchronizer in m_synchronizers.Values) {
          synchronizer.Abort (true);
        }
      }
      finally {
        Lemoine.Core.Environment.LogAndForceExit (log);
      }
    }

    /// <summary>
    /// Stop this service.
    /// </summary>
    public void OnStop ()
    {
      m_cancellationTokenSource?.Cancel ();

      foreach (var synchronizer in m_synchronizers.Values) {
        synchronizer.Cancel ();
      }
      foreach (var synchronizer in m_synchronizers.Values) {
        synchronizer.Abort (false);
      }
    }

    /// <summary>
    /// Get an every parameter in a IFactory object
    /// 
    /// If case the every parameter could not be determined,
    /// a default every parameter is taken
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="everyAttribute">every or nodataevery</param>
    /// <returns></returns>
    static TimeSpan GetEveryParameter (ODBCFactory factory,
                                       string everyAttribute)
    {
      TimeSpan every = DEFAULT_EVERY;

      try {
        string xpath = string.Format ("/root/@{0}", everyAttribute);
        every = TimeSpan.Parse (factory.GetConfigurationValue (xpath));
      }
      catch (Exception ex) {
        log.Info ("GetEveryParameter: factory.GetConfigurationValue failed", ex);
      }
      try {
        IDictionary<string, string> prefixToNamespace =
          new Dictionary<string, string> ();
        prefixToNamespace["config"] = PulseResolver.PULSE_ODBCGDBCONFIG_NAMESPACE;
        string xpath = string.Format ("/config:root/@config:{0}", everyAttribute);
        every = TimeSpan.Parse (factory.GetConfigurationValue (xpath,
                                                               prefixToNamespace));
      }
      catch (Exception ex) {
        log.Info ("GetEveryParameter: factory.GetConfigurationValue with namespace failed", ex);
      }
      try {
        IDictionary<string, string> prefixToNamespace =
          new Dictionary<string, string> ();
        prefixToNamespace["config"] = PulseResolver.PULSE_ODBCGDBCONFIG_NAMESPACE;
        string xpath = string.Format ("/root/@config:{0}", everyAttribute);
        every = TimeSpan.Parse (factory.GetConfigurationValue (xpath,
                                                               prefixToNamespace));
      }
      catch (Exception ex) {
        log.Info ("GetEveryParameter: factory.GetConfigurationValue with namespace failed", ex);
      }

      return every;
    }
    #endregion // Methods

    #region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    ~SynchronizationService () => Dispose (false);

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
        m_cancellationTokenSource?.Dispose ();
        foreach (var synchronizer in m_synchronizers.Values) {
          synchronizer.Dispose ();
        }
      }

      m_disposed = true;
    }
    #endregion // IDisposable implementation
  }
}
