// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// It associates a given machining resource to a list of operation types it is able to do.
  /// 
  /// A preference value may be added. For example, if the preference is 1,
  /// this is the main operation type this machine was designed to do.
  /// If the preference is 2, this machine is able to do such an operation type,
  /// but there may be some machines that have a better performance for this type of operation.
  /// </summary>
  [Serializable]
  public class MachineOperationType: IMachineOperationType
  {
    #region Members
    IMachine m_machine;
    IOperationType m_operationType;
    int m_preference = 2;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineOperationType).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected MachineOperationType ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="operationType"></param>
    public MachineOperationType (IMachine machine,
                                 IOperationType operationType)
    {
      m_machine = machine;
      m_operationType = operationType;
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="operationType"></param>
    /// <param name="preference"></param>
    public MachineOperationType (IMachine machine,
                                 IOperationType operationType,
                                 int preference)
    {
      m_machine = machine;
      m_operationType = operationType;
      m_preference = preference;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Machine to associate to an operation type
    /// </summary>
    public virtual IMachine Machine {
      get { return m_machine; }
    }
    
    /// <summary>
    /// Operation type to associate to a machine
    /// </summary>
    public virtual IOperationType OperationType {
      get { return m_operationType; }
    }
    
    /// <summary>
    /// Operation type preference
    /// 
    /// If the preference is 1, this is the main operation type
    /// this machine was designed to do.
    /// If the preference is 2, this machine is able to do
    /// such an operation type, but there may be some machines
    /// that have a better performance for this type of operation.
    /// </summary>
    public virtual int Preference {
      get { return m_preference; }
      set { m_preference = value; }
    }
    #endregion // Getters / Setters
    
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        if (this.Machine != null) {
          hashCode += 1000000007 * this.Machine.GetHashCode();
        }

        if (this.OperationType != null) {
          hashCode += 1000000009 * this.OperationType.GetHashCode();
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
      IMachineOperationType other = obj as MachineOperationType;
      if (other == null) {
        return false;
      }

      return object.Equals(this.Machine, other.Machine)
        && object.Equals(this.OperationType, other.OperationType);
    }
    
  }
}
