// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Globalization;
using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate.SqlTypes;
using System.Data.Common;
using NHibernate.Engine;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Use UpperBound&lt;DateTime&gt; as Object in SQL that is not truncated
  /// </summary>
  [Serializable]
  public sealed class UtcUpperBoundDateTimeFullType : SimpleGenericType<UpperBound<DateTime>>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (UtcUpperBoundDateTimeFullType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public UtcUpperBoundDateTimeFullType ()
      : base (new SqlType (DbType.DateTime), NpgsqlTypes.NpgsqlDbType.Timestamp, false)
    {
    }

    /// <summary>
    /// Get a null object (may not be null according to the type!)
    /// </summary>
    /// <returns></returns>
    protected override object GetNullValue ()
    {
      return GetNullDateTime ();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override UpperBound<DateTime> Get (object v)
    {
      try {
        if (v == null) { // Should not be the case
          return new UpperBound<DateTime> (null);
        }
        DateTime dbValue = Convert.ToDateTime (v);
        return GetFromDb (dbValue);
      }
      catch (Exception ex) {
        throw new FormatException (string.Format ("Input string '{0}' was not in the correct format.", v), ex);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override object Set (UpperBound<DateTime> v)
    {
      if (v.HasValue) {
        DateTime dateValue = v.Value;
        if (dateValue.Kind == DateTimeKind.Local) {
          dateValue = dateValue.ToUniversalTime ();
        }
        return DateTime.SpecifyKind (dateValue, DateTimeKind.Unspecified);
      }
      else {
        return DBNull.Value;
      }
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
      System.Data.IDataParameter parameter =
        (System.Data.IDataParameter)cmd.Parameters[index];

      if (value is DateTime) {
        DateTime dateValue = (DateTime)value;
        parameter.Value = Set ((UpperBound<DateTime>)dateValue);
      }
      else {
        parameter.Value = Set ((UpperBound<DateTime>)value);
      }
    }

    UpperBound<DateTime> GetNullDateTime ()
    {
      return new UpperBound<DateTime> (null);
    }

    UpperBound<DateTime> GetFromDb (DateTime dbValue)
    {
      if (dbValue.Kind.Equals (DateTimeKind.Unspecified)) {
        return new UpperBound<DateTime> (DateTime.SpecifyKind (dbValue, DateTimeKind.Utc));
      }
      else {
        return new UpperBound<DateTime> (dbValue.ToUniversalTime ());
      }
    }

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
    {
      get { return "UtcUpperBoundDateTimeFull"; }
    }
    #endregion // AbstractType implementation
  }
}
