// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Core.ExceptionManagement;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using System.Collections.Generic;
using System.Linq;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// Description of DAOTransaction.
  /// </summary>
  public sealed class DAOTransaction : IDAOTransaction
  {
    #region Members
    DAOSession m_session;
    string m_name;
    string m_beginStackTrace = System.Environment.StackTrace;
    bool m_topTransaction = false;
    bool m_commit = false;
    bool m_failedCommit = false;
    bool m_rollback = false;
    bool m_serializationFailure = false;
    SynchronousCommit? m_synchronousCommitOption = null;
    TransactionLevel m_transactionLevelOption = TransactionLevel.Default;
    bool m_readOnly = false;
    bool m_deferrable = false;
    bool m_synchronousCommitRequestRun = false;
    readonly IList<ILockTableToPartition> m_lockedTables = new List<ILockTableToPartition> ();
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (DAOTransaction).FullName);

    #region Getters / Setters
    /// <summary>
    /// Transaction name
    /// </summary>
    string Name
    {
      get { return m_name; }
      set {
        m_name = value;
        if (!string.IsNullOrEmpty (m_name)) {
          log = LogManager.GetLogger (typeof (DAOTransaction).FullName + "." + value);
        }
        else {
          log = LogManager.GetLogger (typeof (DAOTransaction).FullName);
        }
      }
    }

    /// <summary>
    /// Top transaction ?
    /// </summary>
    public bool TopTransaction
    {
      get { return m_topTransaction; }
      set { m_topTransaction = value; }
    }

    /// <summary>
    /// Synchronous commit option
    /// </summary>
    public SynchronousCommit? SynchronousCommitOption
    {
      get { return m_synchronousCommitOption; }
      set { m_synchronousCommitOption = value; }
    }

    /// <summary>
    /// Transaction level
    /// 
    /// Note: if this is not called on the top transaction before any request, the change won't be applied
    /// </summary>
    public TransactionLevel TransactionLevelOption
    {
      get { return m_transactionLevelOption; }
      set { m_transactionLevelOption = value; RunTransactionLevelRequest (); }
    }

    /// <summary>
    /// Read only transaction property
    /// 
    /// Default is false
    /// </summary>
    public bool ReadOnly
    {
      get { return m_readOnly; }
      set { m_readOnly = value; RunReadOnlyRequest (); }
    }

    /// <summary>
    /// Defferable transaction property
    /// 
    /// Default is false
    /// </summary>
    public bool Deferrable
    {
      get { return m_deferrable; }
      set { m_deferrable = value; RunDeferrableRequest (); }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="session"></param>
    /// <param name="name"></param>
    /// <param name="topTransaction"></param>
    /// <param name="notTop">set to true to specify that this transaction is normally not a top transaction</param>
    internal DAOTransaction (DAOSession session, string name, bool topTransaction, bool notTop = false)
    {
      if (topTransaction && notTop) {
        log.Fatal ($"DAOTransaction: transaction {name} was created although it is flagged as a notTop transaction. Is the notTop agument valid? {System.Environment.StackTrace}");
      }
      m_session = session;
      m_topTransaction = topTransaction;
      this.Name = name;
      RunTransactionNameRequest (notTop: notTop); // Must be run after Name and topTransaction properties are set
    }
    #endregion // Constructors

    /// <summary>
    /// Add a locked table
    /// </summary>
    /// <param name="lockedTable"></param>
    public void Add (ILockTableToPartition lockedTable)
    {
      m_lockedTables.Add (lockedTable);
    }

    /// <summary>
    /// Implements IDAOTransaction
    /// </summary>
    public void Commit ()
    {
      try {
        RunSynchronousCommitRequest ();
      }
      catch (Exception ex) {
        log.Error ($"Commit: RunSynchronousCommitRequest failed", ex);
        throw;
      }

      try {
        m_session.CommitTransaction (m_name, m_readOnly);
      }
      catch (Exception ex) {
        if (ExceptionTest.IsTransactionSerializationFailure (ex)) {
          log.Warn ("Commit: serialization failure", ex);
        }
        else {
          log.Error ("Commit: CommitTransaction failed", ex);
        }
        m_failedCommit = true;
        throw;
      }

      // CommitTransaction may fail
      // Set the m_commit property to true only after CommitTransaction
      // because in case of problem, an implicit rollback is necessary
      m_commit = true;
    }

    /// <summary>
    /// Commit a transaction and re-open a new one (in read-write)
    /// </summary>
    public void CommitNew ()
    {
      // Commit it first
      this.Commit ();

      // Create a new transaction
      if (false == m_session.InitializeTransaction (m_name, readOnly: false)) {
        log.Warn ("CommitNew: the old transaction was still active after the Commit statement, no new transaction was initialized");
      }

      // Consider it is now a new transaction, that has not been committed yet
      m_commit = false;
    }

    /// <summary>
    /// Implements IDAOTransaction
    /// 
    /// Note: This is hard to propagate the rollback event to the parent sessions.
    ///       An exception should be raised in the same time to terminate the parent sessions.
    /// </summary>
    public void Rollback ()
    {
      Rollback (false);
    }

    void RollbackReadOnly ()
    {
      Rollback (false, readOnly: true);
    }

    void Rollback (bool implicitRollback, bool readOnly = false)
    {
      if (!readOnly) {
        log.Warn ("Rollback");
      }
      else {
        log.Debug ("Rollback the read-only transaction");
      }
      m_rollback = true;

      Debug.Assert (null != m_session);
      if (null == m_session) {
        log.Fatal ("Rollback: null associated session, this should not happen");
      }

      try {
        m_session.RollbackTransaction (m_name, implicitRollback, readOnly: readOnly);
      }
      catch (Exception ex) {
        log.Error ("Rollback: RollbackTransaction failed", ex);
        throw;
      }
    }

    /// <summary>
    /// Flag that there was a serialization failure
    /// </summary>
    public void FlagSerializationFailure ()
    {
      m_serializationFailure = true;
    }

    void RunSynchronousCommitRequest ()
    {
      if (m_synchronousCommitRequestRun) {
        log.Debug ("RunSynchronousCommitRequest: " +
                   "it has already been run");
        return;
      }

      if ((null != m_lockedTables) && (m_lockedTables.Any ())
        && this.ReadOnly) {
        m_synchronousCommitOption = SynchronousCommit.Off;
      }

      if (m_synchronousCommitOption.HasValue) {
        switch (m_synchronousCommitOption.Value) {
        case SynchronousCommit.Default:
          break;
        case SynchronousCommit.Off:
          log.Debug ("RunSynchronousCommitRequest: set asynchronous commit to Off");
          NHibernateHelper.GetCurrentSession ()
            .CreateSQLQuery (@"SET LOCAL synchronous_commit TO OFF")
            .ExecuteUpdate ();
          break;
        case SynchronousCommit.On:
          log.Debug ("RunSynchronousCommitRequest: set asynchronous commit to On");
          NHibernateHelper.GetCurrentSession ()
            .CreateSQLQuery (@"SET LOCAL synchronous_commit TO ON")
            .ExecuteUpdate ();
          break;
        case SynchronousCommit.Local:
          log.Debug ("RunSynchronousCommitRequest: set asynchronous commit to Local");
          NHibernateHelper.GetCurrentSession ()
            .CreateSQLQuery (@"SET LOCAL synchronous_commit TO LOCAL")
            .ExecuteUpdate ();
          break;
        default:
          throw new Exception ("Invalid value for AsynchronousCommit");
        }
      }

      m_synchronousCommitRequestRun = true;
    }

    /// <summary>
    /// Implements IDisposable
    /// </summary>
    public void Dispose ()
    {
      if (!m_commit && m_readOnly) { // auto-commit or rollback in case the transaction is read-only
        if (log.IsDebugEnabled) {
          log.Debug ("Dispose: auto commit/rollback of a read-only transaction");
        }
        try {
          if (this.TopTransaction) {
            this.RollbackReadOnly ();
          }
          else {
            this.Commit ();
          }
        }
        catch (Exception ex) {
          log.Error ($"Dispose: auto commit/rollback failed. Top={this.TopTransaction}", ex);
        }
      }

      if (!m_commit && !m_rollback) {
        if (m_failedCommit) {
          log.Warn ($"Dispose: a commit failed => run an implicit rollback. BeginTransaction: {m_beginStackTrace}\n Rollback: {System.Environment.StackTrace}");
        }
        else if (m_serializationFailure) {
          log.Warn ($"Dispose: serialization failure => implicit rollback. BeginTransaction: {m_beginStackTrace}\n Rollback: {System.Environment.StackTrace}");
        }
        else {
          log.Error ($"Dispose: implicit rollback. BeginTransaction: {m_beginStackTrace}\n Rollback: {System.Environment.StackTrace}");
        }

        try {
          this.Rollback (true);
        }
        catch (Exception ex) {
          log.Error ($"Dispose: Rollback failed", ex);
        }
      }

      foreach (var lockedTable in m_lockedTables) {
        lockedTable.Dispose ();
      }
    }

    void RunTransactionLevelRequest ()
    {
      if (!TopTransaction) {
        if (log.IsWarnEnabled) {
          log.WarnFormat ("RunTransactionLevelRequest: " +
                          "do not change the transaction level because it is not the top transaction. " +
                          "\n{0}",
                          System.Environment.StackTrace);
        }
        return;
      }

      switch (m_transactionLevelOption) {
      case TransactionLevel.Default:
        break;
      case TransactionLevel.Serializable:
        log.Debug ("RunTransactionLevelRequest: " +
                   "set transaction level to Serializable");
        NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (@"SET TRANSACTION ISOLATION LEVEL SERIALIZABLE")
          .ExecuteUpdate ();
        break;
      case TransactionLevel.RepeatableRead:
        log.Debug ("RunTransactionLevelRequest: " +
                   "set transaction level to Repeatable Read");
        NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (@"SET TRANSACTION ISOLATION LEVEL REPEATABLE READ")
          .ExecuteUpdate ();
        break;
      case TransactionLevel.ReadCommitted:
        log.Debug ("RunTransactionLevelRequest: " +
                   "set transaction level to Read Committed");
        NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (@"SET TRANSACTION ISOLATION LEVEL READ COMMITTED")
          .ExecuteUpdate ();
        break;
      case TransactionLevel.ReadUncommitted:
        log.Debug ("RunTransactionLevelRequest: " +
                   "set transaction level to Read Uncommitted");
        NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (@"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED")
          .ExecuteUpdate ();
        break;
      default:
        throw new Exception ("Invalid value for TransactionLevel");
      }
    }

    void RunReadOnlyRequest ()
    {
      // Warning message, because there is no reason to run this method with m_readOnly=False
      if (!m_readOnly) {
        Debug.Assert (false);
        log.Error ("RunReadOnlyRequest: read only property is false, this is unusual, it should not be run with readOnly set to false");
      }

      if (!TopTransaction) {
        if (log.IsDebugEnabled) {
          log.Debug ("RunReadOnlyRequest: do not change the read-only property because it is not the top transaction");
        }
        return;
      }

      if (m_readOnly) {
        log.Debug ("RunReadOnlyRequest: set transaction read only");
        NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (@"SET TRANSACTION READ ONLY")
          .ExecuteUpdate ();
      }
      else {
        log.Debug ("RunReadOnlyRequest: set transaction read write");
        NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (@"SET TRANSACTION READ WRITE")
          .ExecuteUpdate ();
      }
    }

    void RunDeferrableRequest ()
    {
      if (!TopTransaction) {
        if (log.IsInfoEnabled) {
          log.Info ($"RunDeferrableRequest: do not change the deferrable property because it is not the top transaction. {System.Environment.StackTrace}");
        }
        return;
      }

      if (m_deferrable) {
        log.Debug ("RunDeferrableRequest: set transaction deferrable");
        NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (@"SET TRANSACTION DEFERRABLE")
          .ExecuteUpdate ();
      }
      else {
        log.Debug ("RunDeferrableRequest: set transaction not deferrable");
        NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (@"SET TRANSACTION NOT DEFERRABLE")
          .ExecuteUpdate ();
      }
    }

    void RunTransactionNameRequest (bool notTop = false)
    {
      if (string.IsNullOrEmpty (this.Name)) {
        return;
      }

      if (!notTop && !TopTransaction) {
        log.Debug ($"RunTransactionRequest: not the top transaction {System.Environment.StackTrace}");
      }

      log.Debug ($"RunTransactionNameRequest: {this.Name}");
      NHibernateHelper.GetCurrentSession ()
        .CreateSQLQuery (@"/*" + this.Name + "*/")
        .ExecuteUpdate ();
    }
  }
}
