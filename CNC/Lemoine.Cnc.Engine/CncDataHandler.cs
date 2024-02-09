// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;

using Lemoine.Collections;
#if NETSTANDARD || NET48 || NETCOREAPP
using Lemoine.ModelDAO;
using Lemoine.Database.Persistent;
#endif // NETSTANDARD || NET48 || NETCOREAPP
using Lemoine.DataRepository;
using Lemoine.Threading;
using Lemoine.Core.Log;
using System.Collections.Concurrent;
using Lemoine.Core.Plugin.TargetSpecific;
using Lemoine.Core.Plugin;
using Lemoine.Cnc;
using Lemoine.FileRepository;
using Lemoine.Model;
using System.Xml.Linq;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// Exception to reload the configuration
  /// </summary>
  [Serializable]
  public class ConfigReloadRequired : Exception
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public ConfigReloadRequired ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public ConfigReloadRequired (string message)
      : base (message)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public ConfigReloadRequired (string message, Exception inner)
      : base (message, inner)
    {
    }
  }

  /// <summary>
  /// CNC Data Handler
  /// 
  /// Class that calls sequentially all the modules
  /// </summary>
  public sealed class CncDataHandler : ThreadClass, IChecked
  {
    static readonly TimeSpan DEFAULT_EVERY = TimeSpan.FromSeconds (2);
    static readonly TimeSpan DEFAULT_NOT_RESPONDING_TIMEOUT = TimeSpan.FromMinutes (2);
    static readonly TimeSpan DEFAULT_RESTART_SLEEP = TimeSpan.FromSeconds (10);
    static readonly string DATA_LOG = "Lemoine.Cnc.Data";

    static readonly string GARBAGE_COLLECTION_FREQUENCY_KEY = "Cnc.DataHandler.GarbageCollection.Frequency";
    static readonly TimeSpan GARBAGE_COLLECTION_FREQUENCY_DEFAULT = TimeSpan.FromMinutes (1);

    static readonly string GARBAGE_COLLECTION_MIN_TIME_KEY = "Cnc.DataHandler.GarbageCollection.MinTime"; // Minimum free time to run a garbage collection
    static readonly TimeSpan GARBAGE_COLLECTION_MIN_TIME_DEFAULT = TimeSpan.FromSeconds (1);

    static readonly string GARBAGE_COLLECTION_MIN_MEMORY_KEY = "Cnc.DataHandler.GarbageCollection.MinMemory"; // Minimum memory usage in bytes to run a garbage collection
    static readonly long GARBAGE_COLLECTION_MIN_MEMORY_DEFAULT = 1024 * 1024 * 80; // 80 MB

    static readonly string DATA_STDOUT_KEY = "Cnc.DataHandler.DataStdout";
    static readonly bool DATA_STDOUT_DEFAULT = false; // Alternative to the logs to output the data in the standard output

    static readonly string CNC_MODULES_DISTANT_DIRECTORY = "CncModules";

    #region Members
    readonly IAssemblyLoader m_assemblyLoader;
    readonly IFileRepoClientFactory m_fileRepoClientFactory;
    readonly int m_cncAcquisitionId = 0;
    readonly string m_cncAcquisitionName;

    // Run parameters
    TimeSpan m_every = DEFAULT_EVERY; // run ProcessTasks every 2 s by default
    TimeSpan m_notRespondingTimeout = DEFAULT_NOT_RESPONDING_TIMEOUT;
    TimeSpan m_restartSleep = DEFAULT_RESTART_SLEEP; // time to sleep just in case of restart
    ReaderWriterLock m_runningParamLock = new ReaderWriterLock ();
    DateTime m_lastExecution = DateTime.UtcNow;

    Repository m_configuration;
    IDictionary<string, object> m_data = new Dictionary<string, object> ();
    ConcurrentDictionary<string, object> m_finalData = new ConcurrentDictionary<string, object> ();
    // TODO: modules after configuration ? List <IModule> ?
    readonly IDictionary<XmlElement, CncModuleExecutor> m_moduleObjects = new ConcurrentDictionary<XmlElement, CncModuleExecutor> ();
    IList<XmlElement> m_moduleElements = new List<XmlElement> ();
    readonly IDictionary<string, CncModuleExecutor> m_moduleReferences = new ConcurrentDictionary<string, CncModuleExecutor> ();
    bool m_disposed = false;

    DateTime m_lastGarbageCollection = DateTime.UtcNow;
    #endregion

    readonly ILog log = LogManager.GetLogger (typeof (CncDataHandler).FullName);
    ILog dataLog = LogManager.GetLogger (DATA_LOG);

    #region Getters / Setters
    /// <summary>
    /// Cnc Acquisition ID
    /// </summary>
    public int CncAcquisitionId => m_cncAcquisitionId;

    /// <summary>
    /// Machine name
    /// </summary>
    public string CncAcquisitionName => m_cncAcquisitionName;

    /// <summary>
    /// Frequency when ProcessTasks is run
    /// 
    /// It can be defined in the XML configuration file
    /// 
    /// Default is 2 seconds
    /// </summary>
    public TimeSpan Every
    {
      get { return m_every; }
      set { m_every = value; }
    }

    /// <summary>
    /// Set of data to be filled by the different modules
    /// 
    /// Not thread safe
    /// </summary>
    public IDictionary<string, Object> Data => m_data;

    /// <summary>
    /// Final data once all the module requests are completed
    /// 
    /// Thread safe access (a concurrent dictionary is used)
    /// 
    /// This is possible that from time to time a deprecated value is returned,
    /// but this should be pretty rare (there is no global lock)
    /// </summary>
    public IDictionary<string, object> FinalData => m_finalData;
    #endregion

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// 
    /// Because it is using <see cref="Repository"/>, the constructor can be pretty long
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="assemblyLoader">not null</param>
    /// <param name="fileRepoClientFactory"></param>
    /// <param name="cancellationToken"></param>
    public CncDataHandler (Repository configuration, IAssemblyLoader assemblyLoader, IFileRepoClientFactory fileRepoClientFactory, CancellationToken cancellationToken)
    {
      Debug.Assert (null != assemblyLoader);

      m_assemblyLoader = assemblyLoader;
      m_fileRepoClientFactory = fileRepoClientFactory;

      this.m_configuration = configuration;
      try {
        this.m_configuration.ForceReadData (TimeSpan.FromSeconds (3), cancellationToken, checkedThread: this);
      }
      catch (Exception ex) {
        log.Error ("CncDataHandler: ForceReadData failed", ex);
        throw;
      }
      log.Debug ("CncDataHandler: ReadData successful");
      SetActive ();

      var xmlnsManager = new XmlNamespaceManager (this.m_configuration.Document.NameTable);
      xmlnsManager.AddNamespace ("cnc", "http://config.cnc.pulse.lemoinetechnologies.com");
      try {
        // Note:
        // the document element is cnc with namespace http://config.cnc.pulse.lemoinetechnologies.com
        // Then to get the document element in XPath you must:
        // - either use *
        // - or add an XmlNamespaceManager to it
        // Anyway /cnc/@cncacquisitionid will not work because the namespace is not specified
        var cncAcquisitionIdAttributeValue = this.m_configuration
          .GetData ("/cnc:cnc/@cncacquisitionid", xmlnsManager, false, cancellationToken);
        if (!string.IsNullOrEmpty (cncAcquisitionIdAttributeValue)) {
          m_cncAcquisitionId = Int32.Parse (cncAcquisitionIdAttributeValue);
        }
        else if (log.IsInfoEnabled) {
          log.Info ($"CncDataHandler: no attribute cncacquisitionid");
        }
      }
      catch (Exception ex) {
        log.Error ("CncDataHandler: exception when trying to get the cncAcquisitionId, it may not be an integer", ex);
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"CncDataHandler: cncAcquisitionId is {m_cncAcquisitionId}");
      }
      SetActive ();

      log = LogManager.GetLogger ($"{typeof (CncDataHandler).FullName}.{m_cncAcquisitionId}");
      dataLog = LogManager.GetLogger ($"{DATA_LOG}.{m_cncAcquisitionId}");
      SetActive ();

      try {
        m_cncAcquisitionName = this.m_configuration.GetData ("/cnc:cnc/@cncacquisitionname", xmlnsManager, false, cancellationToken) ?? "";
      }
      catch (Exception ex) {
        log.Error ("CncDataHandler: cnc acquisition name could not be read", ex);
        throw;
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"CncDataHandler: cncAcquisitionName is {m_cncAcquisitionName}");
      }
      SetActive ();

