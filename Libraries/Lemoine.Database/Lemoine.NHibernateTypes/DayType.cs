// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

using Lemoine.Core.Log;
using NHibernate.Engine;
using NHibernate.Type;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// NHibernate type to get a Pulse day object
  /// </summary>
  [Serializable]
  public class DayType : DateTimeType
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DayType).FullName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rs"></param>
    /// <param name="index"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public override object Get (DbDataReader rs, int index, ISessionImplementor session)
    {
      DateTime dbValue = Convert.ToDateTime (rs[index]);
      Debug.Assert (0 == dbValue.Hour);
      Debug.Assert (0 == dbValue.Minute);
      Debug.Assert (0 == dbValue.Second);
      return new DateTime (dbValue.Year, dbValue.Month, dbValue.Day,
                          0, 0, 0,
                          DateTimeKind.Unspecified);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="st"></param>
    /// <param name="value"></param>
    /// <param name="index"></param>
    /// <param name="session"></param>
    public override void Set (DbCommand st, object value, int index, ISessionImplementor session)
    {
      IDataParameter parm = st.Parameters[index] as IDataParameter;
      DateTime dateValue = (DateTime)value;
      parm.Value =
        new DateTime (dateValue.Year, dateValue.Month, dateValue.Day,
                     0, 0, 0);
      parm.DbType = DbType.Date;
    }
  }
}
