// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.UserTypes;
using System.Data.Common;
using NHibernate.Engine;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Use the Lemoine.Model.TimePeriodOfDay as string in SQL
  /// </summary>
  [Serializable]
  public class TimePeriodOfDayNullIsFullDayType: SimpleGenericType<TimePeriodOfDay>, IUserType
  {
    static readonly ILog log = LogManager.GetLogger(typeof (TimePeriodOfDayNullIsFullDayType).FullName);
    
    /// <summary>
    /// Constructor
    /// </summary>
    public TimePeriodOfDayNullIsFullDayType ()
      : base(NHibernateUtil.String.SqlType, NpgsqlTypes.NpgsqlDbType.Varchar, false)
    {
    }
    
    /// <summary>
    /// Convert from a database value
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override TimePeriodOfDay Get (object v)
    {
      try
      {
        return new TimePeriodOfDay (v.ToString ());
      }
      catch (Exception ex) {
        throw new FormatException($"Input string '{v}' was not in the correct format.", ex);
      }
    }
    
    /// <summary>
    /// Convert to a database value
    /// </summary>
    /// <param name="v">not null</param>
    /// <returns></returns>
    protected override object Set (TimePeriodOfDay v)
    {
      if (v.IsFullDay()) {
        return DBNull.Value;
      }
      else {
        return v;
      }
    }
    
    /// <summary>
    /// Get a null object (may not be null according to the type!)
    /// </summary>
    /// <returns></returns>
    protected override object GetNullValue()
    {
      return new TimePeriodOfDay ();
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
      
      if (value == null) {
        parameter.Value = DBNull.Value;
      }
      else {
        TimePeriodOfDay timePeriodOfDay = (TimePeriodOfDay) value;
        if (timePeriodOfDay.IsFullDay ()) {
          parameter.Value = DBNull.Value;
        }
        else {
          if (false == timePeriodOfDay.IsValid ()) {
            log.FatalFormat ("NullSafeSet: " +
                             "TimePeriodOfDay {0} is not valid",
                             timePeriodOfDay);
            Debug.Assert (false);
            throw new InvalidOperationException ("Invalid TimePeriodOfDay");
          }
          else {
            parameter.Value = timePeriodOfDay.ToString ();
          }
        }
      }
    }

    /// <summary>
		/// AbstractType implementation
		/// </summary>
		public override string Name
		{
			get { return "TimePeriodOfDayNullIsFullDay"; }
		}
  }
}
