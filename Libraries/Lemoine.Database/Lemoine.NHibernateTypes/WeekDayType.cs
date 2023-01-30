// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Get a Integer object from a column in WeekDay format in database
  /// type="Lemoine.NHibernateTypes.WeekDayType"
  /// </summary>
  [Serializable]
  public class WeekDayType: SimpleGenericType<WeekDay>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (WeekDayType).FullName);
    
    /// <summary>
    /// Constructor
    /// </summary>
    public WeekDayType ()
      : base (NHibernateUtil.Int32.SqlType, NpgsqlTypes.NpgsqlDbType.Integer, false)
    {
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override WeekDay Get (object v)
    {
      int intWeekDay;
      try {
        intWeekDay = (Int32)Convert.ToInt32(v);
      } catch (Exception ex) {
        throw new FormatException(
          string.Format("Input WeekDay '{0}' was not an correct format", v),
          ex);
      }
      
      if(intWeekDay > 127){
        return WeekDay.AllDays;
      }
      else if( intWeekDay < 0){
        return WeekDay.None;
      }
      else {
        return (WeekDay)v;
      }
      
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override object Set (WeekDay v)
    {
      return (Int32)Convert.ToInt32(v);
    }

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
    {
      get { return "WeekDay"; }
    }
    #endregion // AbstractType implementation
  }
}
