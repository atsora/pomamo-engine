// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Threading
{
  /// <summary>
  /// Hold a ReaderWriterLock in read mode.
  /// Helps using a ReaderWriterLock with a using directive.
  /// </summary>
  public class ReadLockHolder: IDisposable
  {
    #region Members
    readonly ReaderWriterLock rwLock;
    #endregion
    
    static readonly ILog log = LogManager.GetLogger(typeof (ReadLockHolder).FullName);

    #region Getters / Setters
    /// <summary>
    /// Lock
    /// </summary>
    public ReaderWriterLock RwLock {
      get { return rwLock; }
    }
    #endregion

    #region Constructors / Dispose methods
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rwLock">Read/Write lock to hold</param>
    public ReadLockHolder (ReaderWriterLock rwLock)
    {
      this.rwLock = rwLock;
      DateTime timer = DateTime.UtcNow;
      log.DebugFormat ("ReadLockHolder: " +
                       "waiting for acquiring lock {0}",
                       rwLock.GetHashCode ());
      rwLock.AcquireReaderLock (-1);
      TimeSpan duration = DateTime.UtcNow - timer;
      log.DebugFormat ("ReadLockHolder: " +
                       "lock {0} acquired in {1}",
                       rwLock.GetHashCode (),
                       duration);
      if (TimeSpan.FromSeconds (1) < duration) {
        log.WarnFormat ("ReadLockHolder: " +
                        "lock {0} has been acquired in only {1}",
                        rwLock.GetHashCode (),
                        duration);
      }
    }
    
    /// <summary>
    /// Dispose method to be used with using ()
    /// </summary>
    public void Dispose ()
    {
      rwLock.ReleaseReaderLock ();
      log.DebugFormat ("ReadLockHolder: " +
                       "lock {0} released",
                       rwLock);
    }
    #endregion
  }
  
  
  /// <summary>
  /// Hold a ReaderWriterLock in write mode.
  /// Helps using a ReaderWriterLock with a using directive.
  /// </summary>
  public class WriteLockHolder: IDisposable
  {
    #region Members
    readonly ReaderWriterLock rwLock;
    #endregion
    
    static readonly ILog log = LogManager.GetLogger(typeof (WriteLockHolder).FullName);

    #region Getters / Setters
    /// <summary>
    /// Lock
    /// </summary>
    public ReaderWriterLock RwLock {
      get { return rwLock; }
    }
    #endregion

    #region Constructors / Dispose methods
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rwLock">Read/Write lock to hold</param>
    public WriteLockHolder (ReaderWriterLock rwLock)
    {
      this.rwLock = rwLock;
      DateTime timer = DateTime.UtcNow;
      log.DebugFormat ("WriteLockHolder: " +
                       "waiting for acquiring lock {0}",
                       rwLock);
      rwLock.AcquireWriterLock (-1);
      TimeSpan duration = DateTime.UtcNow - timer;
      log.DebugFormat ("WriteLockHolder: " +
                       "lock {0} acquired in {1}",
                       rwLock,
                       duration);
      if (TimeSpan.FromSeconds (1) < duration) {
        log.WarnFormat ("WriteLockHolder: " +
                        "lock {0} has been acquired in only {1}",
                        rwLock.GetHashCode (),
                        duration);
      }
    }
    
    /// <summary>
    /// Dispose method to be used with using ()
    /// </summary>
    public void Dispose ()
    {
      rwLock.ReleaseWriterLock ();
      log.DebugFormat ("WriteLockHolder: " +
                       "lock {0} released",
                       rwLock);
    }
    #endregion
  }
  
  
  /// <summary>
  /// Upgrade a ReaderWriterLock.
  /// Helps using a ReaderWriterLock with a using directive.
  /// </summary>
  public class UpgradeLockHolder: IDisposable
  {
    #region Members
    readonly ReaderWriterLock rwLock;
    LockCookie lc;
    #endregion
    
    static readonly ILog log = LogManager.GetLogger(typeof (UpgradeLockHolder).FullName);

    #region Getters / Setters
    /// <summary>
    /// Lock
    /// </summary>
    public ReaderWriterLock RwLock {
      get { return rwLock; }
    }
    #endregion

    #region Constructors / Dispose methods
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rwLock">Read/Write lock to hold</param>
    public UpgradeLockHolder (ReaderWriterLock rwLock)
    {
      this.rwLock = rwLock;
      DateTime timer = DateTime.UtcNow;
      log.DebugFormat ("UpgradeLockHolder: " +
                       "waiting for upgrading lock {0}",
                       rwLock);
      this.lc = rwLock.UpgradeToWriterLock (-1);
      TimeSpan duration = DateTime.UtcNow - timer;
      log.DebugFormat ("UpgradeLockHolder: " +
                       "lock {0} upgraded in {1}",
                       rwLock.GetHashCode (),
                       duration);
      if (TimeSpan.FromSeconds (1) < duration) {
        log.WarnFormat ("UpgradeLockHolder: " +
                        "lock {0} has been acquired in only {1}",
                        rwLock.GetHashCode (),
                        duration);
      }
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="readLockHolder">Read lock holder</param>
    public UpgradeLockHolder (ReadLockHolder readLockHolder)
    {
      this.rwLock = readLockHolder.RwLock;
      DateTime timer = DateTime.UtcNow;
      log.DebugFormat ("UpgradeLockHolder: " +
                       "waiting for upgrading lock {0}",
                       rwLock);
      this.lc = rwLock.UpgradeToWriterLock (-1);
      TimeSpan duration = DateTime.UtcNow - timer;
      log.DebugFormat ("UpgradeLockHolder: " +
                       "lock {0} upgraded in {1}",
                       rwLock.GetHashCode (),
                       duration);
      if (TimeSpan.FromSeconds (1) < duration) {
        log.WarnFormat ("UpgradeLockHolder: " +
                        "lock {0} has been acquired in only {1}",
                        rwLock.GetHashCode (),
                        duration);
      }
    }
    
    /// <summary>
    /// Dispose method to be used with using ()
    /// </summary>
    public void Dispose ()
    {
      rwLock.DowngradeFromWriterLock (ref this.lc);
      log.DebugFormat ("UpgradeLockHolder: " +
                       "lock {0} downgraded",
                       rwLock);
    }
    #endregion
  }
}