#if NETSTANDARD || NET48 || NETCOREAPP
      // Check if there is no deprecated field
      foreach (XmlNode node in this.m_configuration.Document.GetElementsByTagName ("deprecated")) {
        log.Debug ("CncDataHandler: process a deprecated element");
        SetActive ();
        XmlElement element = node as XmlElement;
        System.Diagnostics.Debug.Assert (null != element);
        if (element.HasAttribute ("file")) {
          string configFile = element.GetAttribute ("file");
          string configParameters = element.GetAttribute ("parameters");
          string configNamedParameters = element.GetAttribute ("configNamedParameters");
          log.Info ($"CncDataHandler: deprecated node with file {configFile} parameters {configParameters} namedParameters={configNamedParameters}");
          bool configReload = false;
          try {
            using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
              using (IDAOTransaction transaction = session.BeginTransaction ("Cnc.Deprecated")) {
                var cncAcquisition = ModelDAOHelper.DAOFactory.CncAcquisitionDAO
                  .FindById (m_cncAcquisitionId);
                cncAcquisition.ConfigFile = configFile;
                cncAcquisition.ConfigParameters = configParameters;
                cncAcquisition.ConfigKeyParams = Lemoine.Collections.EnumerableString.ParseDictionaryString (configNamedParameters);
                if (element.HasAttribute ("useprocess")) {
                  string useProcess = element.GetAttribute ("useprocess");
                  cncAcquisition.UseProcess = useProcess.Equals ("true", StringComparison.InvariantCultureIgnoreCase);
                }
                ModelDAOHelper.DAOFactory.CncAcquisitionDAO.MakePersistent (cncAcquisition);
                transaction.Commit ();
              }
            }
            configReload = true;
          }
          catch (Exception ex) {
            log.Error ($"CncDataHandler: error while updating the configuration of file {configFile} parameters {configParameters} namedParameters {configNamedParameters}", ex);
          }
          if (configReload) {
            if (log.IsDebugEnabled) {
              log.Debug ("CncDataHandler: configuration was updated, reload the configuration");
            }
            throw new ConfigReloadRequired ();
          }
        } // if
      } // loop on deprecated elements
      if (log.IsDebugEnabled) {
        log.Debug ("CncDataHandler: deprecated elements processed");
      }
      SetActive ();
#endif // NETSTANDARD || NET48 || NETCOREAPP

      // Set the Every property
      try {
        string attribute = this.m_configuration.GetData ("/cnc:cnc/@every",
                                                         xmlnsManager, false, cancellationToken);
        if (null == attribute) {
          log.Debug ("CncDataHandler: no every parameter is set");
        }
        else {
          int every = Int32.Parse (attribute);
          if (every <= 0) {
            log.Error ($"CncDataHandler: invalid every parameter {every}");
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"CncDataHandler: the every parameter is {every} ms");
            }
            m_every = TimeSpan.FromMilliseconds (every);
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"CncDataHandler: exception when trying to read the every parameter, it may not be an integer", ex);
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"CncDataHandler: every is {m_every}");
      }
      SetActive ();

      // Set the NotRespondingTimeout property
      try {
        string attribute = this.m_configuration
          .GetData ("/cnc:cnc/@notRespondingTimeout", xmlnsManager, false, cancellationToken);
        if (null == attribute) {
          if (log.IsDebugEnabled) {
            log.Debug ($"CncDataHandler: no notRespondingTimeout parameter is set");
          }
        }
        else {
          double notRespondingTimeout = double.Parse (attribute, CultureInfo.InvariantCulture);
          if (notRespondingTimeout <= 0) {
            log.Error ($"CncDataHandler: invalid notRespondingTimeout parameter {notRespondingTimeout}");
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"CncDataHandler: the notRespondingTimeout parameter is {notRespondingTimeout}");
            }
            m_notRespondingTimeout = TimeSpan.FromSeconds (notRespondingTimeout);
          }
        }
      }
      catch (Exception ex) {
        log.Error ("CncDataHandler: exception when trying to read the notRespondingTimeout parameter, it may not be a double", ex);
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"CncDataHandler: not responding timeout is {m_notRespondingTimeout}");
      }
      SetActive ();

      // Set the RestartSleep property
      try {
        string attribute = this.m_configuration
          .GetData ("/cnc:cnc/@restartSleep", xmlnsManager, false, cancellationToken);
        if (null == attribute) {
          if (log.IsDebugEnabled) {
            log.Debug ("CncDataHandler: no restartSleep parameter is set");
          }
        }
        else {
          int restartSleep = Int32.Parse (attribute);
          if (restartSleep <= 0) {
            log.Error ($"CncDataHandler: invalid restartSleep parameter {restartSleep}");
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"CncDataHandler: the restartSleep parameter is {restartSleep}");
            }
            m_restartSleep = TimeSpan.FromMilliseconds (restartSleep);
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"CncDataHandler: exception when trying to read the restartSleep parameter, it may not be an integer", ex);
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"CncDataHandler: restart sleep is {m_restartSleep}");
      }
      SetActive ();

      // Initialize the modules
      log.Debug ("CncDataHandler: about to initialize the modules");
      foreach (XmlNode node in this.m_configuration.Document.GetElementsByTagName ("*")) {
        XmlElement element = node as XmlElement;
        Debug.Assert (null != element);
        if (element.Name.Equals ("module")) {
          if (log.IsDebugEnabled) {
            log.Debug ("CncDataHandler: about to load one module");
          }
          SetActive ();
          try {
            LoadModule (element);
          }
          catch (Exception ex) {
            log.Error ("CncDataHandler: module could not be loaded", ex);
            throw;
          }
        }
        else if (element.Name.Equals ("moduleref")) {
          if (log.IsDebugEnabled) {
            log.Debug ($"CncDataHandler: about to load one module ref");
          }
          SetActive ();
          try {
            LoadModuleRef (element);
          }
          catch (Exception ex) {
            log.Error ("CncDataHandler: moduleref could not be loaded", ex);
            throw;
          }
        }
      }

      CheckCncModuleLicense ();
      log.Debug ("CncDataHandler: constructor completed");
    }

    void CheckCncModuleLicense ()
    {
#if !NET40
      // Check the license value is correct
      var license = m_moduleObjects.Values
        .Select (x => this.GetCncModuleLicense (x.CncModule))
        .Aggregate (CncModuleLicense.None, (x, y) => x | y);
      bool configReload = false;
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginTransaction ("Cnc.License")) {
            var cncAcquisition = ModelDAOHelper.DAOFactory.CncAcquisitionDAO
              .FindById (m_cncAcquisitionId);
            if (cncAcquisition is null) {
              log.Fatal ($"CheckCncModuleLicense: no cnc acquisition row with id={m_cncAcquisitionId}");
              throw new InvalidOperationException ("Unexpected behavior");
            }
            else if (cncAcquisition.License != license) {
              cncAcquisition.License = license;
              ModelDAOHelper.DAOFactory.CncAcquisitionDAO.MakePersistent (cncAcquisition);
              transaction.Commit ();
              configReload = true;
            }
            else {
              transaction.Rollback ();
            }
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"CncDataHandler: error while updating the license of cnc acquisition id {m_cncAcquisitionId} to {license}", ex);
      }
      if (configReload) {
        if (log.IsDebugEnabled) {
          log.Debug ("CncDataHandler: configuration was updated, reload the configuration");
        }
        throw new ConfigReloadRequired ();
      }
