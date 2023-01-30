// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Get a TimeSpan object from a column in seconds in database
  /// </summary>
  [Serializable]
  public class SecondsAsTimeSpanType : SimpleGenericType<TimeSpan?>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SecondsAsTimeSpanType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public SecondsAsTimeSpanType ()
      : base (NHibernateUtil.Double.SqlType, NpgsqlTypes.NpgsqlDbType.Double, false)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override TimeSpan? Get (object v)
    {
      if (v is null || (v == DBNull.Value)) {
        return null;
      }
      else {
        return TimeSpan.FromSeconds (Convert.ToDouble (v));
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override object Set (TimeSpan? v)
    {
      if (!v.HasValue) {
        return DBNull.Value;
      }
      else {
        return v.Value.TotalSeconds;
      }
    }

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
    {
      get { return "SecondsAsTimeSpan"; }
    }
    #endregion // AbstractType implementation
  }
}
