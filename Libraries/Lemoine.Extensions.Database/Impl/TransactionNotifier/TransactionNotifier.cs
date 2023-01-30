// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Threading;

namespace Lemoine.Extensions.Database.Impl.TransactionNotifier
{
  /// <summary>
  /// Description of TransactionNotifier.
  /// </summary>
  public sealed class TransactionNotifier
  {
    #region Members
    IList<ITransactionListener> m_listeners = new List<ITransactionListener> ();
    readonly SemaphoreSlim m_listenersSemaphore = new SemaphoreSlim (1, 1);
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (TransactionNotifier).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private TransactionNotifier ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a listener
    /// </summary>
    /// <param name="listener"></param>
    public static void AddListener (ITransactionListener listener)
    {
      using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (Instance.m_listenersSemaphore)) {
        Instance.m_listeners.Add (listener);
      }
    }

    /// <summary>
    /// Notify a commit is about to be run
    /// </summary>
    public static void BeforeCommit ()
    {
      using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (Instance.m_listenersSemaphore)) {
        foreach (var listener in Instance.m_listeners) {
          listener.BeforeCommit ();
        }
      }
    }

    /// <summary>
    /// Notify a transaction rollback
    /// </summary>
    public static void AfterRollback ()
    {
      using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (Instance.m_listenersSemaphore)) {
        foreach (var listener in Instance.m_listeners) {
          listener.AfterRollback ();
        }
      }
    }

    /// <summary>
    /// Notify a commit
    /// </summary>
    public static void AfterCommit ()
    {
      using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (Instance.m_listenersSemaphore)) {
        foreach (var listener in Instance.m_listeners) {
          listener.AfterCommit ();
        }
      }
    }
    #endregion // Methods

    #region Instance
    static TransactionNotifier Instance
    {
      get { return Nested.instance; }
    }

    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested ()
      {
      }

      internal static readonly TransactionNotifier instance = new TransactionNotifier ();
    }
    #endregion // Instance
  }
}
