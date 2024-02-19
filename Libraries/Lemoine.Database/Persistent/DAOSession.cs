// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Context;
using System.Linq;
using System.Threading;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// Description of DAOSession.
  /// </summary>
  public sealed class DAOSession : IDAOSession
  {
    const string CHECK_NO_TEMP_VIEW_KEY = "Database.Session.CheckNoTempView";
    const bool CHECK_NO_TEMP_VIEW_DEFAULT = false;

    const string SEND_MESSAGES_METHOD_KEY = "Database.Session.SendMessagesMethod";
    const string SEND_MESSAGES_METHOD_DEFAULT = ""; // "pool" or "thread" or "async" (previous default but could drive to a deadlock) or "nowait" or default ("sync" / "")

    #region Members
    static volatile ISession s_uniqueSession = null;

    ISession m_session;
    bool m_sessionCreated = false;
    bool m_transactionCreated = false;
    DateTime m_transactionCreationDateTime;
    bool m_rolledBack = false; // Only active if the same DAOSession is used
    bool m_valid = true; // Just an additional protection to check a not valid session is still used
                         // If m_valid is false, DAOSession is not in a correct state

    IEnumerable<Lemoine.Extensions.Database.ITransactionExtension> m_transactionExtensions = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (DAOSession).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated NHibernate Session
    /// </summary>
    internal ISession Session => m_session;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor with the default SessionFactory
    /// </summary>
    public DAOSession ()
    {
      log.Trace ("DAOSession /B");

      ISessionFactory sessionFactory = NHibernateHelper.SessionFactory;

      if (null != s_uniqueSession) {
        m_session = s_uniqueSession;
        CurrentSessionContext.Bind (m_session);
        m_sessionCreated = false;
        return;
      }

      if (CurrentSessionContext.HasBind (sessionFactory)) {
        log.Debug ("DAOSession: re-use an existing session");
        m_session = NHibernateHelper.GetCurrentSession ();
        if (!m_session.IsConnected) {
          log.Warn ("DAOSession: existing session was not connected");
        }
      }
      else { // No session bound to the current context
        log.Debug ("DAOSession: open a new session");
        m_session = NHibernateHelper.OpenSession ();
        CurrentSessionContext.Bind (m_session);
        m_sessionCreated = true;
      }

      if (null == m_session) {
        log.Fatal ("DAOSession: m_session is null");
        throw new NullReferenceException ("m_session");
      }
    }

    /// <summary>
    /// Alternative constructor:
    /// create a new DAOSession from an alternative ISessionFactory
    /// </summary>
    /// <param name="sessionFactory"></param>
    public DAOSession (ISessionFactory sessionFactory)
    {
      log.Debug ("DAOSession /B");

      if (null != s_uniqueSession) {
        m_session = s_uniqueSession;
        CurrentSessionContext.Bind (m_session);
        m_sessionCreated = false;
        return;
      }

      if (CurrentSessionContext.HasBind (sessionFactory)) {
        log.Debug ("DAOSession: re-use an existing session");
        m_session = sessionFactory.GetCurrentSession ();
        if (!m_session.IsConnected) {
          log.Warn ("DAOSession: existing session was not connected");
        }
      }
      else { // No session bound to the current context
        log.Info ("DAOSession: open a new session");
        m_session = sessionFactory.OpenSession ();
        CurrentSessionContext.Bind (m_session);
        m_sessionCreated = true;
      }

      if (null == m_session) {
        log.Fatal ("DAOSession: m_session is null");
        throw new NullReferenceException ("m_session");
      }
    }
    #endregion // Constructors

    /// <summary>
    /// Force to use a unique session in all the asynchronous tasks
    /// 
    /// Useful in the unit tests
    /// </summary>
    public void ForceUniqueSession ()
    {
      s_uniqueSession = m_session;
    }

    /// <summary>
    /// Get the extensions and load them if needed
    /// </summary>
    /// <returns></returns>
    IEnumerable<Lemoine.Extensions.Database.ITransactionExtension> GetTransactionExtensions ()
    {
      LoadExtensions ();
      return m_transactionExtensions;
    }

    /// <summary>
    /// Load the extensions
    /// </summary>
    void LoadExtensions ()
    {
      if (null == m_transactionExtensions) { // Initialization
        var request = new Lemoine.Business.Extension
          .GlobalExtensions<Lemoine.Extensions.Database.ITransactionExtension> ();
        m_transactionExtensions = Lemoine.Business.ServiceProvider
          .Get (request);
      }
    }

    /// <summary>
    /// Create LockTableToPartition items
    /// </summary>
    /// <param name="partitionId"></param>
    /// <param name="tableNames"></param>
    /// <returns></returns>
    public IEnumerable<ILockTableToPartition> CreateLockTableToPartition (object partitionId, string[] tableNames)
    {

      return tableNames
        .Select (t => new LockTableToPartition (partitionId, t))
        .Cast<ILockTableToPartition> ()
        .ToList (); // Do it now, this is better than after the transaction creation
    }

    /// <summary>
    /// Initialize a new transaction
    /// </summary>
    /// <param name="transactionName"></param>
    /// <returns>a new transaction was created</returns>
    internal bool InitializeTransaction (string transactionName, bool readOnly = false)
    {
      Debug.Assert (null != m_session, "Unexpected null internal session");

      try {
        if (!m_valid) {
          log.Fatal ("InitializeTransaction: try to initialize a transaction in a not valid session");
        }

        if (m_session.GetCurrentTransaction ()?.IsActive ?? false) {
          log.Debug ("InitializeTransaction: a transaction is already active, do nothing");
          return false;
        }

        if (m_transactionCreated) { // Possible implicit rollback
          try { // Clear the accumulators in case of a previous implicit rollback
            foreach (var sessionAccumulator in NHibernateHelper.SessionAccumulators) {
              sessionAccumulator.Clear (m_session);
            }
          }
          catch (Exception ex) {
            log.Error ($"InitializeTransaction: the accumulators could not be cleared", ex);
          }
        }
        else {
          var notEmptySessionAccumulators = NHibernateHelper.SessionAccumulators
            .Where (x => !x.IsEmpty (m_session));
          if (notEmptySessionAccumulators.Any ()) {
            log.Fatal ($"InitializeTransaction: there are some accumulators ! Clear them");
            foreach (var notEmptySessionAccumulator in notEmptySessionAccumulators) {
              try {
                notEmptySessionAccumulator.Clear (m_session);
              }
              catch (Exception ex) {
                log.Error ($"InitializeTransaction: the accumulators could not be cleared", ex);
              }
            }
          }
        }

        log.Debug ("InitializeTransaction: no transaction is active yet, begin a new one");
        m_session.BeginTransaction ();
        m_transactionCreated = true;
        m_transactionCreationDateTime = DateTime.UtcNow;
        m_rolledBack = false;

        if (!readOnly) {
          foreach (var transactionExtension in GetTransactionExtensions ()) {
            transactionExtension.BeginTransaction (transactionName);
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"InitializeTransaction: exception", ex);
        throw;
      }

      return true;
    }

    /// <summary>
    /// <see cref="IDAOTransaction"></see>
    /// </summary>
    public IDAOTransaction BeginTransaction (string name = "", TransactionLevel transactionLevel = TransactionLevel.Default, bool transactionLevelOptional = false, IEnumerable<ILockTableToPartition> lockedTables = null, bool notTop = false)
    {
      return BeginTransaction (name, transactionLevel, transactionLevelOptional, lockedTables, notTop, readOnly: false);
    }

    IDAOTransaction BeginTransaction (string name = "", TransactionLevel transactionLevel = TransactionLevel.Default, bool transactionLevelOptional = false, IEnumerable<ILockTableToPartition> lockedTables = null, bool notTop = false, bool readOnly = false)
    {
      try {
        if (!m_valid) {
          log.Fatal ($"BeginTransaction: name={name} try to initialize a transaction in a not valid session");
        }

        if (false == InitializeTransaction (name, readOnly)) {
          if (log.IsWarnEnabled && !transactionLevel.Equals (TransactionLevel.Default)) {
            if (!transactionLevelOptional) {
              log.Warn ($"BeginTransaction: name={name} change the transactionLevel on a transaction that is not a top transaction. \n{System.Environment.StackTrace}");
            }
            else {
              if (log.IsDebugEnabled) {
                if (notTop) {
                  log.Debug ($"BeginTransaction: name={name} transaction Level won't be applied because it is not the top transaction but flagged as it is (and so normal)");
                }
                else {
                  log.Debug ($"BeginTransaction: name={name} transaction Level won't be applied because it is not the top transaction");
                }
              }
            }
          }
        }
        if (m_transactionCreated && notTop) {
          log.Fatal ($"BeginTransaction: transaction {name} was created although it is flagged as a notTop transaction. Is the notTop argument valid? {System.Environment.StackTrace}");
        }
        IDAOTransaction transaction = new DAOTransaction (this, name, m_transactionCreated, notTop: notTop);
        if (!transactionLevel.Equals (TransactionLevel.Default)
          && (m_transactionCreated || !transactionLevelOptional)) {
          transaction.TransactionLevelOption = transactionLevel;
        }
        if (m_transactionCreated) {
          CheckNoTempView ();
        }
        if ((null != lockedTables) && lockedTables.Any ()) {
          if (!m_transactionCreated) {
            log.Error ($"BeginTransaction: name={name} not the top transaction to lock tables.\nStackTrace={System.Environment.StackTrace}");
          }
          else {
            foreach (var lockedTable in lockedTables) {
              lockedTable.Run ();
              transaction.Add (lockedTable);
            }
          }
        }
        return transaction;
      }
      catch (Exception ex) {
        log.Error ("BeginTransaction: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IDAOTransaction"/>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="transactionLevel"></param>
    /// <param name="transactionLevelOptional"></param>
    /// <param name="lockedTables"></param>
    /// <returns></returns>
    public IDAOTransaction BeginReadOnlyTransaction (string name = "", TransactionLevel transactionLevel = TransactionLevel.Default, bool transactionLevelOptional = false, IEnumerable<ILockTableToPartition> lockedTables = null)
    {
      var transaction = BeginTransaction (name, transactionLevel, transactionLevelOptional, lockedTables, readOnly: true);
      transaction.ReadOnly = true;
      return transaction;
    }

    /// <summary>
    /// <see cref="IDAOTransaction"/>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="transactionLevel"></param>
    /// <param name="transactionLevelOptional"></param>
    /// <param name="lockedTables"></param>
    /// <returns></returns>
    public IDAOTransaction BeginReadOnlyDeferrableTransaction (string name = "", TransactionLevel transactionLevel = TransactionLevel.Default, bool transactionLevelOptional = false, IEnumerable<ILockTableToPartition> lockedTables = null)
    {
      var transaction = BeginTransaction (name, transactionLevel, transactionLevelOptional, lockedTables, readOnly: true);
      transaction.ReadOnly = true;
      transaction.Deferrable = true;
      return transaction;
    }

    /// <summary>
    /// Rollback a potentially active transaction
    /// in case any existing transaction has not been rollbacked correctly
    /// </summary>
    public void CancelActiveTransaction ()
    {
      if (m_session.GetCurrentTransaction ()?.IsActive ?? false) {
        log.Warn ("CancelActiveTransaction: there is one active transaction, rollback it");
        RollbackTransaction ();
      }
    }

    /// <summary>
    /// Cancel all the pending saves
    /// 
    /// Clear the first level cache
    /// </summary>
    public void Clear ()
    {
      CancelActiveTransaction ();
      m_session.Clear ();
      m_valid = true;
    }

    void CheckNoTempView ()
    {
      var checkNoTempView = Lemoine.Info.ConfigSet
        .LoadAndGet (CHECK_NO_TEMP_VIEW_KEY, CHECK_NO_TEMP_VIEW_DEFAULT);
      if (!checkNoTempView) {
        return;
      }

      string viewExistsRequest = $@"
SELECT EXISTS (
SELECT 1 FROM information_schema.tables
WHERE table_type='LOCAL TEMPORARY'
) AS exists;
";
      var exists = NHibernateHelper.GetCurrentSession ()
        .CreateSQLQuery (viewExistsRequest)
        .AddScalar ("exists", NHibernate.NHibernateUtil.Boolean)
        .UniqueResult<bool> ();
      if (exists) {
        log.Fatal ("CheckNoActiveTempView: there are temporary views");
        Debug.Assert (false);
      }
    }

    /// <summary>
    /// Implements IDisposable
    /// </summary>
    public void Dispose ()
    {
      log.Trace ("Dispose /B");

      if (m_sessionCreated) {
        log.Debug ("Dispose: the session was created by this");

        if (null != s_uniqueSession) {
          s_uniqueSession = null;
        }

        // m_session is reset to null only after being disposed
        Debug.Assert (null != m_session);
        if (null == m_session) {
          log.Fatal ($"Dispose: DAOSession has already been disposed: at {System.Environment.StackTrace}");
          CurrentSessionContext.Unbind (NHibernateHelper.SessionFactory); // Just in case
          return;
        }

        // Check there is no remaining active transaction
        if (m_session.GetCurrentTransaction ()?.IsActive ?? false) {
          log.Fatal ($"Dispose: there is still one active transaction, try to rollback it: at {System.Environment.StackTrace}");
          try {
            RollbackTransaction ();
          }
          catch (Exception ex) {
            log.Error ("Dispose: RollbackTransaction failed", ex);
          }
        }
        else if (!m_rolledBack) {
          // In case of active updates outside a transaction,
          // and if no transaction was rolled back before
          log.Debug ($"Dispose: flush");
          try {
            m_session.Flush ();
          }
          catch (Exception ex) {
            log.Error ($"Dispose: flush failed", ex);
          }
        }

        // Unbind the session and dispose it
        CurrentSessionContext.Unbind (NHibernateHelper.SessionFactory);
        m_session.Dispose ();
        m_session = null;
      }
    }

    internal void CommitTransaction (string transactionName, bool readOnly)
    {
      if (!m_valid) {
        log.Fatal ("CommitTransaction: try to commit a transaction in a not valid session");
      }

      if (m_transactionCreated) {
        if (log.IsDebugEnabled) {
          log.Debug ("CommitTransaction: about to empty the analysis accumulators and to commit the transaction");
        }
        ILog transactionLog = GetTransactionLog (transactionName);

        TimeSpan transactionDuration = DateTime.UtcNow.Subtract (m_transactionCreationDateTime);
        if (m_rolledBack) {
          log.Error ($"CommitTransaction: the current transaction has already been rolled back after {transactionDuration}\n{System.Environment.StackTrace}");
          transactionLog.Error ($"Commit - already rolled back - duration={transactionDuration}");
          foreach (var sessionAccumulator in NHibernateHelper.SessionAccumulators) {
            sessionAccumulator.Clear (m_session);
          }
          var exn = new InvalidOperationException ("CommitTransaction with an already rolled back transaction");
          if (!readOnly) {
            foreach (var transactionExtension in GetTransactionExtensions ()) {
              transactionExtension.CommitFailure (transactionName, exn);
            }
          }
          throw exn;
        }

        // m_session should not be null
        Debug.Assert (null != m_session);
        if (null == m_session) {
          log.Fatal ($"CommitTransaction: current session is null after {transactionDuration} => can't commit any transaction. \n{System.Environment.StackTrace}");
          transactionLog.Fatal ($"Commit - null session - duration={transactionDuration}");
          m_valid = false;
          foreach (var sessionAccumulator in NHibernateHelper.SessionAccumulators) {
            sessionAccumulator.Clear (m_session);
          }
          var exn = new InvalidOperationException ("CommitTransaction with a null internal session");
          if (!readOnly) {
            foreach (var transactionExtension in GetTransactionExtensions ()) {
              transactionExtension.CommitFailure (transactionName, exn);
            }
          }
          throw exn;
        }

        ITransaction transaction = m_session.GetCurrentTransaction ();
        if (transaction is null) {
          log.Fatal ($"CommitTransaction: the transaction associated to session {m_session} is null after {transactionDuration} => can't commit the transaction");
          transactionLog.Fatal ($"Commit - null transaction - duration={transactionDuration}");
          m_valid = false;
          foreach (var sessionAccumulator in NHibernateHelper.SessionAccumulators) {
            sessionAccumulator.Clear (m_session);
          }
          var exn = new InvalidOperationException ("CommitTransaction with a null internal transaction");
          if (!readOnly) {
            foreach (var transactionExtension in GetTransactionExtensions ()) {
              transactionExtension.CommitFailure (transactionName, exn);
            }
          }
          throw exn;
        }
        if (!transaction.IsActive) {
          log.Fatal ($"CommitTransaction: try to commit a transaction while it is not active any more after {transactionDuration}. Has it already been rolled back and no exception raised?\n{System.Environment.StackTrace}");
          transactionLog.Fatal ($"Commit - inactive transaction - duration={transactionDuration}");
          m_valid = false;
          foreach (var sessionAccumulator in NHibernateHelper.SessionAccumulators) {
            sessionAccumulator.Clear (m_session);
          }
          var exn = new InvalidOperationException ("CommitTransaction with an inactive transaction");
          if (!readOnly) {
            foreach (var transactionExtension in GetTransactionExtensions ()) {
              transactionExtension.CommitFailure (transactionName, exn);
            }
          }
          throw exn;
        }

        if (!readOnly) {
          if (log.IsDebugEnabled) {
            log.Debug ("CommitTransaction: not a read only transaction, store the accumulator content");
          }
          try {
            foreach (var sessionAccumulator in NHibernateHelper.SessionAccumulators) {
              sessionAccumulator.Store (m_session, transactionName);
            }
          }
          catch (Exception ex) {
            transactionDuration = DateTime.UtcNow.Subtract (m_transactionCreationDateTime);
            log.Error ($"CommitTransaction: storing and purging the accumulators failed. Transaction duration: {transactionDuration}");
            transactionLog.Error ($"Commit - accumulator error - duration={transactionDuration}");
            m_valid = false;
            foreach (var transactionExtension in GetTransactionExtensions ()) {
              transactionExtension.CommitFailure (transactionName, ex);
            }
            throw;
          }
        }
#if DEBUG
        else { // m_readOnly
          if (log.IsDebugEnabled) {
            log.Debug ("CommitTransaction: read-only transaction => do not store the accumulator");
          }
          // TODO: check the accumulators are empty
        }
#endif // DEBUG

        if (transaction.WasRolledBack) {
          transactionDuration = DateTime.UtcNow.Subtract (m_transactionCreationDateTime);
          log.Error ($"CommitTransaction: try to commit a transaction that has already been rolled back. Transaction duration: {transactionDuration}\n{System.Environment.StackTrace}");
          transactionLog.Error ($"Commit - already rolled back 2 - duration={transactionDuration}");
        }
        if (!transaction.IsActive) {
          transactionDuration = DateTime.UtcNow.Subtract (m_transactionCreationDateTime);
          log.Fatal ($"CommitTransaction: try to commit a transaction while it is not active any more. Has it already been rolled back and no exception raised? Transaction duration: {transactionDuration}\n{System.Environment.StackTrace}");
          transactionLog.Fatal ($"Commit - inactive transaction 2 - duration={transactionDuration}");
          m_valid = false;
          var exn = new InvalidOperationException ("CommitTransaction with an inactive transaction");
          if (!readOnly) {
            foreach (var transactionExtension in GetTransactionExtensions ()) {
              transactionExtension.CommitFailure (transactionName, exn);
            }
          }
          throw exn;
        }

        if (!readOnly) {
          foreach (var transactionExtension in GetTransactionExtensions ()) {
            transactionExtension.BeforeCommit (transactionName);
          }
        }

        transaction.Commit ();
        ResetTransactionProperties ();
        transactionDuration = DateTime.UtcNow.Subtract (m_transactionCreationDateTime);
        if (log.IsDebugEnabled) {
          log.Debug ($"CommitTransaction: transaction committed. Transaction duration: {transactionDuration}");
        }
        transactionLog.Info ($"Commit - Success - duration={transactionDuration}");
        if (!readOnly) {
          // Messages
          var sendMessagesMethod = Lemoine.Info.ConfigSet
            .LoadAndGet (SEND_MESSAGES_METHOD_KEY, SEND_MESSAGES_METHOD_DEFAULT);
          switch (sendMessagesMethod) {
            case "pool":
              SendAccumulatorMessagesInPool ();
              break;
            case "thread":
              SendAccumulatorMessagesInThread ();
              break;
            case "async":
              SendAccumulatorMessagesAsync ().Wait ();
              break;
            case "nowait":
              var _ = SendAccumulatorMessagesAsync ();
              break;
            case "":
            case "sync":
            default:
              SendAccumulatorMessages ();
              break;
          }
          if (log.IsDebugEnabled) {
            log.Debug ($"CommitTransactions: messages were sent");
          }
        }
        // Extensions
        if (!readOnly) {
          foreach (var transactionExtension in GetTransactionExtensions ()) {
            if (log.IsDebugEnabled) {
              log.Debug ($"CommitTransactions: about to commit success in {transactionExtension} for transation {transactionName}");
            }
            transactionExtension.CommitSuccess (transactionName);
          }
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"CommitTransactions: Commit transaction completed");
        }
      } // m_transactionCreated
    }

    bool SendAccumulatorMessagesInPool ()
    {
      void waitCallback (object x) => SendAccumulatorMessagesThreadVersion (x);
      if (!ThreadPool.QueueUserWorkItem (new WaitCallback (waitCallback))) {
        log.Error ($"SendAccumulatorMessagesInPool: task was not queued in the thread pool");
        return false;
      }
      else { // A thread was queued
        return true;
      }

    }

    void SendAccumulatorMessagesInThread ()
    {
      var thread = new System.Threading.Thread (new System.Threading.ParameterizedThreadStart (SendAccumulatorMessagesThreadVersion));
      thread.Start ();
    }

    void SendAccumulatorMessagesThreadVersion (object _)
    {
      try {
        SendAccumulatorMessages ();
      }
      catch (Exception ex) {
        log.Error ("SendAccumulatorMessagesThreadVersion: exception", ex);
        throw;
      }
    }

    void SendAccumulatorMessages ()
    {
      foreach (var sessionAccumulator in NHibernateHelper.SessionAccumulators) {
        SendAccumulatorMessages (sessionAccumulator);
      }
    }

    void SendAccumulatorMessages (ISessionAccumulator sessionAccumulator)
    {
      sessionAccumulator.SendMessages (m_session);
    }

    async System.Threading.Tasks.Task SendAccumulatorMessagesAsync ()
    {
      var tasks = NHibernateHelper.SessionAccumulators
        .Select (x => SendAccumulatorMessagesAsync (x));
      await System.Threading.Tasks.Task.WhenAll (tasks);
    }

    async System.Threading.Tasks.Task SendAccumulatorMessagesAsync (ISessionAccumulator sessionAccumulator)
    {
      await sessionAccumulator.SendMessagesAsync (m_session);
    }

    void ResetTransactionProperties ()
    {
      m_transactionCreated = false;
      m_rolledBack = false;
    }

    void RollbackTransaction ()
    {
      RollbackTransaction (null);
    }

    internal void RollbackTransaction (string transactionName)
    {
      RollbackTransaction (transactionName, false);
    }

    internal void RollbackTransaction (string transactionName, bool implicitRollback, bool readOnly = false)
    {
      // Note: this is hard to propagate the rollback event to parent sessions
      //       An exception should be raised in the same time

      if (!readOnly) {
        log.Warn ("RollbackTransaction: about to rollback the transaction");
      }
      else {
        log.Debug ("RollbackTransaction: roll back a read-only transaction");
      }
      ILog transactionLog = GetTransactionLog (transactionName);
      TimeSpan transactionDuration = DateTime.UtcNow.Subtract (m_transactionCreationDateTime);

      if (m_rolledBack) {
        if (implicitRollback) {
          log.Info ("RollbackTransaction: (implicit) the transaction has already been rolled back");
          transactionLog.Info ($"Rollback - (implicit) already rolled back - duration={transactionDuration}");
        }
        else {
          log.Warn ("RollbackTransaction: (not implicit) the transaction has already been rolled back");
          transactionLog.Warn ($"Rollback - (not implicit) already rolled back - duration={transactionDuration}");
        }
        return;
      }

      // Clear the accumulators
      foreach (var sessionAccumulator in NHibernateHelper.SessionAccumulators) {
        try {
          sessionAccumulator.Clear (m_session);
        }
        catch (Exception ex) {
          log.Error ("RollbackTransaction: the accumulators could not be cleared", ex);
        }
      }

      Debug.Assert (null != m_session);
      ITransaction transaction = m_session.GetCurrentTransaction ();
      if (transaction is null) {
        m_valid = false;
        log.Error ("RollbackTransaction: the transaction associated to session is null => can't rollback the transaction");
        transactionLog.Error ($"Rollback - null session - duration={transactionDuration}");
        var exn = new InvalidOperationException ("RollbackTransaction with a null internal session");
        if (!readOnly) {
          foreach (var transactionExtension in GetTransactionExtensions ()) {
            transactionExtension.RollbackFailure (transactionName, exn);
          }
        }
        throw exn;
      }
      if (transaction.WasRolledBack) {
        if (log.IsDebugEnabled) {
          log.Debug ($"RollbackTransaction: the transaction has already been rolled back once. at {System.Environment.StackTrace}");
        }
        if (transactionLog.IsDebugEnabled) {
          transactionLog.Debug ($"Rollback - already rolled back - duration={transactionDuration}");
        }
      }
      if (!transaction.IsActive) {
        if (implicitRollback) {
          if (log.IsInfoEnabled) {
            log.Info ($"RollbackTransaction: (implicit) the transaction is already not active any more. This is probably ok because usually the implicit rollback occur with an exception and there was already a automatic rollback in database at {System.Environment.StackTrace}");
          }
          transactionLog.InfoFormat ("Rollback - (implicit) inactive transaction - duration={0}",
                                     transactionDuration);
        }
        else {
          log.Fatal ($"RollbackTransaction: try to rollback a transaction while it is not active any more. Has it already been rolled back and no exception raised ? => do not try to roll it back once again at {System.Environment.StackTrace}");
          transactionLog.Fatal ($"Rollback - inactive transaction - duration={transactionDuration}");
        }
      }
      else {
        try {
          transaction.Rollback ();
          transactionLog.Info ($"Rollback - Success - duration={transactionDuration}");
        }
        catch (NHibernate.TransactionException ex) { // NullReferenceException in Npgsql 2.x
          // To remove once Npgsql version >= 4.x is used
          if (IsNpgsqlNullReferenceException (ex)) {
            log.Fatal ("RollbackTransaction: bug in NHibernate / Npgsql 2.x => consider it is as a broken connection");
            transactionLog.Fatal ($"Rollback - Failure - duration={transactionDuration}");
          }
          else {
            log.Fatal ("RollbackTransaction: Rollback failed", ex);
            transactionLog.Fatal ($"Rollback - Failure - duration={transactionDuration}");
          }
          if (!readOnly) {
            foreach (var transactionExtension in GetTransactionExtensions ()) {
              transactionExtension.RollbackFailure (transactionName, ex);
            }
          }
          m_valid = false;
          throw;
        }
        catch (Exception ex) {
          log.Fatal ("RollbackTransaction: Rollback failed", ex);
          transactionLog.Fatal ($"Rollback - Failure - duration={transactionDuration}");
          if (!readOnly) {
            foreach (var transactionExtension in GetTransactionExtensions ()) {
              transactionExtension.RollbackFailure (transactionName, ex);
            }
          }
          m_valid = false;
          throw;
        }
        if (!readOnly) {
          foreach (var transactionExtension in GetTransactionExtensions ()) {
            transactionExtension.RollbackSuccess (transactionName, implicitRollback);
          }
        }
        m_rolledBack = true;
      }

      // To prevent parent sessions to use again the same connection,
      // and to success a commit in another transaction,
      // add an additional protection in case of an implicit rollback:
      // disconnect from the database
      if (implicitRollback) {
        m_valid = false;
        log.Info ("RollbackTransaction: implicit rollback, disconnect. Consider it as an additional protection");
        try {
          m_session.Disconnect ();
        }
        catch (Exception ex) {
          log.Error ("RollbackTransaction: disconnect failed", ex);
        }
      }
    }

    bool IsNpgsqlNullReferenceException (Exception ex)
    {
      if (ex is NullReferenceException) {
        if (ex.StackTrace.Contains ("NpgsqlCommand.ClearPoolAndCreateException")) {
          return true;
        }
      }
      if (null != ex.InnerException) {
        return IsNpgsqlNullReferenceException (ex.InnerException);
      }
      return false;
    }

    ILog GetTransactionLog (string transactionName)
    {
      return LogManager.GetLogger ("Transaction." + (transactionName ?? "NoName"));
    }
  }
}