#endif // !NET40
    }

    Pomamo.CncModule.ICncModule LoadModule (string typeQualifiedName)
    {
      TypeLoader typeLoader;
      if (null != m_fileRepoClientFactory) {
        typeLoader = new TypeLoader (m_assemblyLoader, m_fileRepoClientFactory, CNC_MODULES_DISTANT_DIRECTORY);
      }
      else {
        typeLoader = new TypeLoader (m_assemblyLoader, Lemoine.FileRepository.FileRepoClient.Implementation, CNC_MODULES_DISTANT_DIRECTORY);
      }
      return typeLoader.Load<Pomamo.CncModule.ICncModule> (typeQualifiedName);
    }

    void LoadModule (XmlElement moduleElement)
    {
      Debug.Assert (null != moduleElement);

      if (!IsModuleElementActive (moduleElement)) {
        log.Debug ("LoadModule: element is not active, skip it");
        return;
      }

      string typeName = moduleElement.GetAttribute ("type");
      if (log.IsDebugEnabled) {
        log.Debug ($"LoadModule: about to load module of type {typeName}");
      }
      var module = LoadModule (typeName);

      // Check if a license is required
      if (!IsLicenseOk (module)) {
        log.Warn ($"LoadModule: the license is not ok for {module}, skip it");
        return;
      }
      SetActive ();

      try {
        module.CncAcquisitionId = this.CncAcquisitionId;
        module.CncAcquisitionName = this.CncAcquisitionName;
        // Configure it
        foreach (XmlAttribute attribute in moduleElement.Attributes) {
          // TODO attributes of element module and namespaces
          // instead of hard-coding type
          if (!attribute.Name.Equals ("type")
              && !attribute.Name.Equals ("if")
              && !attribute.Name.Equals ("ifnot")
              && !attribute.Name.Equals ("ifnotorunknown")
              && !attribute.Name.Equals ("ifandnotunknown")
              && !attribute.Name.Equals ("ifnotempty")
              && !attribute.Name.Equals ("ifempty")
              && !attribute.Name.Equals ("ref")) {
            SetProperty (module, attribute.Name, attribute.Value);
          }
        }
        var cncModuleExecutor = new CncModuleExecutor (module);
        m_moduleObjects[moduleElement] = cncModuleExecutor;
        if (module is Lemoine.Cnc.ICncModule lemoineModule) {
          lemoineModule.SetDataHandler (this);
        }
        m_moduleElements.Add (moduleElement);
        if (moduleElement.HasAttribute ("ref")) {
          var refLabel = moduleElement.GetAttribute ("ref");
          m_moduleReferences[refLabel] = cncModuleExecutor;
        }
      }
      catch (Exception ex) {
        log.Error ($"LoadModule: module of type {typeName} could not be loaded", ex);
        throw;
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"LoadModule: module of type {typeName} successfully loaded");
      }
    }

    void LoadModuleRef (XmlElement moduleElement)
    {
      Debug.Assert (null != moduleElement);

      if (!IsModuleElementActive (moduleElement)) {
        log.Debug ("LoadModuleRef: element is not active, skip it");
        return;
      }

      Debug.Assert (moduleElement.HasAttribute ("ref"));
      var refAttribute = moduleElement.GetAttribute ("ref");

      CncModuleExecutor cncModuleExecutor;
      if (!m_moduleReferences.TryGetValue (refAttribute, out cncModuleExecutor)) {
        log.Error ($"LoadModuleRef: module with reference was not declared {refAttribute}");
        return;
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"LoadModuleRef: module with ref {refAttribute} is {cncModuleExecutor}");
      }

      SetActive ();

      try {
        m_moduleObjects[moduleElement] = cncModuleExecutor;
        m_moduleElements.Add (moduleElement);
      }
      catch (Exception ex) {
        log.Error ($"LoadModuleRef: for module with ref {refAttribute}, exception", ex);
        throw;
      }

      if (log.IsDebugEnabled) {
        log.DebugFormat ($"LoadModuleRef: module with ref {refAttribute} successfully appended");
      }
    }

    bool IsModuleElementActive (XmlElement moduleElement)
    {
      if (moduleElement.HasAttribute ("if")) {
        string condition = moduleElement.GetAttribute ("if");
        if (condition.Equals ("False", StringComparison.InvariantCultureIgnoreCase)) {
          log.InfoFormat ("IsModuleElementActive: " +
                          "if condition {0} is False " +
                          "=> do not load {1}",
                          condition,
                          moduleElement.GetAttribute ("type"));
          return false;
        }
      }
      if (moduleElement.HasAttribute ("ifnot")) {
        string condition = moduleElement.GetAttribute ("ifnot");
        if (condition.Equals ("True", StringComparison.InvariantCultureIgnoreCase)) {
          log.InfoFormat ("IsModuleElementActive: " +
                          "ifnot condition {0} is True " +
                          "=> do not load {1}",
                          condition,
                          moduleElement.GetAttribute ("type"));
          return false;
        }
      }

      return true;
    }

#if !NET40
    CncModuleLicense GetCncModuleLicense (Pomamo.CncModule.ICncModule module)
    {
      return GetLicenseInfo (module) switch {
        CncModuleLicenseInfo.None => CncModuleLicense.None,
        CncModuleLicenseInfo.Gpl => CncModuleLicense.Gpl,
        CncModuleLicenseInfo.Propriatory => CncModuleLicense.Proprietary,
        _ => throw new NotImplementedException ()
      };
    }

    CncModuleLicenseInfo GetLicenseInfo (Pomamo.CncModule.ICncModule module)
    {
      try {
        var moduleType = module.GetType ();
        var propertyInfo = moduleType.GetProperty ("LicenseInfo");
        if (propertyInfo is null) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetLicenseInfo: no license info for module={module}");
          }
          // If the name starts with Lemoine., then consider it is in GPL
          if (moduleType.Namespace.StartsWith ("Lemoine.Cnc")) {
            return CncModuleLicenseInfo.Gpl;
          }
          else {
            return CncModuleLicenseInfo.None;
          }
        }
        else {
          var licenseInfo = (CncModuleLicenseInfo)propertyInfo.GetValue (module);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetLicenseInfo: {licenseInfo} for {module}");
          }
          return licenseInfo;
        }
      }
      catch (Exception ex) {
        log.Error ($"GetLicenseInfo: exception for module={module}", ex);
        return CncModuleLicenseInfo.None;
      }
    }
