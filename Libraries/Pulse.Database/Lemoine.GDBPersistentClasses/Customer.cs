// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Customer
  /// </summary>
  [Serializable]
  public class Customer : DataWithDisplayFunction, ICustomer, IMergeable<Customer>, IEquatable<ICustomer>, Lemoine.Collections.IDataWithId
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    string m_code;
    string m_externalCode;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (Customer).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id", "ExternalCode", "Code", "Name" }; }
    }

    /// <summary>
    /// Customer ID
    /// </summary>
    [XmlAttribute ("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Full name of the customer as used in the shop
    /// </summary>
    [XmlAttribute ("Name"), MergeAuto]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }

    /// <summary>
    /// Code given to the customer
    /// </summary>
    [XmlAttribute ("Code"), MergeAuto]
    public virtual string Code
    {
      get { return m_code; }
      set { m_code = value; }
    }

    /// <summary>
    /// External code
    /// 
    /// It may help synchronizing PUSLE data with an external database
    /// </summary>
    [XmlAttribute ("ExternalCode"), MergeAuto]
    public virtual string ExternalCode
    {
      get { return m_externalCode; }
      set { m_externalCode = value; }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Check if the customer is undefined
    /// 
    /// A customer is considered as undefined if it has no name or no code
    /// </summary>
    /// <returns></returns>
    public virtual bool IsUndefined ()
    {
      return string.IsNullOrEmpty (this.Name) && string.IsNullOrEmpty (this.Code);
    }

    #region IMergeable implementation
    /// <summary>
    /// <see cref="IMergeable&lt;T&gt;.Merge" />
    /// </summary>
    /// <param name="other"></param>
    /// <param name="conflictResolution"></param>
    public virtual void Merge (Customer other,
                              ConflictResolution conflictResolution)
    {
      Mergeable.MergeAuto (this, other, conflictResolution);
    }
    #endregion // IMergeable implementation

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[Customer {this.Id} Name={this.Name} Code={this.Code}]";
      }
      else {
        return $"[Customer {this.Id}]";
      }
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
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
      ICustomer other = obj as Customer;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (((Lemoine.Collections.IDataWithId)other).Id == ((Lemoine.Collections.IDataWithId)this).Id);
      }
      return false;
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="other">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public virtual bool Equals (ICustomer other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
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
