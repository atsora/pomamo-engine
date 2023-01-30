// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;

namespace Lemoine.Core.Hosting.LctrChecker
{
  /// <summary>
  /// If available, check in database first, else use <see cref="PfrDataDirectoryLctrChecker"/>
  /// </summary>
  public class CheckDatabaseLctrChecker: ILctrChecker
  {
    readonly ILog log = LogManager.GetLogger (typeof (CheckDatabaseLctrChecker).FullName);

    static readonly string FORCE_LCTR_KEY = "ForceLctr";
    const bool FORCE_LCTR_DEFAULT = false;

    readonly IDatabaseConnectionStatus m_databaseConnectionStatus = null;
    readonly ILctrChecker m_databaseLctrChecker;
    readonly ILctrChecker m_fallbackLctrChecker = new PfrDataDirectoryLctrChecker ();

    /// <summary>
    /// Force lctr key
    /// </summary>
    public string ForceLctrKey { get; set; } = FORCE_LCTR_KEY;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="databaseLctrChecker">not null</param>
    /// <param name="databaseConnectionStatus">nullable. If forced to null, try anyway in the database first</param>
    public CheckDatabaseLctrChecker (IDatabaseLctrChecker databaseLctrChecker, IDatabaseConnectionStatus databaseConnectionStatus)
    {
      Debug.Assert (null != databaseLctrChecker);

      m_databaseLctrChecker = new CachedLctrChecker (databaseLctrChecker);
      m_databaseConnectionStatus = databaseConnectionStatus;
    }

    /// <summary>
    /// <see cref="ILctrChecker"/>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public bool IsLctr ()
    {
      var forceLctr = Lemoine.Info.ConfigSet
        .LoadAndGet<bool> (this.ForceLctrKey, FORCE_LCTR_DEFAULT);
      if (forceLctr) {
        return true;
      }

      if (m_databaseConnectionStatus?.IsDatabaseConnectionUp ?? true) {
        try {
          return m_databaseLctrChecker.IsLctr ();
        }
        catch (Exception ex) {
          log.Error ($"IsLctr: trying to determine if this computer is lctr from the database failed", ex);
        }

        var isLctr = m_fallbackLctrChecker.IsLctr ();
        log.Warn ($"IsLctr: get isLctr={isLctr} from {m_fallbackLctrChecker}");
        return isLctr;
      }
      else {
        var isLctr = m_fallbackLctrChecker.IsLctr ();
        if (log.IsDebugEnabled) {
          log.Debug ($"IsLctr: get isLctr={isLctr} from {m_fallbackLctrChecker} because the database is not up");
        }
        return isLctr;
      }
    }
  }
}
