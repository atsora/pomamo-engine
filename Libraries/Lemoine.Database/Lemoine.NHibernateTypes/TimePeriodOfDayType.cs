// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Use the Lemoine.Model.TimePeriodOfDay as string in SQL
  /// </summary>
  [Serializable]
  public class TimePeriodOfDayType: SimpleGenericType<TimePeriodOfDay>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (TimePeriodOfDayType).FullName);
    
    /// <summary>
    /// Constructor
    /// </summary>
    public TimePeriodOfDayType ()
      : base (NHibernateUtil.String.SqlType, NpgsqlTypes.NpgsqlDbType.Varchar, false)
    {
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override TimePeriodOfDay Get (object v)
    {
      try {
        return new TimePeriodOfDay (v.ToString ());
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
    protected override object Set (TimePeriodOfDay v)
    {
      if (false == v.IsValid ()) {
        log.FatalFormat ("NullSafeSet: " +
                         "TimePeriodOfDay {0} is not valid",
                         v);
        Debug.Assert (false);
        throw new ArgumentException ("Invalid TimePeriodOfDay");
      }
      else {
        return v.ToString ();
      }
    }

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
		{
			get { return "TimePeriodOfDay"; }
		}
		#endregion // AbstractType implementation
  }
}
