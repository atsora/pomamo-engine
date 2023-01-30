// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;
using System.Linq;

using Lemoine.Collections;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table SequenceOperationModel
  /// 
  /// Implements <see cref="ISequenceOperationModel"/>
  /// </summary>
  [Serializable]
  public class SequenceOperationModel : BaseData, ISequenceOperationModel, IEquatable<ISequenceOperationModel>, Lemoine.Collections.IDataWithId
  {
    #region Members
    IOperationModel m_operationModel;
    ISequence m_sequence;
    int? m_pathNumber;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (SequenceOperationModel).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id" }; }
    }

    /// <summary>
    /// ID
    /// 
    /// Note: for the moment, same as the Sequence Id
    /// </summary>
    [XmlAttribute ("Id")]
    public virtual int Id => m_sequence.Id;

    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version => m_sequence.Version;

    /// <summary>
    /// <see cref="ISequenceOperationModel"/>
    /// </summary>
    public virtual IOperationModel OperationModel => m_operationModel;

    /// <summary>
    /// <see cref="ISequenceOperationModel"/>
    /// </summary>
    public virtual ISequence Sequence => m_sequence;

    /// <summary>
    /// <see cref="ISequenceOperationModel"/>
    /// </summary>
    [XmlAttribute("PathNumber")]
    public virtual  int? PathNumber => m_pathNumber;

    /// <summary>
    /// <see cref="ISequenceOperationModel"/>
    /// </summary>
    [XmlAttribute ("Order")]
    public virtual double Order { get; set; }

    /// <summary>
    /// <see cref="ISequenceOperationModel"/>
    /// </summary>
    public virtual IEnumerable<ISequenceDuration> Durations =>
      this.Sequence.EstimatedTime.HasValue
      ? new List<ISequenceDuration> { new SequenceDuration (this, m_sequence.EstimatedTime.Value) }
      : new List<ISequenceDuration> { };
    #endregion // Getters / Setters

    #region Contructors
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected SequenceOperationModel ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operationModel">not null</param>
    /// <param name="sequence">not null</param>
    /// <param name="order"></param>
    /// <param name="pathNumber"></param>
    public SequenceOperationModel (IOperationModel operationModel, ISequence sequence, double order, int? pathNumber = null)
    {
      Debug.Assert (null != operationModel);
      Debug.Assert (null != sequence);

      m_operationModel = operationModel;
      m_sequence = sequence;
      this.Order = order;
      m_pathNumber = pathNumber;
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IOperationModel> (ref m_operationModel);
      // NHibernateHelper.Unproxy<ISequence> (ref m_sequence); // Sequence does not implement ISerializableModel
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[SequenceOperationModel {this.Id} OperationModel={this.OperationModel.Id} Sequence={this.Sequence.Id} Path={this.PathNumber}]";
      }
      else {
        return $"[SequenceOperationModel {this.Id}]";
      }
    }

    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (ISequenceOperationModel other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    /// Determines whether the specified Object
    /// is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      SequenceOperationModel other = obj as SequenceOperationModel;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }

    /// <summary>
    /// Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode ();
        }
        return hashCode;
      }
      else {
        return base.GetHashCode ();
      }
    }
  }
}
