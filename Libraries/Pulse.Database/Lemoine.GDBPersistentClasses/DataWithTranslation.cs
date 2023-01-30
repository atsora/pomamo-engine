// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;

using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Base class for the persistent classes having a translation key column.
  /// </summary>
  public abstract class DataWithTranslation: BaseData, IDisplayable
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DataWithTranslation).FullName);

    #region Members
    string m_name; // Can't be empty but can be null
    string m_translationKey; // Can't be empty but can be null
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Name", "TranslationKey"}; }
    }

    /// <summary>
    /// Name
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name {
      get { return m_name; }
      set { m_name = value; }
    }
    
    /// <summary>
    /// Translation key
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    [XmlAttribute("TranslationKey")]
    public virtual string TranslationKey {
      get { return m_translationKey; }
      set { m_translationKey = value; }
    }
    
    /// <summary>
    /// Display name that is deduced from the translation table
    /// </summary>
    [XmlIgnore]
    public virtual string Display {
      get
      {
        return this.NameOrTranslation;
      }
    }
    
    /// <summary>
    /// Display name that is deduced from the translation table for XML serialization
    /// </summary>
    [XmlAttribute("Display")]
    public virtual string XmlSerializationDisplay {
      get
      {
        return this.Display;
      }
      set
      {
        // For XML serialization only: do nothing
      }
    }
    
    /// <summary>
    /// Translated name of the object (if no name is set, else the name of the object)
    /// </summary>
    [XmlIgnore]
    public virtual string NameOrTranslation {
      get
      {
        return GetTranslationFromNameTranslationKey (this.Name,
                                                     this.TranslationKey);
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    protected DataWithTranslation ()
    {
    }
    
    /// <summary>
    /// Constructor with a translation key
    /// </summary>
    /// <param name="translationKey"></param>
    protected DataWithTranslation (string translationKey)
    {
      m_translationKey = translationKey;
    }
    #endregion // Constructors

    /// <summary>
    /// IDisplay implementation
    /// </summary>
    /// <param name="variant"></param>
    /// <returns></returns>
    public virtual string GetDisplay (string variant)
    {
      log.WarnFormat ("GetDisplay: " +
                      "No variant is supported for DataWithTranslation");
      return this.Display;
    }

    #region Methods
    /// <summary>
    /// Get the display from a name and a translation key fields
    /// 
    /// If the name is available, consider the name, else consider the translation key
    /// </summary>
    /// <param name="name"></param>
    /// <param name="translationKey"></param>
    /// <param name="optional"></param>
    /// <returns></returns>
    internal static string GetTranslationFromNameTranslationKey (string name,
                                                                 string translationKey,
                                                                 bool optional = false)
    {
      if (!string.IsNullOrEmpty (name)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ($"GetTranslationFromNameTranslationKey: name={name} is set, return it");
        }
        return name.Trim ();
      }
      else if (string.IsNullOrEmpty(translationKey)) {
        if (optional) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetTranslationFromNameTranslationKey: both name and translation key are not set");
          }
        }
        else {
          if (log.IsWarnEnabled) {
            log.WarnFormat ("GetTranslationFromNameTranslationKey: both name and translation key are not set. StackTrace=\n{0}", System.Environment.StackTrace);
          }
        }
        return name;
      }
      else { // name is null or empty and translationKey is not null or empty
        return Lemoine.I18N.PulseCatalog.GetString (translationKey); // From text file or database
      }
    }
    #endregion // Methods
  }
}
