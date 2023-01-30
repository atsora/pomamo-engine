// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Lemoine.Core.Log;
using Lemoine.Extensions.Alert;

namespace Lemoine.Plugin.AlertConfigDirectory
{
  /// <summary>
  /// Description of AlertConfig.
  /// </summary>
  internal sealed class AlertConfig
  {
    #region Members
    readonly ICollection<IListener> m_listeners = new List<IListener> ();
    readonly ICollection<TriggeredAction> m_triggeredActions = new List<TriggeredAction> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (AlertConfig).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated listeners
    /// </summary>
    public ICollection<IListener> Listeners
    {
      get { return m_listeners; }
    }

    /// <summary>
    /// Associated triggered actions
    /// </summary>
    public ICollection<TriggeredAction> TriggeredActions
    {
      get { return m_triggeredActions; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public AlertConfig ()
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="configDirectory"></param>
    public AlertConfig (string configDirectory)
    {
      AddConfigDirectory (configDirectory);
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a directory path where triggered actions are configured
    /// </summary>
    internal void AddConfigDirectory (string directoryPath)
    {
      string pathRootedDirectoryPath = new Lemoine.Extensions.Alert.AlertConfigDirectory (directoryPath).AbsolutePath;

      try {
        foreach (string fileName in Directory.GetFiles (pathRootedDirectoryPath, "*.config", SearchOption.AllDirectories)) {
          if (fileName.EndsWith (".template.config", StringComparison.InvariantCultureIgnoreCase)) {
            log.Debug ($"AddConfigDirectory: skip template {fileName}");
            continue;
          }
          log.Debug ($"AddConfigDirectory: add config file {fileName}");
          string filePath = Path.Combine (pathRootedDirectoryPath, fileName);
          try {
            AddConfigFile (filePath);
          }
          catch (Exception ex) {
            log.Error ($"AddConfigDirectory: loading file {filePath} failed", ex);
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"AddConfigDirectory: listing the files of directory {pathRootedDirectoryPath} failed", ex);
      }
    }

    /// <summary>
    /// Add a config file to the main configuration
    /// </summary>
    /// <param name="url"></param>
    internal void AddConfigFile (string url)
    {
      XmlDocument xmlDocument = new XmlDocument ();
      using (var reader = new XmlTextReader (url)) {
        reader.Read ();
        xmlDocument.Load (reader);
      }
      AddXmlConfig (xmlDocument.DocumentElement, url);
    }

    internal void AddXmlConfig (XmlNode xmlNode, string url)
    {
      foreach (XmlNode node in xmlNode.ChildNodes) {
        if (node is XmlElement) {
          XmlElement element = node as XmlElement;
          log.DebugFormat ("AddXmlConfig: " +
                           "parse element {0}",
                           element.Name);
          if (element.Name.Equals ("listeners")) { // listeners section
            foreach (XmlNode listenerNode in element.ChildNodes) {
              if (listenerNode is XmlElement) {
                try {
                  this.Listeners.Add (ParseListenerConfig (listenerNode as XmlElement));
                }
                catch (Exception ex) {
                  log.Error ($"AddXmlConfig: ParseListenerConfig failed on {listenerNode} ({url})", ex);
                  // Continue to parse the next listener
                }
              }
            }
          }
          else if (element.Name.Equals ("configDirectory")) { // configDirectory section
            string directoryPath = element.GetAttribute ("path");
            if (string.IsNullOrEmpty (directoryPath)) {
              log.ErrorFormat ("AddXmlConfig: " +
                               "directoryPath is not specified for element {0} ({1})",
                               element, url);
            }
            else {
              try {
                this.AddConfigDirectory (directoryPath);
              }
              catch (Exception ex) {
                log.Error ($"AddXmlConfig: AddConfigDirectory of {directoryPath} ({url})", ex);
                // Continue
              }
            }
          }
          else if (element.Name.Equals ("configFile")) { // configFile section
            string filePath = element.GetAttribute ("path");
            if (string.IsNullOrEmpty (filePath)) {
              log.ErrorFormat ("AddXmlConfig: " +
                               "filePath is not specified for element {0} ({1})",
                               element, url);
            }
            else {
              try {
                this.AddConfigFile (filePath);
              }
              catch (Exception ex) {
                log.Error ($"AddXmlConfig: AddConfigFile of {filePath} failed ({url})", ex);
                // Continue
              }
            }
          }
          else if (element.Name.Equals ("triggeredAction")) { // triggeredAction section
            try {
              this.TriggeredActions.Add (ParseTriggeredActionConfig (element));
            }
            catch (Exception ex) {
              log.Error ($"AddXmlConfig: ParseTriggeredActionConfig failed on {element} ({url})", ex);
              // Continue to parse the next triggeredAction
            }
          }
        }
      }
    }

    IListener ParseListenerConfig (XmlElement element)
    {
      XmlSerializer serializer = new XmlSerializer (GetTypeFromXmlElement (element));
      IListener listener =
        (IListener)serializer.Deserialize (new StringReader (element.OuterXml));
      return listener;
    }

    TriggeredAction ParseTriggeredActionConfig (XmlElement element)
    {
      TriggeredAction triggeredAction = new TriggeredAction ();
      foreach (XmlNode node in element.ChildNodes) {
        if (node is XmlElement) {
          XmlElement subElement = node as XmlElement;
          if (subElement.Name.Equals ("trigger")) {
            foreach (XmlNode triggerChild in subElement.ChildNodes) {
              if (triggerChild is XmlElement) {
                triggeredAction.Trigger = ParseTriggerConfig (triggerChild as XmlElement);
                break;
              }
            }
          }
          else if (subElement.Name.Equals ("action")) {
            foreach (XmlNode actionChild in subElement.ChildNodes) {
              if (actionChild is XmlElement) {
                triggeredAction.Actions.Add (ParseActionConfig (actionChild as XmlElement));
              }
            }
          }
        }
      }

      return triggeredAction;
    }

    ITrigger ParseTriggerConfig (XmlElement element)
    {
      XmlSerializer serializer = new XmlSerializer (GetTypeFromXmlElement (element));
      ITrigger trigger =
        (ITrigger)serializer.Deserialize (new StringReader (element.OuterXml));
      return trigger;
    }

    IAction ParseActionConfig (XmlElement element)
    {
      XmlSerializer serializer = new XmlSerializer (GetTypeFromXmlElement (element));
      IAction action =
        (IAction)serializer.Deserialize (new StringReader (element.OuterXml));
      return action;
    }

    Type GetTypeFromXmlElement (XmlElement element)
    {
      Type type = null;

      // - Try first with the type attribute
      string typeAttribute = element.GetAttribute ("type");
      if (!string.IsNullOrEmpty (typeAttribute)) {
        type = Type.GetType (typeAttribute, false, true);
      }

      // - Else use the name of the element
      if (null == type) {
        type = Type.GetType ("Lemoine.Alert." + element.Name + ", Lemoine.Alert", true);
      }

      return type;
    }
    #endregion // Methods
  }
}
