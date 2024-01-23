// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

#if NETSTANDARD || NET48 || NETCOREAPP
using Lemoine.Cnc.DataRepository;
using System.Text.Json;
using System.Text.Json.Serialization;
#endif // NETSTANDARD || NET48 || NETCOREAPP
using Lemoine.Collections;
using Lemoine.DataRepository;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;
using Lemoine.Core.Plugin;
using Lemoine.FileRepository;
using System.Linq;
using System.Globalization;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// Main acquisition class to be run in a thread or in a class
  /// </summary>
  public class Acquisition : ProcessOrThreadClass, IThreadClass, IProcessClass, ILogged
  {
#if NETSTANDARD || NET48 || NETCOREAPP
    /// <summary>
    /// Output in standard output with Json format
    /// </summary>
    static readonly string STDOUT_JSON_KEY = "Cnc.Stdout.Json";
    static readonly bool STDOUT_JSON_DEFAULT = false;

    /// <summary>
    /// Keep only the listed keys (in a comma separated list) if not empty
    /// </summary>
    static readonly string STDOUT_FILTER_KEY = "Cnc.Stdout.Filter";
    static readonly string STDOUT_FILTER_DEFAULT = "";

    /// <summary>
    /// Replace some keys replacedKey1:newKey1,replacedKey2:newKey2
    /// </summary>
    static readonly string STDOUT_REPLACE_KEY = "Cnc.Stdout.Replace";
    static readonly string STDOUT_REPLACE_DEFAULT = "";
#endif // NETSTANDARD || NET48 || NETCOREAPP

    #region Members
    readonly ICncEngineConfig m_cncEngineConfig;
    readonly IExtensionsLoader m_extensionsLoader;
    readonly IAssemblyLoader m_assemblyLoader;
    readonly IFileRepoClientFactory m_fileRepoClientFactory;
    readonly int m_cncAcquisitionId;
    volatile CncDataHandler m_cncDataHandler = null;
    readonly TimeSpan m_every = TimeSpan.FromSeconds (2);
    readonly bool m_useStaThread = false;
    readonly bool m_useProcess = false;
    readonly Repository m_repository;
    AcquisitionProcessExecution m_processExecution = null;
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (Acquisition).FullName);
    static readonly ILog slog = LogManager.GetLogger<Acquisition> ();

    #region Getters / Setters
    /// <summary>
    /// CncAcquisitionId
    /// </summary>
    public int CncAcquisitionId
    {
      get { return m_cncAcquisitionId; }
    }

    /// <summary>
    /// Frequency when the data is acquired
    /// </summary>
    public TimeSpan Every
    {
      get { return m_every; }
    }

    /// <summary>
    /// Use an Sta thread (from ICncAcquisition)
    /// </summary>
    public bool UseStaThread
    {
      get { return m_useStaThread; }
    }

    /// <summary>
    /// Use a process (from ICncAcquisition)
    /// </summary>
    public bool UseProcess
    {
      get { return m_useProcess; }
    }

    /// <summary>
    /// Number of requests to run before stopping the acquisition
    /// 
    /// If not set, run until the exit is requested
    /// </summary>
    public int? Calls { get; set; } = null;

    /// <summary>
    /// Latest execution date/time of the method
    /// </summary>
    public override DateTime LastExecution
    {
      get {
        if (null == m_cncDataHandler) {
          log.DebugFormat ("LastExecution.get: " +
                           "Cnc data handler is not set yet " +
                           "=> fallback to base.LastExecution");
          return base.LastExecution;
        }
        else {
          return m_cncDataHandler.LastExecution;
        }
      }
      set {
        if (null == m_cncDataHandler) {
          log.DebugFormat ("LastExecution.set: " +
                           "Cnc data handler is not set yet " +
                           "=> fallback to base.LastExecution");
          base.LastExecution = value;
        }
        else {
          m_cncDataHandler.LastExecution = value;
        }
      }
    }

    /// <summary>
    /// Reference to the CNC Data Handler
    /// </summary>
    public CncDataHandler CncDataHandler => m_cncDataHandler;

    /// <summary>
    /// Final data once all the module requests are completed
    /// 
    /// Thread safe access (a concurrent dictionary is used)
    /// 
    /// This is possible that from time to time a deprecated value is returned,
    /// but this should be pretty rare (there is no global lock)
    /// 
    /// null if the cnc data handler is not initialized yet
    /// </summary>
    public IDictionary<string, object> FinalData
    {
      get {
        if (null == m_cncDataHandler) {
          log.Error ($"FinalData.get: cncDataHandler is not initialized yet => return null");
          return null;
        }
        return m_cncDataHandler.FinalData;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
#if NETSTANDARD || NET48 || NETCOREAPP
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cncEngineConfig">not null</param>
    /// <param name="extensionsLoader">not null</param>
    /// <param name="cncAcquisition">not null</param>
    /// <param name="assemblyLoader"></param>
    /// <param name="fileRepoClientFactory"></param>
    public Acquisition (ICncEngineConfig cncEngineConfig, IExtensionsLoader extensionsLoader, ICncAcquisition cncAcquisition, IAssemblyLoader assemblyLoader, IFileRepoClientFactory fileRepoClientFactory)
    {
      Debug.Assert (null != cncEngineConfig);
      Debug.Assert (null != extensionsLoader);
      Debug.Assert (null != cncAcquisition);

      m_cncEngineConfig = cncEngineConfig;
      m_extensionsLoader = extensionsLoader;
      m_cncAcquisitionId = cncAcquisition.Id;
      m_assemblyLoader = assemblyLoader ?? AssemblyLoaderProvider.AssemblyLoader;
      m_fileRepoClientFactory = fileRepoClientFactory;
      m_every = cncAcquisition.Every;
      m_useStaThread = cncAcquisition.StaThread;
      m_useProcess = cncAcquisition.UseProcess;
      m_repository = CreateRepositoryFromCncAcquisition (cncAcquisition.Id);

      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                typeof (Acquisition).FullName,
                                                cncAcquisition.Id));

      // Set the parent process Id
      try {
        base.ParentProcessId = Process.GetCurrentProcess ().Id;
      }
      catch (Exception ex) {
        log.Error ($"Acquisition: error while getting the parent process Id", ex);
      }
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="cncEngineConfig">not null</param>
    /// <param name="extensionsLoader">not null</param>
    /// <param name="cncAcquisitionId"></param>
    /// <param name="every"></param>
    /// <param name="assemblyLoader"></param>
    /// <param name="fileRepoClientFactory"></param>
    /// <param
    /// <param name="useStaThread"></param>
    /// <param name="useProcess">use process</param>
    public Acquisition (ICncEngineConfig cncEngineConfig, IExtensionsLoader extensionsLoader, int cncAcquisitionId, TimeSpan every, IAssemblyLoader assemblyLoader, IFileRepoClientFactory fileRepoClientFactory, bool useStaThread = false, bool useProcess = false)
    {
      Debug.Assert (null != cncEngineConfig);
      Debug.Assert (null != extensionsLoader);

      m_cncEngineConfig = cncEngineConfig;
      m_extensionsLoader = extensionsLoader;
      m_cncAcquisitionId = cncAcquisitionId;
      m_assemblyLoader = assemblyLoader ?? AssemblyLoaderProvider.AssemblyLoader;
      m_fileRepoClientFactory = fileRepoClientFactory;
      m_every = every;
      m_useStaThread = useStaThread;
      m_useProcess = useProcess;
      m_repository = CreateRepositoryFromCncAcquisition (cncAcquisitionId);

      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                typeof (Acquisition).FullName,
                                                cncAcquisitionId));
    }
