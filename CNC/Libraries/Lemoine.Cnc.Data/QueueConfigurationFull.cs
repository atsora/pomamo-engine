// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Lemoine.Info.ConfigReader;
using Lemoine.Core.Log;
using Lemoine.FileRepository;
using System.Diagnostics;
using Lemoine.Core.Plugin;

namespace Lemoine.Cnc.Data
{
  /// <summary>
  /// Full configuration for queues, based on an xml file (common configuration)
  /// and initial configurations (specific for a process for example)
  /// </summary>
  public class QueueConfigurationFull
    : IStringConfigReader
    , IListStringConfigReader
    , IListConfigReader
  {
    #region Members
    string m_rootType = null;
    readonly IDictionary<string, string> m_configurations = new Dictionary<string, string> (StringComparer.InvariantCultureIgnoreCase);
    readonly IList<QueueConfigurationFull> m_subQueues = new List<QueueConfigurationFull> ();
    #endregion // Members

    static readonly string QUEUE_CONFIG_DIRECTORY_KEY = "Cnc.QueueConfigDirectory";
    static readonly string QUEUE_CONFIG_DIRECTORY_DEFAULT = Lemoine.Info.PulseInfo.CommonConfigurationDirectory;

    static readonly string QUEUE_TYPE_KEY = "QueueType";
    static readonly string QUEUE_TYPE_DEFAULT = "Lemoine.Cnc.SQLiteQueue.MultiSQLiteCncDataQueue, Lemoine.Cnc.SQLiteQueue";
    static readonly ILog log = LogManager.GetLogger (typeof (QueueConfigurationFull).FullName);
    static readonly string CONF_FILENAME = "CncQueues.xml";
    static readonly string CONF_OVERWRITE_FILENAME = "CncQueues.overwrite.xml";

    #region Getters / Setters
    /// <summary>
    /// Subqueues
    /// </summary>
    public IEnumerable<QueueConfigurationFull> SubQueues
    {
      get { return m_subQueues; }
    }

    /// <summary>
    /// Root type (type="")
    /// 
    /// null if not defined
    /// </summary>
    public string RootType
    {
      get { return m_rootType; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// 
    /// This constructor must be called for testing purposes only.
    /// Usually LoadXml will be called after it
    /// </summary>
    internal protected QueueConfigurationFull ()
    {
      m_configurations[QUEUE_TYPE_KEY] = QUEUE_TYPE_DEFAULT;
    }

    /// <summary>
    /// Creation of a full configuration
    /// </summary>
    /// <param name="configurationPath"></param>
    /// <exception cref="Exception">The configuration could not be loaded</exception>
    public QueueConfigurationFull (string configurationPath)
    {
      m_configurations[QUEUE_TYPE_KEY] = QUEUE_TYPE_DEFAULT;

      if (string.IsNullOrEmpty (configurationPath)) {
        LoadDefaultConfigurationFile ();
      }
      else {
        LoadConfiguration (configurationPath);
      }
    }

    /// <summary>
    /// Creation of a full configuration
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="localSubDirectory"></param>
    /// <param name="createNewFile"></param>
    /// <param name="forceSynchronization"></param>
    /// <exception cref="Exception">The configuration could not be loaded</exception>
    public QueueConfigurationFull (int machineId, int machineModuleId, string localSubDirectory, bool createNewFile, bool forceSynchronization)
    {
      m_configurations[QUEUE_TYPE_KEY] = QUEUE_TYPE_DEFAULT;

      LoadMachineConfiguration (machineId, machineModuleId, localSubDirectory, createNewFile, forceSynchronization);
    }

    /// <summary>
    /// Creation of a full configuration
    /// </summary>
    /// <param name="remoteQueueConfigurationPath"></param>
    /// <param name="localSubDirectory"></param>
    /// <param name="createNewFile"></param>
    /// <param name="forceSynchronization"></param>
    /// <exception cref="Exception">The configuration could not be loaded</exception>
    public QueueConfigurationFull (string remoteQueueConfigurationPath, string localSubDirectory, bool createNewFile, bool forceSynchronization)
    {
      m_configurations[QUEUE_TYPE_KEY] = QUEUE_TYPE_DEFAULT;

      if (string.IsNullOrEmpty (remoteQueueConfigurationPath)) {
        LoadDefaultConfigurationFile ();
      }
      else {
        LoadConfigurationRemoteFile (remoteQueueConfigurationPath, localSubDirectory, createNewFile, forceSynchronization);
      }
    }

    /// <summary>
    /// Private constructor used to create sub queues
    /// </summary>
    /// <param name="node"></param>
    /// <param name="nsmgr"></param>
    QueueConfigurationFull (XmlNode node, XmlNamespaceManager nsmgr)
    {
      // Process the sub queue configuration
      ProcessConfiguration (this, node, nsmgr);
    }
    #endregion // Constructors

    /// <summary>
    /// This method must be called for testing purposes only
    /// </summary>
    /// <param name="xmlText"></param>
    internal protected void LoadXml (string xmlText)
    {
      // Load configuration from the xml text
      var document = new XmlDocument ();
      document.LoadXml (xmlText);
      ProcessDocument (document);
    }

    void LoadMachineConfiguration (int machineId, int machineModuleId, string localSubDirectory, bool createNewFile, bool forceSynchronization)
    {
      var remoteQueueConfigurationPath = GetRemoteQueueConfigurationPath (machineId, machineModuleId);
      if (string.IsNullOrEmpty (remoteQueueConfigurationPath)) {
        LoadDefaultConfigurationFile ();
      }
      else { // queueConfiguration is set
        LoadConfigurationRemoteFile (remoteQueueConfigurationPath, localSubDirectory, createNewFile, forceSynchronization);
      }
    }

    string GetRemoteQueueConfigurationPath (int machineId, int machineModuleId)
    {
      var extensions = Lemoine.Extensions.ExtensionManager.GetExtensions<Lemoine.Extensions.Cnc.IQueueConfigurationFullExtension> ();
      foreach (var extension in extensions) {
        string distantDirectory = extension.GetDistantDirectory (machineId, machineModuleId);
        string distantFileName = extension.GetDistantFileName (machineId, machineModuleId);
        if ((null != distantDirectory) && !string.IsNullOrEmpty (distantFileName)) {
          log.InfoFormat ("GetRemoteQueueConfigurationPath: loading distant file {0}/{1}", distantDirectory, distantFileName);
          return System.IO.Path.Combine (distantDirectory, distantFileName);
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ("GetRemoteQueueConfigurationPath: no specific queue");
      }
      return "";
    }

    void LoadConfigurationRemoteFile (string remoteQueueConfigurationPath, string localSubDirectory, bool createNewFile, bool forceSynchronization)
    {
      Debug.Assert (!string.IsNullOrEmpty (remoteQueueConfigurationPath));

      var baseDirectory = Lemoine.Info.PulseInfo.LocalConfigurationDirectory;
      if (!string.IsNullOrEmpty (localSubDirectory)) {
        baseDirectory = Path.Combine (baseDirectory, localSubDirectory);
        if (!Directory.Exists (baseDirectory)) {
          Directory.CreateDirectory (baseDirectory);
        }
      }
      var localPath = Path.Combine (baseDirectory, remoteQueueConfigurationPath);
      if (forceSynchronization || (createNewFile && !File.Exists (localPath))) {
        log.InfoFormat ("LoadConfigurationRemoteFile: synchronize {0}", localPath);
        var distantFileName = System.IO.Path.GetFileName (remoteQueueConfigurationPath);
        var distantDirectory = System.IO.Path.GetDirectoryName (remoteQueueConfigurationPath);
        var localDirectory = System.IO.Path.Combine (baseDirectory, distantDirectory);
        if (!Directory.Exists (localDirectory)) {
          Directory.CreateDirectory (localDirectory);
        }
        try {
          Lemoine.FileRepository.FileRepoClient.GetFile (distantDirectory, distantFileName, localPath);
        }
        catch (Exception ex) {
          log.Error ($"LoadConfigurationRemoteFile: FileRepoClient.GetFile of {distantDirectory}/{distantFileName} to {localPath} failed", ex);
        }
      }

      if (File.Exists (localPath)) {
        try {
          LoadConfiguration (localPath);
          log.InfoFormat ("LoadConfigurationRemoteFile: {0} was correctly loaded",
            localPath);
          return;
        }
        catch (Exception ex) {
          log.Error ($"LoadConfigurationRemoteFile: File {localPath} found but couldn't be loaded", ex);
          throw;
        }
      }
      else { // !File.Exists (localPath)
        log.ErrorFormat ("LoadConfigurationRemoteFile: local file {0} from extension not found", localPath);
        throw new Exception ("LoadConfigurationRemoteFile: no local configuration file found for the extension");
      }
    }

    void LoadDefaultConfigurationFile ()
    {
      string queueConfigDirectory = Lemoine.Info.ConfigSet
        .LoadAndGet (QUEUE_CONFIG_DIRECTORY_KEY, QUEUE_CONFIG_DIRECTORY_DEFAULT);
      if (string.IsNullOrEmpty (queueConfigDirectory)) {
        // An exception is not thrown since it may be possible that the configuration is completed
        // later, without the help of a configuration file.
        // 
        // Error log though because an installation dir is expected
        log.Error ("LoadConfigurationFile: installation dir is not set => fallback: use the default queue");
        return;
      }

      { // Load configuration from the overwrite xml file
        string path = Path.Combine (queueConfigDirectory, CONF_OVERWRITE_FILENAME);
        if (File.Exists (path)) {
          try {
            LoadConfiguration (path);
            return;
          }
          catch (Exception ex) {
            // Something happened
            if (log.IsInfoEnabled) {
              log.Info ($"LoadConfigurationFile: File {path} found but couldn't be loaded", ex);
            }
          }
        }
      }

      { // Load configuration from the default xml file
        string path = Path.Combine (queueConfigDirectory, CONF_FILENAME);
        if (File.Exists (path)) {
          try {
            LoadConfiguration (path);
            return;
          }
          catch (Exception ex) {
            // Something happened
            log.Error ($"LoadConfigurationFile: File {path} found but couldn't be loaded", ex);
            throw;
          }
        }
        else {
          log.Warn ($"LoadConfigurationFile: File {path} does not exist");
        }
      }
    }

    void LoadConfiguration (string path)
    {
      var document = new XmlDocument ();

      using (var reader = new XmlTextReader (path)) {
        reader.Read ();
        document.Load (reader);
      }
      ProcessDocument (document);
    }

    #region IStringConfigReader implementation
    /// <summary>
    /// <see cref="IStringConfigReader" />
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetString (string key)
    {
      return m_configurations[key];
    }
    #endregion

    #region IListConfigReader implementation
    /// <summary>
    /// <see cref="IListStringConfigReader"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IEnumerable<IStringConfigReader> GetStringConfigs (string key)
    {
      if (!string.Equals (key, "queue")) {
        throw new KeyNotFoundException ();
      }

      return SubQueues.Cast<IStringConfigReader> ();
    }
    #endregion


    #region IListConfigReader implementation

    /// <summary>
    /// <see cref="IListConfigReader"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IEnumerable<IGenericConfigReader> GetConfigs (string key)
    {
      return GetStringConfigs (key)
        .Select (configReader => new AutoConvertConfigReader (configReader))
        .Cast<IGenericConfigReader> ();
    }
    #endregion

    #region Methods
    void ProcessDocument (XmlDocument document)
    {
      // Root node and namespace
      XmlNode root = document.DocumentElement;
      var nsmgr = new XmlNamespaceManager (document.NameTable);
      nsmgr.AddNamespace ("queueconfiguration", "http://config.cnc.pulse.lemoinetechnologies.com");

      // Process the main queue configuration
      XmlNode node = root.SelectSingleNode ("queueconfiguration:queue", nsmgr);
      ProcessConfiguration (this, node, nsmgr);
    }

    void ProcessConfiguration (QueueConfigurationFull configuration,
                              XmlNode node, XmlNamespaceManager nsmgr)
    {
      // Assembly and type of the queue
      if (null != node.Attributes["type"]) {
        m_rootType = node.Attributes["type"].Value;
        configuration.m_configurations[QUEUE_TYPE_KEY] = m_rootType;
      }

      // Configuration of the queue
      var nodeConf = node.SelectSingleNode ("queueconfiguration:configuration", nsmgr);
      foreach (XmlAttribute attribute in nodeConf.Attributes) {
        configuration.m_configurations[attribute.Name] = attribute.Value;
      }

      // Process sub queues
      var nodes = node.SelectNodes ("queueconfiguration:queues/queueconfiguration:queue", nsmgr);
      if (nodes != null) {
        foreach (XmlNode nodeTmp in nodes) {
          var conf = new QueueConfigurationFull (nodeTmp, nsmgr);
          m_subQueues.Add (conf);
        }
      }
    }
    #endregion // Methods

    /// <summary>
    /// Get a queue based on a full configuration
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="defaultConfigReader"></param>
    /// <returns></returns>
    public ICncDataQueue CreateQueue (int machineId, int machineModuleId,
                                     IGenericConfigReader defaultConfigReader)
    {
      string queueType = m_rootType;
      if (null == queueType) {
        try {
          queueType = GetString (QUEUE_TYPE_KEY);
        }
        catch (Exception ex) {
          log.Warn ($"CreateQueue: type not found in the configuration", ex);
        }
      }
      if (null == queueType) {
        try {
          queueType = defaultConfigReader.Get<string> (QUEUE_TYPE_KEY);
        }
        catch (Exception ex) {
          if (log.IsInfoEnabled) {
            log.Info ($"CreateQueue: no configuration set for {QUEUE_TYPE_KEY}, use default {QUEUE_TYPE_DEFAULT} instead", ex);
          }
          queueType = QUEUE_TYPE_DEFAULT;
        }
      }

      // Create an instance of the queue
      ICncDataQueue queue = null;
      try {
        var typeLoader = new Lemoine.Core.Plugin.TypeLoader ();
        queue = typeLoader.Load<ICncDataQueue> (queueType);

        if (queue == null) {
          string txt = $"Unknown queue used: {queueType}";
          log.ErrorFormat ($"CreateQueue: {txt}");
          throw new Exception (txt);
        }
      }
      catch (Exception ex) {
        log.Error ($"CreateQueue: Couldn't create type {queueType}", ex);
        throw;
      }

      queue.MachineId = machineId;
      queue.MachineModuleId = machineModuleId;

      if (queue is IConfigurable) {
        IConfigurable configurableQueue = queue as IConfigurable;
        var configReader = new MultiConfigReader ();
        configReader.Add (new AutoConvertConfigReader (this));
        configReader.Add (defaultConfigReader);
        configurableQueue.SetConfigReader (configReader);
      }

      if (queue is IListConfigurable) {
        IListConfigurable configurableQueue = queue as IListConfigurable;
        configurableQueue.SetListConfigReader (this);
      }

      return queue;
    }

  }
}
