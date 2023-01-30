// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.Threading;
using Pulse.Business.Computer;

namespace Lem_AutoReasonService
{
  /// <summary>
  /// ServerChecker
  /// </summary>
  public class ServerChecker : ILctrChecker, IValidServerChecker, IApplicationInitializer
  {
    static readonly string CHECK_AUTO_REASON_SERVER_KEY = "AutoReason.CheckServer";
    static readonly bool CHECK_AUTO_REASON_SERVER_DEFAULT = true;

    readonly ILog log = LogManager.GetLogger (typeof (ServerChecker).FullName);

    SemaphoreSlim m_semaphore = new SemaphoreSlim (1, 1);
    bool m_initialized = false;
    bool m_isAutoReasonServerLctr;
    bool m_isAutoReasonServer;

    /// <summary>
    /// Constructor
    /// </summary>
    public ServerChecker ()
    {
    }

    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      log.Info ("InitializeApplication: check if the server is an auto-reason server");

      // Check the computer is the auto-reason server
      // not to run twice the auto-reason service
      // If not the auto-reason server, just do nothing
      if (!IsValidServerForService () && Lemoine.Info.ConfigSet
        .LoadAndGet<bool> (CHECK_AUTO_REASON_SERVER_KEY, CHECK_AUTO_REASON_SERVER_DEFAULT)) {
        log.Error ("InitializeApplication: do nothing, not the right server");
        cancellationToken.WaitHandle.WaitOne ();
        cancellationToken.ThrowIfCancellationRequested ();
      }
    }

    public async Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      log.Info ("InitializeApplicationAsync: check if the server is an auto-reason server");

      // Check the computer is the auto-reason server
      // not to run twice the auto-reason service
      // If not the auto-reason server, just do nothing
      if (!IsValidServerForService () && Lemoine.Info.ConfigSet
        .LoadAndGet<bool> (CHECK_AUTO_REASON_SERVER_KEY, CHECK_AUTO_REASON_SERVER_DEFAULT)) {
        log.Error ("InitializeApplicationAsync: do nothing, not the right server");
        cancellationToken.WaitHandle.WaitOne ();
        cancellationToken.ThrowIfCancellationRequested ();
        await Task.Delay (0);
      }
    }

    public bool IsLctr ()
    {
      Initialize ();
      return m_isAutoReasonServerLctr;
    }

    public bool IsValidServerForService ()
    {
      Initialize ();
      return m_isAutoReasonServer;
    }

    void Initialize ()
    {
      if (!m_initialized) {
        using (var semaphoreHolder = SemaphoreSlimHolder.Create (m_semaphore)) {
          if (!m_initialized) {
            using (var session = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
              var autoReasonComputer = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.ComputerDAO
                .GetAutoReason ();
              if (autoReasonComputer is null) {
                log.Error ("Initialize: no auto-reason server is defined in table computer => return false");
                m_isAutoReasonServerLctr = false;
                m_isAutoReasonServer = false;
              }
              else {
                m_isAutoReasonServer = autoReasonComputer.IsLocal ();
                if (!m_isAutoReasonServer) {
                  log.Fatal ("Initialize: not the auto-reason server");
                }
                m_isAutoReasonServerLctr = autoReasonComputer.IsLctr;
              }
            }
            m_initialized = true;
          }
        }
      }
    }
  }
}
