// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Threading
{
  /// <summary>
  /// Mutex holder
  /// </summary>
  public class MutexHolder: IDisposable
  {
    #region Members
    readonly Mutex m_mutex;
    bool m_mutexOwned = false;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger(typeof (MutexHolder).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor with an infinite timeout
    /// </summary>
    /// <param name="mutex"></param>
    public MutexHolder (Mutex mutex)
      : this (mutex, Timeout.Infinite)
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="mutex"></param>
    /// <param name="timeout">timeout in ms or Timeout.Infinite</param>
    public MutexHolder (Mutex mutex, int timeout)
    {
      m_mutex = mutex;
      try {
        m_mutexOwned = mutex.WaitOne (timeout, false);
        if (false == m_mutexOwned) {
          log.ErrorFormat ("MutexHolder: " +
                           "mutex was not acquired after {0} ms",
                           timeout);
          throw new TimeoutException ("Timeout for acquiring a mutex");
        }
      }
      catch (AbandonedMutexException) {
        log.WarnFormat ("MutexHolder: " +
                        "mutex was abandoned");
        m_mutexOwned = true;
      }
      if (!m_mutexOwned) {
        log.FatalFormat ("MutexHolder: " +
                         "the mutex is not owned");
      }
    }    
    #endregion // Constructors

    /// <summary>
    /// Dispose method to be used with using ()
    /// </summary>
    public void Dispose ()
    {
      if (m_mutexOwned) {
        try {
          m_mutex.ReleaseMutex ();
        }
        catch (Exception ex) {
          log.FatalFormat ("Dispose: " +
                           "ReleaseMutex failed with: " +
                           "{0}",
                           ex);
        }
      }
    }
  }
}
