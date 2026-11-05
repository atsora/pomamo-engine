// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table dockind
  ///
  /// <see cref="IDocKind"/>
  /// </summary>
  public class DocKind
    : DataWithDisplayFunction
    , IDocKind
    , IEquatable<IDocKind>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DocKind).FullName);

    int m_id = 0;
    int m_version = 0;

    /// <summary>
    /// Possible identifiers
    /// </summary>
    public override string[] Identifiers => new string[] { "Id", "Name", "TranslationKey" };

    /// <summary>
    /// <see cref="IDocKind"/>
    /// </summary>
    public virtual int Id => m_id;

    /// <summary>
    /// <see cref="IDocKind"/>
    /// </summary>
    public virtual int Version => m_version;

    /// <summary>
    /// <see cref="IDocKind"/>
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// <see cref="IDocKind"/>
    /// </summary>
    public virtual string TranslationKey { get; set; }

    /// <summary>
    /// <see cref="IDocKind"/>
    /// </summary>
    public virtual string Icon { get; set; }

    /// <summary>
    /// Protected constructor with no arguments, for NHibernate
    /// </summary>
    protected DocKind ()
    {
    }

    /// <summary>
    /// <see cref="Object.ToString()"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[DocKind {this.Id} {this.TranslationKey ?? this.Name}]";
      }
      else {
        return $"[DocKind {this.Id}]";
      }
    }

    /// <summary>
    /// <see cref="IEquatable{T}"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool Equals (IDocKind other) => this.Equals ((object)other);

    /// <summary>
    /// <see cref="Object.Equals(object)"/>
    ///
    /// Note: a DocKind may be compared with a DocKind proxy
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }
      if (obj is IDocKind other) {
        return (0 != this.Id) && this.Id.Equals (other.Id);
      }
      return false;
    }

    /// <summary>
    /// <see cref="Object.GetHashCode()"/>
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode () => (0 != this.Id)
      ? 1000000007 * this.Id.GetHashCode ()
      : base.GetHashCode ();
  }
}
