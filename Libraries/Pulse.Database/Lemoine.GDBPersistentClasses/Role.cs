// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Model;

using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Role
  /// </summary>
  [Serializable]
  public class Role: DataWithTranslation, IRole, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_webAppKey = "";
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Role).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "Name", "TranslationKey"}; }
    }

    /// <summary>
    /// Role Id
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
      set { m_id = value; } // For XML serialization only
    }
    
    /// <summary>
    /// Role Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText {
      get
      {
        string selectionText =
          string.Format ("{0}: {1}",
                         this.Id,
                         this.NameOrTranslation);
        log.DebugFormat ("SelectionText: " +
                         "selection text is {0}",
                         selectionText);
        return selectionText;
      }
    }


    /// <summary>
    /// Key used to recognize the role in the webapp
    /// </summary>
    [XmlAttribute ("WebAppKey")]
    public virtual string WebAppKey
    {
      get { return m_webAppKey; }
      set { m_webAppKey = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    internal protected Role ()
    { }
    
    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="id"></param>
    /// <param name="translationKey"></param>
    internal protected Role (int id, string translationKey)
    {
      m_id = id;
      this.TranslationKey = translationKey;
    }
    #endregion // Constructors
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      // Nothing to do here for the moment
    }
    
    #region Equals and GetHashCode implementation
    /// <summary>
    /// <see cref="Object.Equals(object)" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      Role other = obj as Role;
      if (other == null) {
        return false;
      }

      return this.Id == other.Id && this.Version == other.Version;
    }
    
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * Id.GetHashCode();
        hashCode += 1000000009 * Version.GetHashCode();

        if (m_webAppKey != null) {
          hashCode += 1000000097 * m_webAppKey.GetHashCode ();
        }
      }
      return hashCode;
    }
    #endregion

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[Role {this.Id} {this.TranslationKey ?? this.Name}]";
      }
      else {
        return $"[Role {this.Id}]";
      }
    }
  }
}