#endif

    bool IsLicenseOk (Pomamo.CncModule.ICncModule module)
    {
      try {
        PropertyInfo propertyInfo =
          module.GetType ().GetProperty ("License");
        if (null == propertyInfo) { // No license property
          log.DebugFormat ("IsLicenseOk: no license is required for {0}", module);
          return true;
        }
        else { // null != propertyInfo
          if (CncLicenseManager.GetLicense ()) {
            log.InfoFormat ("IsLicenseOk: " +
                            "a license has been successfully acquired " +
                            "for {0}",
                            module);
            return true;
          }
          else {
            log.Error ($"IsLicenseOk: no license available for {module}");
            return false;
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"IsLicenseOk: exception occurred => fallback to true", ex);
        return true;
      }
    }

    /// <summary>
    /// Set the property of a module.
    /// For the moment the property can be of type string or int
    /// </summary>
    /// <param name="module"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    void SetProperty (Pomamo.CncModule.ICncModule module, string name, string value)
    {
      PropertyInfo propertyInfo = module.GetType ().GetProperty (name);
      if (propertyInfo == null) {
        log.Warn ($"SetProperty: property {name} does not exist in module {module.GetType ()}");
      }
      else {
        if (propertyInfo.PropertyType.Equals (typeof (string))) {
          propertyInfo.SetValue (module,
                                 value,
                                 null);
        }
        else if (propertyInfo.PropertyType.Equals (typeof (int))) {
          int intValue = 0;
          if (false == int.TryParse (value, out intValue)) {
            log.Warn ($"SetProperty: could not convert property {name}={value} into an integer");
          }
          else {
            propertyInfo.SetValue (module,
                                   intValue,
                                   null);
          }
        }
        else if (propertyInfo.PropertyType.Equals (typeof (long))) {
          long longValue = 0;
          if (false == long.TryParse (value, out longValue)) {
            log.WarnFormat ("SetProperty: " +
                            "could not convert property {0}={1} " +
                            "into a long",
                            name, value);
          }
          else {
            propertyInfo.SetValue (module,
                                   longValue,
                                   null);
          }
        }
        else if (propertyInfo.PropertyType.Equals (typeof (double))) {
          double doubleValue = 0;
          CultureInfo usCultureInfo = new CultureInfo ("en-US");
          if (false == double.TryParse (value, NumberStyles.Any,
                                        usCultureInfo,
                                        out doubleValue)) {
            log.WarnFormat ("SetProperty: " +
                            "could not convert property {0}={1} " +
                            "into an double",
                            name, value);
          }
          else {
            propertyInfo.SetValue (module,
                                   doubleValue,
                                   null);
          }
        }
        else if (propertyInfo.PropertyType.Equals (typeof (bool))) {
          bool boolValue = false;
          if (value.Equals ("true", StringComparison.InvariantCultureIgnoreCase) || value.Equals ("1")) {
            boolValue = true;
          }
          propertyInfo.SetValue (module,
                                 boolValue,
                                 null);
        }
        else {
          log.Error ($"SetProperty: type {propertyInfo.PropertyType} is not supported yet");
        }
      }
    }
    #endregion

    /// <summary>
    /// Get a module that is associated to specific reference
    /// </summary>
    /// <param name="moduleRef"></param>
    /// <returns></returns>
    public CncModuleExecutor GetModule (string moduleRef)
    {
      return m_moduleReferences[moduleRef];
    }

    /// <summary>
    /// <see cref="ThreadClass"/>
    /// </summary>
    /// <returns></returns>
    public override ILog GetLogger ()
    {
      return log;
    }

    /// <summary>
    /// Given the Cnc Acquisition ID, get the stamp file to use
    /// to check a process
    /// </summary>
    /// <returns></returns>
    public string GetStampFileName ()
    {
      return $"CncStamp-{m_cncAcquisitionId}";
    }

    /// <summary>
    /// Thread work: run ProcessTasks continuously until
    /// the end is requested.
    /// 
    /// When the end of the thread is requested,
    /// Dispose is automatically run on the current object
    /// </summary>
    /// <param name="cancellationToken"></param>
    protected override void Run (CancellationToken cancellationToken)
    {
      Work (false, 0, cancellationToken);
    }

    /// <summary>
    /// Run ProcessTasks continuously until
    /// the end is requested.
    /// 
    /// When the end of the thread is requested,
    /// Dispose is automatically run on the current object
    /// 
    /// Optionally, a specific file: CncStamp-Xx can be used
    /// to monitor if the process is still working
    /// 
    /// If parentProcessId is not 0,
    /// the methods returns in case the corresponding process
    /// is not running anymore
    /// </summary>
    /// <param name="useStampFile"></param>
    /// <param name="parentProcessId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="calls">number of calls before stamping if not null and greater than 0</param>
    internal void Work (bool useStampFile, int parentProcessId, CancellationToken cancellationToken = default, int? calls = null)
    {
      if (m_disposed) {
        log.Error ("Work: the object is already disposed");
        throw new ObjectDisposedException (this.GetType ().FullName);
      }

      try {
        int call = 0;
        while (!cancellationToken.IsCancellationRequested
          && (!calls.HasValue || (0 == calls.Value) || (call++ < calls.Value))) {
          if (ExitRequested) {
            log.Info ("Work: exit is requested => return");
            return;
          }

          if (useStampFile) {
            // Update the timestamp of CncStamp-Xx
            var stampFileName = GetStampFileName ();
            var stampFileDirectory = Lemoine.Info.PulseInfo.CommonConfigurationDirectory;
            if (log.IsErrorEnabled && !Directory.Exists (stampFileDirectory)) {
              log.Error ($"Work: directory {stampFileDirectory} does not exist (it should)");
            }
            var stampFilePath = Path.Combine (stampFileDirectory, stampFileName);
            if (!File.Exists (stampFilePath)) {
              using (FileStream stream = File.Create (stampFilePath)) { }
            }
            File.SetLastWriteTimeUtc (stampFilePath, DateTime.UtcNow);
          }

          if (0 != parentProcessId) {
            try {
              Process process = Process.GetProcessById (parentProcessId);
            }
            catch (Exception ex) {
              log.Info ($"Work: parent process with PID {parentProcessId} does not exist any more => return", ex);
              return;
            }
          }

          DateTime dateTime = DateTime.UtcNow;
          try {
            this.ProcessTasks (cancellationToken);
          }
          catch (ObjectDisposedException) {
            log.Error ("Work: Trying to run ProcessTasks, while the object is disposed, return");
            return;
          }
          catch (Exception ex) {
            log.Fatal ("Work: ProcessTasks failed", ex);
            // Continue the job to prevent the service to crash because of a unique machine
            if (ExitRequested) {
              log.Fatal ("Work: exit is requested after exception => return", ex);
              return;
            }
          }
          TimeSpan duration = DateTime.UtcNow - dateTime;
          if (duration < m_every) {
            CheckGarbageCollection (duration);

            TimeSpan newDuration = DateTime.UtcNow - dateTime;
            if (newDuration < m_every) {
              SetActive ();
              log.Debug ($"Work: about to sleep {m_every - newDuration}");
              this.Sleep (m_every - newDuration, cancellationToken);
            }
          }
        }
        if (cancellationToken.IsCancellationRequested) {
          log.Error ($"Work: cancellation requested");
        }
      }
      finally {
        this.Dispose ();
      }
    }

    void CheckGarbageCollection (TimeSpan duration)
    {
      TimeSpan garbageCollectionMinTime = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (GARBAGE_COLLECTION_MIN_TIME_KEY,
        GARBAGE_COLLECTION_MIN_TIME_DEFAULT);
      if (duration < garbageCollectionMinTime) { // Not enough time
        return;
      }

      TimeSpan garbageCollectionFrequency = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (GARBAGE_COLLECTION_FREQUENCY_KEY,
        GARBAGE_COLLECTION_FREQUENCY_DEFAULT);
      if (DateTime.UtcNow < m_lastGarbageCollection.Add (garbageCollectionFrequency)) { // Too recent
        return;
      }

      long physicalMemory = Lemoine.Info.ProgramInfo.GetPhysicalMemory ();
      long memoryLimit = Lemoine.Info.ConfigSet.LoadAndGet<long> (GARBAGE_COLLECTION_MIN_MEMORY_KEY,
        GARBAGE_COLLECTION_MIN_MEMORY_DEFAULT);
      if (physicalMemory < memoryLimit) { // Memory consumption ok
        return;
      }

      log.Debug ("Work: run some garbage collection");
      DateTime garbageCollectionStart = DateTime.UtcNow;
      GC.Collect ();
      log.InfoFormat ("Work: garbage collection completed in {0}",
        DateTime.UtcNow.Subtract (garbageCollectionStart));
      m_lastGarbageCollection = DateTime.UtcNow;
    }

    /// <summary>
    /// Process the tasks of the different modules once.
    /// </summary>
    public void ProcessTasks (CancellationToken cancellationToken)
    {
      if (m_disposed) {
        log.Error ("ProcessTasks: the object is already disposed");
        throw new ObjectDisposedException (this.GetType ().FullName);
      }

      SetActive ();

      // Reset data
      m_data.Clear ();

      // Loop on modules
      try {
        foreach (XmlElement moduleElement in m_moduleElements) {
          SetActive ();

          if (cancellationToken.IsCancellationRequested) {
            log.Error ($"ProcessTasks: cancellation requested => return");
            return;
          }

          if (!CheckIfCondition (moduleElement, m_data)) {
            log.Info ($"ProcessTasks: if/ifnot condition is not checked => do not load {moduleElement.GetAttribute ("type")}");
            continue;
          }

          if (cancellationToken.IsCancellationRequested) {
            log.Error ($"ProcessTasks: cancellation requested => return");
            return;
          }

          var cncModuleExecutor = m_moduleObjects[moduleElement];
          Debug.Assert (null != cncModuleExecutor);

          ProcessModule (moduleElement, cncModuleExecutor, m_data, cancellationToken: cancellationToken);
        }
      }
      catch (Exception ex) {
        log.Fatal ("ProcessTasks: unexpected exception", ex);
        throw;
      }

      if (log.IsDebugEnabled) {
        log.Debug ("ProcessTasks: completed");
      }
      FillFinalData (m_data);
      // Output m_data in the logs
      LogData (m_data);
      if (Lemoine.Info.ConfigSet.LoadAndGet (DATA_STDOUT_KEY, DATA_STDOUT_DEFAULT)) {
        foreach (var item in m_data) {
          try {
            System.Console.WriteLine ($"{item.Key}={GetStringFromKeyValueItem (item)}");
          }
          catch (Exception ex) {
            log.Fatal ($"ProcessTasks: error while trying to output m_data in stdout for key {item.Key}", ex);
          }
        }
        System.Console.WriteLine ("================================");
      }
    }

    /// <summary>
    /// Process a specific module given its module XML Element and its <see cref="CncModuleExecutor"/>
    /// </summary>
    /// <param name="moduleElement"></param>
    /// <param name="cncModuleExecutor"></param>
    /// <param name="data">not null</param>
    /// <param name="cancellationToken"></param>
    internal void ProcessModule (XmlElement moduleElement, CncModuleExecutor cncModuleExecutor, IDictionary<string, object> data, CancellationToken cancellationToken)
    {
      Debug.Assert (null != data);

      using (var semaphoreHolder = SemaphoreSlimHolder.Create (cncModuleExecutor.Semaphore, cancellationToken)) {
        cancellationToken.ThrowIfCancellationRequested ();

        var module = cncModuleExecutor.CncModule;

        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessModule: process module {module}");
        }

        // - Start
        if (!ProcessStart (cncModuleExecutor, moduleElement, data)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessModule: ProcessStart returned false, give up for this module {module}");
          }
          return;
        }
        if (cncModuleExecutor.SkipInstructions) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessModule: SkipInstructions is {cncModuleExecutor.SkipInstructions} => return");
          }
          return;
        }

        // - reset, get and set directives
        foreach (XmlNode child in moduleElement.ChildNodes) {
          SetActive ();
          if (cancellationToken.IsCancellationRequested) {
            log.Error ($"ProcessModule: cancellation requested => return after calling Finish");
            ProcessFinish (cncModuleExecutor, moduleElement);
            return;
          }
          XmlElement instructionElement = child as XmlElement;
          if (null != instructionElement) {
            ProcessInstructionElement (cncModuleExecutor, instructionElement, data);
          }
        }

        // - Finish
        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessModule: process Finish of module {module}");
        }
        ProcessFinish (cncModuleExecutor, moduleElement);
      } // SemaphoreHolder
    }

    void FillFinalData (IDictionary<string, object> data)
    {
      var obsoleteFinalDataKeys = m_finalData.Keys
        .Where (k => !data.ContainsKey (k));
      foreach (var obsoleteFinalDataKey in obsoleteFinalDataKeys) {
        if (!m_finalData.TryRemove (obsoleteFinalDataKey, out object obsoleteValue)) {
          log.Error ($"FillFinalData: key {obsoleteFinalDataKey} could not be removed");
        }
      }

      foreach (KeyValuePair<string, object> item in data) {
        m_finalData[item.Key] = item.Value;
      }
    }

    /// <summary>
    /// If false is returned, skip the module
    /// </summary>
    /// <param name="cncModuleExecutor">not null</param>
    /// <param name="moduleElement">not null</param>
    /// <param name="data">not null</param>
    /// <returns></returns>
    bool ProcessStart (CncModuleExecutor cncModuleExecutor, XmlElement moduleElement, IDictionary<string, object> data)
    {
      Debug.Assert (null != cncModuleExecutor);
      Debug.Assert (null != moduleElement);

      try { // Run the Start method if it exists
        var result = cncModuleExecutor.Start (moduleElement, data);
        if (!result) {
          log.Warn ("ProcessStart: Start method of {cncModuleExecutor.CncModule} failed => skip the elements of this module");
          RecordStartError (moduleElement, data);
          return false;
        }
        else {
          return true;
        }
      }
      catch (Exception ex) {
        log.Error ($"ProcessStart: Start method of {cncModuleExecutor} failed => skip the elements of this module", ex);
        RecordStartError (moduleElement, data);
        if (CheckException (ex)) {
          throw;
        }
        return false;
      }
    }

    void RecordStartError (XmlElement moduleElement, IDictionary<string, object> data)
    {
      if (moduleElement.HasAttribute ("starterror")) {
        string starterror = moduleElement.GetAttribute ("starterror");
        log.DebugFormat ("RecordStartError: " +
                         "set true to starterror property {0} because of an exception",
                         starterror);
        data[starterror] = true;
      }
    }

    void ProcessFinish (CncModuleExecutor cncModuleExecutor, XmlElement moduleElement)
    {
      try { // Run the Finish method if it exists
        cncModuleExecutor.Finish ();
        log.Debug ($"ProcessFinish: Finish method of {cncModuleExecutor.CncModule} is completed");
      }
      catch (Exception ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessFinish: Finish method of {cncModuleExecutor} failed", ex);
        }
        if (CheckException (ex)) {
          throw;
        }
      }
    }

    void ProcessInstructionElement (CncModuleExecutor cncModuleExecutor, XmlElement instructionElement, IDictionary<string, object> data)
    {
      Debug.Assert (null != instructionElement);
      Debug.Assert (null != data);

      if (!CheckIfCondition (instructionElement, data)) {
        log.InfoFormat ("ProcessInstructionElement: " +
                        "if/ifnot condition is not checked " +
                        "=> do not run instruction element {0}",
                        instructionElement);
        return;
      }

      string elementName = instructionElement.Name;
      Debug.Assert (null != elementName);
      if (elementName.Equals ("reset", StringComparison.InvariantCultureIgnoreCase)) { // reset instruction
        ProcessReset (cncModuleExecutor.CncModule, instructionElement, data);
      }
      else if (elementName.Equals ("get", StringComparison.InvariantCultureIgnoreCase)) { // get instruction
        ProcessGet (cncModuleExecutor, instructionElement, data);
      }
      else if (elementName.Equals ("set", StringComparison.InvariantCultureIgnoreCase)) { // set instruction
        ProcessSet (cncModuleExecutor, instructionElement, data);
      }
      else {
        log.FatalFormat ("ProcessInstructionElement: invalid instruction element {0} with name {1} => check the configuration file",
          instructionElement, elementName);
      }
    }

    void ProcessReset (Pomamo.CncModule.ICncModule module, XmlElement instructionElement, IDictionary<string, object> data)
    {
      Debug.Assert (null != data);

      string dataItem = instructionElement.InnerText;
      if (log.IsDebugEnabled) {
        log.Debug ($"ProcessReset: element {dataItem} of {module} will be reset");
      }
      data.Remove (dataItem);
    }

    void ProcessGet (CncModuleExecutor cncModuleExecutor, XmlElement instructionElement, IDictionary<string, object> data)
    {
      string dataItem = instructionElement.InnerText;
      string overwrite = instructionElement.GetAttribute ("overwrite");

      // 0. Process a get instruction only if the data is still unknown
      if ((!overwrite.Equals ("true", StringComparison.InvariantCultureIgnoreCase))
          && (!overwrite.Equals ("1"))
          && data.ContainsKey (dataItem)) {
        log.DebugFormat ("ProcessGet: " +
                         "element {0} is already known " +
                         "in {1} " +
                         "=> skip the get directive",
                         dataItem,
                         cncModuleExecutor);
        return;
      }

      string method = instructionElement.GetAttribute ("method");
      if (0 < method.Length) { // If there is an attribute method, use it
        if (!ProcessGetMethod (cncModuleExecutor, instructionElement, method, data)) {
          log.FatalFormat ("ProcessGet: no method {0} in module {1} element {2}",
            method, cncModuleExecutor, instructionElement);
        }
        return;
      }

      string property = instructionElement.GetAttribute ("property");
      if (0 < property.Length) { // If there is an attribute property, use it
        if (!ProcessGetProperty (cncModuleExecutor, instructionElement, property, data)) {
          log.FatalFormat ("ProcessGet: no property {0} in module {1} element {2}",
            property, cncModuleExecutor, instructionElement);
        }
        return;
      }

      // Else try with the GetDataItem or the DataItem property
      if (ProcessGetMethod (cncModuleExecutor, instructionElement, "Get" + dataItem, data)) {
        return;
      }
      if (ProcessGetProperty (cncModuleExecutor, instructionElement, dataItem, data)) {
        return;
      }
      log.FatalFormat ("ProcessGet: no method or attribute found for {0} in module {1} element {2} => error in the configuration file",
        dataItem, cncModuleExecutor, instructionElement);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cncModuleExecutor"></param>
    /// <param name="instructionElement"></param>
    /// <param name="method"></param>
    /// <param name="data">not null</param>
    /// <returns>The method was found</returns>
    bool ProcessGetMethod (CncModuleExecutor cncModuleExecutor, XmlElement instructionElement, string method, IDictionary<string, object> data)
    {
      Debug.Assert (!string.IsNullOrEmpty (method));
      Debug.Assert (null != data);

      string dataItem = instructionElement.InnerText;
      string param = instructionElement.GetAttribute ("param");

      try {
        if (cncModuleExecutor.TryGet (out object result, method, param)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessGetMethod: use method {method} of {cncModuleExecutor.CncModule} to set {result} to {dataItem}");
          }
          data[dataItem] = result;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessGetMethod: no method {method} in module {cncModuleExecutor.CncModule}");
          }
          return false;
        }
      }
      catch (Exception ex) {
        if (log.IsWarnEnabled) {
          log.Warn ($"ProcessGetMethod: Method {method} of {cncModuleExecutor.CncModule} failed with param {param} => do not store any value for it", ex);
        }
        if (CheckException (ex)) {
          throw;
        }
      }

      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cncModuleExecutor"></param>
    /// <param name="instructionElement"></param>
    /// <param name="property"></param>
    /// <param name="data">not null</param>
    /// <returns>The property was found</returns>
    bool ProcessGetProperty (CncModuleExecutor cncModuleExecutor, XmlElement instructionElement, string property, IDictionary<string, object> data)
    {
      Debug.Assert (!string.IsNullOrEmpty (property));
      Debug.Assert (null != data);

      string dataItem = instructionElement.InnerText;

      try {
        if (cncModuleExecutor.TryGetProperty (out object result, property)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessGetProperty: use the property {property} of {cncModuleExecutor.CncModule} to set {result} to {dataItem}");
          }
          data[dataItem] = result;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessGetProperty: property {property} does not exist in module {cncModuleExecutor.CncModule}");
          }
          return false;
        }
      }
      catch (Exception ex) {
        if (log.IsWarnEnabled) {
          log.Warn ($"ProcessGetProperty: using the property {property} of {cncModuleExecutor} failed => do not store any value for it", ex);
        }
        if (CheckException (ex)) {
          throw;
        }
      }

      return true;
    }

    void ProcessSet (CncModuleExecutor cncModuleExecutor, XmlElement instructionElement, IDictionary<string, object> data)
    {
      Debug.Assert (null != data);

      string dataItem = instructionElement.InnerText;

      try {
        // 1. Get the object
        if (!data.ContainsKey (dataItem)) {
          log.Warn ($"ProcessSet: object {dataItem} is not in data => do nothing");
          return;
        }

        object datavalue = data[dataItem];
        if (null == datavalue) {
          log.Warn ($"ProcessSet: object {dataItem} is null => do nothing");
          return;
        }

        string method = instructionElement.GetAttribute ("method");
        if (0 < method.Length) { // If there is an attribute method, use it
          if (!ProcessSetMethod (cncModuleExecutor, instructionElement, method, datavalue)) {
            log.FatalFormat ("ProcessSet: method {0} does not exist in module {1} element {2}",
              method, cncModuleExecutor, instructionElement);
          }
          return;
        }

        string property = instructionElement.GetAttribute ("property");
        if (0 < property.Length) {
          if (!ProcessSetProperty (cncModuleExecutor, instructionElement, property, datavalue)) {
            log.FatalFormat ("ProcessSet: property {0} does not exist in module {1} element {2}",
              property, cncModuleExecutor, instructionElement);
          }
          return;
        }

        // Else try with the SetDataItem or the DataItem property
        if (ProcessSetMethod (cncModuleExecutor, instructionElement, "Set" + dataItem, datavalue)) {
          return;
        }
        if (ProcessSetProperty (cncModuleExecutor, instructionElement, dataItem, datavalue)) {
          return;
        }
        log.FatalFormat ("ProcessSet: no method or attribute found for {0} in module {1} element {2} => error in the configuration file",
          dataItem, cncModuleExecutor, instructionElement.OuterXml);
      }
      catch (Exception ex) {
        log.Error ($"ProcessSet: the set instruction {instructionElement.OuterXml} of module {cncModuleExecutor} failed", ex);
      }
    }

    bool ProcessSetMethod (CncModuleExecutor cncModuleExecutor, XmlElement instructionElement, string method, object datavalue)
    {
      Debug.Assert (!string.IsNullOrEmpty ("method"));

      try {
        if (cncModuleExecutor.TrySet (method, datavalue, () => instructionElement.GetAttribute ("param"))) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessSetMethod: success");
          }
          return true;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessSetMethod: method {method} does not exist");
          }
          return false;
        }
      }
      catch (Exception ex) {
        log.Error ($"ProcessSetMethod: using the method {method} of {cncModuleExecutor} failed", ex);
        if (CheckException (ex)) {
          throw;
        }
      }

      return true;
    }

    bool ProcessSetProperty (CncModuleExecutor cncModuleExecutor, XmlElement instructionElement, string property, object datavalue)
    {
      try {
        if (cncModuleExecutor.TrySetProperty (property, datavalue)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessSetProperty: success");
          }
          return true;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessSetProperty: no property {property} in module {cncModuleExecutor.CncModule}");
          }
          return false;
        }
      }
      catch (Exception ex) {
        log.Error ($"ProcessSetProperty: using the property {property} of {cncModuleExecutor.CncModule} failed", ex);
        if (CheckException (ex)) {
          throw;
        }
      }

      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ex"></param>
    /// <returns>throw again the exception</returns>
    bool CheckException (Exception ex)
    {
      if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExitExceptFromDatabase (ex)) {
        log.Fatal ("CheckException: exception requires to exit", ex);
        this.SetExitRequested ();
        return true;
      }
      else if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) {
#if NETSTANDARD || NET48 || NETCOREAPP
        log.Error ("CheckException: serious problem in database, kill all orphaned connections just in case but don't exit", ex);
        try {
          ModelDAOHelper.DAOFactory.KillOrphanedConnections ();
        }
        catch (Exception ex1) {
          log.Error ("CheckException: KillOrphanedConnections failed, but continue", ex1);
        }
#endif // NETSTANDARD || NET48 || NETCOREAPP
        return false;
      }
      else {
        return false;
      }
    }

    object ConvertData (object datavalue, Type type)
    {
      var converter = new Lemoine.Conversion.DefaultAutoConverter ();
      return converter.ConvertAuto (datavalue, type);
    }

    /// <summary>
    /// If the element has a if or ifnot attribute condition that is not checked, return false
    /// </summary>
    /// <param name="element"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    bool CheckIfCondition (XmlElement element, IDictionary<string, object> data)
    {
      string attribute;
      if (!CheckIfCondition (element, data, out attribute)) {
        if (log.IsInfoEnabled) {
          string condition = element.GetAttribute (attribute);
          log.InfoFormat ("CheckIfCondition: " +
                          "{0} condition {1} is not checked " +
                          "do not load {2}",
                          attribute, condition, element);
        }
        return false;
      }

      return true;
    }

    bool CheckIfCondition (XmlElement element, IDictionary<string, object> data, out string attribute)
    {
      if (element.HasAttribute ("ifdefined")) {
        string condition = element.GetAttribute ("ifdefined");
        if (!IsDefined (condition, data)) {
          attribute = "ifdefined";
          return false;
        }
      }
      if (element.HasAttribute ("ifnotdefined")) {
        string condition = element.GetAttribute ("ifnotdefined");
        if (IsDefined (condition, data)) {
          attribute = "ifnotdefined";
          return false;
        }
      }
      if (element.HasAttribute ("if")) { // == iforunknown
        string condition = element.GetAttribute ("if");
        if (string.IsNullOrEmpty (condition)) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("CheckIfCondition: if condition {0} is empty, return false", condition);
          }
          attribute = "if";
          return false;
        }
        var b = CheckBoolCondition (condition, data);
        if (b.HasValue) {
          if (b.Value) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("CheckIfCondition: if condition {0} is {1}, continue", condition, b);
            }
          }
          else { // !b.Value
            attribute = "if";
            return false;
          }
        }
        else { // !b.HasValue
          if (log.IsDebugEnabled) {
            log.DebugFormat ("CheckIfCondition: if condition {0} could not be evaluated as a boolean, return false", condition);
          }
          attribute = "if";
          return false;
        }
      }
      if (element.HasAttribute ("iforunknown")) { // == iforunknown
        string condition = element.GetAttribute ("iforunknown");
        if (string.IsNullOrEmpty (condition)) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("CheckIfCondition: iforunknown condition {0} is empty, return false", condition);
          }
          attribute = "if";
          return false;
        }
        var b = CheckBoolCondition (condition, data);
        if (b.HasValue) {
          if (b.Value) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("CheckIfCondition: iforunknown condition {0} is {1}, continue", condition, b);
            }
          }
          else { // !b.Value
            attribute = "iforunknown";
            return false;
          }
        }
        else { // !b.HasValue
          if (log.IsDebugEnabled) {
            log.DebugFormat ("CheckIfCondition: iforunknown condition {0} could not be evaluated as a boolean, continue", condition);
          }
        }
      }
      if (element.HasAttribute ("ifnot")) { // == ifnotandnotunknown
        string condition = element.GetAttribute ("ifnot");
        if (string.IsNullOrEmpty (condition)) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("CheckIfCondition: ifnot condition {0} is empty, return false", condition);
          }
          attribute = "ifnot";
          return false;
        }
        var b = CheckBoolCondition (condition, data);
        if (b.HasValue) {
          if (b.Value) {
            attribute = "ifnot";
            return false;
          }
          else { // !b.Value
            if (log.IsDebugEnabled) {
              log.DebugFormat ("CheckIfCondition: ifnot condition {0} is {1}, continue", condition, b);
            }
          }
        }
        else { // !b.HasValue
          attribute = "ifnot";
          if (log.IsDebugEnabled) {
            log.DebugFormat ("CheckIfCondition: ifnot condition {0} could not be evaluated as a boolean", condition);
          }
          return false;
        }
      }
      if (element.HasAttribute ("ifnotorunknown")) {
        string condition = element.GetAttribute ("ifnotorunknown");
        var b = CheckBoolCondition (condition, data);
        if (b.HasValue) {
          if (b.Value) {
            attribute = "ifnotorunknown";
            return false;
          }
          else { // !b.Value
            if (log.IsDebugEnabled) {
              log.DebugFormat ("CheckIfCondition: ifnotorunknown condition {0} is {1}, continue", condition, b);
            }
          }
        }
        else { // !b.HasValue
          if (log.IsDebugEnabled) {
            log.DebugFormat ("CheckIfCondition: ifnotorunknown condition {0} is unknown or not valid, continue", condition);
          }
        }
      }
      if (element.HasAttribute ("ifandnotunknown")) {
        string condition = element.GetAttribute ("ifandnotunknown");
        var b = CheckBoolCondition (condition, data);
        if (b.HasValue) {
          if (b.Value) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("CheckIfCondition: ifandnotunknown condition {0} is {1}, continue", condition, b);
            }
          }
          else { // !b.Value
            attribute = "ifandnotunknown";
            return false;
          }
        }
        else { // !b.HasValue
          attribute = "ifandnotunknown";
          return false;
        }
      }
      if (element.HasAttribute ("ifnotempty")) {
        string condition = element.GetAttribute ("ifnotempty");
        if (string.IsNullOrEmpty (condition)) {
          attribute = "ifnotempty";
          return false;
        }
      }
      if (element.HasAttribute ("ifempty")) {
        string condition = element.GetAttribute ("ifempty");
        if (!string.IsNullOrEmpty (condition)) {
          attribute = "ifempty";
          return false;
        }
      }

      attribute = "";
      return true;
    }

    bool IsDefined (string condition, IDictionary<string, object> data)
    {
      return data.ContainsKey (condition);
    }

    bool? CheckBoolCondition (string condition, IDictionary<string, object> data)
    {
      if (string.IsNullOrEmpty (condition)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"CheckBoolCondition: condition {condition} is empty, return false");
        }
        return false;
      }
      if (condition.Equals ("True", StringComparison.InvariantCultureIgnoreCase)
        || condition.Equals ("1", StringComparison.InvariantCultureIgnoreCase)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"CheckBoolCondition: {condition} is True");
        }
        return true;
      }
      if (condition.Equals ("False", StringComparison.InvariantCultureIgnoreCase)
        || condition.Equals ("0", StringComparison.InvariantCultureIgnoreCase)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"CheckBoolCondition: {condition} is False");
        }
        return false;
      }
      if (IsDefined (condition, data)) {
        var v = data[condition];
        try {
          var b = (bool)v;
          if (log.IsDebugEnabled) {
            log.Debug ($"CheckBoolCondition: {condition} value is {b}");
          }
          return b;
        }
        catch (Exception ex) {
          if (log.IsDebugEnabled) {
            log.Debug ($"CheckBoolCondition: {condition} is not a boolean", ex);
          }
        }
        try {
          var b = (bool)ConvertData (v, typeof (bool));
          if (log.IsDebugEnabled) {
            log.DebugFormat ("CheckBoolCondition: {0} value is {1} after conversion", condition, b);
          }
          return b;
        }
        catch (Exception ex) {
          log.Error ($"CheckBoolCondition: {condition} value is {v}, it is not a boolean (conversion exception)", ex);
          return null;
        }
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"CheckBoolCondition: {condition} is unknown");
        }
        return null;
      }
    }

    void LogData (IDictionary<string, object> data)
    {
      if (dataLog.IsInfoEnabled) {
        foreach (var item in data) {
          try {
            dataLog.Info ($"{item.Key}={GetStringFromKeyValueItem (item)}");
          }
          catch (Exception ex) {
            log.Fatal ($"LogData: error while trying to output m_data in the logs for key {item.Key}", ex);
          }
        }
      }
    }

    /// <summary>
    /// Get a string for the key=value item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public string GetStringFromKeyValueItem (KeyValuePair<string, object> item)
    {
      bool withType = !item.Key.Equals ("CncVariableSet", StringComparison.InvariantCultureIgnoreCase);
      if (item.Value is IDictionary<string, object>) {
        IDictionary<string, object> dictionary = (IDictionary<string, object>)ConvertData (item.Value, typeof (IDictionary<string, object>));
        return dictionary.ToDictionaryString (withType);
      }
      else if (item.Value is IDictionary<string, int>) {
        IDictionary<string, int> dictionary = (IDictionary<string, int>)ConvertData (item.Value, typeof (IDictionary<string, int>));
        return dictionary.ToDictionaryString (withType);
      }
      else if (item.Value is IDictionary<string, double>) {
        IDictionary<string, double> dictionary = (IDictionary<string, double>)ConvertData (item.Value, typeof (IDictionary<string, double>));
        return dictionary.ToDictionaryString (withType);
      }
      else if (item.Value is IDictionary<string, string>) {
        IDictionary<string, string> dictionary = (IDictionary<string, string>)ConvertData (item.Value, typeof (IDictionary<string, string>));
        return dictionary.ToDictionaryString (withType);
      }
#if NETSTANDARD || NET48 || NETCOREAPP
      else if (item.Value is IList<CncAlarm>) {
        IList<CncAlarm> list = (IList<CncAlarm>)ConvertData (item.Value, typeof (IList<CncAlarm>));
        return list.ToListString ();
      }
#endif // NETSTANDARD || NET48 || NETCOREAPP

      // Default
      return GetStringFromItemValue (item.Value);
    }

    /// <summary>
    /// Get a string for a log for the item value
    /// </summary>
    /// <param name="itemValue"></param>
    /// <returns></returns>
    public string GetStringFromItemValue (object itemValue)
    {
      if (null == itemValue) {
        return null;
      }

      var s = itemValue.ToString ();
      if (!s.StartsWith ("System.")) {
        return s;
      }

      try {
        if (itemValue is System.Collections.IEnumerable) { // Includes ICollection and IList
          var v = itemValue as System.Collections.IEnumerable;
          return EnumerableString.ConvertNotGenericToListString (v);
        }
      }
      catch (Exception ex) {
        log.Error ($"GetStringForLog: error in formatting {itemValue}", ex);
      }

      return s;
    }

    /// <summary>
    /// Dispose(bool disposing) executes in two distinct scenarios.
    /// If disposing equals true, the method has been called directly
    /// or indirectly by a user's code. Managed and unmanaged resources
    /// can be disposed.
    /// If disposing equals false, the method has been called by the
    /// runtime from inside the finalizer and you should not reference
    /// other objects. Only unmanaged resources can be disposed.
    /// 
    /// Note all the variables are not really needed here.
    /// But they are nevertheless here because this class could be used an example
    /// for other classes that could need them.
    /// </summary>
    /// <param name="disposing">Dispose also the managed resources</param>
    protected override void Dispose (bool disposing)
    {
      if (m_disposed) {
        return;
      }

      if (disposing) {
        // Dispose managed resources
        foreach (CncModuleExecutor v in m_moduleObjects.Values) {
          v.Dispose ();
        }
      }

      // Dispose unmanaged resources: here nothing to do

      m_disposed = true;

      base.Dispose (disposing);
    }
  }
}
