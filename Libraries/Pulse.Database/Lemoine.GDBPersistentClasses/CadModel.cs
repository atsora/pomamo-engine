// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Collections;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table CADModel
  /// 
  /// This table lists the different CADModel names
  /// that were found in the ISO files and
  /// allows to keep to which component or operation
  /// a CAD Model name should point to.
  /// </summary>
  [Serializable]
  public class CadModel: BaseData, ICadModel
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    IComponent m_component;
    IOperation m_operation;
    ISet<ISequence> m_sequences = new InitialNullIdSet<ISequence, int>();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ICadModel).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "Name"}; }
    }

    /// <summary>
    /// CADModel ID
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
    /// CADModel name
    /// 
    /// Name as found in the ISO file
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }

    /// <summary>
    /// Reference to a Component in case the reference is known
    /// </summary>
    public virtual IComponent Component {
      get { return m_component; }
      set { m_component = value; }
    }
    
    /// <summary>
    /// Reference to an Operation in case the reference is known
    /// </summary>
    public virtual IOperation Operation {
      get { return m_operation; }
      set { m_operation = value; }
    }
    
    /// <summary>
    /// Set of sequences that reference this CAD Model
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<ISequence> Sequences {
      get
      {
        return m_sequences;
      }
    }
    #endregion // Getters / Setters
  }
}
