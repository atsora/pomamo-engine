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
  /// Use LowerBound&lt;DateTime&gt; as Object in SQL
  /// </summary>
  [Serializable]
  [Obsolete ("Use UtcLowerBoundDateTimeSecondsType or UtcLowerBoundDateTimeFullType instead", false)]
  public sealed class UtcLowerBoundDateTimeType : SimpleGenericType<LowerBound<DateTime>>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (UtcLowerBoundDateTimeType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public UtcLowerBoundDateTimeType ()
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
    protected override LowerBound<DateTime> Get (object v)
    {
      try {
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
    protected override object Set (LowerBound<DateTime> v)
    {
      if (v.HasValue) {
        DateTime dateValue = v.Value;
        if (dateValue.Kind == DateTimeKind.Local) {
          dateValue = dateValue.ToUniversalTime ();
        }
        return new DateTime (dateValue.Year, dateValue.Month, dateValue.Day,
                            dateValue.Hour, dateValue.Minute, dateValue.Second);
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
        parameter.Value = Set ((LowerBound<DateTime>)(DateTime)value);
      }
      else {
        parameter.Value = Set ((LowerBound<DateTime>)value);
      }
    }

    LowerBound<DateTime> GetNullDateTime ()
    {
      return new LowerBound<DateTime> (null);
    }

    /// <summary>
    /// <see cref="SimpleGenericType{T}"/>
    /// </summary>
    /// <param name="dbValue"></param>
    /// <returns></returns>
    LowerBound<DateTime> GetFromDb (DateTime dbValue)
    {
      return new LowerBound<DateTime> (new DateTime (dbValue.Year, dbValue.Month, dbValue.Day,
                                                    dbValue.Hour, dbValue.Minute, dbValue.Second,
                                                    DateTimeKind.Utc));
    }

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
    {
      get { return "UtcLowerBoundDateTime"; }
    }
    #endregion // AbstractType implementation
  }
}
