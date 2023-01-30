// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate.SqlTypes;
using System.Diagnostics;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Use DateTimeRange as Object in SQL
  /// </summary>
  [Serializable]
  public abstract class GenericDateTimeRangeType<T>
    : SimpleGenericType<T>
    where T: Range<DateTime>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GenericDateTimeRangeType<T>).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public GenericDateTimeRangeType () : base (new SqlType (DbType.Object), NpgsqlTypes.NpgsqlDbType.Range | NpgsqlTypes.NpgsqlDbType.Timestamp, false)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override T Get (object v)
    {
      if (v is NpgsqlTypes.NpgsqlRange<System.DateTime>) {
        var r = (NpgsqlTypes.NpgsqlRange<System.DateTime>)v;
        LowerBound<DateTime> lower;
        if (r.LowerBoundInfinite) {
          lower = new LowerBound<DateTime> (null);
        }
        else {
          var dbValue = r.LowerBound;
          Debug.Assert (dbValue.Kind.Equals (DateTimeKind.Unspecified));
          lower = new LowerBound<DateTime> (GetBoundFromDb (dbValue));
        }
        UpperBound<DateTime> upper;
        if (r.UpperBoundInfinite) {
          upper = new UpperBound<DateTime> (null);
        }
        else {
          var dbValue = r.UpperBound;
          Debug.Assert (dbValue.Kind.Equals (DateTimeKind.Unspecified));
          upper = new UpperBound<DateTime> (GetBoundFromDb (dbValue));
        }
        return GetFromBounds (lower, upper, r.LowerBoundIsInclusive, r.UpperBoundIsInclusive);
      }

      try {
        return GetFromDbValueString (v.ToString ());
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
    protected override object Set (T v)
    {
      var lower = v.Lower.HasValue ? SetBoundToDb (v.Lower.Value) : DateTime.UtcNow;
      var upper = v.Upper.HasValue ? SetBoundToDb (v.Upper.Value) : DateTime.UtcNow;
      var lowerInfinite = !v.Lower.HasValue;
      var upperInfinite = !v.Upper.HasValue;
      return new NpgsqlTypes.NpgsqlRange<System.DateTime> (lower, v.LowerInclusive, lowerInfinite, upper, v.UpperInclusive, upperInfinite);
    }

    /// <summary>
    /// Convert a string to a type T
    /// 
    /// (Fallback in case a wrong database type is found)
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    protected abstract T GetFromDbValueString (string s);

    /// <summary>
    /// Convert a database date/time range bound to an internal date/time range bound
    /// </summary>
    /// <param name="dbBound"></param>
    /// <returns></returns>
    protected abstract DateTime GetBoundFromDb (DateTime dbBound);

    /// <summary>
    /// Create a date/time range from bounds
    /// </summary>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    /// <param name="lowerInclusive"></param>
    /// <param name="upperInclusive"></param>
    /// <returns></returns>
    protected abstract T GetFromBounds (LowerBound<DateTime> lower, UpperBound<DateTime> upper, bool lowerInclusive, bool upperInclusive);

    /// <summary>
    /// Convert a date/time range bound to a database date/time range bound
    /// </summary>
    /// <param name="bound"></param>
    /// <returns></returns>
    protected abstract DateTime SetBoundToDb (DateTime bound);
  }
}
