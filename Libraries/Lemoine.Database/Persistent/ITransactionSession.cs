// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// Session that manages the transactions of a <see cref="DAOTransaction"/>
  /// </summary>
  internal interface ITransactionSession
  {
    /// <summary>
    /// Initialize a new transaction
    /// </summary>
    /// <param name="transactionName"></param>
    /// <param name="readOnly"></param>
    /// <returns>a new transaction was created</returns>
    bool InitializeTransaction (string transactionName, bool readOnly = false);

    /// <summary>
    /// Commit the transaction
    /// </summary>
    /// <param name="transactionName"></param>
    /// <param name="readOnly"></param>
    void CommitTransaction (string transactionName, bool readOnly);

    /// <summary>
    /// Rollback the transaction
    /// </summary>
    /// <param name="transactionName"></param>
    /// <param name="implicitRollback"></param>
    /// <param name="readOnly"></param>
    void RollbackTransaction (string transactionName, bool implicitRollback, bool readOnly = false);
  }

  /// <summary>
  /// <see cref="ITransactionSession"/> implementation for a <see cref="DAOSession"/>
  /// </summary>
  internal sealed class DAOSessionTransactionAdapter : ITransactionSession
  {
    readonly DAOSession m_session;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="session"></param>
    public DAOSessionTransactionAdapter (DAOSession session)
    {
      m_session = session;
    }

    /// <summary>
    /// <see cref="ITransactionSession"/>
    /// </summary>
    public bool InitializeTransaction (string transactionName, bool readOnly = false) => m_session.InitializeTransaction (transactionName, readOnly);

    /// <summary>
    /// <see cref="ITransactionSession"/>
    /// </summary>
    public void CommitTransaction (string transactionName, bool readOnly) => m_session.CommitTransaction (transactionName, readOnly);

    /// <summary>
    /// <see cref="ITransactionSession"/>
    /// </summary>
    public void RollbackTransaction (string transactionName, bool implicitRollback, bool readOnly = false) => m_session.RollbackTransaction (transactionName, implicitRollback, readOnly);
  }
}
