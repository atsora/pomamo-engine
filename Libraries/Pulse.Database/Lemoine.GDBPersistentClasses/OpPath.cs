// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Collections;

using Lemoine.Core.Log;
using System.Diagnostics;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Path
  /// </summary>
  [Serializable]
  public class OpPath: DataWithDisplayFunction, IPath, Lemoine.Collections.IDataWithId
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    int m_number = 1;
    IOperation m_operation;
    ICollection<ISequence> m_sequences = new InitialNullIdSortedSet<ISequence, int>();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (OpPath).FullName);

    #region Getters / Setters
    /// <summary>
    /// Path Id
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Path Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Path number
    /// </summary>
    [XmlAttribute("Number")]
    public virtual int Number
    {
      get { return this.m_number; }
      set { this.m_number = value; }
    }
    
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "Number"}; }
    }

    /// <summary>
    /// Parent operation
    /// </summary>
    [XmlIgnore]
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
          (m_operation as Operation).RemovePathForInternalUse (this);
        }
        m_operation = value;
        foreach (ISequence sequence in this.Sequences) {
          sequence.Operation = value;
        }
        if ( (null != m_operation)
            && (m_operation is Operation)) {
          (m_operation as Operation).AddPathForInternalUse (this);
        }
      }
    }

    /// <summary>
    /// Parent operation for Xml Serialization
    /// </summary>
    [XmlElement("Operation")]
    public virtual Operation XmlSerializationOperation {
      get { return this.Operation as Operation; }
      set { this.Operation = value; }
    }
    
    /// <summary>
    /// Set of sequences that are associated to this path
    /// </summary>
    // do not merge automatically (collection implemented as a sorted set on sequence order
    // and there are issues if inserting a sequence with an already existing key)
    //[XmlIgnore, MergeChildren("Operation")]
    [XmlIgnore]
    public virtual ICollection<ISequence> Sequences {
      get
      {
        return m_sequences;
      }
    }

    #endregion // Getters / Setters

    /// <summary>
    /// Default constructor
    /// </summary>
    public OpPath ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operation">not null</param>
    public OpPath (IOperation operation)
    {
      Debug.Assert (null != operation);

      this.Operation = operation;
    }

    #region add/remove methods
    /// <summary>
    /// Remove a sequence
    /// </summary>
    /// <param name="sequence"></param>
    public virtual void RemoveSequence (ISequence sequence)
    {
      if (!this.Sequences.Contains (sequence)) {
        log.WarnFormat ("RemoveSequence: " +
                        "Sequence {0} is not in {1}",
                        sequence, this);
      }
      else {
        sequence.Operation = null; // important side-effect here
        RemoveSequenceForInternalUse(sequence);
      }
    }

    #endregion // add/remove methods

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IOperation> (ref m_operation);
    }

    #region Methods
    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[Path {this.Id} Number={this.Number}]";
      }
      else {
        return $"[Path {this.Id}]";
      }
    }

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IPath other)
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
      IPath other = obj as OpPath;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (((Lemoine.Collections.IDataWithId)other).Id == this.Id);
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
    
    /// <summary>
    /// Comparison of paths based on their order
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual int CompareTo(object obj) {
      if (object.ReferenceEquals(this, obj)) {
        return 0;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      if (obj == null) {
        throw new ArgumentException("Comparison of a path with a null");
      }
      IPath other = obj as OpPath;
      if (null == other) {
        throw new ArgumentException ("Comparison of a path with another type");
      }
      return this.Number.CompareTo(other.Number);
    }
    
    /// <summary>
    /// Add a sequence in the member directly
    /// 
    /// To be used by the Sequence class only
    /// </summary>
    /// <param name="sequence"></param>
    protected internal virtual void AddSequenceForInternalUse (ISequence sequence)
    {
      AddToProxyCollection<ISequence> (m_sequences, sequence);
    }

    /// <summary>
    /// Remove a sequence in the member directly
    /// 
    /// To be used by the Sequence class only
    /// </summary>
    /// <param name="sequence"></param>
    protected internal virtual void RemoveSequenceForInternalUse (ISequence sequence)
    {
      RemoveFromProxyCollection<ISequence> (m_sequences, sequence);
    }

    #endregion // Methods
    
  }
}
