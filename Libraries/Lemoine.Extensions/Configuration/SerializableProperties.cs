// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Xml;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Configuration
{
  /// <summary>
  /// Deprecated
  /// 
  /// This class stores properties, each property being a string associated to a key
  /// (the key being also a string)
  /// The whole content can be exported as a single string (xml or json formatted).
  /// This string can be imported to retrieve all properties.
  /// </summary>
  public class SerializableProperties
  {
    #region Members
    readonly IDictionary<string, object> m_properties = new Dictionary<string, object> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (SerializableProperties).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public SerializableProperties ()
    {
    }
    #endregion // Constructors

    #region Load / Save methods
    /// <summary>
    /// Create a text from all properties that are stored
    /// </summary>
    /// <returns></returns>
    public string GetTextToSave ()
    {
      return GetTextToSaveFromXml ();
    }

    /// <summary>
    /// The XML format is deprecated
    /// </summary>
    /// <returns></returns>
    string GetTextToSaveFromXml ()
    {
      var xmlDoc = new XmlDocument ();

      // Xml declaration
      XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration ("1.0", "UTF-8", null);
      XmlElement root = xmlDoc.DocumentElement;
      xmlDoc.InsertBefore (xmlDeclaration, root);

      // "properties"
      var element1 = xmlDoc.CreateElement (string.Empty, "properties", string.Empty);
      xmlDoc.AppendChild (element1);

      // A node "property" for each property
      foreach (var key in m_properties.Keys) {
        string val = m_properties[key].ToString ();

        // Node
        XmlElement element2 = xmlDoc.CreateElement (string.Empty, "property", string.Empty);
        element1.AppendChild (element2);

        // Key
        XmlElement element3 = xmlDoc.CreateElement (string.Empty, "key", string.Empty);
        element3.AppendChild (xmlDoc.CreateTextNode (key));
        element2.AppendChild (element3);

        // Value
        XmlElement element4 = xmlDoc.CreateElement (string.Empty, "value", string.Empty);
        element4.AppendChild (xmlDoc.CreateTextNode (val));
        element2.AppendChild (element4);
      }

      return xmlDoc.OuterXml;
    }

    /// <summary>
    /// Load properties from text
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public bool LoadFromText (string text)
    {
      if (text.StartsWith ("<", StringComparison.InvariantCulture)) { // Consider it is an XML then
        return LoadFromXml (text);
      }
      else if (text.StartsWith ("{", StringComparison.InvariantCulture)) { // Consider it is a JSON then
        throw new NotImplementedException ();
      }
      else {
        log.Error ($"LoadFromText: unknown format in {text}");
        throw new InvalidOperationException ("Unknown format");
      }
    }

    bool LoadFromXml (string xml)
    {
      bool ok = true;
      var xmlDoc = new XmlDocument ();
      try {
        xmlDoc.LoadXml (xml);
        XmlNodeList nodes = xmlDoc.SelectNodes ("/properties/property");
        foreach (XmlNode node in nodes) {
          string key = node["key"].InnerText;
          string val = node["value"].InnerText;
          m_properties[key] = val;
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("Couldn't load properties from xml text. {0}", ex);
        ok = false;
      }

      return ok;
    }
    #endregion // Load / Save methods

    #region Other methods
    /// <summary>
    /// Return true if a key is defined
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsDefined (string key)
    {
      return m_properties.ContainsKey (key);
    }

    /// <summary>
    /// Return the value of a key
    /// If the key doesn't exist, return null
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetValue (string key)
    {
      return IsDefined (key) ? m_properties[key].ToString () : null;
    }

    /// <summary>
    /// Clear all values
    /// </summary>
    public void Clear ()
    {
      m_properties.Clear ();
    }

    /// <summary>
    /// Set a value
    /// </summary>
    /// <param name="key">cannot be null or empty</param>
    /// <param name="value"></param>
    public void SetValue (string key, string value)
    {
      if (string.IsNullOrEmpty (key)) {
        throw new ArgumentNullException ("key");
      }

      m_properties[key] = value;
    }
    #endregion // Other methods
  }
}
