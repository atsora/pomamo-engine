// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Generic interface for a session
  /// </summary>
  public interface IDAOSession: IDisposable
  {
    /// <summary>
    /// Force to use a unique session in all the asynchronous tasks
    /// 
    /// Useful in the unit tests
    /// </summary>
    void ForceUniqueSession ();

    /// <summary>
    /// Create LockTableToPartition items
    /// </summary>
    /// <param name="partitionId"></param>
    /// <param name="tableNames"></param>
    /// <returns></returns>
    IEnumerable<ILockTableToPartition> CreateLockTableToPartition (object partitionId, string[] tableNames);

    /// <summary>
    /// Begin a transaction
    /// </summary>
    /// <param name="name"></param>
    /// <param name="transactionLevel"></param>
    /// <param name="transactionLevelOptional"></param>
    /// <param name="lockedTables"></param>
    /// <param name="notTop">set to true to specify that this transaction is normally not a top transaction</param>
    /// <returns></returns>
    IDAOTransaction BeginTransaction (string name = "", TransactionLevel transactionLevel = TransactionLevel.Default, bool transactionLevelOptional = false, IEnumerable<ILockTableToPartition> lockedTables = null, bool notTop = false);

    /// <summary>
    /// Begin a read-only transaction
    /// </summary>
    /// <param name="name"></param>
    /// <param name="transactionLevel"></param>
    /// <param name="transactionLevelOptional"></param>
    /// <param name="lockedTables"></param>
    /// <returns></returns>
    IDAOTransaction BeginReadOnlyTransaction (string name = "", TransactionLevel transactionLevel = TransactionLevel.Default, bool transactionLevelOptional = false, IEnumerable<ILockTableToPartition> lockedTables = null);

    /// <summary>
    /// Begin a read-only and deferrable transaction
    /// 
    /// If the transaction level is not SERIALIZABLE, this has no effect
    /// (see PostgreSQL documentation)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="transactionLevel"></param>
    /// <param name="transactionLevelOptional"></param>
    /// <param name="lockedTables"></param>
    /// <returns></returns>
    IDAOTransaction BeginReadOnlyDeferrableTransaction (string name = "", TransactionLevel transactionLevel = TransactionLevel.Default, bool transactionLevelOptional = false, IEnumerable<ILockTableToPartition> lockedTables = null);

    /// <summary>
    /// Rollback a potentially active transaction
    /// </summary>
    void CancelActiveTransaction ();
    
    /// <summary>
    /// Cancel all the pending saves
    /// 
    /// Clear the first level cache
    /// </summary>
    void Clear ();

    /// <summary>
    /// Check if the persistent instance is in the session cache
    /// </summary>
    /// <param name="persistent"></param>
    /// <returns></returns>
    bool Contains (object persistent);
  }
}
