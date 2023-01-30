// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Web;

namespace Pulse.Web.Misc
{
  /// <summary>
  /// Description of XxxService
  /// </summary>
  public class SwitchCatchUpModeService
    : GenericNoCacheService<SwitchCatchUpModeRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SwitchCatchUpModeService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public SwitchCatchUpModeService ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (SwitchCatchUpModeRequestDTO request)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var machines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindAll ();
        foreach (var machine in machines) {
          var key = $"Analysis.CatchUp.{machine.Id}";
          var applicationState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
            .GetApplicationState (key);
          if (request.Active) {
            if (applicationState is null) {
              applicationState = ModelDAOHelper.ModelFactory.CreateApplicationState (key);
              applicationState.Value = true;
              ModelDAOHelper.DAOFactory.ApplicationStateDAO
                .MakePersistent (applicationState);
            }
          }
          else {
            if (null != applicationState) {
              ModelDAOHelper.DAOFactory.ApplicationStateDAO
                .MakeTransient (applicationState);
            }
          }
        }
      }
      return new OkDTO ($"Catch up mode switched to {request.Active}");
    }
    #endregion // Methods
  }
}
