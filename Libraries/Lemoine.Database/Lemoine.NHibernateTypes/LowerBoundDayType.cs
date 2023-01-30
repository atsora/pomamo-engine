// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Globalization;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Use LowerBound&lt;DateTime&gt; as Object in SQL
  /// </summary>
  [Serializable]
  public class LowerBoundDayType: SimpleGenericType<LowerBound<DateTime>>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (LowerBoundDayType).FullName);
    
    /// <summary>
    /// Constructor
    /// </summary>
    public LowerBoundDayType ()
      : base (new SqlType (DbType.Date), NpgsqlTypes.NpgsqlDbType.Timestamp, false)
    {
    }
    
    /// <summary>
    /// Get a null object (may not be null according to the type!)
    /// </summary>
    /// <returns></returns>
    protected override object GetNullValue()
    {
      return new UpperBound<DateTime> (null);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override LowerBound<DateTime> Get (object v)
    {
      try {
        DateTime dbValue = Convert.ToDateTime(v);
        return new LowerBound<DateTime> (new DateTime(dbValue.Year, dbValue.Month, dbValue.Day));
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
    protected override object Set (LowerBound<DateTime> v)
    {
      if (v.HasValue) {
        DateTime dateValue = v.Value;
        return new DateTime(dateValue.Year, dateValue.Month, dateValue.Day);
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
    public override void Set(DbCommand cmd, object value, int index, ISessionImplementor session)
    {
      System.Data.IDataParameter parameter =
        (System.Data.IDataParameter)cmd.Parameters [index];
      
      if (value is DateTime) {
        DateTime dateValue = (DateTime) value;
        parameter.Value = new DateTime(dateValue.Year, dateValue.Month, dateValue.Day);
      }
      else {
        parameter.Value = Set ((LowerBound<DateTime>) value);
      }
    }

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
		{
			get { return "LowerBoundDay"; }
		}
		#endregion // AbstractType implementation
  }
}
