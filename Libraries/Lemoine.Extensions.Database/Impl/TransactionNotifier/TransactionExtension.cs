// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Extensions.Database.Impl.TransactionNotifier
{
  /// <summary>
  /// Description of TransactionExtension.
  /// </summary>
  public class TransactionExtension: Lemoine.Extensions.Database.ITransactionExtension
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (TransactionExtension).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TransactionExtension ()
    {
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region ITransactionExtension implementation
    /// <summary>
    /// Called when a transaction begins
    /// </summary>
    /// <param name="transactionName"></param>
    public void BeginTransaction(string transactionName)
    {
    }

    /// <summary>
    /// Called before a commit
    /// </summary>
    /// <param name="transactionName"></param>
    public void BeforeCommit(string transactionName)
    {
      TransactionNotifier.BeforeCommit ();
    }

    /// <summary>
    /// Called after a commit success
    /// </summary>
    /// <param name="transactionName"></param>
    public void CommitSuccess(string transactionName)
    {
      TransactionNotifier.AfterCommit ();
    }

    /// <summary>
    /// Called after a commit failure
    /// </summary>
    /// <param name="transactionName"></param>
    /// <param name="exception"></param>
    public void CommitFailure(string transactionName, Exception exception)
    {
      TransactionNotifier.AfterRollback ();
    }

    /// <summary>
    /// Called after a rollback success
    /// </summary>
    /// <param name="transactionName"></param>
    /// <param name="implicitRollback"></param>
    public void RollbackSuccess(string transactionName, bool implicitRollback)
    {
      TransactionNotifier.AfterRollback ();
    }

    /// <summary>
    /// Called after a rollback failure
    /// </summary>
    /// <param name="transactionName"></param>
    /// <param name="exception"></param>
    public void RollbackFailure(string transactionName, Exception exception)
    {
      TransactionNotifier.AfterRollback ();
    }

    #endregion

    #region IExtension implementation
    /// <summary>
    /// Unique instance (no configuration)
    /// </summary>
    public bool UniqueInstance {
      get {
        return true;
      }
    }

    #endregion
  }
}
