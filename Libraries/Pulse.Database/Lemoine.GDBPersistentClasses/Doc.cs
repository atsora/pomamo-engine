// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table doc
  /// 
  /// <see cref="IDoc"/>
  /// </summary>
  public class Doc
    : IDoc
    , IVersionable
    , IEquatable<IDoc>
    , ISelectionable
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Doc).FullName);

    int m_id = 0;
    int m_version = 0;
    string m_path;

    /// <summary>
    /// <see cref="IDoc"/>
    /// </summary>
    public virtual int Id => m_id;

    /// <summary>
    /// <see cref="IDoc"/>
    /// </summary>
    public virtual int Version => m_version;

    /// <summary>
    /// <see cref="IDoc"/>
    /// </summary>
    public virtual string Path {
      get => m_path;
      set {
        if (string.IsNullOrEmpty (value)) {
          log.Fatal ("Path.set: the path is null or empty");
          throw new ArgumentNullException ("value");
        }
        m_path = value;
      }
    }

    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText => this.Path;

    /// <summary>
    /// Protected constructor with no arguments
    /// </summary>
    protected Doc ()
    {
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="path">not null or empty</param>
    public Doc (string path)
    {
      Debug.Assert (!string.IsNullOrEmpty (path));

      this.Path = path;
    }

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[Doc {this.Id} {this.Path}]";
      }
      else {
        return $"[Doc {this.Id}]";
      }
    }

    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IDoc other)
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
      IDoc other = obj as Doc;
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
          hashCode += 1000000009 * Path.GetHashCode ();
        }
        return hashCode;
      }
    }
  }
}
