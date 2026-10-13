// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: LGPL-2.1-only

using System;
using System.Data;
using System.Data.Common;

namespace NHibernate.Driver
{
  /// <summary>
  /// Npgsql driver that keeps the Npgsql 5 behavior for the date/time parameters
  /// </summary>
  /// <remarks>
  /// <para>
  /// In the database, all the date/times are stored in UTC in columns
  /// of type timestamp without time zone.
  /// </para>
  /// <para>
  /// Since Npgsql 6, a DateTime with Kind=Utc and the DbType.DateTime type
  /// are mapped to timestamp with time zone, and a DateTime with Kind=Utc
  /// can't be written any more in a timestamp without time zone parameter.
  /// To keep the behavior of Npgsql 5 and below, all the DateTime parameters
  /// are sent as timestamp without time zone (DbType.DateTime2),
  /// and the UTC date/times are sent without any conversion.
  /// </para>
  /// <para>
  /// See https://www.npgsql.org/doc/release-notes/6.0.html#detailed-notes
  /// </para>
  /// </remarks>
  public class Npgsql5CompatibilityDriver : NpgsqlDriver
  {
    /// <summary>
    /// <see cref="NpgsqlDriver"/>
    /// </summary>
    /// <param name="command"></param>
    public override void AdjustCommand (DbCommand command)
    {
      base.AdjustCommand (command);

      if (6 <= (DriverVersion?.Major ?? 3)) {
        for (var i = 0; i < command.Parameters.Count; i++) {
          var parameter = command.Parameters[i];
          if (parameter.Value is DateTime dateTime) {
            if (DbType.DateTime == parameter.DbType) {
              // Note: with Npgsql 6+, a DateTime with Kind=Utc is kept by the base method
              // as DbType.DateTime, which is mapped to timestamp with time zone
              parameter.DbType = DbType.DateTime2;
            }
            if ((DbType.DateTime2 == parameter.DbType) && (dateTime.Kind == DateTimeKind.Utc)) {
              parameter.Value = DateTime.SpecifyKind (dateTime, DateTimeKind.Unspecified);
            }
          }
          else if ((parameter.Value is null || parameter.Value is DBNull) && (DbType.DateTime == parameter.DbType)) {
            parameter.DbType = DbType.DateTime2;
          }
        }
      }
    }
  }
}
