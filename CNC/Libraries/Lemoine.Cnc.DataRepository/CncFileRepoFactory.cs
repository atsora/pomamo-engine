// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;
using Lemoine.Collections;
using Lemoine.DataRepository;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Cnc;
using Lemoine.Extensions.Interfaces;
using Lemoine.Core.Log;
using System.Linq;
using NHibernate.Util;

namespace Lemoine.Cnc.DataRepository
{
  /// <summary>
  /// CNC File Repository Factory: builds a DOMDocument from an XML file stored in the File Repository
  /// The File Repository namespace used here is 'cncconfigs'.
  /// Some keywords are replaced by an attribute taken in the Machine table.
  /// </summary>
  public class CncFileRepoFactory : FileRepoFactory
  {
    static readonly string GET_CONFIG_METHOD_KEY = "Cnc.FileRepo.GetConfigMethod";
    static readonly string GET_CONFIG_METHOD_DEFAULT = ""; // Default is use Lemoine.Info.ConfigSet
    static readonly string GET_CONFIG_METHOD_DATABASEONLY = "DatabaseOnly"; // DatabaseOnly: check in the database only

    /// <summary>
    /// Database exception
    /// </summary>
    public class DatabaseException : RepositoryException
    {
      /// <summary>
      /// <see cref="RepositoryException"/>
      /// </summary>
      /// <param name="message"></param>
      /// <param name="innerException"></param>
      public DatabaseException (string message, Exception innerException)
        : base (message, innerException)
      {
      }
    }

    /// <summary>
    /// Extension exception
    /// </summary>
    public class ExtensionException : RepositoryException
    {
      /// <summary>
      /// <see cref="RepositoryException"/>
      /// </summary>
      /// <param name="message"></param>
      /// <param name="innerException"></param>
      public ExtensionException (string message, Exception innerException)
        : base (message, innerException)
      {
      }
    }

    #region Members
    readonly IExtensionsLoader m_extensionsLoader;
    readonly int m_cncAcquisitionId;
    ICncAcquisition m_cncAcquisition = null;
    IEnumerable<ICncFileRepoExtension> m_cncFileRepoExtensions = null;
    IEnumerable<IQueueConfigurationFullExtension> m_queueConfigurationFullExtensions = null;
    bool m_local = false;
    readonly Lemoine.Threading.IChecked m_checkedThread = null;
    #endregion

    ILog log = LogManager.GetLogger (typeof (CncFileRepoFactory).FullName);
    static readonly string FILE_REPOSITORY_NAMESPACE = "cncconfigs";

    #region Getters/Setters
    /// <summary>
    /// Use local files instead of file repository
    /// </summary>
    public bool Local
    {
      get { return m_local; }
      set { m_local = value; }
    }
    #endregion // Getters/Setters

    #region Constructors
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="extensionsLoader">not null</param>
    /// <param name="cncAcquisitionId">Cnc acquisition ID</param>
    /// <param name="checkedThread">Checked thread (nullable)</param>
    public CncFileRepoFactory (IExtensionsLoader extensionsLoader, int cncAcquisitionId, Lemoine.Threading.IChecked checkedThread)
      : base (FILE_REPOSITORY_NAMESPACE, "")
    {
      Debug.Assert (null != extensionsLoader);

      m_extensionsLoader = extensionsLoader;
      this.m_cncAcquisitionId = cncAcquisitionId;
      m_checkedThread = checkedThread;
      log = LogManager.GetLogger (string.Format ("{0}.{1}", typeof (CncFileRepoFactory).FullName, cncAcquisitionId));
    }

    /// <summary>
    /// Constructor to test files.
    /// 
    /// The whole Cnc Acquisition with the machine modules is provided
    /// </summary>
    /// <param name="extensionsLoader">not null</param>
    /// <param name="cncAcquisition">Cnc acquisition, not null</param>
    /// <param name="checkedThread">Checked thread (nullable)</param>
    public CncFileRepoFactory (IExtensionsLoader extensionsLoader, ICncAcquisition cncAcquisition, Lemoine.Threading.IChecked checkedThread)
      : base (FILE_REPOSITORY_NAMESPACE, "")
    {
      Debug.Assert (null != extensionsLoader);
      Debug.Assert (null != cncAcquisition);

      m_extensionsLoader = extensionsLoader;
      this.m_cncAcquisition = cncAcquisition;
      m_checkedThread = checkedThread;
      log = LogManager.GetLogger ($"{typeof (CncFileRepoFactory).FullName}.{cncAcquisition.Id}");
    }

    /// <summary>
    /// Constructor for include directives
    /// 
    /// The whole Cnc Acquisition with the machine modules is provided
    /// </summary>
    /// <param name="extensionsLoader">not null</param>
    /// <param name="cncAcquisition">Cnc acquisition, not null</param>
    /// <param name="path">Path</param>
    /// <param name="checkedThread">Checked thread (nullable)</param>
    public CncFileRepoFactory (IExtensionsLoader extensionsLoader, ICncAcquisition cncAcquisition,
                               string path, Lemoine.Threading.IChecked checkedThread)
      : base (FILE_REPOSITORY_NAMESPACE, "")
    {
      Debug.Assert (null != extensionsLoader);
      Debug.Assert (null != cncAcquisition);

      m_extensionsLoader = extensionsLoader;
      this.m_cncAcquisition = cncAcquisition;
      m_checkedThread = checkedThread;
      log = LogManager.GetLogger ($"{typeof (CncFileRepoFactory).FullName}.{cncAcquisition.Id}");
      this.Path = path;
    }
    #endregion

