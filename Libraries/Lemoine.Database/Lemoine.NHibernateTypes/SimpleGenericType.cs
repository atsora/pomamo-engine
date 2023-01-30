// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.UserTypes;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Generic simple type converter from/to a .NET type to/from a SqlType
  /// </summary>
  [Serializable]
  public abstract class SimpleGenericType<T> : NHibernate.Type.NullableType, IUserType, NHibernate.Type.IType
  {
    // disable once StaticFieldInGenericType
    static readonly ILog log = LogManager.GetLogger (typeof (SimpleGenericType<T>).FullName);

    #region Members
    readonly bool m_mutable = false;
    readonly NpgsqlTypes.NpgsqlDbType m_dbType;
    #endregion // Members

    #region Constructor
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="sqlType"></param>
    /// <param name="mutable"></param>
    /// <param name="dbType"></param>
    protected SimpleGenericType (NHibernate.SqlTypes.SqlType sqlType, NpgsqlTypes.NpgsqlDbType dbType, bool mutable)
      : base (sqlType)
    {
      m_dbType = dbType;
      m_mutable = mutable;
    }
    #endregion // Constructor

    #region Abstract methods
    /// <summary>
    /// Convert from a database value
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected abstract T Get (object v);

    /// <summary>
    /// Convert to a database value
    /// </summary>
    /// <param name="v">not null</param>
    /// <returns></returns>
    protected abstract object Set (T v);

    /// <summary>
    /// Deep copy implementation
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected virtual T DeepCopyValue (T v)
    {
      return v;
    }
    #endregion// Abstract methods

    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public sealed override System.Type ReturnedClass
    {
      get { return typeof (T); }
    }

    /// <summary>
    /// <see cref="IUserType.SqlTypes" />
    /// </summary>
    public new NHibernate.SqlTypes.SqlType[] SqlTypes
    {
      get
      {
        return base.SqlTypes (null);
      }
    }

    /// <summary>
    /// <see cref="IUserType.ReturnedType" />
    /// </summary>
    public Type ReturnedType
    {
      get
      {
        return typeof (T);
      }
    }

    /// <summary>
    /// <see cref="IUserType.IsMutable" />
    /// </summary>
    public override bool IsMutable
    {
      get
      {
        return m_mutable;
      }
    }

    /// <summary>
    /// <see cref="IUserType.GetHashCode" />
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public override int GetHashCode (object x)
    {
      return (x == null) ? typeof (T).GetHashCode () + 123 : base.GetHashCode (x);
    }

    /// <summary>
    /// To override if required (for example for dictionaries)
    /// and test if the object is dirty
    /// </summary>
    /// <param name="x">not null</param>
    /// <param name="y">not null</param>
    /// <returns></returns>
    public virtual bool TestEquality (T x, T y)
    {
      Debug.Assert (null != x);
      Debug.Assert (null != y);

      return x.Equals (y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public override bool IsEqual (object x, object y)
    {
      if (ReferenceEquals (x, y)) {
        return true;
      }

      if (x == null || y == null) {
        return false;
      }

      if ((x is T xt) && (y is T yt)) {
        return TestEquality (xt, yt);
      }
      else {
        return x.Equals (y);
      }
    }

    /// <summary>
    /// <see cref="IUserType.Equals" />
    /// 
    /// To override it required (for example for dictionaries)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public new bool Equals (object x, object y)
    {
      if (ReferenceEquals (x, y)) {
        return true;
      }

      if (x == null || y == null) {
        return false;
      }

      if ((x is T xt) && (y is T yt)) {
        return TestEquality (xt, yt);
      }
      else {
        return x.Equals (y);
      }
    }

    /// <summary>
    /// <see cref="IUserType.NullSafeGet" />
    /// </summary>
    /// <param name="rs"></param>
    /// <param name="name"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public sealed override object NullSafeGet (DbDataReader rs, string name, NHibernate.Engine.ISessionImplementor session)
    {
      if (rs == null) {
        return GetNullValue ();
      }

      if (null == rs[name]) {
        return GetNullValue ();
      }

      int index = rs.GetOrdinal (name);
      if (rs.IsDBNull (index)) {
        return GetNullValue ();
      }

      try {
        return Get (rs[name]);
      }
      catch (InvalidCastException ex) {
        string message = string.Format (@"Could not cast the value in field {0} of type {1} to the type {2}",
                                        name, rs[index].GetType (), GetType ().Name);
        log.Error ($"NullSafeGet: {message}", ex);
        throw new ADOException (message, ex);
      }
    }

    /// <summary>
    /// Return the object that corresponds to a null value in database
    /// 
    /// By default null is returned
    /// </summary>
    /// <returns></returns>
    protected virtual object GetNullValue ()
    {
      return null;
    }

    /// <summary>
    /// <see cref="IUserType.NullSafeSet" />
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="value"></param>
    /// <param name="index"></param>
    /// <param name="session"></param>
    public override void Set (DbCommand cmd, object value, int index, NHibernate.Engine.ISessionImplementor session)
    {
      Debug.Assert (null != value); // Managed in NullableType.cs

      Npgsql.NpgsqlParameter parameter =
        (Npgsql.NpgsqlParameter)cmd.Parameters[index];
      Set (parameter, (T)value);
    }

    /// <summary>
    /// Set a value into a NpgsqlParameter
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    public virtual void Set (Npgsql.NpgsqlParameter parameter, T value)
    {
      parameter.Value = Set (value);
      parameter.NpgsqlDbType = m_dbType;
    }

    /// <summary>
    /// <see cref="IUserType.DeepCopy" />
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public object DeepCopy (object value)
    {
      if (m_mutable) {
        if (null == value) {
          return null;
        }

        return DeepCopyValue ((T)value);
      }
      else { // !m_mutable
        return value;
      }
    }

    /// <summary>
    /// <see cref="IUserType.Replace" />
    /// </summary>
    /// <param name="original"></param>
    /// <param name="target"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public object Replace (object original, object target, object owner)
    {
      if (m_mutable) {
        return DeepCopy (original);
      }
      else { // !m_mutable
        return original;
      }
    }

    /// <summary>
    /// <see cref="IUserType.Assemble" />
    /// </summary>
    /// <param name="cached"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public object Assemble (object cached, object owner)
    {
      return DeepCopy (cached);
    }

    /// <summary>
    /// <see cref="IUserType.Disassemble" />
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public object Disassemble (object value)
    {
      return DeepCopy (value);
    }

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    /// <param name="rs"></param>
    /// <param name="index"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public sealed override object Get (DbDataReader rs, int index, NHibernate.Engine.ISessionImplementor session)
    {
      object result = Get (rs[index]);
      log.DebugFormat ("returning '{0}' as column: {1}", result, rs.GetName (index));
      return result;
    }

    /// <summary>
    /// AbstractType implementation
    /// </summary>
    /// <param name="rs"></param>
    /// <param name="name"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public sealed override object Get (DbDataReader rs, string name, NHibernate.Engine.ISessionImplementor session)
    {
      object result = Get (rs[name]);
      log.DebugFormat ("returning '{0}' as column: {1}", result, name);
      return result;
    }

    /// <summary>
    /// AbstractType implementation
    /// </summary>
    /// <param name="original"></param>
    /// <param name="current">target</param>
    /// <param name="session"></param>
    /// <param name="owner"></param>
    /// <param name="copiedAlready"></param>
    /// <returns></returns>
    public override object Replace (object original, object current, NHibernate.Engine.ISessionImplementor session, object owner,
                                   System.Collections.IDictionary copiedAlready)
    {
      if (m_mutable) {
        if (IsEqual (original, current, session.Factory)) {
          return original;
        }
        return DeepCopy (original, session.Factory);
      }
      else {
        return original;
      }
    }

    /// <summary>
    /// <see cref="NHibernate.Type.IType"/>
    /// </summary>
    /// <param name="original"></param>
    /// <param name="target"></param>
    /// <param name="session"></param>
    /// <param name="owner"></param>
    /// <param name="copiedAlready"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<object> ReplaceAsync (object original, object target, NHibernate.Engine.ISessionImplementor session, object owner,
                     IDictionary copiedAlready, CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested) {
        return Task.FromCanceled<object> (cancellationToken);
      }
      try {
        return Task.FromResult<object> (Replace (original, target, session, owner, copiedAlready));
      }
      catch (Exception ex) {
        return Task.FromException<object> (ex);
      }
    }

    /// <summary>
    /// AbstractType implementation
    /// </summary>
    /// <param name="val"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public override object DeepCopy (object val, NHibernate.Engine.ISessionFactoryImplementor factory)
    {
      return (val == null) ? null : DeepCopy (val);
    }
    #endregion // AbstractType implementation
  }
}