#endif // NETSTANDARD || NET48 || NETCOREAPP

    /// <summary>
    /// Acquisition using a local file
    /// </summary>
    /// <param name="cncEngineConfig"></param>
    /// <param name="localFilePath"></param>
    /// <param name="useProcess"></param>
    /// <param name="assemblyLoader"></param>
    /// <param name="fileRepoClientFactory"></param>
    /// <param name="extensionsLoader">not null</param>
    public Acquisition (ICncEngineConfig cncEngineConfig, string localFilePath, IAssemblyLoader assemblyLoader, IFileRepoClientFactory fileRepoClientFactory, IExtensionsLoader extensionsLoader, bool useProcess = false)
    {
      Debug.Assert (null != cncEngineConfig);
      Debug.Assert (null != extensionsLoader, "extensionsLoader not null");

      m_cncEngineConfig = cncEngineConfig;
      m_cncAcquisitionId = 0;
      m_assemblyLoader = assemblyLoader;
      m_fileRepoClientFactory = fileRepoClientFactory;
      m_extensionsLoader = extensionsLoader;
      m_useProcess = useProcess;
      m_repository = CreateRepositoryFromLocalFile (localFilePath);
    }

#if NETSTANDARD || NET48 || NETCOREAPP
    /// <summary>
    /// Acquisition using a local file and parameters
    /// </summary>
    /// <param name="cncEngineConfig"></param>
    /// <param name="localFilePath"></param>
    /// <param name="numParameters"></param>
    /// <param name="useProcess"></param>
    /// <param name="assemblyLoader"></param>
    /// <param name="fileRepoClientFactory"></param>
    /// <param name="extensionsLoader">not null</param>
    public Acquisition (ICncEngineConfig cncEngineConfig, string localFilePath, string numParameters, IDictionary<string, string> jsonParameters, IAssemblyLoader assemblyLoader, IFileRepoClientFactory fileRepoClientFactory, IExtensionsLoader extensionsLoader, bool useProcess = false)
    {
      Debug.Assert (null != cncEngineConfig);
      Debug.Assert (null != extensionsLoader, "extensionsLoader not null");

      m_cncEngineConfig = cncEngineConfig;
      m_cncAcquisitionId = 0;
      m_assemblyLoader = assemblyLoader;
      m_fileRepoClientFactory = fileRepoClientFactory;
      m_extensionsLoader = extensionsLoader;
      m_useProcess = useProcess;
      m_repository = CreateRepositoryFromLocalFileParameters (localFilePath, numParameters, jsonParameters);
    }

    /// <summary>
    /// Acquisition using a <see cref="Repository"/>
    /// </summary>
    /// <param name="cncEngineConfig">not null</param>
    /// <param name="cncAcquisitionId"></param>
    /// <param name="repository"></param>
    /// <param name="assemblyLoader"></param>
    /// <param name="fileRepoClientFactory"></param>
    /// <param name="extensionsLoader">not null</param>
    public Acquisition (ICncEngineConfig cncEngineConfig, int cncAcquisitionId, Repository repository, IAssemblyLoader assemblyLoader, IFileRepoClientFactory fileRepoClientFactory, IExtensionsLoader extensionsLoader)
    {
      Debug.Assert (null != cncEngineConfig);
      Debug.Assert (null != repository);
      Debug.Assert (null != extensionsLoader);

      m_cncEngineConfig = cncEngineConfig;
      m_cncAcquisitionId = cncAcquisitionId;
      m_repository = repository;
      m_assemblyLoader = assemblyLoader;
      m_fileRepoClientFactory = fileRepoClientFactory;
      m_extensionsLoader = extensionsLoader;
    }