    #region Methods
    void SetActive ()
    {
      if (null != m_checkedThread) {
        m_checkedThread.SetActive ();
      }
    }

    /// <summary>
    /// Specialized method to build the DOMDocument
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>The DOMDocument</returns>
    public override XmlDocument GetData (System.Threading.CancellationToken cancellationToken, bool optional = false)
    {
      return GetData (null, cancellationToken, optional);
    }

    void InsertPath (XmlDocument xmlDocument, XmlElement includeElement, IList<string> xmlParameters, string subConfigPath, CancellationToken cancellationToken)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"InsertPath: about to add sub config {subConfigPath}");
      }
      var optionalAttribute = includeElement.GetAttribute ("optional");
      var optional = optionalAttribute.Equals ("true", StringComparison.InvariantCultureIgnoreCase)
            || optionalAttribute.Equals ("1");
      CncFileRepoFactory subRepoFactory = new CncFileRepoFactory (m_extensionsLoader,
        m_cncAcquisition, subConfigPath, m_checkedThread);
      subRepoFactory.Local = this.Local;
      XmlDocument subXmlDocument;
      try {
        subXmlDocument = subRepoFactory.GetData (xmlParameters, cancellationToken, optional);
      }
      catch (Exception ex) {
        if (optional) {
          log.Info ($"InsertPath: {subConfigPath} could not be loaded, but continue because it is optional");
          return;
        }
        else {
          log.Error ($"InsertPath: {subConfigPath} could not be loaded and it is required", ex);
          throw new FileRepoException ("SubConfig load error", ex);
        }
      }
      if (optional && (subXmlDocument is null)) {
        log.Info ($"InsertPath: {subConfigPath} not found and optional => return");
        return;
      }
      Replace (subXmlDocument, "{prefix}", includeElement.GetAttribute ("prefix"));
      if (log.IsDebugEnabled) {
        log.Debug ($"InsertPath: insert element of path {subConfigPath}");
      }
      XmlElement parent = includeElement.ParentNode as XmlElement;
      XmlNode lastNode = includeElement;
      foreach (XmlNode node in subXmlDocument.DocumentElement.ChildNodes) {
        if (!(node is XmlElement)) {
          continue;
        }
        XmlNode importedNode = xmlDocument.ImportNode (node, true);
        parent.InsertAfter (importedNode, lastNode);
        lastNode = importedNode;
      }
    }

    void InsertXml (XmlDocument xmlDocument, XmlElement includeElement, Dictionary<string, string> configParameters, string xmlTemplatePath, CancellationToken cancellationToken)
    {
      if (log.IsDebugEnabled) {
        log.DebugFormat ("InsertXml: about to add sub config template");
      }
      CncFileRepoFactory subRepoFactory = new CncFileRepoFactory (m_extensionsLoader,
        m_cncAcquisition, xmlTemplatePath, m_checkedThread);
      subRepoFactory.Local = this.Local;
      XmlDocument configXml;
      try {
        configXml = subRepoFactory.GetData (null, cancellationToken);
      }
      catch (Exception ex) {
        log.Error ($"InsertXml: {xmlTemplatePath} could not be loaded and it is required", ex);
        throw new FileRepoException ("SubConfig load error", ex);
      }

      // replace config parameters
      foreach (var configParameter in configParameters) {
        Replace (configXml, configParameter.Key, configParameter.Value);
      }

      XmlElement parent = includeElement.ParentNode as XmlElement;
      XmlNode lastNode = includeElement;
      foreach (XmlNode node in configXml.DocumentElement.ChildNodes) {
        if (!(node is XmlElement)) {
          continue;
        }
        XmlNode importedNode = xmlDocument.ImportNode (node, true);
        parent.InsertAfter (importedNode, lastNode);
        lastNode = importedNode;
      }
    }


    /// <summary>
    /// Specialized method to build the DOMDocument
    /// </summary>
    /// <param name="xmlParameters">NULL at first, then filled and passed through all sub documents</param>
    /// <param name="cancellationToken"></param>
    /// <param name="optional"></param>
    /// <returns>The DOMDocument</returns>
    XmlDocument GetData (IList<string> xmlParameters, CancellationToken cancellationToken, bool optional = false)
    {
      Debug.Assert (0 != m_cncAcquisitionId || null != m_cncAcquisition);

      // - Get the right row in the Machine table
      //   and set the path
      if (null == m_cncAcquisition) {
        try {
          // Note: check a basic connection so that an exception is returned faster
          //       in case the database is down
          ModelDAOHelper.DAOFactory.CheckBasicConnection ();

          using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            m_cncAcquisition =
              ModelDAOHelper.DAOFactory.CncAcquisitionDAO
              .FindByIdWithMonitoredMachine (m_cncAcquisitionId);
          }
        }
        catch (Exception ex) {
          log.Error ($"GetData: error while getting the Cnc acquisition id {m_cncAcquisitionId} => DatabaseException", ex);
          throw new DatabaseException ("CncAcquisition read problem", ex);
        }
        cancellationToken.ThrowIfCancellationRequested ();
        if (null == m_cncAcquisition) {
          log.Error ($"GetData: there is no CncAcquisition for Id={m_cncAcquisitionId}");
          throw new Exception ("No acquisition data");
        }
      }
      if (string.IsNullOrEmpty (this.Path)) {
        this.Path = m_cncAcquisition.ConfigFile;
      }
      if (string.IsNullOrEmpty (this.Path)) {
        log.Error ($"GetData: invalid path for Cnc Acquisition {m_cncAcquisitionId}");
        throw new Exception ("Invalid path");
      }
      cancellationToken.ThrowIfCancellationRequested ();

      // - Get the content of the file in the File Repository or locally
      System.Xml.XmlDocument xmlDocument;
      if (this.Local) {
        xmlDocument = new XmlDocument ();
        using (var reader = new XmlTextReader (this.Path)) {
          reader.Read ();
          xmlDocument.Load (reader);
        }
      }
      else {
        xmlDocument = base.GetData (cancellationToken, optional);
      }
      if (xmlDocument is null) {
        if (optional) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetData: optional and xmlDocument null");
          }
          return null;
        }
        else {
          log.Fatal ($"GetData: xmlDocument is null although optional={optional}");
          return null;
        }
      }

      if (xmlParameters == null) {
        xmlParameters = GetParameters (xmlDocument);
      }
      SetActive ();
      cancellationToken.ThrowIfCancellationRequested ();

      // - Extensions
      m_extensionsLoader.LoadExtensions ();
      IEnumerable<ICncFileRepoExtension> cncFileRepoExtensions = GetCncFileRepoExtensions ();
      IEnumerable<IQueueConfigurationFullExtension> queueConfigurationFullExtensions = GetQueueConfigurationFullExtensions ();
      if (log.IsDebugEnabled) {
        log.DebugFormat ("GetData: {0} extensions CncFileRepo, {1} extensions QueueConfigurationFull",
          cncFileRepoExtensions.Count (), queueConfigurationFullExtensions.Count ());
      }

      ProcessIncludeElements (xmlDocument, xmlParameters, cancellationToken);
      if (log.IsDebugEnabled) {
        log.DebugFormat ("GetData: ProcessIncludeElements done");
      }
      SetActive ();
      cancellationToken.ThrowIfCancellationRequested ();
      ProcessExtensionElements (xmlDocument, xmlParameters, cancellationToken);
      if (log.IsDebugEnabled) {
        log.DebugFormat ("GetData: ProcessExtensionElements done");
      }
      SetActive ();
      cancellationToken.ThrowIfCancellationRequested ();
      ReplaceKeywords (xmlDocument, xmlParameters);
      if (log.IsDebugEnabled) {
        log.DebugFormat ("GetData: ReplaceKeywords done, end");
      }
      SetActive ();
      cancellationToken.ThrowIfCancellationRequested ();

      return xmlDocument;
    }

    IEnumerable<ICncFileRepoExtension> GetCncFileRepoExtensions ()
    {
      if (null != m_cncFileRepoExtensions) {
        return m_cncFileRepoExtensions;
      }

      try {
        m_cncFileRepoExtensions = Lemoine.Extensions.ExtensionManager
          .GetExtensions<Lemoine.Extensions.Cnc.ICncFileRepoExtension> (checkedThread: m_checkedThread)
          .Where (i => CncFileRepoExtensionInitialize (i))
          .ToList ();
      }
      catch (Exception ex) {
        log.Error ("GetCncFileRepoExtensions: error in getting and initializing the CncFileRepoExtensions", ex);
        throw new ExtensionException ("Error in getting and initializing the CncFileRepoExtension", ex);
      }
      return m_cncFileRepoExtensions;
    }

    IEnumerable<IQueueConfigurationFullExtension> GetQueueConfigurationFullExtensions ()
    {
      if (null != m_queueConfigurationFullExtensions) {
        return m_queueConfigurationFullExtensions;
      }

      try {
        m_queueConfigurationFullExtensions = Lemoine.Extensions.ExtensionManager
          .GetExtensions<Lemoine.Extensions.Cnc.IQueueConfigurationFullExtension> (checkedThread: m_checkedThread)
          .ToList ();
      }
      catch (Exception ex) {
        log.Error ("GetQueueConfigurationFullExtensions: error in getting the QueueConfigurationFullExtensions", ex);
        throw new ExtensionException ("Error in getting the QueueConfigurationFullExtensions", ex);
      }
      return m_queueConfigurationFullExtensions;
    }

    bool CncFileRepoExtensionInitialize (ICncFileRepoExtension extension)
    {
      bool result = extension.Initialize (m_cncAcquisition);
      SetActive ();
      if (!result && log.IsInfoEnabled) {
        log.Info ($"CncFileRepoExtensionInitialize: do not keep extension {extension}, initialize returned false");
      }
      return result;
    }

    void ProcessIncludeElements (XmlDocument xmlDocument, IList<string> xmlParameters, CancellationToken cancellationToken)
    {
      // - Process the include directive
      var includeElements = new List<XmlElement> ();
      foreach (XmlElement includeElement in xmlDocument.GetElementsByTagName ("include")) {
        SetActive ();
        cancellationToken.ThrowIfCancellationRequested ();
        includeElements.Add (includeElement);
        string subConfigPath = includeElement.GetAttribute ("name");
        if (!string.IsNullOrEmpty (subConfigPath)) {
          InsertPath (xmlDocument, includeElement, xmlParameters, subConfigPath, cancellationToken);
        }
        else {
          log.Error ($"ProcessIncludeElements: no name for the include element {includeElement}");
          throw new InvalidDataException ("include with no name attribute");
        }
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"ProcessIncludeElements: {includeElements.Count} elements were processed");
      }
      foreach (XmlElement element in includeElements) {
        cancellationToken.ThrowIfCancellationRequested ();
        // Remove it, the process is completed
        XmlNode parent = element.ParentNode;
        parent.RemoveChild (element);
      }
    }

    void ProcessExtensionElements (XmlDocument xmlDocument, IList<string> xmlParameters, CancellationToken cancellationToken)
    {
      // - Process the extension directive
      var extensionElements = new List<XmlElement> ();
      foreach (XmlElement extensionElement in xmlDocument.GetElementsByTagName ("extension")) {
        cancellationToken.ThrowIfCancellationRequested ();
        extensionElements.Add (extensionElement);
        string extensionName = extensionElement.GetAttribute ("name");
        if (string.IsNullOrEmpty (extensionName)) {
          log.Error ($"ProcessExtensionElements: no name or extension attribute for the extension element {extensionElement}");
          throw new InvalidDataException ("extension with no name or extension attribute");
        }
        else { // extensionExtension not null or empty
          // extensions with config parameters
          log.DebugFormat ("ProcessExtensionElements: GetIncludedXmlTemplate");
          var xmlTemplates = GetIncludedXmlTemplates (extensionName);
          if (!xmlTemplates.Any ()) {
            log.Debug ($"ProcessExtensionElements: no extension sets the include XML for extension {extensionName}, skip it");
          }
          else {
            foreach (var xmlTemplate in xmlTemplates) {
              log.Debug ($"ProcessExtensionElements: insert {xmlTemplate.Item1} for extension {extensionName}");
              cancellationToken.ThrowIfCancellationRequested ();

              InsertXml (xmlDocument, extensionElement, xmlTemplate.Item2, xmlTemplate.Item1, cancellationToken);
            }
          }

          // static extensions
          log.Debug ("ProcessExtensionElements: GetIncludePaths");
          var subConfigPaths = GetIncludePaths (extensionName);
          if (!subConfigPaths.Any ()) {
            log.Debug ($"ProcessExtensionElements: no extension sets the include path for extension {extensionName}, skip it");
          }
          else {
            foreach (var path in subConfigPaths) {
              log.DebugFormat ($"ProcessExtensionElements: insert {path} for extension {extensionName}");
              cancellationToken.ThrowIfCancellationRequested ();
              InsertPath (xmlDocument, extensionElement, xmlParameters, path, cancellationToken);
            }
          }
        }
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"ProcessExtensionElements: {extensionElements.Count} elements were processed");
      }
      foreach (XmlElement element in extensionElements) {
        cancellationToken.ThrowIfCancellationRequested ();
        // Remove it, the process is completed
        XmlNode parent = element.ParentNode;
        parent.RemoveChild (element);
      }
    }

    void ReplaceKeywords (XmlDocument xmlDocument, IList<string> xmlParameters)
    {
      // - Replace the keywords in {}
      foreach (XmlElement element in xmlDocument.GetElementsByTagName ("*")) {
        SetActive ();
        foreach (XmlAttribute attribute in element.Attributes) {
          SetActive ();
          if (attribute.Value.Contains ("{")) {
            if (log.IsDebugEnabled) {
              log.Debug ($"ReplaceKeywords: analyzing attribute {attribute.Value}");
            }

            // {config....}
            const string configPrefix = "config.";
            if (attribute.Value.Contains ("{" + configPrefix)) {
              // Get the config key
              int index = attribute.Value.IndexOf ("{" + configPrefix, StringComparison.InvariantCultureIgnoreCase);
              string configKey = attribute.Value
                .Substring (index + 1 + configPrefix.Length);
              configKey = configKey.Substring (0, configKey.IndexOf ('}'));
              var configValue = GetConfig (configKey)?.ToString () ?? "";
              if (configValue is null) {
                log.Error ($"ReplaceKeywords: configValue is null for key {configKey} => replace it by an empty string");
                configValue = "";
              }
              else if (log.IsDebugEnabled && string.IsNullOrEmpty (configValue)) {
                log.Debug ($"ReplaceKeywords: configValue is empty for key {configKey}");
              }
              attribute.Value = attribute.Value.Replace ("{" + configPrefix + configKey + "}",
                                                         configValue.ToString ());
            }

            // {AutoSequenceMachineModeTranslationKey} / {AutoSequenceMachineModeNameTranslation} / {AutoSequenceMachineModeId}
            const string autoSequenceMachineModeTranslationKeyPattern = "{AutoSequenceMachineModeTranslationKey}";
            if (attribute.Value.Contains (autoSequenceMachineModeTranslationKeyPattern)) {
              IList<IMachineMode> machineModes = GetAutoSequenceMachineModes ();
              string[] machineModesTranslationKey = new string[machineModes.Count];
              for (int i = 0; i < machineModes.Count; i++) {
                machineModesTranslationKey[i] = machineModes[i].TranslationKey;
              }
              const string separator = "/";
              string machineModesString = separator;
              machineModesString += string.Join (separator, machineModesTranslationKey);
              attribute.Value = attribute.Value.Replace (autoSequenceMachineModeTranslationKeyPattern, machineModesString);
            }
            const string autoSequenceMachineModeNameOrTranslationPattern = "{AutoSequenceMachineModeNameOrTranslation}";
            if (attribute.Value.Contains (autoSequenceMachineModeNameOrTranslationPattern)) {
              IList<IMachineMode> machineModes = GetAutoSequenceMachineModes ();
              string[] machineModesNameOrTranslation = new string[machineModes.Count];
              for (int i = 0; i < machineModes.Count; i++) {
                machineModesNameOrTranslation[i] = machineModes[i].NameOrTranslation;
              }
              const string separator = "/";
              string machineModesString = separator;
              machineModesString += string.Join (separator, machineModesNameOrTranslation);
              attribute.Value = attribute.Value.Replace (autoSequenceMachineModeNameOrTranslationPattern, machineModesString);
            }
            const string autoSequenceMachineModeIdPattern = "{AutoSequenceMachineModeId}";
            if (attribute.Value.Contains (autoSequenceMachineModeIdPattern)) {
              IList<IMachineMode> machineModes = GetAutoSequenceMachineModes ();
              string[] machineModesId = new string[machineModes.Count];
              for (int i = 0; i < machineModes.Count; i++) {
                machineModesId[i] = machineModes[i].Id.ToString ();
              }
              const string separator = "/";
              string machineModesString = separator;
              machineModesString += string.Join (separator, machineModesId);
              attribute.Value = attribute.Value.Replace (autoSequenceMachineModeIdPattern, machineModesString);
            }

            // {RunningMachineModeTranslationKey} / {RunningMachineModeNameOrTranslation} / {RunningMachineModeId}
            const string runningMachineModeTranslationKeyPattern = "{RunningMachineModeTranslationKey}";
            if (attribute.Value.Contains (runningMachineModeTranslationKeyPattern)) {
              IList<IMachineMode> machineModes = GetRunningMachineModes ();
              string[] machineModesTranslationKey = new string[machineModes.Count];
              for (int i = 0; i < machineModes.Count; i++) {
                machineModesTranslationKey[i] = machineModes[i].TranslationKey;
              }
              const string separator = "/";
              string machineModesString = separator;
              machineModesString += string.Join (separator, machineModesTranslationKey);
              attribute.Value = attribute.Value.Replace (runningMachineModeTranslationKeyPattern, machineModesString);
            }
            const string runningMachineModeNameOrTranslationPattern = "{RunningMachineModeNameOrTranslation}";
            if (attribute.Value.Contains (runningMachineModeNameOrTranslationPattern)) {
              IList<IMachineMode> machineModes = GetRunningMachineModes ();
              string[] machineModesNameOrTranslation = new string[machineModes.Count];
              for (int i = 0; i < machineModes.Count; i++) {
                machineModesNameOrTranslation[i] = machineModes[i].NameOrTranslation;
              }
              const string separator = "/";
              string machineModesString = separator;
              machineModesString += string.Join (separator, machineModesNameOrTranslation);
              attribute.Value = attribute.Value.Replace (runningMachineModeNameOrTranslationPattern, machineModesString);
            }
            const string runningMachineModeIdPattern = "{RunningMachineModeId}";
            if (attribute.Value.Contains (runningMachineModeIdPattern)) {
              IList<IMachineMode> machineModes = GetRunningMachineModes ();
              string[] machineModesId = new string[machineModes.Count];
              for (int i = 0; i < machineModes.Count; i++) {
                machineModesId[i] = machineModes[i].Id.ToString ();
              }
              const string separator = "/";
              string machineModesString = separator;
              machineModesString += string.Join (separator, machineModesId);
              attribute.Value = attribute.Value.Replace (runningMachineModeIdPattern, machineModesString);
            }

            // {...Id}
            ReplaceValue (attribute, m_cncAcquisition.ConfigPrefix + "Id",
                          m_cncAcquisition.Id.ToString ());
            // {...Name}
            ReplaceValue (attribute, m_cncAcquisition.ConfigPrefix + "Name",
                          m_cncAcquisition.Name);
            Debug.Assert (null != m_cncAcquisition.ConfigPrefix);
            if (null != m_cncAcquisition.ConfigParameters) {
              // {..Parameters}
              string parametersKey = "{" + m_cncAcquisition.ConfigPrefix + "Parameter}";
              if (attribute.Value.Contains (parametersKey)) {
                attribute.Value = attribute.Value.Replace (parametersKey, m_cncAcquisition.ConfigParameters);
              }
              // {..Parami}
              for (int i = 0; i < xmlParameters.Count; i++) {
                ReplaceValue (attribute,
                              m_cncAcquisition.ConfigPrefix + "Param" + (i + 1),
                              xmlParameters[i]);
              }
            }
            foreach (IMachineModule machineModule in m_cncAcquisition.MachineModules) {
              var defaultDetectionMethod = GetDefaultDetectionMethod (machineModule);
              // {..MachineParami}
              if ((null != machineModule.ConfigParameters) && (1 < machineModule.ConfigParameters.Length)) {
                string[] parameters = machineModule.ConfigParameters.Split (machineModule.ConfigParameters[0]);
                for (int i = 1; i < parameters.Length; ++i) {
                  ReplaceValue (attribute,
                                machineModule.ConfigPrefix + "MachineParam" + i,
                                parameters[i]);
                }
              }
              // {...MachineModuleId}
              ReplaceValue (attribute, machineModule.ConfigPrefix + "MachineModuleId",
                            machineModule.Id.ToString ());
              // {...MachineModuleName}
              ReplaceValue (attribute, machineModule.ConfigPrefix + "MachineModuleName",
                            machineModule.Name);
              // {...MachineId}
              ReplaceValue (attribute, machineModule.ConfigPrefix + "MachineId",
                            machineModule.MonitoredMachine.Id.ToString ());
              // {...MachineName}
              ReplaceValue (attribute, machineModule.ConfigPrefix + "MachineName",
                            machineModule.MonitoredMachine.Name);
              // {...SequenceVariable}
              ReplaceValue (attribute, machineModule.ConfigPrefix + "SequenceVariable",
                            machineModule.SequenceVariable);
              // {...SequenceVariableSet}
              ReplaceValue (attribute, machineModule.ConfigPrefix + "SequenceVariableSet",
                            string.IsNullOrEmpty (machineModule.SequenceVariable) || defaultDetectionMethod.HasFlag (DetectionMethod.CncVariableSet) ? "False" : "True");
              // {...MilestoneVariable}
              ReplaceValue (attribute, machineModule.ConfigPrefix + "MilestoneVariable",
                            machineModule.MilestoneVariable);
              // {...MilestoneVariableSet}
              ReplaceValue (attribute, machineModule.ConfigPrefix + "MilestoneVariableSet",
                            string.IsNullOrEmpty (machineModule.MilestoneVariable) || defaultDetectionMethod.HasFlag (DetectionMethod.CncVariableSet) ? "False" : "True");
              // {...CycleVariable}
              ReplaceValue (attribute, machineModule.ConfigPrefix + "CycleVariable",
                            machineModule.CycleVariable);
              // {...CycleVariableSet}
              ReplaceValue (attribute, machineModule.ConfigPrefix + "CycleVariableSet",
                            string.IsNullOrEmpty (machineModule.CycleVariable) || defaultDetectionMethod.HasFlag (DetectionMethod.CncVariableSet) ? "False" : "True");
              // {...StartCycleVariable}
              ReplaceValue (attribute, machineModule.ConfigPrefix + "StartCycleVariable",
                            machineModule.StartCycleVariable);
              // {...StartCycleVariableSet}
              ReplaceValue (attribute, machineModule.ConfigPrefix + "StartCycleVariableSet",
                            string.IsNullOrEmpty (machineModule.StartCycleVariable) || defaultDetectionMethod.HasFlag (DetectionMethod.CncVariableSet) ? "False" : "True");
              // {...SequenceVariableOrXxx}
              ReplaceValueOrDefault (attribute,
                                     machineModule.ConfigPrefix + "SequenceVariable",
                                     machineModule.SequenceVariable);
              // {...CycleVariableOrXxx}
              ReplaceValueOrDefault (attribute,
                                     machineModule.ConfigPrefix + "CycleVariable",
                                     machineModule.CycleVariable);
              // {...StartCycleVariableOrXxx}
              ReplaceValueOrDefault (attribute,
                                     machineModule.ConfigPrefix + "StartCycleVariable",
                                     machineModule.StartCycleVariable);
              // {..CncVariableKeys}
              ReplaceValue (attribute,
                            machineModule.ConfigPrefix + "CncVariableKeys",
                            GetCncVariableKeys (machineModule).ToListString ());
              // {..QueueConfiguration}
              ReplaceValue (attribute,
                machineModule.ConfigPrefix + "QueueConfiguration",
                GetQueueConfiguration (machineModule.MonitoredMachine, machineModule));
            }
          }
        }
      }
    }

    DetectionMethod GetDefaultDetectionMethod (IMachineModule machineModule)
    {
      foreach (var extension in GetCncFileRepoExtensions ()) {
        SetActive ();
        var defaultDetectionMethod = extension.GetDefaultDetectionMethod (machineModule);
        if (defaultDetectionMethod.HasValue) {
          log.Debug ("GetDefaultDetectionMethod: return default detection method from plugin");
          return defaultDetectionMethod.Value;
        }
      }
      if (log.IsDebugEnabled) {
        log.Debug ("GetDefaultDetectionMethod: no detection method");
      }
      return DetectionMethod.None;
    }

    IEnumerable<string> GetCncVariableKeys (IMachineModule machineModule)
    {
      IEnumerable<string> keys = new List<string> ();
      foreach (var extension in GetCncFileRepoExtensions ()) {
        SetActive ();
        var cncVariableKeys = extension.GetCncVariableKeys (machineModule);
        if (null != cncVariableKeys) {
          keys = keys.Union (cncVariableKeys);
        }
      }
      if (log.IsDebugEnabled) {
        log.DebugFormat ("GetCncVariableKeys: {0} keys", keys.Distinct ().Count ());
      }
      return keys.Distinct ();
    }

    IEnumerable<string> GetIncludePaths (string extensionName)
    {
      return GetCncFileRepoExtensions ()
        .Select (extension => extension.GetIncludePath (extensionName))
        .Where (path => !string.IsNullOrEmpty (path));
    }


    IEnumerable<Tuple<string, Dictionary<string, string>>> GetIncludedXmlTemplates (string extensionName)
    {
      IEnumerable<Tuple<string, Dictionary<string, string>>> result = null;
      result = GetCncFileRepoExtensions ()
        .Select (extension => extension.GetIncludedXmlTemplate (extensionName))
        .Where (configXml => null != configXml);

      return result;
    }

    string GetQueueConfiguration (IMonitoredMachine machine, IMachineModule machineModule)
    {
      Debug.Assert (null != m_cncAcquisition);
      Debug.Assert (null != machine);
      Debug.Assert (null != machineModule);

      foreach (var extension in GetQueueConfigurationFullExtensions ()) {
        string localSubDirectory = "CncAcquisition-" + m_cncAcquisition.Id;
        string distantDirectory = extension.GetDistantDirectory (machine.Id, machineModule.Id);
        string distantFileName = extension.GetDistantFileName (machine.Id, machineModule.Id);
        if ((null != distantDirectory) && !string.IsNullOrEmpty (distantFileName)) {
          log.InfoFormat ("GetQueueConfiguration: loading distant file {0}/{1}", distantDirectory, distantFileName);
          var baseDirectory = Lemoine.Info.PulseInfo.LocalConfigurationDirectory;
          baseDirectory = System.IO.Path.Combine (baseDirectory, localSubDirectory);
          string localDirectory = System.IO.Path.Combine (baseDirectory, distantDirectory);
          if (!Directory.Exists (localDirectory)) {
            log.Debug ($"GetQueueConfiguration: create directory {localDirectory}");
            Directory.CreateDirectory (localDirectory);
          }
          string localPath = System.IO.Path.Combine (localDirectory, distantFileName);
          try {
            Lemoine.FileRepository.FileRepoClient.GetFile (distantDirectory, distantFileName, localPath);
          }
          catch (Exception ex) {
            log.Error ($"GetQueueConfiguration: FileRepoClient.GetFile of {distantDirectory}/{distantFileName} to {localPath} failed", ex);
            throw new FileRepoException ("Error in getting a remote queue configuration file", ex);
          }
          return System.IO.Path.Combine (distantDirectory, distantFileName) + ":" + localSubDirectory;
        }
      }

      log.DebugFormat ("GetQueueConfiguration: no specific queue");
      return "";
    }

    /// <summary>
    /// Replace in a whole XML document a pattern by a value
    /// </summary>
    /// <param name="xmlDocument"></param>
    /// <param name="key"></param>
    /// <param name="replacement"></param>
    void Replace (XmlDocument xmlDocument,
                  string key,
                  string replacement)
    {
      foreach (XmlElement element in xmlDocument.GetElementsByTagName ("*")) {
        foreach (XmlAttribute attribute in element.Attributes) {
          if (attribute.Value.Contains (key)) {
            attribute.Value = attribute.Value.Replace (key, replacement);
          }
        }
        if ((1 == element.ChildNodes.Count)
            && element.FirstChild is System.Xml.XmlText) { // This is a leaf
          string text = element.InnerText;
          if (text.Contains (key)) {
            element.InnerText = text.Replace (key, replacement);
          }
        }
      }
    }

    /// <summary>
    /// Replace in an attribute {keyword} by the specified value v
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="keyword"></param>
    /// <param name="v"></param>
    void ReplaceValue (XmlAttribute attribute,
                       string keyword,
                       string v)
    {
      string fullKey = "{" + keyword + "}";
      if (attribute.Value.Contains (fullKey)) {
        attribute.Value = attribute.Value.Replace (fullKey, v);
      }
    }

    /// <summary>
    /// Replace in an attribute {prefixOrXxx} by the right value, where:
    /// <item>prefix is keyword that would correspond to the specified value</item>
    /// <item>Xxx is the default value</item>
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="prefix"></param>
    /// <param name="v"></param>
    void ReplaceValueOrDefault (XmlAttribute attribute,
                                string prefix,
                                string v)
    {
      string fullPrefix = "{" + prefix + "Or";
      if (attribute.Value.Contains (fullPrefix)) {
        int index = attribute.Value.IndexOf (fullPrefix);
        string fullPattern = attribute.Value
          .Substring (index);
        fullPattern = fullPattern
          .Substring (0, fullPattern.IndexOf ('}') + 1);
        if (!string.IsNullOrEmpty (v)) {
          attribute.Value = attribute.Value.Replace (fullPattern, v);
        }
        else {
          string defaultValue = fullPattern
            .Substring (fullPrefix.Length, fullPattern.Length - fullPrefix.Length - 1);
          attribute.Value = attribute.Value.Replace (fullPattern, defaultValue);
        }
      }
    }

    /// <summary>
    /// Get the config for a specific key. If not found, an empty string is returned
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="DatabaseException"></exception>
    string GetConfig (string key)
    {
      if (Lemoine.Info.ConfigSet.LoadAndGet (GET_CONFIG_METHOD_KEY, GET_CONFIG_METHOD_DEFAULT).Equals (GET_CONFIG_METHOD_DATABASEONLY, StringComparison.InvariantCultureIgnoreCase)) {
        IConfig config;
        try {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            config = ModelDAOHelper.DAOFactory.ConfigDAO
              .GetConfig (key);
          }
        }
        catch (Exception ex) {
          log.Error ($"GetConfig: config key {key} could not be read", ex);
          throw new DatabaseException ("Config read problem", ex);
        }
        if (null == config) {
          log.WarnFormat ($"GetConfig: config key {key} does not exist");
          return "";
        }
        else {
          return config.Value?.ToString () ?? "";
        }
      }
      else { // Else use by default Lemoine.Info.ConfigSet
        return Lemoine.Info.ConfigSet.LoadAndGet (key, "");
      }
    }

    IList<IMachineMode> GetAutoSequenceMachineModes ()
    {
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          IList<IMachineMode> machineModes = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindAutoSequence ();
          return machineModes;
        }
      }
      catch (Exception ex) {
        log.Error ("GetAutoSequenceMachineModes: could not be read", ex);
        throw new DatabaseException ("AutoSequenceMachineModes read problem", ex);
      }
    }

    IList<IMachineMode> GetRunningMachineModes ()
    {
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          IList<IMachineMode> machineModes = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindRunning ();
          return machineModes;
        }
      }
      catch (Exception ex) {
        log.Error ("GetRunningMachineModes: could not be read", ex);
        throw new DatabaseException ("Running machine modes read problem", ex);
      }
    }

    IList<string> GetParameters (XmlDocument xmlDocument)
    {
      // Find all default parameters that are not optional
      // Because if optional, an empty value should not be replaced
      var parameters = new List<string> ();
      foreach (XmlElement element in xmlDocument.GetElementsByTagName ("parameter")) {
        string value = "";
        string optional = "";
        foreach (XmlAttribute attribute in element.Attributes) {
          if (attribute.Name == "default") {
            value = attribute.Value;
          }
          else if (attribute.Name == "optional") {
            optional = attribute.Value;
          }
        }
        if (String.Equals (optional, "true", StringComparison.CurrentCultureIgnoreCase) ||
            String.Equals (optional, "1", StringComparison.CurrentCultureIgnoreCase)) {
          parameters.Add ("");
        }
        else {
          parameters.Add (value);
        }
      }

      // Find all parameters in database
      string[] definedParams = { "" };
      if (m_cncAcquisition.ConfigParameters != null && m_cncAcquisition.ConfigParameters.Length > 1) {
        definedParams = m_cncAcquisition.ConfigParameters.Split (m_cncAcquisition.ConfigParameters[0]);
      }

      // Merge (the first element in definedParams is skipped => empty)
      for (int i = 1; i < definedParams.Length; i++) {
        if (i > parameters.Count) {
          parameters.Add ("");
        }

        parameters[i - 1] = definedParams[i];
      }

      GetPathsInFileRepo (xmlDocument, parameters);

      return parameters;
    }

    void GetPathsInFileRepo (XmlDocument xmlDocument, IList<string> parameters)
    {
      // Complete the path for the type "file" if the path is not absolute
      int position = 0;
      foreach (XmlElement element in xmlDocument.GetElementsByTagName ("parameter")) {
        try {
          TryGetPathInFileRepoForParameter (element, position, parameters);
        }
        catch (Exception ex) {
          log.Error ($"GetPathsInFileRepo: exception for parameter in position {position}", ex);
        }
        position++;
      }
    }

    void TryGetPathInFileRepoForParameter (XmlElement element, int position, IList<string> parameters)
    {
      if (!IsParameterOfTypeFile (element)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"TryGetPathInFileRepoForParameter: parameter in position {position} is not of type file");
        }
        return;
      }

      var path = parameters[position];

      if (string.IsNullOrEmpty (path)) {
        log.Warn ($"TryGetPathInFileRepoForParameter: parameter[{position}] is empty");
        return;
      }

      if (File.Exists (path)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"TryGetPathInFileRepoForParameter: file {path} already exists => nothing to do");
        }
        return;
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"TryGetPathInFileRepoForParameter: file {path} does not exist => try to get it in file repo");
        }
        GetPathInFileRepoForParameter (position, parameters);
      }
    }

    bool IsParameterOfTypeFile (XmlElement element)
    {
      foreach (XmlAttribute attribute in element.Attributes) {
        if (IsParameterAttributeTypeFile (attribute)) {
          return true;
        }
      }

      return false;
    }

    bool IsParameterAttributeTypeFile (XmlAttribute attribute)
    {
      return attribute.Name.Equals ("type", StringComparison.InvariantCultureIgnoreCase)
        && attribute.Value.Equals ("file", StringComparison.InvariantCultureIgnoreCase);
    }

    void GetPathInFileRepoForParameter (int position, IList<string> parameters)
    {
      // Local path
      // Make it depend on the CNC Acquisition ID to avoid any conflict between threads
      var localPath = System.IO.Path.Combine (Lemoine.Info.PulseInfo.LocalConfigurationDirectory,
        "cnc_resources",
        $"acquisition-{this.m_cncAcquisitionId}",
        parameters[position]);

      // Prepare the directories
      try {
        var localDirectory = System.IO.Path.GetDirectoryName (localPath);
        if (string.IsNullOrEmpty (localDirectory)) {
          log.Error ($"GetPathInFileRepoForParameter: no directory for path {localPath}");
          return;
        }
        if (!Directory.Exists (localDirectory)) {
          Directory.CreateDirectory (localDirectory);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetPathInFileRepoForParameter: successfully created directory {localDirectory}");
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"GetPathInFileRepoForParameter: couldn't create directory of {localPath}", ex);
      }

      string backupFileName = null;
      if (File.Exists (localPath)) {
        try {
          backupFileName = System.IO.Path.GetTempFileName ();
          File.Copy (localPath, backupFileName, true);
        }
        catch (Exception ex) {
          log.Error ($"GetPathInFileRepoForParameter: error backing up {localPath} to {backupFileName}", ex);
          backupFileName = null;
        }
      }

      // Synchronize the file
      try {
        Lemoine.FileRepository.FileRepoClient.SynchronizeFile ("cnc_resources", parameters[position], localPath);
      }
      catch (Exception ex) {
        log.Error ($"GetPathInFileRepoForParameter: couldn't synchronize file {parameters[position]} of cnc_resources with {localPath}", ex);
        if (File.Exists (localPath)) {
          log.Error ($"GetPathInFileRepoForParameter: {localPath} exists although SynchronizeFile failed, continue", ex);
        }
        else { // Try to restore the backup file
          if (null != backupFileName) {
            log.Info ($"GetPathInFileRepoForParameter: try to restore {backupFileName} into {localPath}");
            try {
              File.Move (backupFileName, localPath);
              log.Info ($"GetPathInFileRepoForParameter: old resource file successfully restored");
            }
            catch (Exception ex1) {
              log.Error ($"GetPathInFileRepoForParameter: restore of {backupFileName} into {localPath} failed => throw an exception", ex1);
              throw new FileRepoException ("cnc_resources synchronization error", ex);
            }
          }
          else {
            throw new FileRepoException ("cnc_resources synchronization error", ex);
          }
        }
      }
      if ((null != backupFileName) && File.Exists (backupFileName)) {
        try {
          File.Delete (backupFileName);
        }
        catch (Exception ex) {
          log.Error ($"GetPathInFileRepoForParameter: delete of backup file {backupFileName} failed", ex);
        }
      }
      if (!File.Exists (localPath)) {
        log.Fatal ($"GetPathInFileRepoForParameter: file {localPath} not found after synchronization");
      }

      // Update the parameter
      if (log.IsInfoEnabled) {
        log.Info ($"GetPathInFileRepoForParameter: relative path '{parameters[position]}' becomes '{localPath}'");
      }
      parameters[position] = localPath;
    }
    #endregion
  }
}
