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

namespace Lem_SynchronizationService
{
  /// <summary>
  /// ServerChecker
  /// </summary>
  public class ServerChecker : ILctrChecker, IValidServerChecker, IApplicationInitializer
  {
    static readonly string CHECK_SYNCHRONIZATION_SERVER_KEY = "Synchronization.CheckServer";
    static readonly bool CHECK_SYNCHRONIZATION_SERVER_DEFAULT = true;

    readonly ILog log = LogManager.GetLogger (typeof (ServerChecker).FullName);

    SemaphoreSlim m_semaphore = new SemaphoreSlim (1, 1);
    bool m_initialized = false;
    bool m_isSynchronizationServerLctr;
    bool m_isSynchronizationServer;

    /// <summary>
    /// Constructor
    /// </summary>
    public ServerChecker ()
    {
    }

    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      log.Info ("InitializeApplication: check if the server is the synchronization server");

      // Check the computer is the auto-reason server
      // not to run twice the auto-reason service
      // If not the auto-reason server, just do nothing
      if (!IsValidServerForService () && Lemoine.Info.ConfigSet
        .LoadAndGet<bool> (CHECK_SYNCHRONIZATION_SERVER_KEY, CHECK_SYNCHRONIZATION_SERVER_DEFAULT)) {
        log.Error ("InitializeApplication: do nothing, not the right server");
        cancellationToken.WaitHandle.WaitOne ();
        cancellationToken.ThrowIfCancellationRequested ();
      }
    }

    public async Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      log.Info ("InitializeApplicationAsync: check if the server is the synchronization server");

      // Check the computer is the auto-reason server
      // not to run twice the auto-reason service
      // If not the auto-reason server, just do nothing
      if (!IsValidServerForService () && Lemoine.Info.ConfigSet
        .LoadAndGet<bool> (CHECK_SYNCHRONIZATION_SERVER_KEY, CHECK_SYNCHRONIZATION_SERVER_DEFAULT)) {
        log.Error ("InitializeApplicationAsync: do nothing, not the right server");
        cancellationToken.WaitHandle.WaitOne ();
        cancellationToken.ThrowIfCancellationRequested ();
        await Task.Delay (0);
      }
    }

    public bool IsLctr ()
    {
      Initialize ();
      return m_isSynchronizationServerLctr;
    }

    public bool IsValidServerForService ()
    {
      Initialize ();
      return m_isSynchronizationServer;
    }

    void Initialize ()
    {
      if (!m_initialized) {
        using (var semaphoreHolder = SemaphoreSlimHolder.Create (m_semaphore)) {
          if (!m_initialized) {
            using (var session = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
              var synchronizationComputer = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.ComputerDAO
                .GetSynchronization ();
              if (synchronizationComputer is null) {
                log.Error ("Initialize: no alert server is defined in table computer => return false");
                m_isSynchronizationServerLctr = false;
                m_isSynchronizationServer = false;
              }
              else {
                m_isSynchronizationServer = synchronizationComputer.IsLocal ();
                if (!m_isSynchronizationServer) {
                  log.Fatal ("Initialize: not the synchronization server");
                }
                m_isSynchronizationServerLctr = synchronizationComputer.IsLctr;
              }
            }
            m_initialized = true;
          }
        }
      }
    }
  }
}
