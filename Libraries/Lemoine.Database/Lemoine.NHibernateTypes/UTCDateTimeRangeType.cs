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
  public sealed class UTCDateTimeRangeType: GenericDateTimeRangeType<UtcDateTimeRange>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UTCDateTimeRangeType).FullName);
    
    /// <summary>
    /// Constructor
    /// </summary>
    public UTCDateTimeRangeType ()
    {
    }
    
    #region AbstractType implementation
    /// <summary>
    /// <see cref="GenericDateTimeRangeType{T}"/>
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    protected override UtcDateTimeRange GetFromDbValueString (string s)
    {
      return new UtcDateTimeRange (s);
    }

    /// <summary>
    /// <see cref="GenericDateTimeRangeType{T}"/>
    /// </summary>
    /// <param name="dbBound"></param>
    /// <returns></returns>
    protected override DateTime GetBoundFromDb (DateTime dbBound)
    {
      if (dbBound.Kind.Equals (DateTimeKind.Unspecified)) {
        return DateTime.SpecifyKind (dbBound, DateTimeKind.Utc);
      }
      else {
        return dbBound;
      }
    }

    /// <summary>
    /// <see cref="GenericDateTimeRangeType{T}"/>
    /// </summary>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    /// <param name="lowerInclusive"></param>
    /// <param name="upperInclusive"></param>
    /// <returns></returns>
    protected override UtcDateTimeRange GetFromBounds (LowerBound<DateTime> lower, UpperBound<DateTime> upper, bool lowerInclusive, bool upperInclusive)
    {
      return new UtcDateTimeRange (lower, upper, lowerInclusive, upperInclusive);
    }

    /// <summary>
    /// <see cref="GenericDateTimeRangeType{T}"/>
    /// </summary>
    /// <param name="bound"></param>
    /// <returns></returns>
    protected override DateTime SetBoundToDb (DateTime bound)
    {
      return bound;
    }

    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
		{
			get { return "UtcDateTimeRange"; }
		}
		#endregion // AbstractType implementation
  }
}
