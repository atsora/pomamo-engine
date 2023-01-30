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
  /// Persistent class of table AutoMachineStateTemplate
  /// </summary>
  public class AutoMachineStateTemplate: IAutoMachineStateTemplate, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachineMode m_machineMode;
    IMachineStateTemplate m_current;
    IMachineStateTemplate m_new;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (AutoMachineStateTemplate).FullName);

    #region Getters / Setters
    /// <summary>
    /// AutoMachineStateTemplate Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// AutoMachineStateTemplate Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// New detected machine mode
    /// 
    /// Not null
    /// </summary>
    public virtual IMachineMode MachineMode {
      get { return m_machineMode; }
      set
      {
        if (null == value) {
          log.ErrorFormat ("MachineMode.set: " +
                           "null value");
          throw new ArgumentNullException ("AutoMachineStateTemplate.MachineMode");
        }
        m_machineMode = value;
      }
    }

    /// <summary>
    /// Current machine state template
    /// 
    /// If null, any current machine state template applies
    /// </summary>
    public virtual IMachineStateTemplate Current {
      get { return m_current; }
      set { m_current = value; }
    }
    
    /// <summary>
    /// New machine state template
    /// 
    /// Not null
    /// </summary>
    public virtual IMachineStateTemplate New
    {
      get { return m_new; }
      set
      {
        if (null == value) {
          log.ErrorFormat ("New.set: " +
                           "null value");
          throw new ArgumentNullException ("AutoMachineStateTemplate.New");
        }
        m_new = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected AutoMachineStateTemplate ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <param name="newMachineStateTemplate">not null</param>
    internal protected AutoMachineStateTemplate (IMachineMode machineMode, IMachineStateTemplate newMachineStateTemplate)
    {
      this.MachineMode = machineMode;
      this.New = newMachineStateTemplate;
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
        return $"[AutoMachineStateTemplate {this.Id} ({this.MachineMode?.ToStringIfInitialized ()},{this.Current?.ToStringIfInitialized ()})=>{this.New?.ToStringIfInitialized ()}]";
      }
      else {
        return $"[AutoMachineStateTemplate {this.Id}]";
      }
    }
    #endregion // Members
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IMachineMode> (ref m_machineMode);
      NHibernateHelper.Unproxy<IMachineStateTemplate> (ref m_current);
      NHibernateHelper.Unproxy<IMachineStateTemplate> (ref m_new);
    }
  }
}
