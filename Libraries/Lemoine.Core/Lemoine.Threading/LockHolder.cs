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
    readonly ReaderWriterLock rwLock;
    
    static readonly ILog log = LogManager.GetLogger(typeof (ReadLockHolder).FullName);

    /// <summary>
    /// Lock
    /// </summary>
    public ReaderWriterLock RwLock {
      get { return rwLock; }
    }

    #region Constructors / Dispose methods
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rwLock">Read/Write lock to hold</param>
    public ReadLockHolder (ReaderWriterLock rwLock)
    {
      this.rwLock = rwLock;
      DateTime timer = DateTime.UtcNow;
      if (log.IsDebugEnabled) {
        log.Debug ($"ReadLockHolder: waiting for acquiring lock {rwLock.GetHashCode ()}");
      }
      rwLock.AcquireReaderLock (-1);
      TimeSpan duration = DateTime.UtcNow - timer;
      if (log.IsDebugEnabled) {
        log.Debug ($"ReadLockHolder: lock {rwLock.GetHashCode ()} acquired in {duration}");
      }
      if (TimeSpan.FromSeconds (1) < duration) {
        log.Warn ($"ReadLockHolder: lock {rwLock.GetHashCode ()} has been acquired in only {duration}");
      }
    }
    
    /// <summary>
    /// Dispose method to be used with using ()
    /// </summary>
    public void Dispose ()
    {
      rwLock.ReleaseReaderLock ();
      if (log.IsDebugEnabled) {
        log.Debug ($"ReadLockHolder: lock {rwLock} released");
      }
    }
    #endregion
  }
  
  
  /// <summary>
  /// Hold a ReaderWriterLock in write mode.
  /// Helps using a ReaderWriterLock with a using directive.
  /// </summary>
  public class WriteLockHolder: IDisposable
  {
    readonly ReaderWriterLock rwLock;
    
    static readonly ILog log = LogManager.GetLogger(typeof (WriteLockHolder).FullName);

    /// <summary>
    /// Lock
    /// </summary>
    public ReaderWriterLock RwLock {
      get { return rwLock; }
    }

    #region Constructors / Dispose methods
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rwLock">Read/Write lock to hold</param>
    public WriteLockHolder (ReaderWriterLock rwLock)
    {
      this.rwLock = rwLock;
      DateTime timer = DateTime.UtcNow;
      if (log.IsDebugEnabled) {
        log.Debug ($"WriteLockHolder: waiting for acquiring lock {rwLock}");
      }
      rwLock.AcquireWriterLock (-1);
      TimeSpan duration = DateTime.UtcNow - timer;
      if (log.IsDebugEnabled) {
        log.Debug ($"WriteLockHolder: lock {rwLock} acquired in {duration}");
      }
      if (TimeSpan.FromSeconds (1) < duration) {
        log.Warn ($"WriteLockHolder: lock {rwLock.GetHashCode ()} has been acquired in only {duration}");
      }
    }
    
    /// <summary>
    /// Dispose method to be used with using ()
    /// </summary>
    public void Dispose ()
    {
      rwLock.ReleaseWriterLock ();
      if (log.IsDebugEnabled) {
        log.Debug ($"WriteLockHolder: lock {rwLock} released");
      }
    }
    #endregion
  }
  
  
  /// <summary>
  /// Upgrade a ReaderWriterLock.
  /// Helps using a ReaderWriterLock with a using directive.
  /// </summary>
  public class UpgradeLockHolder: IDisposable
  {
    readonly ReaderWriterLock rwLock;
    LockCookie lc;
    
    static readonly ILog log = LogManager.GetLogger(typeof (UpgradeLockHolder).FullName);

    /// <summary>
    /// Lock
    /// </summary>
    public ReaderWriterLock RwLock {
      get { return rwLock; }
    }

    #region Constructors / Dispose methods
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rwLock">Read/Write lock to hold</param>
    public UpgradeLockHolder (ReaderWriterLock rwLock)
    {
      this.rwLock = rwLock;
      DateTime timer = DateTime.UtcNow;
      if (log.IsDebugEnabled) {
        log.Debug ($"UpgradeLockHolder: waiting for upgrading lock {rwLock}");
      }
      this.lc = rwLock.UpgradeToWriterLock (-1);
      TimeSpan duration = DateTime.UtcNow - timer;
      if (log.IsDebugEnabled) {
        log.Debug ($"UpgradeLockHolder: lock {rwLock.GetHashCode ()} upgraded in {duration}");
      }
      if (TimeSpan.FromSeconds (1) < duration) {
        log.Warn ($"UpgradeLockHolder: lock {rwLock.GetHashCode ()} has been acquired in only {duration}");
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
      if (log.IsDebugEnabled) {
        log.Debug ($"UpgradeLockHolder: waiting for upgrading lock {rwLock}");
      }
      this.lc = rwLock.UpgradeToWriterLock (-1);
      TimeSpan duration = DateTime.UtcNow - timer;
      if (log.IsDebugEnabled) {
        log.Debug ($"UpgradeLockHolder: lock {rwLock.GetHashCode ()} upgraded in {duration}");
      }
      if (TimeSpan.FromSeconds (1) < duration) {
        log.Warn ($"UpgradeLockHolder: lock {rwLock.GetHashCode ()} has been acquired in only {duration}");
      }
    }
    
    /// <summary>
    /// Dispose method to be used with using ()
    /// </summary>
    public void Dispose ()
    {
      rwLock.DowngradeFromWriterLock (ref this.lc);
      if (log.IsDebugEnabled) {
        log.Debug ($"UpgradeLockHolder: lock {rwLock} downgraded");
      }
    }
    #endregion
  }
}
