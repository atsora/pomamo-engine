// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.WebMiddleware.Log;
using Lemoine.WebMiddleware.Response;
using Lemoine.Extensions.Web.Responses;
using Microsoft.AspNetCore.Http;
using Lemoine.Core.ExceptionManagement;
using System.Net.Sockets;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.WebMiddleware;

namespace Lem_AspService
{
  /// <summary>
  /// MaintenanceMiddleware: if an option is active, turn on the maintenance mode returning ErrorStatus.PulseMaintenance
  /// </summary>
  public class MaintenanceMiddleware
  {
    const string MAINTENANCE_ACTIVE_KEY = "MaintenanceActive";
    const bool MAINTENANCE_ACTIVE_DEFAULT = false;

    const string AUTHORIZED_SOURCES_KEY = "MaintenanceMiddleware.AuthorizedSources";
    const string AUTHORIZED_SOURCES_DEFAULT = "localhost"; // ; separated list

    const string BROADCAST_KEY = "Web.Broadcast";
    const bool BROADCAST_DEFAULT = true;

    readonly ILog log = LogManager.GetLogger<ExceptionMiddleware> ();

    readonly RequestDelegate m_next;
    readonly ResponseWriter m_responseWriter;

    #region Getters / Setters
    #endregion // Getters / Setters

    /// <summary>
    /// Constructor
    /// </summary>
    public MaintenanceMiddleware (RequestDelegate next, ResponseWriter responseWriter)
    {
      m_next = next;
      m_responseWriter = responseWriter;
    }

    public async Task InvokeAsync (HttpContext context)
    {
      var path = context.Request.Path;
      if (path.HasValue) {
        var lowerPath = path.Value?.ToLowerInvariant ();
        if (lowerPath?.StartsWith ("/maintenance/") ?? false) {
          var broadcastQuery = context.Request.Query["Broadcast"];
          var broadcast = !broadcastQuery.Any (s => s?.Equals ("false", StringComparison.InvariantCultureIgnoreCase) ?? false);
          switch (lowerPath) {
          case "/maintenance/on":
            CheckSource (context.Connection);
            await TurnOnAsync (broadcast);
            await WriteOkAsync (context, "Maintenance mode successfully turned on");
            return;
          case "/maintenance/off":
            CheckSource (context.Connection);
            await TurnOffAsync (broadcast);
            await WriteOkAsync (context, "Maintenance mode successfully turned off");
            return;
          default:
            break;
          }
        }
      }

      var maintenanceActive = Lemoine.Info.ConfigSet
        .LoadAndGet (MAINTENANCE_ACTIVE_KEY, MAINTENANCE_ACTIVE_DEFAULT);
      if (maintenanceActive) {
        var errorDto = new ErrorDTO ("Pulse maintenance in progress", ErrorStatus.PulseMaintenance);
        await m_responseWriter.WriteToBodyAsync (context, errorDto);
      }
      else {
        await m_next.Invoke (context);
      }
    }

    void CheckSource (ConnectionInfo connectionInfo)
    {
      var remoteIpAddress = connectionInfo.RemoteIpAddress;
      var authorizedSources = Lemoine.Info.ConfigSet
        .LoadAndGet (AUTHORIZED_SOURCES_KEY, AUTHORIZED_SOURCES_DEFAULT);
      foreach (var authorizedSource in authorizedSources
        .Split (';', StringSplitOptions.RemoveEmptyEntries)) {
        var ipAddresses = Lemoine.Net.NetworkAddress.GetIPAddresses (authorizedSource);
        if (ipAddresses.Contains (remoteIpAddress)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"CheckSource: remoteIp={remoteIpAddress} matched authorizedSource={authorizedSource}");
          }
          return;
        }
      }

      var multiAspServices = Lemoine.Info.ConfigSet
        .LoadAndGet (BROADCAST_KEY, BROADCAST_DEFAULT);
      if (multiAspServices) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (var transaction = session.BeginReadOnlyTransaction ("Web.MaintenanceMiddleware.CheckSource")) {
          var aspServiceAddresses = ModelDAOHelper.DAOFactory.ComputerDAO
            .GetWeb ()
            .Select (c => c.Address);
          foreach (var aspServiceAddress in aspServiceAddresses) {
            var ipAddresses = Lemoine.Net.NetworkAddress.GetIPAddresses (aspServiceAddress);
            if (ipAddresses.Contains (remoteIpAddress)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"CheckSource: remoteIp={remoteIpAddress} matched asp service address={aspServiceAddress}");
              }
              return;
            }
          }
        }
      }

      log.Error ($"CheckSource: remote {connectionInfo.RemoteIpAddress} is not authorized");
      throw new System.Security.SecurityException ();
    }

    async Task TurnOnAsync (bool broadcast)
    {
      Lemoine.Info.ConfigSet.SetPersistentConfig (MAINTENANCE_ACTIVE_KEY, true);

      if (broadcast && Lemoine.Info.ConfigSet.LoadAndGet (BROADCAST_KEY, BROADCAST_DEFAULT)) {
        await BroadcastAsync ("/maintenance/on");
      }
    }

    async Task TurnOffAsync (bool broadcast)
    {
      Lemoine.Info.ConfigSet.ResetPersistentConfig (MAINTENANCE_ACTIVE_KEY);

      if (broadcast && Lemoine.Info.ConfigSet.LoadAndGet (BROADCAST_KEY, BROADCAST_DEFAULT)) {
        await BroadcastAsync ("/maintenance/off");
      }
    }

    async Task WriteOkAsync (Microsoft.AspNetCore.Http.HttpContext context, string message)
    {
      var okDto = new OkDTO (message);
      await m_responseWriter.WriteToBodyAsync (context, okDto);
    }

    async Task BroadcastAsync (string s)
    {
      IList<IComputer> webComputers;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginReadOnlyTransaction ("Web.MaintenanceMiddleware.Broadcast")) {
        webComputers = await ModelDAOHelper.DAOFactory.ComputerDAO.GetWebAsync ();
      }
      var localComputerIpAddresses = Lemoine.Info.ComputerInfo.GetIPAddresses ();
      if (1 < webComputers.Count) {
        await Lemoine.WebClient.Query
          .BroadcastAsync (webComputers.Select (c => new Tuple<string, string, string> (c.Name, c.Address, c.WebServiceUrl)),
                      s + "?Broadcast=false");
      }
    }
  }
}
