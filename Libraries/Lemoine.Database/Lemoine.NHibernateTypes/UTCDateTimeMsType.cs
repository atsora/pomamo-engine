// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Globalization;
using Lemoine.Model;
using NHibernate.SqlTypes;
using Lemoine.Core.Log;
using System.Data.Common;
using NHibernate.Engine;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// NHibernate type to get a UTC DateTime object that is truncated to the millisecond
  /// </summary>
  [Serializable]
  public sealed class UTCDateTimeMsType : GenericDateTimeType
  {
    static readonly ILog log = LogManager.GetLogger (typeof (UTCDateTimeMsType).FullName);

    #region AbstractType implementation
    /// <summary>
    /// <see cref="GenericDateTimeType"/>
    /// </summary>
    /// <param name="dbValue"></param>
    /// <returns></returns>
    protected override DateTime GetFromDb (DateTime dbValue)
    {
      return new DateTime (dbValue.Year, dbValue.Month, dbValue.Day,
                          dbValue.Hour, dbValue.Minute, dbValue.Second,
                          dbValue.Millisecond,
                          DateTimeKind.Utc);
    }

    /// <summary>
    /// <see cref="GenericDateTimeType"/>
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    protected override DateTime SetToDb (DateTime dateTime)
    {
      var dateValue = dateTime;
      if (dateValue.Kind == DateTimeKind.Local) {
        dateValue = dateValue.ToUniversalTime ();
      }
      return new DateTime (dateValue.Year, dateValue.Month, dateValue.Day,
                           dateValue.Hour, dateValue.Minute, dateValue.Second,
                           dateValue.Millisecond);
    }

    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
    {
      get { return "UTCDateTimeMs"; }
    }
    #endregion // AbstractType implementation
  }
}
