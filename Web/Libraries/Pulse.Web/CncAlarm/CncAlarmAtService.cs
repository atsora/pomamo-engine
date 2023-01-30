// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.CncAlarm
{
  /// <summary>
  /// Description of CncAlarmAtService
  /// </summary>
  public class CncAlarmAtService : GenericCachedService<CncAlarmAtRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncAlarmColorService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public CncAlarmAtService () : base(Lemoine.Core.Cache.CacheTimeOut.PastLong)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    protected override TimeSpan GetCacheTimeOut (string url, CncAlarmAtRequestDTO requestDTO)
    {
      try {
        var at = ConvertDTO.IsoStringToDateTimeUtc (requestDTO.At).Value;
        
        TimeSpan cacheTimeSpan;
        if (at < DateTime.UtcNow) { // Old / Past
          cacheTimeSpan = CacheTimeOut.PastLong.GetTimeSpan ();
        }
        else { // Current or future
          cacheTimeSpan = CacheTimeOut.CurrentShort.GetTimeSpan ();
        }
        log.DebugFormat ("GetCacheTimeOut: " +
                         "cacheTimeSpan is {0} for url={1}",
                         cacheTimeSpan, url);
        return cacheTimeSpan;
      }
      catch (Exception ex) {
        log.ErrorFormat ("GetCacheTimeOut: " +
                         "exception {0} " +
                         "=> fallback: {1}",
                         ex, CacheTimeOut.PastLong);
        return CacheTimeOut.PastLong.GetTimeSpan ();
      }
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(CncAlarmAtRequestDTO request)
    {
      CncAlarmAtResponseDTO response = new CncAlarmAtResponseDTO (request.At);
      
      DateTime at = ConvertDTO.IsoStringToDateTimeUtc (request.At).Value;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        int machineId = request.MachineId;
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindByIdWithMachineModules (machineId);
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
        
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.CncAlarm.At"))
        {
          foreach (var machineModule in machineModules) {
            var cncAlarms = ModelDAOHelper.DAOFactory.CncAlarmDAO.FindAtWithSeverity (machineModule, at);
            if (cncAlarms.Any ()) {
              var byMachineModule =
                new CncAlarmAtByMachineModuleDTO (machineModule,
                                                  IsMainMachineModule (machineModule, mainMachineModule));
              response.ByMachineModule.Add (byMachineModule);
              foreach (var cncAlarm in cncAlarms.OrderBy (a => a.Priority)) {
                var severity = cncAlarm.Severity;
                if (null == severity) {
                  log.WarnFormat ("GetWithoutCache: no severity defined for cnc alarm at {0} on machine module id {1}", at, machineModule.Id);
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
                var data =
                  new CncAlarmAtByMachineModuleDataDTO ();
                data.Range = cncAlarm.DateTimeRange.ToString (dt => ConvertDTO.DateTimeUtcToIsoString (dt));
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
                byMachineModule.CncAlarms.Add (data);
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
