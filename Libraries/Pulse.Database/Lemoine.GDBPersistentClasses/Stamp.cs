// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Stamp
  /// 
  /// This new table is aimed at replacing with the Stamp Detection
  /// and Sequence tables the sfkstartend and sfkoperation tables.
  /// 
  /// This table makes the association between a stamp and:
  /// <item>an operation cycle</item>
  /// <item>a sequence</item>
  /// <item>an operation</item>
  /// <item>a component</item>
  /// </summary>
  [Serializable]
  public class Stamp: IStamp, Lemoine.Collections.IDataWithId
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IIsoFile m_isoFile;
    int? m_position;
    ISequence m_sequence;
    IOperation m_operation;
    IComponent m_component;
    bool m_operationCycleBegin = false;
    bool m_operationCycleEnd = false;
    bool m_isoFileEnd = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Stamp).FullName);

    #region Getters / Setters
    /// <summary>
    /// ID number found in the ISO file that identifies an operation cycle,
    /// a sequence, an operation or a component
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Reference to the ISO file where this identifier is found
    /// </summary>
    public virtual IIsoFile IsoFile {
      get { return m_isoFile; }
      set
      {
        if (object.Equals (m_isoFile, value)) {
          // Nothing to do
          return;
        }
        if (null != m_isoFile) {
          m_isoFile.Stamps.Remove (this);
        }
        m_isoFile = value;
        if (null != m_isoFile) {
          m_isoFile.Stamps.Add (this);
        }
      }
    }
    
    /// <summary>
    /// Position in the ISO file of the stamp
    /// </summary>
    public virtual Nullable<int> Position {
      get { return m_position; }
      set { m_position = value; }
    }
    
    /// <summary>
    /// Reference to the associated sequence
    /// </summary>
    public virtual ISequence Sequence {
      get { return m_sequence; }
      set
      {
        if (object.Equals (m_sequence, value)) {
          // Nothing to do
          return;
        }
        if ( (null != m_sequence)
            && (m_sequence is Sequence)) {
          (m_sequence as Sequence)
            .RemoveStampForInternalUse (this);
        }
        m_sequence = value;
        if ( (null != m_sequence)
            && (m_sequence is Sequence)) {
          (m_sequence as Sequence)
            .AddStampForInternalUse (this);
        }
      }
    }
    
    /// <summary>
    /// Reference to the associated operation
    /// </summary>
    public virtual IOperation Operation {
      get { return m_operation; }
      set
      {
        if (object.Equals (m_operation, value)) {
          // Nothing to do
          return;
        }
        if ( (null != m_operation)
            && (m_operation is Operation)) {
          (m_operation as Operation).RemoveStampForInternalUse (this);
        }
        m_operation = value;
        if ( (null != m_operation)
            && (m_operation is Operation)) {
          (m_operation as Operation).AddStampForInternalUse (this);
        }
      }
    }
    
    /// <summary>
    /// Reference to the associated component
    /// </summary>
    public virtual IComponent Component {
      get { return m_component; }
      set
      {
        if (object.Equals (m_component, value)) {
          // Nothing to do
          return;
        }
        if ( (null != m_component)
            && (m_component is Component)) {
          (m_component as Component)
            .RemoveStampForInternalUse (this);
        }
        m_component = value;
        if ( (null != m_component)
            && (m_component is Component)) {
          (m_component as Component)
            .AddStampForInternalUse (this);
        }
      }
    }
    
    /// <summary>
    /// Does this stamp refers to an operation cycle begin ?
    /// 
    /// Default is false
    /// </summary>
    public virtual bool OperationCycleBegin {
      get { return m_operationCycleBegin; }
      set { m_operationCycleBegin = value; }
    }
    
    /// <summary>
    /// Does this stamp refers to an operation cycle end ?
    /// 
    /// Default is false
    /// </summary>
    public virtual bool OperationCycleEnd {
      get { return m_operationCycleEnd; }
      set { m_operationCycleEnd = value; }
    }
    
    /// <summary>
    /// Does this stamp refers to an ISO File end ?
    /// 
    /// Default is false
    /// </summary>
    public virtual bool IsoFileEnd {
      get { return m_isoFileEnd; }
      set { m_isoFileEnd = value; }
    }
    #endregion // Getters / Setters
    
    #region Methods
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IStamp other)
    {
      return this.Equals ((object) other);
    }
    
    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this,obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      Stamp other = obj as Stamp;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }
    
    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode();
        }
        return hashCode;
      }
      else {
        return base.GetHashCode ();
      }
    }
    #endregion // Methods
  }
}
