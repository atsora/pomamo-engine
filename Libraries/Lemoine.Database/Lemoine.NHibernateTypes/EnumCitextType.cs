// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using NHibernate.Type;
using System.Data;
using NHibernate.Engine;
using NHibernate.Dialect;
using System.Data.Common;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Like EnumStringType but for a citext column
  /// </summary>
  [Serializable]
  public class EnumCitextType : AbstractEnumType, IIdentifierType
  {
    Type m_enumClass;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enumClass"></param>
    protected EnumCitextType (System.Type enumClass) : base (new NHibernate.SqlTypes.SqlType (DbType.Object), enumClass)
    {
      m_enumClass = enumClass;
    }

    /// <summary>
    /// 
    /// </summary>
    public override string Name
    {
      get { return "enumcitext - " + ReturnedClass.Name; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cached"></param>
    /// <param name="session"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public override object Assemble (object cached, ISessionImplementor session, object owner)
    {
      if (cached == null) {
        return null;
      }
      else {
        return GetInstance (cached);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="session"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public override object Disassemble (object value, ISessionImplementor session, object owner)
    {
      return (value == null) ? null : GetValue (value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rs"></param>
    /// <param name="name"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public override object Get (DbDataReader rs, string name, ISessionImplementor session)
    {
      return Get (rs, rs.GetOrdinal (name), session);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rs"></param>
    /// <param name="index"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public override object Get (DbDataReader rs, int index, ISessionImplementor session)
    {
      object code = rs[index];
      if (code == DBNull.Value || code == null) {
        return null;
      }
      else {
        return GetInstance (code);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public virtual object GetInstance (object code)
    {
      //code is an named constants defined for the enumeration.
      try {
        return Enum.Parse (m_enumClass, code as string, true);
      }
      catch (ArgumentException ae) {
        throw new NHibernate.HibernateException (string.Format ("Can't Parse {0} as {1}", code, ReturnedClass.Name), ae);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public virtual object GetValue (object code)
    {
      return code == null ? string.Empty : Enum.Format (ReturnedClass, code, "G");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="dialect"></param>
    /// <returns></returns>
    public override string ObjectToSQLString (object value, Dialect dialect)
    {
      return GetValue (value).ToString ();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="value"></param>
    /// <param name="index"></param>
    /// <param name="session"></param>
    public override void Set (DbCommand cmd, object value, int index, ISessionImplementor session)
    {
      Npgsql.NpgsqlParameter parameter =
        (Npgsql.NpgsqlParameter)cmd.Parameters[index];
      parameter.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Citext;
      parameter.Value = value == null ? DBNull.Value : GetValue (value);
    }

    #region IIdentifierType Members
    /// <summary>
    /// 
    /// </summary>
    /// <param name="xml"></param>
    /// <returns></returns>
    public new object StringToObject (string xml)
    {
      return Enum.Parse (m_enumClass, xml, true);
    }

    #endregion
  }

  /// <summary>
  /// Convert an enum to a string in database
  /// 
  /// To use it in the mapping file:
  /// type="Lemoine.NHibernateTypes.GenericEnumCitextType`1[[Lemoine.GDBPersistentClasses.MyEnum, Pulse.Database]], Lemoine.NHibernateTypes"
  /// 
  /// I made a try but it did not work. Was it a typo error ?
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Serializable]
  public class EnumCitextType<T> : EnumCitextType
  {
    private readonly string m_typeName;

    /// <summary>
    /// Constructor
    /// </summary>
    public EnumCitextType ()
      : base (typeof (T))
    {
      System.Type type = GetType ();
      m_typeName = type.FullName + ", " + type.Assembly.GetName ().Name;
    }

    /// <summary>
    /// 
    /// </summary>
    public override string Name
    {
      get { return m_typeName; }
    }
  }
}
