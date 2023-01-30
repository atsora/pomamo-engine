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
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.IsoFile
{
  /// <summary>
  /// Return the active detected iso file from a stamped program
  /// </summary>
  public class IsoFileCurrentService
    : GenericCachedService<IsoFileCurrentRequestDTO>
  {
    static readonly string CURRENT_MARGIN_KEY = "Web.IsoFile.Current.CurrentMargin";
    static readonly TimeSpan CURRENT_MARGIN_DEFAULT = TimeSpan.FromMinutes (1);

    static readonly ILog log = LogManager.GetLogger (typeof (IsoFileCurrentService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public IsoFileCurrentService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (IsoFileCurrentRequestDTO request)
    {
      var now = DateTime.UtcNow;
      var response = new IsoFileCurrentResponseDTO (now);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineId = request.MachineId;
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindByIdWithMachineModules (machineId);
        if (null == machine) {
          log.Error ($"GetWithoutCache: unknown monitored machine with ID {machineId}");
          return new ErrorDTO ("No monitored machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        DateTime? detectionStatus = null;
        foreach (var machineModule in machine.MachineModules) {
          var machineModuleDetectionStatus = AddMachineModuleData (now, response, machineModule, request.IncludeDetectionStatus);
          if (machineModuleDetectionStatus.HasValue) {
            if (detectionStatus.HasValue) {
              detectionStatus = detectionStatus.Value < machineModuleDetectionStatus.Value ? detectionStatus : machineModuleDetectionStatus;
            }
            else {
              detectionStatus = machineModuleDetectionStatus;
            }
          }
        }
        if (detectionStatus.HasValue) {
          response.DetectionStatus = ConvertDTO.DateTimeUtcToIsoString (detectionStatus.Value);
          var currentMargin = Lemoine.Info.ConfigSet
            .LoadAndGet (CURRENT_MARGIN_KEY, CURRENT_MARGIN_DEFAULT);
          response.TooOld = detectionStatus.Value.Add (currentMargin) < now;
        }
      }
      var isoFileStrings = response.ByMachineModule
        .Where (x => null != x.IsoFile)
        .OrderBy (x => x.MachineModule.Main ? 0 : 1)
        .Select (x => x.IsoFile.Display);
      if (isoFileStrings.Any ()) {
        response.IsoFiles = String.Join (" ", isoFileStrings.ToArray ());
      }
      return response;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="at"></param>
    /// <param name="responseDto"></param>
    /// <param name="machineModule"></param>
    /// <param name="includeDetectionStatus"></param>
    /// <returns>detection status if requested and known</returns>
    DateTime? AddMachineModuleData (DateTime at, IsoFileCurrentResponseDTO responseDto, IMachineModule machineModule, bool includeDetectionStatus)
    {
      DateTime? detectionStatus = null;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        if (includeDetectionStatus) {
          detectionStatus = GetDetectionStatus (machineModule);
        }

        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.Operation.CurrentSequence.ByMachineModule")) {
          var isoFileSlot = ModelDAOHelper.DAOFactory.IsoFileSlotDAO
            .FindLast (machineModule);
          if (isoFileSlot is null) {
            if (log.IsDebugEnabled) {
              log.Debug ($"AddMachineModuleData: no iso file slot for machine module id {machineModule.Id}");
            }
            return detectionStatus;
          }
          else if (isoFileSlot.DateTimeRange.Upper.HasValue || isoFileSlot.IsoFile is null) {
            if (log.IsDebugEnabled) {
              log.Debug ($"AddMachineModuleData: iso file slot end={isoFileSlot.DateTimeRange.Upper} isoFileId={isoFileSlot.IsoFile?.Id} => skip it since it was discontinued");
            }
            var byMachineModuleDto = new IsoFileCurrentByMachineModuleDTO (machineModule, isoFileSlot.DateTimeRange);
            if (detectionStatus.HasValue) {
              byMachineModuleDto.DetectionStatus = ConvertDTO.DateTimeUtcToIsoString (detectionStatus.Value);
              var currentMargin = Lemoine.Info.ConfigSet
                .LoadAndGet (CURRENT_MARGIN_KEY, CURRENT_MARGIN_DEFAULT);
              byMachineModuleDto.TooOld = detectionStatus.Value.Add (currentMargin) < at;
            }
            responseDto.ByMachineModule.Add (byMachineModuleDto);
            return detectionStatus;
          }
          else {
            var byMachineModuleDto = new IsoFileCurrentByMachineModuleDTO (machineModule, isoFileSlot.DateTimeRange);
            if (log.IsDebugEnabled) {
              log.Debug ($"AddMachineModuleData: range={isoFileSlot.DateTimeRange}, iso file id {isoFileSlot.IsoFile.Id}");
            }
            byMachineModuleDto.IsoFile = new IsoFileDTO (isoFileSlot.IsoFile);
            if (detectionStatus.HasValue) {
              byMachineModuleDto.DetectionStatus = ConvertDTO.DateTimeUtcToIsoString (detectionStatus.Value);
              var currentMargin = Lemoine.Info.ConfigSet
                .LoadAndGet (CURRENT_MARGIN_KEY, CURRENT_MARGIN_DEFAULT);
              byMachineModuleDto.TooOld = detectionStatus.Value.Add (currentMargin) < at;
            }
            responseDto.ByMachineModule.Add (byMachineModuleDto);
            return detectionStatus;
          }
        }
      }
    }

    DateTime? GetDetectionStatus (IMachineModule machineModule)
    {
      Debug.Assert (machineModule != null);

      var machineModuleAnalysisStatus = ModelDAOHelper.DAOFactory.MachineModuleAnalysisStatusDAO
        .FindById (machineModule.Id);
      if (machineModuleAnalysisStatus is null) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetDetectionStatus: no machine module analysis status data for machine module {machineModule.Id} => return null");
        }
        return null;
      }
      else { // machineModuleAnalysisStatus not null
        var nextDetection = ModelDAOHelper.DAOFactory.MachineModuleDetectionDAO
          .FindAfter (machineModule, machineModuleAnalysisStatus.LastMachineModuleDetectionId, 1)
          .FirstOrDefault ();
        if (nextDetection is null) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetDetectionStatus: no detection after {machineModuleAnalysisStatus.LastMachineModuleDetectionId}");
          }
          var acquisitionStatus = ModelDAOHelper.DAOFactory.AcquisitionStateDAO
            .GetAcquisitionState (machineModule, AcquisitionStateKey.Detection);
          if (acquisitionStatus is null) {
            log.Error ($"GetDetectionStatus: no acquisition status for machine module {machineModule.Id} => return null");
            return null;
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetDetectionStatus: return acquisition status {acquisitionStatus.DateTime} since there is no pending detection to analyze");
            }
            return acquisitionStatus.DateTime;
          }
        }
        else { // nextDetection not null
          if (log.IsDebugEnabled) {
            log.Debug ($"GetDetectionStatus: return {nextDetection.DateTime.AddSeconds (-1)}");
          }
          return nextDetection.DateTime.AddSeconds (-1);
        }
      }
    }
  }
}
