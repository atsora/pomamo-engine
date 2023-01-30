// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Globalization;
using Lemoine.Model;
using NHibernate.SqlTypes;
using Lemoine.Core.Log;
using System.Data.Common;
using NHibernate.Engine;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// NHibernate type to get a UTC DateTime object truncated to the second
  /// (so without any millisecond)
  /// </summary>
  [Serializable]
  public abstract class GenericDateTimeType : SimpleGenericType<DateTime>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GenericDateTimeType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public GenericDateTimeType ()
      : base (new SqlType (DbType.DateTime), NpgsqlTypes.NpgsqlDbType.Timestamp, false)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override DateTime Get (object v)
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
    protected override object Set (DateTime v)
    {
      return SetToDb (v);
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

      if (value is Bound<DateTime>) {
        Bound<DateTime> boundValue = (Bound<DateTime>)value;
        if (boundValue.HasValue) {
          parameter.Value = Set (boundValue.Value);
        }
        else {
          parameter.Value = DBNull.Value;
        }
      }
      else if (value is LowerBound<DateTime>) {
        LowerBound<DateTime> boundValue = (LowerBound<DateTime>)value;
        if (boundValue.HasValue) {
          parameter.Value = Set (boundValue.Value);
        }
        else {
          parameter.Value = DBNull.Value;
        }
      }
      else if (value is UpperBound<DateTime>) {
        UpperBound<DateTime> boundValue = (UpperBound<DateTime>)value;
        if (boundValue.HasValue) {
          parameter.Value = Set (boundValue.Value);
        }
        else {
          parameter.Value = DBNull.Value;
        }
      }
      else {
        parameter.Value = Set ((DateTime)value);
      }
    }

    #region AbstractType implementation
    /// <summary>
    /// Convert a database date/time value to an internal date/time value
    /// </summary>
    /// <param name="dbValue"></param>
    /// <returns></returns>
    protected abstract DateTime GetFromDb (DateTime dbValue);

    /// <summary>
    /// Convert a date/time to a database date/time value
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    protected abstract DateTime SetToDb (DateTime dateTime);
    #endregion // AbstractType implementation
  }
}
