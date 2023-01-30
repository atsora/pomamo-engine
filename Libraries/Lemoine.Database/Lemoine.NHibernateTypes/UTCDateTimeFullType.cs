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
using System.Diagnostics;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// NHibernate type to get a UTC DateTime object that is not truncated
  /// </summary>
  [Serializable]
  public sealed class UTCDateTimeFullType : GenericDateTimeType
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
      Debug.Assert (dbValue.Kind.Equals (DateTimeKind.Unspecified));
      return DateTime.SpecifyKind (dbValue, DateTimeKind.Utc);
    }

    /// <summary>
    /// <see cref="GenericDateTimeType"/>
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    protected override DateTime SetToDb (DateTime dateTime)
    {
      DateTime dateValue = dateTime;
      if (dateValue.Kind == DateTimeKind.Local) {
        dateValue = dateValue.ToUniversalTime ();
      }
      return DateTime.SpecifyKind (dateValue, DateTimeKind.Unspecified);
    }

    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
    {
      get { return "UTCDateTimeFull"; }
    }
    #endregion // AbstractType implementation
  }
}
