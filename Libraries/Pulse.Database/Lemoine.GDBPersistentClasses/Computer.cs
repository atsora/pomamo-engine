// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Computer
  /// </summary>
  [Serializable]
  public class Computer : DataWithDisplayFunction, IComputer, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    string m_address;
    bool m_isLctr = false;
    bool m_isLpst = false;
    bool m_isCnc = false;
    bool m_isWeb = false;
    bool m_isAutoReason = false;
    bool m_isAlert = false;
    bool m_isSynchronization = false;
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (Computer).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id", "Name", "Address", "IsLctr" }; }
    }

    /// <summary>
    /// Computer ID
    /// </summary>
    [XmlIgnore]
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// Computer ID for XML serialization
    /// </summary>
    [XmlAttribute ("Id")]
    public virtual int XmlSerializationId
    {
      get { return this.Id; }
      set { m_id = value; }
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
    /// Computer name
    /// </summary>
    [XmlAttribute ("Name")]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }

    /// <summary>
    /// Computer address
    /// </summary>
    [XmlAttribute ("Address")]
    public virtual string Address
    {
      get { return m_address; }
      set { m_address = value; }
    }

    /// <summary>
    /// Is this computer the LCtr ?
    /// </summary>
    [XmlAttribute ("IsLctr")]
    public virtual bool IsLctr
    {
      get { return m_isLctr; }
      set { m_isLctr = value; }
    }

    /// <summary>
    /// Is this computer a LPst ?
    /// </summary>
    [XmlAttribute ("IsLpst")]
    public virtual bool IsLpst
    {
      get { return m_isLpst; }
      set { m_isLpst = value; }
    }

    /// <summary>
    /// Is this computer a Cnc ?
    /// </summary>
    [XmlAttribute ("IsCnc")]
    public virtual bool IsCnc
    {
      get { return m_isCnc; }
      set { m_isCnc = value; }
    }

    /// <summary>
    /// Is a web service running on this computer ?
    /// </summary>
    [XmlAttribute ("IsWeb")]
    public virtual bool IsWeb
    {
      get { return m_isWeb; }
      set { m_isWeb = value; }
    }

    /// <summary>
    /// Is this computer the auto-reason server ?
    /// </summary>
    [XmlAttribute ("IsAutoReason")]
    public virtual bool IsAutoReason
    {
      get { return m_isAutoReason; }
      set { m_isAutoReason = value; }
    }

    /// <summary>
    /// Is this computer the alert server ?
    /// </summary>
    [XmlAttribute ("IsAlert")]
    public virtual bool IsAlert
    {
      get { return m_isAlert; }
      set { m_isAlert = value; }
    }

    /// <summary>
    /// Is this computer the synchronization server ?
    /// </summary>
    [XmlAttribute ("IsSynchronization")]
    public virtual bool IsSynchronization
    {
      get { return m_isSynchronization; }
      set { m_isSynchronization = value; }
    }

    /// <summary>
    /// <see cref="IComputer.WebServiceUrl"/>
    /// </summary>
    [XmlAttribute ("WebServiceUrl")]
    public virtual string WebServiceUrl { get; set; }

    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText
    {
      get {
        return string.Format ("{0}: {1} ({2})",
                              this.Id, this.Name, this.Address);
      }
    }
    #endregion

    #region Equals and GetHashCode implementation
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="obj">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      var other = obj as Computer;
      if (other == null) {
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
    public override int GetHashCode ()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * Id.GetHashCode ();
      }
      return hashCode;
    }
    #endregion


    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      // Nothing to do here for the moment
    }
  }
}
