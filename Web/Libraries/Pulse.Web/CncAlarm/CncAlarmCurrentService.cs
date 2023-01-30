// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.CncAlarm
{
  /// <summary>
  /// Description of CncAlarmCurrentService
  /// </summary>
  public class CncAlarmCurrentService
    : GenericCachedService<CncAlarmCurrentRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncAlarmCurrentService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public CncAlarmCurrentService () : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (CncAlarmCurrentRequestDTO request)
    {
      var response = new CncAlarmCurrentResponseDTO();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        int machineId = request.MachineId;
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindByIdWithMachineModules(machineId);
        if (null == machine) {
          log.ErrorFormat ("GetWithoutCache: " +
                           "unknown monitored machine with ID {0}",
                           machineId);
          return new ErrorDTO ("No monitored machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }
        
        var mainMachineModule = machine.MainMachineModule;
        if (null == mainMachineModule) {
          log.InfoFormat ("GetWithoutCache: " +
                          "no main machine module for monitored machine with ID {0}",
                          machineId);
        }
        
        IEnumerable<IMachineModule> machineModules = machine.MachineModules
          .OrderBy (machineModule => IsMainMachineModule (machineModule, mainMachineModule) ? 0 : 1);
        
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.CncAlarm.Current"))
        {
          foreach (var machineModule in machineModules) {
            var cncAlarms = ModelDAOHelper.DAOFactory.CurrentCncAlarmDAO.FindByMachineModuleWithSeverity(machineModule);
            if (cncAlarms.Any ()) {
              var byMachineModule =
                new CncAlarmCurrentByMachineModuleDTO (machineModule,
                                                       IsMainMachineModule (machineModule, mainMachineModule));
              response.ByMachineModule.Add (byMachineModule);
              foreach (var cncAlarm in cncAlarms.OrderBy (a => a.Priority)) {
                var severity = cncAlarm.Severity;
                if (null == severity) {
                  log.WarnFormat ("GetWithoutCache: null severity for current cnc alarm on machine module id {0}", machineModule.Id);
                  if (request.KeepFocusOnly) { // Skip it
                    continue;
                  }
                }
                else if (!severity.Focus.HasValue) { // Unknown
                  if (request.KeepFocusOnly) { // Skip it
                    continue;
                  }
                }
                else if (!severity.Focus.Value) { // Ignored
                  if (request.KeepFocusOnly) { // Skip it
                    continue;
                  }
                  if (!request.IncludeIgnored) {
                    continue;
                  }
                }
                var data = new CncAlarmCurrentByMachineModuleDataDTO();
                data.DateTime = ConvertDTO.DateTimeUtcToIsoString(cncAlarm.DateTime);
                data.Display = cncAlarm.Display;
                data.Color = cncAlarm.Color;
                data.CncInfo = cncAlarm.CncInfo;
                data.CncSubInfo = cncAlarm.CncSubInfo;
                data.Type = cncAlarm.Type;
                data.Number = cncAlarm.Number;
                data.Message = cncAlarm.Message;
                data.Properties = cncAlarm.Properties;
                if (null != severity) {
                  data.Severity = severity.Name;
                  data.SeverityDescription = severity.Description;
                  data.Stop = severity.StopStatus.ToString();
                  data.Focus = severity.Focus;
                }
                byMachineModule.CncAlarms.Add(data);
              }
            }
          }
        }
      }

      return response;
    }

    bool IsMainMachineModule (IMachineModule machineModule, IMachineModule mainMachineModule)
    {
      Debug.Assert (null != machineModule);

      return (null != mainMachineModule) && machineModule.Id.Equals (mainMachineModule.Id);
    }
    #endregion // Methods
  }
}
