// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using Lemoine.Threading;
using Lemoine.Core.Log;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// Description of CncLicenseManager.
  /// </summary>
  public sealed class CncLicenseManager
  {
    #region Members
    ReaderWriterLock licenseLock = new ReaderWriterLock ();
    int freeLicenses = 0;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (CncLicenseManager).FullName);

    #region Getters / Setters
    /// <summary>
    /// Remaining free licenses
    /// </summary>
    public static int FreeLicenses {
      get
      {
        using (ReadLockHolder readHolder =
               new ReadLockHolder (Instance.licenseLock))
        {
          return Instance.freeLicenses;
        }
      }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// Initialize the number of licenses
    /// </summary>
    private CncLicenseManager()
    {
      // TODO: use a new license system
      try {
        bool keyValid = true;
        if (true == keyValid) {
          freeLicenses = int.MaxValue;
          log.Info ($"CncLicenseManager: the number of licenses is {freeLicenses}");
        }
      }
      catch (Exception ex) {
        log.Error ($"CncLicenseManager: got exception", ex);
      }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Get a license.
    /// If there is no CNC license available any more,
    /// false is returned
    /// </summary>
    /// <returns>Could get a CNC license</returns>
    public static bool GetLicense ()
    {
      using (ReadLockHolder readHolder =
             new ReadLockHolder (Instance.licenseLock))
      {
        if (0 == Instance.freeLicenses) {
          log.ErrorFormat ("GetLicense: " +
                           "no license available");
          return false;
        }
        using (UpgradeLockHolder upgradeHolder =
               new UpgradeLockHolder (readHolder))
        {
          --Instance.freeLicenses;
          log.DebugFormat ("GetLicense: " +
                           "{0} remaining licenses",
                           Instance.freeLicenses);
          return true;
        }
      }
    }
    #endregion
    
    #region Instance
    static CncLicenseManager Instance
    {
      get { return Nested.instance; }
    }
    
    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested()
      {
      }

      internal static readonly CncLicenseManager instance = new CncLicenseManager ();
    }
    #endregion
  }
}
