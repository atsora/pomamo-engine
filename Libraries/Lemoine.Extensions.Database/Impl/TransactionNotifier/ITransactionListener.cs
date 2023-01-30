// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Extensions.Database.Impl.TransactionNotifier
{
  /// <summary>
  /// Description of ITransactionListener.
  /// </summary>
  public interface ITransactionListener
  {
    /// <summary>
    /// Run before a commit
    /// </summary>
    void BeforeCommit ();
    
    /// <summary>
    /// Run after a rollback
    /// </summary>
    void AfterRollback ();
    
    /// <summary>
    /// Run after a commit
    /// </summary>
    void AfterCommit ();
  }
}
