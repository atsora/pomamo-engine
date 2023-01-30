// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Xml.Serialization;

using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Translation
  /// </summary>
  [Serializable]
  public class Translation: BaseData, ITranslation
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_locale;
    string m_translationKey;
    string m_translationValue;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (Translation).FullName);

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected Translation ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="locale"></param>
    /// <param name="translationKey"></param>
    public Translation (string locale, string translationKey)
    {
      m_locale = locale;
      m_translationKey = translationKey;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// ID
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "TranslationKey"}; }
    }

    /// <summary>
    /// Locale
    /// 
    /// Use the empty string for the default translation
    /// </summary>
    [XmlAttribute("Locale")]
    public virtual string Locale {
      get { return m_locale; }
      set { m_locale = value; }
    }
    
    /// <summary>
    /// Translation key
    /// </summary>
    [XmlAttribute("TranslationKey")]
    public virtual string TranslationKey {
      get { return m_translationKey; }
      set { m_translationKey = value; }
    }
    
    /// <summary>
    /// Translation value
    /// </summary>
    [XmlAttribute("TranslationValue")]
    public virtual string TranslationValue {
      get { return m_translationValue; }
      set { m_translationValue = value; }
    }
    #endregion
    
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        if (Locale != null) {
          hashCode += 1000000007 * Locale.GetHashCode();
        }

        if (TranslationKey != null) {
          hashCode += 1000000009 * TranslationKey.GetHashCode();
        }
      }
      return hashCode;
    }
    
    /// <summary>
    /// <see cref="Object.Equals(object)" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      ITranslation other = obj as Translation;
      if (other == null) {
        return false;
      }

      return this.Locale == other.Locale
        && this.TranslationKey == other.TranslationKey;
    }
  }
  
  /// <summary>
  /// Translation display
  /// </summary>
  public class TranslationDisplay
  {
    #region Members
    string m_key;
    static readonly ICatalog m_catalog = new CachedCatalog (new Lemoine.ModelDAO.i18n.ModelDAOCatalog ());
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (TranslationDisplay).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key"></param>
    public TranslationDisplay(string key)
    {
      this.m_key = key;
    }
    
    /// <summary>
    /// Translation key
    /// </summary>
    public string Key {
      get { return m_key; }
    }
        
    /// <summary>
    /// Display that corresponds to the given key,
    /// taking automatically the right locale
    /// </summary>
    public string Display {
      get
      {
        return m_catalog.GetString (this.Key, LocaleSettings.CurrentCulture);
      }
    }
  }
}

