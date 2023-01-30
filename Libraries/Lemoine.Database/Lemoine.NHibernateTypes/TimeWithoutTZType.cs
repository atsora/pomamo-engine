// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Description of TimeWithoutTZType.
  /// </summary>
  [Serializable]
  public class TimeWithoutTZType: SimpleGenericType<TimeSpan>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (TimeWithoutTZType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public TimeWithoutTZType ()
      : base (NHibernateUtil.Time.SqlType, NpgsqlTypes.NpgsqlDbType.Time, false)
    {
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override TimeSpan Get (object v)
    {
      try
      {
        if (v is TimeSpan) { //For those dialects where DbType.Time means TimeSpan.
          return (TimeSpan) v;
        }
        else {
          DateTime dbValue = Convert.ToDateTime (v);
          DateTime utcTime = new DateTime(1753, 01, 01, dbValue.Hour, dbValue.Minute, dbValue.Second, DateTimeKind.Utc);
          return utcTime.TimeOfDay;
        }
      }
      catch (Exception ex) {
        throw new FormatException(string.Format("Input string '{0}' was not in the correct format.", v), ex);
      }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override object Set (TimeSpan v)
    {
      return v;
    }

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
		{
			get { return "TimeWithoutTZ"; }
		}
		#endregion // AbstractType implementation
  }
}
