// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// LockTableToPartition
  /// </summary>
  public sealed class LockTableToPartition : ILockTableToPartition
  {
    ILog log = LogManager.GetLogger (typeof (LockTableToPartition).FullName);

    const string CHECK_VIEW_EXISTS_KEY = "Database.LockTableToPartition.CheckViewExistsFirsts";
    const bool CHECK_VIEW_EXISTS_DEFAULT = true;

    readonly object m_partitionId;
    readonly string m_tableName;

    bool m_partitioned;
    bool m_view = false;
    bool m_reset = false;
    bool m_error = false;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="partitionId"></param>
    /// <param name="tableName"></param>
    public LockTableToPartition (object partitionId, string tableName)
    {
      m_partitionId = partitionId;
      m_tableName = tableName;

      log = LogManager.GetLogger (this.GetType ().FullName + "." + partitionId);

      if (log.IsFatalEnabled && !(partitionId is int) && !(partitionId is long)) {
        log.Fatal ($"LockTableToPartition: probably invalid partitionId {partitionId}");
      }
    }

    /// <summary>
    /// <see cref="ILockTableToPartition"/>
    /// </summary>
    public void Run ()
    {
      try {
        string partitionExistsRequest = $@"
SELECT EXISTS (
SELECT 1 FROM information_schema.tables
WHERE table_schema='pgfkpart'
  AND table_name='{m_tableName}_p{m_partitionId}'
) AS exists;
";
        m_partitioned = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (partitionExistsRequest)
          .AddScalar ("exists", NHibernate.NHibernateUtil.Boolean)
          .UniqueResult<bool> ();
        if (!m_partitioned) {
          if (log.IsWarnEnabled) {
            log.Warn ($"Run: table pgfkpart.{m_tableName}_p{m_partitionId} does not exist, is {m_tableName} partitioned ?");
          }
        }
        else {
          NHibernateHelper.GetCurrentSession ()
    .CreateSQLQuery ($@"CREATE TEMP VIEW {m_tableName} AS
SELECT * FROM pgfkpart.{m_tableName}_p{m_partitionId};")
    .ExecuteUpdate ();
          m_view = true;
        }
      }
      catch (Exception ex) {
        log.Error ("Run", ex);
        m_error = true;
      }
    }

    #endregion // Constructors

    /// <summary>
    /// Reset the partition lock with default parameters
    /// </summary>
    public void Reset ()
    {
      var checkViewExistsFirst = Lemoine.Info.ConfigSet
        .LoadAndGet (CHECK_VIEW_EXISTS_KEY, CHECK_VIEW_EXISTS_DEFAULT);
      Reset (checkViewExistsFirst, true);
    }

    /// <summary>
    /// Reset the partition lock
    /// </summary>
    /// <param name="checkViewExistsFirst"></param>
    /// <param name="logIfNotExists"></param>
    public void Reset (bool checkViewExistsFirst, bool logIfNotExists = true)
    {
      if (m_reset) { // Already done
        return;
      }

      try {
        if (m_view) {
          bool dropView;
          if (checkViewExistsFirst) {
            try {
              string viewExistsRequest = $@"
SELECT EXISTS (
SELECT 1 FROM information_schema.tables
WHERE table_name='{m_tableName}'
  AND table_type='LOCAL TEMPORARY'
) AS exists;
";
              dropView = NHibernateHelper.GetCurrentSession ()
                .CreateSQLQuery (viewExistsRequest)
                .AddScalar ("exists", NHibernate.NHibernateUtil.Boolean)
                .UniqueResult<bool> ();
              if (!dropView && logIfNotExists) {
                log.Fatal ($"Reset: view {m_tableName} does not exist, which is unexpected");
              }
            }
            catch (Exception checkViewException) {
              log.Error ($"Reset: exception in check the view {m_tableName} exists", checkViewException);
              dropView = true;
            }
          }
          else {
            dropView = true;
          }
          if (dropView) {
            NHibernateHelper.GetCurrentSession ()
      .CreateSQLQuery ($"DROP VIEW IF EXISTS {m_tableName};")
      .ExecuteUpdate ();
          }
        }
      }
      catch (Exception ex) {
        if (m_error) {
          if (log.IsDebugEnabled) {
            log.Debug ("Reset: after error", ex);
          }
        }
        else {
          log.Fatal ($"Reset: error which may leave locked to partition {m_tableName}", ex);
        }
      }
      finally {
        m_reset = true;
      }
    }

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    public void Dispose ()
    {
    }
  }
}
