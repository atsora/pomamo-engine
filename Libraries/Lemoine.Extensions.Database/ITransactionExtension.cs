// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Extensions.Database
{
  /// <summary>
  /// Extension for a database transaction
  /// </summary>
  public interface ITransactionExtension: Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Begin a transaction
    /// </summary>
    /// <param name="transactionName"></param>
    void BeginTransaction (string transactionName);
    
    /// <summary>
    /// Run before a commit
    /// </summary>
    /// <param name="transactionName"></param>
    void BeforeCommit (string transactionName);
    
    /// <summary>
    /// Committing the transaction was successful
    /// </summary>
    /// <param name="transactionName"></param>
    void CommitSuccess (string transactionName);
    
    /// <summary>
    /// Committing the transaction failed
    /// </summary>
    /// <param name="transactionName"></param>
    /// <param name="exception"></param>
    void CommitFailure (string transactionName, Exception exception);
    
    /// <summary>
    /// Rolling back the transaction was successful
    /// </summary>
    /// <param name="transactionName"></param>
    /// <param name="implicitRollback"></param>
    void RollbackSuccess (string transactionName, bool implicitRollback);
    
    /// <summary>
    /// Rolling back the transaction failed
    /// </summary>
    /// <param name="transactionName"></param>
    /// <param name="exception"></param>
    void RollbackFailure (string transactionName, Exception exception);
  }
}