#endif // NETSTANDARD || NET48 || NETCOREAPP
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Build a parameters string from a dictionary
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static string GetParametersString (IDictionary<string, object> parameters)
    {
      var a = new string[10];
      int maxParamNumber = 0;
      foreach (var kv in parameters) {
        var k = kv.Key;
        var v = kv.Value;
        if (k.StartsWith ("Param", StringComparison.CurrentCultureIgnoreCase)) {
          if (int.TryParse (k.Substring ("Param".Length), out var paramNumber)) {
            if (a.Length < paramNumber) {
              slog.Error ($"GetParametersString: param number {paramNumber} for name {k} is not supported yet");
            }
            else {
              var vs = v switch {
                double d => d.ToString (CultureInfo.InvariantCulture),
                _ => v.ToString ()
              };
              a[paramNumber - 1] = vs;
              if (maxParamNumber < paramNumber) {
                maxParamNumber = paramNumber;
              }
            }
          }
          else {
            slog.Error ($"GetParametersString: parameter name {k} does not contain the param number");
          }
        }
        else {
          slog.Error ($"GetParametersString: skip the parameter of name {k}");
        }
      }
      if (0 < maxParamNumber) {
        return a.Take (maxParamNumber).ToListString ();
      }
      else {
        slog.Warn ($"GetParametersString: no parameter found => return an empty string");
        return "";
      }
    }

    /// <summary>
    /// Get a module that is associated to specific reference
    /// </summary>
    /// <param name="moduleRef"></param>
    /// <returns>null if the cnc data handler is not initialized yet</returns>
    public CncModuleExecutor GetModule (string moduleRef)
    {
      if (null == m_cncDataHandler) {
        log.Error ($"GetModule: cncDataHandler is not initialized yet, moduleRef={moduleRef} => return null");
        return null;
      }
      return m_cncDataHandler.GetModule (moduleRef);
    }

    /// <summary>
    /// Process a specific module given its module XML Element and its <see cref="CncModuleExecutor"/>
    /// 
    /// Support only the element with tag moduleref for the moment
    /// </summary>
    /// <param name="moduleElement"></param>
    /// <param name="data">not null</param>
    /// <param name="cancellationToken"></param>
    public void ProcessModule (System.Xml.XmlElement moduleElement, IDictionary<string, object> data, CancellationToken cancellationToken)
    {
      Debug.Assert (null != data);

      if (null == m_cncDataHandler) {
        log.Error ($"ProcessModule: cncDataHandler is not initialized yet => throw an exception");
        throw new Exception ("Cnc data handler is not initialized yet");
      }

      if (moduleElement.Name.Equals ("moduleref")) {
        var cncModuleExecutor = GetModule (moduleElement.GetAttribute ("ref"));
        m_cncDataHandler.ProcessModule (moduleElement, cncModuleExecutor, data, cancellationToken);
      }
      else { // Not supported
        log.Error ($"ProcessModule: element with name {moduleElement.Name} is not supported");
        throw new NotSupportedException ($"Element {moduleElement.Name} is not supported");
      }
    }

