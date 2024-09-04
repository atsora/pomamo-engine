// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Linq;
using System.Xml;
using Lemoine.Core.Log;

namespace ConfiguratorRedirectEvent
{
  /// <summary>
  /// Description of Action.
  /// </summary>
  public class Action
  {
    #region Members
    string m_filePath;
    string m_fileName;
    #endregion // Members
    
    static readonly ILog log = LogManager.GetLogger(typeof (Action).FullName);

    #region Getters / Setters
    /// <summary>
    /// True if the action is valid
    /// </summary>
    public bool IsValid { get; private set; }
    
    /// <summary>
    /// Action title
    /// </summary>
    public string Title { get; private set; }
    
    /// <summary>
    /// Action description
    /// </summary>
    public string Description { get; private set; }
    
    /// <summary>
    /// Type of the event (cnc value, long period, tool life, ...)
    /// </summary>
    public string EventType { get; private set; }
    
    /// <summary>
    /// Type of the action (email, sms, ...)
    /// </summary>
    public string ActionType { get; private set; }
    
    /// <summary>
    /// Advanced mode
    /// </summary>
    public bool AdvancedMode { get; private set; }
    
    /// <summary>
    /// Activated state of the action
    /// </summary>
    public bool Activated {
      get {
        if (!IsValid) {
          return false;
        }

        var split = m_fileName.Split('.');
        return split[split.Length - 2] != "template";
      }
      set {
        // Rename the file
        string newName = Name + (value ? ".config" : ".template.config");
        if (File.Exists(Path.Combine(m_filePath, newName))) {
          log.WarnFormat("ConfiguratorRedirectEvent: deleted file {0} and keep file {1}", m_fileName, newName);
          File.Delete(Path.Combine(m_filePath, m_fileName));
        } else {
          File.Move(Path.Combine(m_filePath, m_fileName), Path.Combine(m_filePath, newName));
        }

        m_fileName = newName;
      }
    }
    
    /// <summary>
    /// Name of the file, without the extension
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// Full text of the action
    /// </summary>
    public string FullText { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor of the action, taking as parameter its name
    /// </summary>
    /// <param name="file"></param>
    public Action(FileInfo file)
    {
      m_fileName = file.Name;
      m_filePath = file.DirectoryName;
      IsValid = true;
      Description = "no description";
      EventType = "unknown";
      ActionType = "unknown";
      AdvancedMode = false;
      
      var split = m_fileName.Split('.');
      if (split.Length < 2 || split.Last() != "config") {
        IsValid = false;
      }
      else {
        // The extension is removed
        Name = String.Join(".", split, 0, split.Length - (Activated ? 1 : 2));
        Title = Name;
        
        try {
          using (var sr = new StreamReader(Path.Combine(m_filePath, m_fileName))) {
            FullText = sr.ReadToEnd();
          }
          var xmlDoc = new XmlDocument();
          xmlDoc.Load(Path.Combine(m_filePath, m_fileName));
          FullText = xmlDoc.InnerXml;
          Parse(xmlDoc);
        } catch {
          IsValid = false;
          throw;
        }
      }
    }
    #endregion // Constructors

    #region Methods
    void Parse(XmlDocument xmlDoc)
    {
      // Root node and namespace
      XmlNode root = xmlDoc.DocumentElement;
      
      // Description
      XmlNode node = root.SelectSingleNode("description");
      if (node != null) {
        Description = node.InnerText;
      }

      // Title
      node = root.SelectSingleNode("title");
      if (node != null) {
        Title = node.InnerText;
      }

      // Event type
      node = root.SelectSingleNode("eventtype");
      if (node != null) {
        EventType = node.InnerText;
      }

      // Action type
      node = root.SelectSingleNode("actiontype");
      if (node != null) {
        ActionType = node.InnerText;
      }

      // Advanced mode
      node = root.SelectSingleNode("advanced");
      if (node != null) {
        AdvancedMode = (node.InnerText.ToLower() == "true");
      }
    }
    #endregion // Methods
  }
}
