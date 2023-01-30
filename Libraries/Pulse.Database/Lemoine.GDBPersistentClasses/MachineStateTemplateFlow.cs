// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Database.Persistent;
using Lemoine.Model;

using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineStateTemplateFlow
  /// </summary>
  public class MachineStateTemplateFlow: IMachineStateTemplateFlow, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachineStateTemplate m_from;
    IMachineStateTemplate m_to;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateFlow).FullName);

    #region Getters / Setters
    /// <summary>
    /// MachineStateTemplateFlow Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// MachineStateTemplateFlow Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }
    
    /// <summary>
    /// Possible transition from this machine state template
    /// 
    /// Not null
    /// </summary>
    public virtual IMachineStateTemplate From
    {
      get { return m_from; }
      set
      {
        if (null == value) {
          log.ErrorFormat ("From.set: " +
                           "null value");
          throw new ArgumentNullException ("MachineStateTemplateFlow.From");
        }
        m_from = value;
      }
    }

    /// <summary>
    /// Possible transition to this machine state template
    /// 
    /// Not null
    /// </summary>
    public virtual IMachineStateTemplate To
    {
      get { return m_to; }
      set
      {
        if (null == value) {
          log.ErrorFormat ("To.set: " +
                           "null value");
          throw new ArgumentNullException ("MachineStateTemplateFlow.To");
        }
        m_to = value;
      }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected MachineStateTemplateFlow ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="from">not null</param>
    /// <param name="to">not null</param>
    internal protected MachineStateTemplateFlow (IMachineStateTemplate from, IMachineStateTemplate to)
    {
      this.From = from;
      this.To = to;
    }
    #endregion // Constructors
    
    #region Members
    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[MachineStateTemplateFlow {this.Id} {this.From?.ToStringIfInitialized ()}=>{this.To?.ToStringIfInitialized ()}]";
      }
      else {
        return $"[MachineStateTemplateFlow {this.Id}]";
      }
    }
    #endregion // Members

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IMachineStateTemplate> (ref m_from);
      NHibernateHelper.Unproxy<IMachineStateTemplate> (ref m_to);
    }
  }
}
