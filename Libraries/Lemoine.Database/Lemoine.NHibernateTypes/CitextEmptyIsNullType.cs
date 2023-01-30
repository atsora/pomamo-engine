// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Data.Common;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// NHibernate type for citext postgresql type.
  /// 
  /// In case the string is empty, null is stored in the database.
  /// 
  /// TODO: this does not work as an ID yet
  /// </summary>
  [Serializable]
  public class CitextEmptyIsNullType: IUserType
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CitextEmptyIsNullType).FullName);

    /// <summary>
    /// <see cref="IUserType.SqlTypes" />
    /// </summary>
    public SqlType[] SqlTypes {
      get {
        return new SqlType [1] {
          new SqlType (DbType.Object)
            // Note: NHibernateUtil.String.SqlType does not work
            // because it converts the data to varchar first
        };
      }
    }
    
    /// <summary>
    /// <see cref="IUserType.ReturnedType" />
    /// </summary>
    public Type ReturnedType {
      get {
        return typeof (String);
      }
    }
    
    /// <summary>
    /// <see cref="IUserType.IsMutable" />
    /// </summary>
    public bool IsMutable {
      get {
        return false;
      }
    }
    
    /// <summary>
    /// <see cref="IUserType.GetHashCode" />
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public int GetHashCode(object x)
    {
      if ( (null == x)
          || (string.IsNullOrEmpty (x as string))) {
        return typeof (String).GetHashCode () + 123;
      }
      else {
        return x.GetHashCode ();
      }
    }
    
    /// <summary>
    /// <see cref="IUserType.Equals" />
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public new bool Equals(object x, object y)
    {
      if (ReferenceEquals(x, y)) {
        return true;
      }

      if (string.IsNullOrEmpty (x as string)
          && string.IsNullOrEmpty (y as string)) {
        return true;
      }
      
      if (x == null || y == null) {
        return false;
      }

      string stringx = (string) x;
      string stringy = (string) y;
      return stringx.Equals(stringy, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// <see cref="IUserType.NullSafeGet" />
    /// </summary>
    /// <param name="rs"></param>
    /// <param name="names"></param>
    /// <param name="session"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
      return NullSafeGet (rs, names [0], owner);
    }
    
    /// <summary>
    /// <see cref="IUserType.NullSafeGet" />
    /// </summary>
    /// <param name="rs"></param>
    /// <param name="name"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public object NullSafeGet(DbDataReader rs, string name, object owner)
    {
      if (rs == null) {
        return null;
      }

      string result = (string) Convert.ToString (rs[name]);
      if (string.IsNullOrEmpty (result)) {
        return null;
      }
      else {
        return result;
      }
    }
    
    /// <summary>
    /// <see cref="IUserType.NullSafeSet" />
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="value"></param>
    /// <param name="index"></param>
    /// <param name="session"></param>
    public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
    {
      Npgsql.NpgsqlParameter parameter =
        (Npgsql.NpgsqlParameter)cmd.Parameters[index];
      parameter.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Citext;

      if (string.IsNullOrEmpty (value as string)) {
        parameter.Value = DBNull.Value;
      }
      else {
        parameter.Value = value;
      }
    }
    
    /// <summary>
    /// <see cref="IUserType.DeepCopy" />
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public object DeepCopy(object value)
    {
      return value;
    }
    
    /// <summary>
    /// <see cref="IUserType.Replace" />
    /// </summary>
    /// <param name="original"></param>
    /// <param name="target"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public object Replace(object original, object target, object owner)
    {
      return DeepCopy (original);
    }
    
    /// <summary>
    /// <see cref="IUserType.Assemble" />
    /// </summary>
    /// <param name="cached"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public object Assemble(object cached, object owner)
    {
      return DeepCopy (cached);
    }
    
    /// <summary>
    /// <see cref="IUserType.Disassemble" />
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public object Disassemble(object value)
    {
      return DeepCopy (value);
    }
  }
}
