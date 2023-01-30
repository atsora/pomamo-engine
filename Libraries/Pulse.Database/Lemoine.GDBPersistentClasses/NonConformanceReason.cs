// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Model;

using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table NonConformanceReason
  /// </summary>
  [Serializable]
  public class NonConformanceReason:  DataWithDisplayFunction, INonConformanceReason, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_code;
    string m_name;
    string m_description;
    bool   m_detailsrequired;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (NonConformanceReason).FullName);
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    internal protected NonConformanceReason ()
    { }
    
    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="name">not null and not empty</param>
    internal protected NonConformanceReason (string name)
    {
      this.Name = name;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "Name", "Code"}; }
    }

    /// <summary>
    /// NonConformanceReason Id
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// NonConformanceReason Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// NonConformanceReason name
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }

    /// <summary>
    /// Code of the nonconformancereason
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return this.m_code; }
      set { this.m_code = value; }
    }

    /// <summary>
    /// Description of the nonconformancereason
    /// </summary>
    [XmlAttribute("Description")]
    public virtual string Description {
      get { return this.m_description; }
      set { this.m_description = value; }
    } 
    
    /// <summary>
    /// nonconformancereason details are required ?
    /// </summary>
    [XmlAttribute("DetailsRequired")]
    public virtual bool DetailsRequired {
      get { return this.m_detailsrequired; }
      set { this.m_detailsrequired = value; }
    }    
    
    #endregion // Getters / Setters
    
    #region Members
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(INonConformanceReason other)
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
      NonConformanceReason other = obj as NonConformanceReason;
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

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[NonConformanceReason {this.Id} {this.Name}]";
      }
      else {
        return $"[NonConformanceReason {this.Id}]";
      }
    }
    #endregion // Members
    
    #region methods
    
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      //Nothing to do here because there is no field to serialize
    }

    #endregion
  }
}
