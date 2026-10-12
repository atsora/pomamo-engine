// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Partitioned cncdata tables
  /// </summary>
  public enum CncDataTable
  {
    /// <summary>
    /// cncdatasample: samples, partitioned by machine module and week
    /// </summary>
    Sample,
    /// <summary>
    /// cncdataperiod: aggregated periods, partitioned by machine module and week
    /// </summary>
    Period,
    /// <summary>
    /// cncdatasamplearchive: partitioned by machine module and quarter
    /// </summary>
    SampleArchive,
    /// <summary>
    /// cncdataperiodarchive: partitioned by machine module and quarter
    /// </summary>
    PeriodArchive,
  }

  /// <summary>
  /// Leaf partition of a cncdata table
  /// </summary>
  public interface ICncDataPartition
  {
    /// <summary>
    /// Partitioned table
    /// </summary>
    CncDataTable Table { get; }

    /// <summary>
    /// Machine module
    /// </summary>
    int MachineModuleId { get; }

    /// <summary>
    /// Name of the partition (table)
    /// </summary>
    string Name { get; }

    /// <summary>
    /// UTC lower bound (included)
    /// </summary>
    DateTime LowerBound { get; }

    /// <summary>
    /// UTC upper bound (excluded)
    /// </summary>
    DateTime UpperBound { get; }
  }

  /// <summary>
  /// DAO interface to manage the partitions of the cncdata tables
  ///
  /// The partitions are created in advance (plugin CncDataPartitions).
  /// <see cref="RunWithPartition{T}(CncDataTable, int, DateTime, Func{T})"/> is the safety net of the writers:
  /// if a partition is missing, it is created and the unit of work is run again
  ///
  /// The methods that create partitions open their own session and transaction:
  /// they must not be called inside another session
  /// </summary>
  public interface ICncDataPartitionDAO
  {
    /// <summary>
    /// Cut-off day of the week partitions (0 = Sunday ... 6 = Saturday), config Cnc.DataPartitionWeekCutOffDay
    /// </summary>
    /// <returns></returns>
    int GetWeekCutOffDay ();

    /// <summary>
    /// Is the exception (or one of its inner exceptions) raised because no partition matches a row?
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    bool IsMissingPartition (Exception ex);

    /// <summary>
    /// Create (if needed) the partition of a cncdata table that contains a date/time, in its own transaction
    /// </summary>
    /// <param name="table"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime">UTC date/time</param>
    void CreatePartition (CncDataTable table, int machineModuleId, DateTime dateTime);

    /// <summary>
    /// Create (if needed) the week partitions of cncdatasample and cncdataperiod for a machine module,
    /// day by day from the day of fromDateTime, in its own transaction
    ///
    /// Day by day, no day is left without a partition, even after a change of the cut-off day
    /// </summary>
    /// <param name="machineModuleId"></param>
    /// <param name="fromDateTime">UTC date/time</param>
    /// <param name="days">number of days after the first one</param>
    void CreateWeekPartitions (int machineModuleId, DateTime fromDateTime, int days);

    /// <summary>
    /// Make sure the partition of a cncdata table that contains a date/time exists,
    /// inside the current session and transaction (safety net of the DAOs that write in a transaction)
    ///
    /// The known partitions are cached. A missing partition is created in the current transaction:
    /// it is cached only once it is seen again, so that a rolled back creation is not cached
    /// </summary>
    /// <param name="table"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime">UTC date/time</param>
    void EnsurePartition (CncDataTable table, int machineModuleId, DateTime dateTime);

    /// <summary>
    /// Run a unit of work that writes in a cncdata table.
    /// If it fails because the partition is missing, create the partition and run the unit of work once again.
    ///
    /// The unit of work must open and close its own session and transaction,
    /// and it must not be run inside another session (a failed transaction can't be used any more)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="table"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime">UTC date/time of the written row (its begin)</param>
    /// <param name="unitOfWork">not null</param>
    /// <returns></returns>
    T RunWithPartition<T> (CncDataTable table, int machineModuleId, DateTime dateTime, Func<T> unitOfWork);

    /// <summary>
    /// <see cref="RunWithPartition{T}(CncDataTable, int, DateTime, Func{T})"/> for a unit of work without result
    /// </summary>
    /// <param name="table"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime">UTC date/time of the written row (its begin)</param>
    /// <param name="unitOfWork">not null</param>
    void RunWithPartition (CncDataTable table, int machineModuleId, DateTime dateTime, Action unitOfWork);

    /// <summary>
    /// Leaf partitions of a cncdata table, sorted by lower bound and machine module, in their own session
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    System.Collections.Generic.IList<ICncDataPartition> FindPartitions (CncDataTable table);

    /// <summary>
    /// Archive a week partition of cncdatasample or cncdataperiod, without any loss,
    /// in a single transaction that also drops the archived week partition:
    /// <item>samples: regrouped by field and by minute in cncdatasamplearchive</item>
    /// <item>periods: regrouped by field and by day in cncdataperiodarchive</item>
    ///
    /// The number of archived samples or periods is checked before the commit.
    /// If the maximum duration is reached, the transaction is cancelled (nothing is changed)
    /// </summary>
    /// <param name="partition">week partition of cncdatasample or cncdataperiod</param>
    /// <param name="maxDuration">maximum duration of the transaction</param>
    /// <returns>number of archived samples or periods</returns>
    long ArchivePartition (ICncDataPartition partition, TimeSpan maxDuration);

    /// <summary>
    /// Drop a partition (for example an archive partition after the retention), in its own transaction
    /// </summary>
    /// <param name="partition"></param>
    /// <param name="maxDuration">maximum duration of the transaction</param>
    void DropPartition (ICncDataPartition partition, TimeSpan maxDuration);
  }
}
