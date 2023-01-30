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
  /// Persistent class of table OperationModel
  /// 
  /// Implements <see cref="IOperationModel"/>
  /// </summary>
  [Serializable]
  public class OperationModel : BaseData, IOperationModel, IEquatable<IOperationModel>, Lemoine.Collections.IDataWithId
  {
    #region Members
    IOperation m_operation;
    IOperationRevision m_revision;
    /* TODO: for later once the table exists
    DateTime m_dateTime = DateTime.UtcNow;
    */
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (OperationModel).FullName);

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
    /// OperationModel ID
    /// 
    /// For the moment, same as Operation ID
    /// </summary>
    [XmlAttribute ("Id")]
    public virtual int Id => m_operation.Id;

    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version => m_operation.Version;

    /// <summary>
    /// <see cref="IOperationRevision"/>
    /// </summary>
    [XmlIgnore]
    public virtual IOperation Operation => m_operation;

    /// <summary>
    /// <see cref="IOperationRevision"/>
    /// </summary>
    [XmlIgnore]
    public virtual IOperationRevision Revision => m_revision;

    /// <summary>
    /// <see cref="IOperationRevision"/>
    /// </summary>
    [XmlIgnore]
    public virtual string Display => m_operation.Display;

    /// <summary>
    /// <see cref="IOperationModel"/>
    /// </summary>
    [XmlAttribute ("Description")]
    public virtual string Description { get; set; } = "";

    /// <summary>
    /// <see cref="IOperationModel"/>
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? ArchiveDateTime { get; set; } = null;

    /// <summary>
    /// <see cref="IOperationModel"/>
    /// </summary>
    [XmlAttribute ("Code")]
    public virtual string Code { get; set; } = "";

    /// <summary>
    /// <see cref="IOperationModel"/>
    /// </summary>
    [XmlAttribute ("Default")]
    public virtual bool Default { get; set; } = true;

    /// <summary>
    /// <see cref="IOperationModel"/>
    /// </summary>
    [XmlAttribute ("CadModelName")]
    public virtual string CadModelName { get; set; } = "";

    /// <summary>
    /// <see cref="IOperationModel"/>
    /// </summary>
    public virtual IEnumerable<ISequenceOperationModel> SequenceOperationModels =>
      this.Operation.Sequences
      .Select (x => new SequenceOperationModel (x.Operation.DefaultActiveModel, x, x.Order, x.Path?.Number));
    #endregion // Getters / Setters

    #region Contructors
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected OperationModel ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="revision">not null</param>
    internal protected OperationModel (IOperationRevision revision)
    {
      Debug.Assert (null != revision);

      m_operation = revision.Operation;
      m_revision = revision;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="revision">not null</param>
    internal protected OperationModel (IOperation operation)
    {
      Debug.Assert (null != operation);

      m_operation = operation;
      m_revision = operation.ActiveRevision;
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IOperation> (ref m_operation);
      NHibernateHelper.Unproxy<IOperationRevision> (ref m_revision);
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[OperationModel {this.Id} Description={this.Description}]";
      }
      else {
        return $"[OperationModel {this.Id}]";
      }
    }

    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IOperationModel other)
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
      OperationModel other = obj as OperationModel;
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
