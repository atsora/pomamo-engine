// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;

using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Abstract right class
  /// </summary>
  [Serializable,
   XmlInclude(typeof(MachineStateTemplateRight))]
  public abstract class Right: IRight, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IRole m_role;
    RightAccessPrivilege m_accessPrivilege = RightAccessPrivilege.Granted;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Right).FullName);

    #region Getters / Setters
    /// <summary>
    /// Right Id
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
      internal protected set { m_id = value; }
    }
    
    /// <summary>
    /// Right Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
      internal protected set { m_version = value; }
    }

    /// <summary>
    /// Associated role
    /// 
    /// null means it is applicable to all roles
    /// </summary>
    [XmlElement("Role")]
    public virtual IRole Role
    {
      get { return m_role; }
      internal protected set { m_role = value; }
    }
    
    /// <summary>
    /// Associated access privilege
    /// </summary>
    [XmlAttribute("AccessPrivilege")]
    public virtual RightAccessPrivilege AccessPrivilege
    {
      get { return m_accessPrivilege; }
      set { m_accessPrivilege = value; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    internal protected Right ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="role"></param>
    /// <param name="accessPrivilege"></param>
    protected Right (IRole role, RightAccessPrivilege accessPrivilege)
    {
      m_role = role;
      m_accessPrivilege = accessPrivilege;
    }
    #endregion // Constructors
    
    /// <summary>
    /// Implementation of Lemoine.Model.ISerializable
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IRole> (ref m_role);
    }
  }
}
