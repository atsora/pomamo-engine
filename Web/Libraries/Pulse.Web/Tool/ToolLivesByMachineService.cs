// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Business.Operation;
using Lemoine.Business.Tool;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.Tool
{
  /// <summary>
  /// Description of ToolLivesByMachineService.
  /// </summary>
  public class ToolLivesByMachineService : GenericCachedService<ToolLivesByMachineRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ToolLivesByMachineService).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ToolLivesByMachineService () : base (Lemoine.Core.Cache.CacheTimeOut.CurrentLong) { }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (ToolLivesByMachineRequestDTO request)
    {
      // Check that the request is not null
      if (request == null) {
        log.Fatal ("ToolLivesByMachineService: the request cannot be null");
        return new ErrorDTO ("Invalid request", ErrorStatus.UnexpectedError);
      }

      // Prepare the answer
      var response = new ToolLivesByMachineResponseDTO ();

      IMonitoredMachine monitoredMachine;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // Get the machine, check that it exists and that it is monitored
        monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindByIdWithMachineModules (request.MachineId);
        if (null == monitoredMachine) {
          log.ErrorFormat ("GetWithoutCache: machine not monitored with ID {0}", request.MachineId);
          return new ErrorDTO ("Machine with the specified ID is not monitored", ErrorStatus.WrongRequestParameter);
        }

        var toolLivesByMachine = new ToolLivesByMachine (monitoredMachine);
        if (request.MaxExpirationTime.HasValue) {
          toolLivesByMachine.MaxExpirationTime = TimeSpan.FromSeconds (request.MaxExpirationTime.Value);
        }
        ToolLivesByMachineResponse toolLives = Lemoine.Business.ServiceProvider
          .Get (toolLivesByMachine);
        response.DateTime = ConvertDTO.DateTimeUtcToIsoStringMs (toolLives.DateTime);
        if (null != toolLives.Operation) {
          response.UpdateOperation (toolLives.Operation);
        }
        foreach (var toolLivesTool in toolLives.Tools) {
          var toolProperty = new ToolPropertyDTO ();
          if (toolLivesTool.ExpirationDateTime.HasValue) {
            toolProperty.ExpirationDateTime = ConvertDTO.DateTimeUtcToIsoString (toolLivesTool.ExpirationDateTime.Value);
          }
          if ((null != toolLivesTool.ExpirationDateTimeRange) && !toolLivesTool.ExpirationDateTimeRange.IsEmpty ()) {
            toolProperty.ExpirationDateTimeRange = toolLivesTool.ExpirationDateTimeRange
              .ToString (r => ConvertDTO.DateTimeUtcToIsoString (r));
          }
          toolProperty.Expired = toolLivesTool.Expired;
          if (0 <= toolLivesTool.RemainingCyclesToLimit) {
            toolProperty.RemainingCycles = toolLivesTool.RemainingCyclesToLimit;
          }
          toolProperty.Group = toolLivesTool.Group;
          toolProperty.Display = toolLivesTool.Display;
          toolProperty.ActiveSisterTool = toolLivesTool.ActiveSisterTool;
          toolProperty.ValidSisterTools = toolLivesTool.ValidSisterTools;
          toolProperty.Warning = toolLivesTool.Warning;
          response.Tools.Add (toolProperty);
        }
      }

      return response;
    }
    #endregion // Methods
  }
}
