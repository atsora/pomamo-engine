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
using Lemoine.Business.CncAlarm;
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
          log.Error ($"GetWithoutCache: unknown monitored machine with ID {machineId}");
          return new ErrorDTO ("No monitored machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }
        
        var mainMachineModule = machine.MainMachineModule;
        if (null == mainMachineModule) {
          log.Info ($"GetWithoutCache: no main machine module for monitored machine with ID {machineId}");
        }
        
        IEnumerable<IMachineModule> machineModules = machine.MachineModules
          .OrderBy (machineModule => IsMainMachineModule (machineModule, mainMachineModule) ? 0 : 1);
        
        bool businessSeverity = CncAlarmSeverityOption.IsBusinessSeverity ();
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.CncAlarm.Current"))
        {
          foreach (var machineModule in machineModules) {
            var cncAlarms = GetCurrentCncAlarmsWithSeverity (machineModule, businessSeverity);
            if (cncAlarms.Any ()) {
              var byMachineModule =
                new CncAlarmCurrentByMachineModuleDTO (machineModule,
                                                       IsMainMachineModule (machineModule, mainMachineModule));
              response.ByMachineModule.Add (byMachineModule);
              foreach (var (cncAlarm, severity) in cncAlarms.OrderBy (a => a.Severity?.Priority ?? 1000)) { // 1000: same as CurrentCncAlarm.Priority
                if (null == severity) {
                  log.Warn ($"GetWithoutCache: null severity for current cnc alarm on machine module id {machineModule.Id}");
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
                data.Color = severity?.Color;
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

    IList<(ICurrentCncAlarm CncAlarm, ICncAlarmSeverity Severity)> GetCurrentCncAlarmsWithSeverity (IMachineModule machineModule, bool businessSeverity)
    {
      if (businessSeverity) {
        return ModelDAOHelper.DAOFactory.CurrentCncAlarmDAO
          .FindByMachineModuleWithoutSeverity (machineModule)
          .Select (a => (a, Lemoine.Business.ServiceProvider.Get (new CncAlarmSeverityFromAttributes (a))))
          .ToList ();
      }
      else {
        return ModelDAOHelper.DAOFactory.CurrentCncAlarmDAO
          .FindByMachineModuleWithSeverity (machineModule)
          .Select (a => (a, a.Severity))
          .ToList ();
      }
    }

    bool IsMainMachineModule (IMachineModule machineModule, IMachineModule mainMachineModule)
    {
      Debug.Assert (null != machineModule);

      return (null != mainMachineModule) && machineModule.Id.Equals (mainMachineModule.Id);
    }
  }
}
