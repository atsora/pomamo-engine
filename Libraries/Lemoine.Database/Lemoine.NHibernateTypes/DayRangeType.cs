// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate.SqlTypes;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Use DateTimeRange as Object in SQL
  /// </summary>
  [Serializable]
  public class DayRangeType : SimpleGenericType<DayRange>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DayRangeType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DayRangeType ()
      : base (new SqlType (DbType.Object), NpgsqlTypes.NpgsqlDbType.Range | NpgsqlTypes.NpgsqlDbType.Date, false)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override DayRange Get (object v)
    {
      if (v is NpgsqlTypes.NpgsqlRange<System.DateTime>) {
        var r = (NpgsqlTypes.NpgsqlRange<System.DateTime>)v;
        var lower = r.LowerBoundInfinite
          ? new LowerBound<DateTime> (null)
          : new LowerBound<DateTime> (r.LowerBound);
        var upper = r.UpperBoundInfinite
          ? new UpperBound<DateTime> (null)
          : new UpperBound<DateTime> (r.UpperBound);
        return new DayRange (lower, upper, r.LowerBoundIsInclusive, r.UpperBoundIsInclusive);
      }

      try {
        return new DayRange (v.ToString ());
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
    protected override object Set (DayRange v)
    {
      var lower = v.Lower.HasValue ? v.Lower.Value : DateTime.UtcNow;
      var upper = v.Upper.HasValue ? v.Upper.Value : DateTime.UtcNow;
      var lowerInfinite = !v.Lower.HasValue;
      var upperInfinite = !v.Upper.HasValue;
      return new NpgsqlTypes.NpgsqlRange<System.DateTime> (lower, v.LowerInclusive, lowerInfinite, upper, v.UpperInclusive, upperInfinite);
    }

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
    {
      get { return "DayRange"; }
    }
    #endregion // AbstractType implementation
  }
}