#if NETSTANDARD || NET48 || NETCOREAPP
    Repository CreateRepositoryFromCncAcquisition (int cncAcquisitionId)
    {
      var cncDirectory = Path.Combine (PulseInfo.LocalConfigurationDirectory, "Cnc");
      var factory = new CncFileRepoFactory (m_extensionsLoader, m_cncAcquisitionId, this);
      var localConfigurationFile = $"CncAcquisition-{m_cncAcquisitionId}.xml";
      if (Directory.Exists (cncDirectory)) {
        localConfigurationFile = Path.Combine (cncDirectory,
                                               localConfigurationFile);
      }
      else {
        localConfigurationFile = Path.Combine (PulseInfo.LocalConfigurationDirectory,
                                               localConfigurationFile);
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"CreateRepositoryFromCncAcquisition: localConfigurationFile is {localConfigurationFile}");
      }
      try {
        var copyFactory = new XMLFactory (XmlSourceType.URI, localConfigurationFile);
        var copyBuilder = new XMLBuilder (localConfigurationFile);
        log.Debug ("CreateRepositoryFromCncAcquisition: copy factory and builder created");
        return new Repository (factory, copyBuilder, copyFactory);
      }
      catch (Exception ex) {
        log.Error ("CreateRepositoryFromCncAcquisition: new Repository failed", ex);
        throw;
      }
    }
