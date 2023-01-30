// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Collections;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table UserMachineAssociationMachine
  /// </summary>
  [Serializable]
  public class UserMachineAssociationMachine: IUserMachineAssociationMachine, ISerializableModel, IDataWithId
  {
    #region Members
    int m_id = 0;
    IMachine m_machine;
    IMachineStateTemplate m_machineStateTemplate;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (UserMachineAssociationMachine).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected UserMachineAssociationMachine ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">Not null</param>
    /// <param name="machineStateTemplate">Not null</param>
    internal UserMachineAssociationMachine (IMachine machine,
                                            IMachineStateTemplate machineStateTemplate)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != machineStateTemplate);
      
      m_machine = machine;
      m_machineStateTemplate = machineStateTemplate;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Id
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// Machine that is associated to the user
    /// </summary>
    [XmlIgnore]
    public virtual IMachine Machine {
      get { return m_machine; }
    }

    /// <summary>
    /// Machine state template that is associated to the machine and the user
    /// </summary>
    [XmlIgnore]
    public virtual IMachineStateTemplate MachineStateTemplate {
      get { return m_machineStateTemplate; }
    }
    #endregion // Getters / Setters
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IMachine> (ref m_machine);
      NHibernateHelper.Unproxy<IMachineStateTemplate> (ref m_machineStateTemplate);
    }
    
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * this.Id.GetHashCode();
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
      UserMachineAssociationMachine other = obj as UserMachineAssociationMachine;
      if (other == null) {
        return false;
      }

      return object.Equals(this.Id, other.Id);
    }
  }
}
