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
  /// Persistent class of table OperationRevision
  /// 
  /// Implements <see cref="IOperationRevision"/>
  /// </summary>
  [Serializable]
  public class OperationRevision : BaseData, IOperationRevision, IEquatable<IOperationRevision>, Lemoine.Collections.IDataWithId
  {
    #region Members
    IOperation m_operation;
    /* TODO: for later once the table exists
    DateTime m_dateTime = DateTime.UtcNow;
    */
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (OperationRevision).FullName);

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
    /// OperationRevision ID
    /// 
    /// Note: for the moment, same as Operation ID
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
    public virtual string Display => $"{this.Operation?.Display} at {this.DateTime.ToLocalTime ()}";

    /// <summary>
    /// <see cref="IOperationRevision"/>
    /// </summary>
    [XmlAttribute("DateTime")]
    public virtual DateTime DateTime => m_operation.CreationDateTime;

    /// <summary>
    /// <see cref="IOperationRevision"/>
    /// </summary>
    [XmlAttribute ("Number")]
    public virtual int? Number { get; set; } = null;

    /// <summary>
    /// <see cref="IOperationRevision"/>
    /// </summary>
    [XmlAttribute ("Description")]
    public virtual string Description { get; set; } = "";

    /// <summary>
    /// <see cref="IOperationRevision"/>
    /// </summary>
    public virtual IEnumerable<IOperationModel> OperationModels => new List<IOperationModel> { new OperationModel (this.Operation) };
    #endregion // Getters / Setters

    #region Contructors
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected OperationRevision ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operation">not null</param>
    internal protected OperationRevision (IOperation operation)
    {
      Debug.Assert (null != operation);

      m_operation = operation;
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IOperation> (ref m_operation);
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[OperationRevision {this.Id} Number={this.Number} DateTime={this.DateTime}]";
      }
      else {
        return $"[OperationRevision {this.Id}]";
      }
    }

    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IOperationRevision other)
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
      OperationRevision other = obj as OperationRevision;
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