#endif // NETSTANDARD || NET48 || NETCOREAPP

    Repository CreateRepositoryFromLocalFile (string localFilePath)
    {
      var factory = new XMLFactory (XmlSourceType.URI, localFilePath);
      return new Repository (factory);
    }

#if NETSTANDARD || NET48 || NETCOREAPP
    Repository CreateRepositoryFromLocalFileParameters (string localFilePath, string parameters, IDictionary<string, string> keyParams)
    {
      var name = Path.GetFileNameWithoutExtension (localFilePath);
      var cncAcquisition = ModelDAO.ModelDAOHelper.ModelFactory.CreateCncAcquisition ();
      cncAcquisition.Name = name;
      cncAcquisition.ConfigFile = localFilePath;
      cncAcquisition.ConfigParameters = parameters;
      cncAcquisition.ConfigKeyParams = keyParams;
      var factory = new CncFileRepoFactory (m_extensionsLoader, cncAcquisition, checkedThread: null);
      return new Repository (factory);
    }
#endif // NETSTANDARD || NET48 || NETCOREAPP

    /// <summary>
    /// Initialize the CNC data handler
    /// 
    /// It is already called by the Run method, so normally (except in Lem_CncGUI)
    /// it should not be called manually
    /// </summary>
    /// <returns>success</returns>
    public bool InitDataHandler (CancellationToken cancellationToken)
    {
      try {
        m_cncDataHandler = new CncDataHandler (m_repository, m_assemblyLoader, m_fileRepoClientFactory, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested ();
      }
      catch (ConfigReloadRequired) {
        log.Info ("InitDataHandler: ConfigReloadRequired was returned, return false");
        return false;
      }
      catch (Exception ex) {
        log.Error ("InitDataHandler: new CncDataHandler failed", ex);
        throw;
      }
      Debug.Assert (null != m_cncDataHandler);
      m_cncDataHandler.Every = m_every;
      if (log.IsDebugEnabled) {
        log.Debug ($"InitDataHandler: successfully initialized with every={m_every}");
      }
      return true;
    }
    #endregion // Methods

    #region Implementation of ProcessOrThreadClass
    /// <summary>
    /// Main thread method
    /// 
    /// Implements <see cref="ProcessOrThreadClass">ProcessOrThreadClass</see>
    /// </summary>
    protected override void Run (System.Threading.CancellationToken cancellationToken)
    {
      try {
        SetActive ();
        if (null != m_cncDataHandler) {
          log.Warn ("Run: reset the data handler");
          m_cncDataHandler.Dispose ();
          m_cncDataHandler = null;
        }
        SetActive ();
        while (!InitDataHandler (cancellationToken) && !cancellationToken.IsCancellationRequested) {
          // Try to reload once the configuration
          SetActive ();
          log.Info ($"Run: the data handler is not initialized, retry in 1s");
          this.Sleep (TimeSpan.FromSeconds (1), cancellationToken);
        }
        SetActive ();
        if (!cancellationToken.IsCancellationRequested) {
          m_cncDataHandler.Work (this.UseStampFile, this.ParentProcessId, cancellationToken, this.Calls);
        }
      }
      catch (Exception ex) {
        log.Fatal ("Run: exception", ex);
      }

      if (cancellationToken.IsCancellationRequested) {
        log.Error ("Run: cancellation was requested");
      }
    }

    /// <summary>
    /// Given the Cnc Acquisition ID, get the stamp file to use
    /// to check a process
    ///
    /// Implements <see cref="ProcessOrThreadClass">ProcessOrThreadClass</see>
    /// </summary>
    /// <returns></returns>
    public override string GetStampFileName ()
    {
      if (null == m_cncDataHandler) { // Fallback
        return $"CncStamp-{m_cncAcquisitionId}";
      }
      else {
        return m_cncDataHandler.GetStampFileName ();
      }
    }

    /// <summary>
    /// Logger
    /// 
    /// Implements <see cref="ProcessOrThreadClass">ProcessOrThreadClass</see>
    /// </summary>
    /// <returns></returns>
    public override ILog GetLogger ()
    {
      return log;
    }
    #endregion // Implementation of ProcessOrThreadClass

    /// <summary>
    /// Start the acquisition in a thread if UseProcess is false, else in a process
    /// </summary>
    public void StartThreadOrProcess (CancellationToken cancellationToken)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"StartThreadOrProcess: start UseProcess={this.UseProcess}");
      }

      try {
        if (this.UseProcess) {
          m_processExecution = new AcquisitionProcessExecution (m_cncEngineConfig, this);
          m_processExecution.Start ();
        }
        else {
          this.Start (cancellationToken, this.UseStaThread ? ApartmentState.STA : ApartmentState.MTA);
        }
        if (log.IsDebugEnabled) {
          log.Debug ("StartThreadOrProcess: successfully started");
        }
      }
      catch (Exception ex) {
        log.Error ("StartThreadOrProcess: error while trying to start the acquisition", ex);
      }
    }

    /// <summary>
    /// Add the acquisition to a <see cref="CheckThreadsAndProcesses"/>
    /// </summary>
    /// <param name="threadsAndProcessesChecker"></param>
    public void AddToThreadsAndProcessesChecker (CheckThreadsAndProcesses threadsAndProcessesChecker)
    {
      if (null != m_processExecution) {
        threadsAndProcessesChecker.AddProcess (m_processExecution);
      }
      else {
        threadsAndProcessesChecker.AddThread (this);
      }
    }

    public void WriteFinalDataToStdout ()
    {
      IDictionary<string, object> d = this.FinalData;
#if NETSTANDARD || NET48 || NETCOREAPP
      var stdoutJson = Lemoine.Info.ConfigSet
        .LoadAndGet (STDOUT_JSON_KEY, STDOUT_JSON_DEFAULT);
      var filter = Lemoine.Info.ConfigSet
        .LoadAndGet (STDOUT_FILTER_KEY, STDOUT_FILTER_DEFAULT);
      if (!string.IsNullOrEmpty (filter)) {
        var filterItems = filter.Split (',');
        d = d
          .Where (x => filterItems.Contains (x.Key))
          .ToDictionary (x => x.Key, x => x.Value);
      }
      var replace = Lemoine.Info.ConfigSet
        .LoadAndGet (STDOUT_REPLACE_KEY, STDOUT_REPLACE_DEFAULT);
      if (!string.IsNullOrEmpty (replace)) {
        var replaceItems = replace
          .Split (',')
          .Select (x => x.Split (':'));
        d = d
          .ToDictionary (x => Replace (x.Key, replaceItems), x => x.Value);
      }
      if (stdoutJson) {
        try {
          var json = ConvertToJson (d);
          System.Console.WriteLine (json);
        }
        catch (Exception ex) {
          log.Fatal ($"WriteFinalDataToStdout: error while trying to output FinalData in json", ex);
        }
      }
      else {
#endif // NETSTANDARD || NET48 || NETCOREAPP
        foreach (var item in d) {
          try {
            System.Console.WriteLine ($"{item.Key}={m_cncDataHandler.GetStringFromKeyValueItem (item)}");
          }
          catch (Exception ex) {
            log.Fatal ($"WriteFinalDataToStdout: error while trying to output FinalData in the logs for key {item.Key}", ex);
          }
        }
#if NETSTANDARD || NET48 || NETCOREAPP
      }
#endif // NETSTANDARD || NET48 || NETCOREAPP
    }

#if NETSTANDARD || NET48 || NETCOREAPP
    string ConvertToJson (object v)
    {
      var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
      options.Converters.Add (new JsonStringEnumConverter ());
      return JsonSerializer.Serialize (v, options);
    }

    string Replace (string s, IEnumerable<string[]> r)
    {
      if (r.Select (x => x[0]).Contains (s)) {
        return r.First (x => x[0].Equals (s))[1];
      }
      else {
        return s;
      }
    }
#endif  // NETSTANDARD || NET48 || NETCOREAPP
  }
}
