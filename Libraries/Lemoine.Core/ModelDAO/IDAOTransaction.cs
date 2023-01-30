// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Description of IDAOTransaction.
  /// </summary>
  public interface IDAOTransaction: IDisposable
  {
    /// <summary>
    /// Synchronous commit option.
    /// </summary>
    SynchronousCommit? SynchronousCommitOption { get; set; }
    
    /// <summary>
    /// Transaction level option.
    /// 
    /// Default should be TransactionLevel.Default
    /// </summary>
    TransactionLevel TransactionLevelOption { get; set; }
    
    /// <summary>
    /// Read only transaction property
    /// 
    /// Default is false
    /// </summary>
    bool ReadOnly { get; set; }
    
    /// <summary>
    /// Defferable transaction property
    /// 
    /// Default is false
    /// </summary>
    bool Deferrable { get; set; }

    /// <summary>
    /// Top transaction ?
    /// </summary>
    bool TopTransaction { get; }

    /// <summary>
    /// Add a locked table
    /// </summary>
    /// <param name="lockedTable"></param>
    void Add (ILockTableToPartition lockedTable);

    /// <summary>
    /// Commit the transaction
    /// </summary>
    void Commit ();
    
    /// <summary>
    /// Commit the transaction and re-open a new one (in read-write)
    /// </summary>
    void CommitNew ();

    /// <summary>
    /// Rollback the transaction
    /// 
    /// Note: This is hard to propagate the rollback event to the parent sessions.
    ///       An exception should be raised in the same time to terminate the parent sessions.
    /// </summary>
    void Rollback ();

    /// <summary>
    /// Flag that there was a serialization failure
    /// </summary>
    void FlagSerializationFailure ();
  }
}
