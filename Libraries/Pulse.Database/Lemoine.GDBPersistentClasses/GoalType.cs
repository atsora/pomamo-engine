// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table goaltype
  /// </summary>
  [Serializable]
  public class GoalType: DataWithTranslation, IVersionable, IGoalType
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IUnit m_unit = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (GoalType).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected internal GoalType()
    {
    }
    
    /// <summary>
    /// Goaltype with default values
    /// </summary>
    /// <param name="name">Can't be null</param>
    public GoalType(string name)
    {
      if (string.IsNullOrEmpty (name)) {
        log.Error("Attempted to create a goal type with a null/empty value as name");
        throw new ArgumentNullException();
      }
      Name = name;
    }

    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="id"></param>
    /// <param name="translationKey"></param>
    internal GoalType (GoalTypeId id, string translationKey)
      : base (translationKey)
    {
      m_id = (int)id;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers {
      get { return new string[] {"Id", "Name", "TranslationKey"}; }
    }
    
    /// <summary>
    /// ID
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id {
      get { return m_id; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version {
      get { return m_version; }
    }
    
    /// <summary>
    /// Unit
    /// </summary>
    [XmlAttribute("Unit")]
    public virtual IUnit Unit {
      get { return m_unit; }
      set { m_unit = value; }
    }
    #endregion // Getters / Setters
  }
}
