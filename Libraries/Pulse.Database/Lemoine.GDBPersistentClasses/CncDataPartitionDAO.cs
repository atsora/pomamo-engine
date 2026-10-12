// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="ICncDataPartitionDAO"/>
  /// with the PostgreSQL functions of migration 2001 (cncdatacreatepartition)
  /// </summary>
  public class CncDataPartitionDAO : ICncDataPartitionDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncDataPartitionDAO).FullName);

    // Error raised by PostgreSQL when no partition matches a row:
    // check_violation in routine ExecFindPartition (independent of the language of the server messages)
    static readonly string MISSING_PARTITION_SQL_STATE = "23514";
    static readonly string MISSING_PARTITION_ROUTINE = "ExecFindPartition";

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    /// <returns></returns>
    public int GetWeekCutOffDay ()
    {
      return (int)Lemoine.Info.ConfigSet
        .LoadAndGet<DayOfWeek> (ConfigKeys.GetCncConfigKey (CncConfigKey.DataPartitionWeekCutOffDay), DayOfWeek.Sunday);
    }

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public bool IsMissingPartition (Exception ex)
    {
      for (var e = ex; e is not null; e = e.InnerException) {
        if (e is Npgsql.PostgresException postgresException) {
          return string.Equals (postgresException.SqlState, MISSING_PARTITION_SQL_STATE, StringComparison.InvariantCulture)
            && string.Equals (postgresException.Routine, MISSING_PARTITION_ROUTINE, StringComparison.InvariantCulture);
        }
      }
      return false;
    }

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    /// <param name="table"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime">UTC date/time</param>
    public void CreatePartition (CncDataTable table, int machineModuleId, DateTime dateTime)
    {
      ExecuteInTransaction ("CncDataPartition.CreatePartition",
        "SELECT public.cncdatacreatepartition (@table, @machinemoduleid, @datetime, @cutoffday)",
        command => {
          AddParameter (command, "table", DbType.String, GetTableName (table));
          AddParameter (command, "machinemoduleid", DbType.Int32, machineModuleId);
          AddParameter (command, "datetime", DbType.DateTime, ToTimestamp (dateTime));
          AddParameter (command, "cutoffday", DbType.Int32, GetWeekCutOffDay ());
        });
      if (log.IsDebugEnabled) {
        log.Debug ($"CreatePartition: partition of {table} for machine module {machineModuleId} at {dateTime} checked");
      }
    }

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    /// <param name="machineModuleId"></param>
    /// <param name="fromDateTime">UTC date/time</param>
    /// <param name="days"></param>
    public void CreateWeekPartitions (int machineModuleId, DateTime fromDateTime, int days)
    {
      ExecuteInTransaction ("CncDataPartition.CreateWeekPartitions", $@"
SELECT public.cncdatacreatepartition (t, @machinemoduleid, d, @cutoffday)
  FROM unnest (ARRAY['{GetTableName (CncDataTable.Sample)}', '{GetTableName (CncDataTable.Period)}']) AS t,
       generate_series (date_trunc ('day', @from), date_trunc ('day', @from) + make_interval (days => @days), interval '1 day') AS d",
        command => {
          AddParameter (command, "machinemoduleid", DbType.Int32, machineModuleId);
          AddParameter (command, "cutoffday", DbType.Int32, GetWeekCutOffDay ());
          AddParameter (command, "from", DbType.DateTime, ToTimestamp (fromDateTime));
          AddParameter (command, "days", DbType.Int32, days);
        });
    }

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    public T RunWithPartition<T> (CncDataTable table, int machineModuleId, DateTime dateTime, Func<T> unitOfWork)
    {
      try {
        return unitOfWork ();
      }
      catch (Exception ex) when (IsMissingPartition (ex)) {
        log.Warn ($"RunWithPartition: missing partition of {table} for machine module {machineModuleId} at {dateTime} => create it and try again", ex);
        CreatePartition (table, machineModuleId, dateTime);
        return unitOfWork ();
      }
    }

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    public void RunWithPartition (CncDataTable table, int machineModuleId, DateTime dateTime, Action unitOfWork)
    {
      RunWithPartition<bool> (table, machineModuleId, dateTime, () => { unitOfWork (); return true; });
    }

    static string GetTableName (CncDataTable table) => table switch {
      CncDataTable.Sample => "cncdatasample",
      CncDataTable.Period => "cncdataperiod",
      CncDataTable.SampleArchive => "cncdatasamplearchive",
      CncDataTable.PeriodArchive => "cncdataperiodarchive",
      _ => throw new ArgumentOutOfRangeException (nameof (table), table, "Unknown cncdata table"),
    };

    /// <summary>
    /// UTC date/time as a timestamp without time zone (Kind Unspecified, so that no time zone conversion is made)
    /// </summary>
    static DateTime ToTimestamp (DateTime dateTime)
    {
      var utc = (dateTime.Kind == DateTimeKind.Local) ? dateTime.ToUniversalTime () : dateTime;
      return DateTime.SpecifyKind (utc, DateTimeKind.Unspecified);
    }

    static void AddParameter (IDbCommand command, string name, DbType dbType, object value)
    {
      var parameter = command.CreateParameter ();
      parameter.ParameterName = name;
      parameter.DbType = dbType;
      parameter.Value = value;
      command.Parameters.Add (parameter);
    }

    static void ExecuteInTransaction (string transactionName, string sql, Action<IDbCommand> addParameters)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction (transactionName)) {
        using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand ()) {
          command.CommandText = sql;
          addParameters (command);
          command.ExecuteNonQuery ();
        }
        transaction.Commit ();
      }
    }
  }
}
