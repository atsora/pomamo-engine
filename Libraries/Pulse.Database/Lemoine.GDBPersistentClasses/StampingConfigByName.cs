// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table StampingConfigByName
  /// </summary>
  public class StampingConfigByName
    : IStampingConfigByName
    , IVersionable
    , IEquatable<IStampingConfigByName>
    , ISelectionable
  {
    static readonly ILog log = LogManager.GetLogger (typeof (StampingConfigByName).FullName);

    #region Members
    int m_id = 0;
    int m_version = 0;
    #endregion // Members

    /// <summary>
    /// <see cref="IStampingConfigByName"/>
    /// </summary>
    public virtual int Id => m_id;

    /// <summary>
    /// <see cref="IStampingConfigByName"/>
    /// </summary>
    public virtual int Version => m_version;


    /// <summary>
    /// <see cref="IStampingConfigByName"/>
    /// </summary>
    public virtual string Name { get; set; }


    /// <summary>
    /// <see cref="IStampingConfigByName"/>
    /// </summary>
    public virtual StampingConfig Config { get; set; }

    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText => this.Name;

    /// <summary>
    /// Protected constructor with no arguments
    /// </summary>
    protected StampingConfigByName ()
    {
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="name">not null or empty</param>
    public StampingConfigByName (string name)
    {
      Debug.Assert (!string.IsNullOrEmpty (name));

      Name = name;
    }

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[StampingConfigByName {this.Id} {this.Name}]";
      }
      else {
        return $"[StampingConfigByName {this.Id}]";
      }
    }

    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IStampingConfigByName other)
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
      if (obj is null) {
        return false;
      }

      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      IStampingConfigByName other = obj as StampingConfigByName;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return this.Id.Equals (other.Id)
          && this.Version.Equals (other.Version);
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
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * base.GetHashCode ();
          hashCode += 1000000009 * Name.ToLowerInvariant ().GetHashCode ();
        }
        return hashCode;
      }
    }
  }
}
