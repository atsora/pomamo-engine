// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Use TimeSpan as String in SQL
  /// </summary>
  [Serializable]
  public class StringAsTimeSpanType: SimpleGenericType<TimeSpan>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (StringAsTimeSpanType).FullName);
    
    /// <summary>
    /// Constructor
    /// </summary>
    public StringAsTimeSpanType ()
      : base (NHibernateUtil.String.SqlType, NpgsqlTypes.NpgsqlDbType.Varchar, false)
    {
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override TimeSpan Get (object v)
    {
      return TimeSpan.Parse (v.ToString ());
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override object Set (TimeSpan v)
    {
      return v.ToString ();
    }

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
		{
			get { return "StringAsTimeSpan"; }
		}
		#endregion // AbstractType implementation
  }
}
