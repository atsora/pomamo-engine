// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
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

namespace Pulse.Web.CncValue
{
  /// <summary>
  /// Description of CncValueAtService
  /// </summary>
  public class CncValueAtService
    : GenericCachedService<CncValueAtRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncValueColorService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public CncValueAtService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.PastLong)
    {
    }

    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    protected override TimeSpan GetCacheTimeOut (string url, CncValueAtRequestDTO requestDTO)
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
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCacheTimeOut: cacheTimeSpan is {cacheTimeSpan} for url={url}");
        }
        return cacheTimeSpan;
      }
      catch (Exception ex) {
        log.Error ($"GetCacheTimeOut: exception => fallback: {CacheTimeOut.PastLong}", ex);
        return CacheTimeOut.PastLong.GetTimeSpan ();
      }
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (CncValueAtRequestDTO request)
    {
      try {
        var response = new CncValueAtResponseDTO (request.At);

        var at = ConvertDTO.IsoStringToDateTimeUtc (request.At).Value;

        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var machineId = request.MachineId;
          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindByIdWithMainMachineModulePerformanceFieldUnit (machineId);
          if (machine is null) {
            log.Error ($"GetWithoutCache: unknown monitored machine with ID {machineId}");
            return new ErrorDTO ("No monitored machine with the specified ID",
                                 ErrorStatus.WrongRequestParameter);
          }

          var mainMachineModule = machine.MainMachineModule;
          if (mainMachineModule is null) {
            log.Info ($"GetWithoutCache: no main machine module for monitored machine with ID {machineId}");
          }

          var performanceField = machine.PerformanceField;
          if (performanceField is null) {
            log.Info ($"GetWithoutCache: no performance field for monitored machine with ID {machine.Id}");
          }

          IEnumerable<IMachineModule> machineModules = machine.MachineModules
            .OrderBy (machineModule => IsMainMachineModule (machineModule, mainMachineModule) ? 0 : 1);
          if (log.IsErrorEnabled && !machineModules.Any ()) {
            log.Error ($"GetWithoutCache: machine id={machineId} with no machine module");
          }

          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.CncValueAt")) {
            foreach (var machineModule in machineModules) {
              var cncValues = ModelDAOHelper.DAOFactory.CncValueDAO.FindAtWithFieldUnit (machineModule, at)
                .OrderBy (cncValue => cncValue.Field.Display)
                .OrderBy (cncValue => IsPerformanceField (cncValue.Field, performanceField) ? 0 : 1);
              if (cncValues.Any ()) {
                var byMachineModule =
                  new CncValueAtByMachineModuleDTO (machineModule, IsMainMachineModule (machineModule, mainMachineModule));
                response.ByMachineModule.Add (byMachineModule);
                foreach (var cncValue in cncValues) {
                  var byField =
                    new CncValueAtByMachineModuleFieldDTO (cncValue.Field, IsPerformanceField (cncValue.Field, performanceField));
                  if (cncValue.Field.Id.Equals ((int)FieldId.StackLight)) {
                    byField.Value = new StackLightDTO ((StackLight)cncValue.Value);
                  }
                  else {
                    byField.Value = cncValue.Value;
                  }
                  byField.Color = Lemoine.Business.ServiceProvider
                    .Get (new Lemoine.Business.Field.FieldValueColor (cncValue)); ;
                  byMachineModule.ByField.Add (byField);
                }
              }
              else if (log.IsDebugEnabled) {
                log.Debug ($"GetWithoutCache: no cnc value for machineModule id={machineModule.Id} at {at}");
              }
            }
          }
        }

        return response;
      }
      catch (Exception ex) {
        log.Error ($"GetWithoutCache: exception", ex);
        throw;
      }
    }

    bool IsMainMachineModule (IMachineModule machineModule, IMachineModule mainMachineModule)
    {
      Debug.Assert (null != machineModule);

      return (null != mainMachineModule) && (machineModule.Id == mainMachineModule.Id);
    }

    bool IsPerformanceField (IField field, IField performanceField)
    {
      Debug.Assert (null != field);

      return (null != field) && (null != performanceField) && (field.Id == performanceField.Id);
    }
  }
}
