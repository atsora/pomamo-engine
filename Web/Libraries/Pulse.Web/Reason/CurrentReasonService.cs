// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Database;
using Lemoine.Business.Reason;
using Lemoine.Extensions.Web.Responses;
using Pulse.Extensions.Web.Reason;
using System.Threading;
using Lemoine.Web;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Description of ReasonColorLegendService
  /// </summary>
  public class CurrentReasonService
  : GenericCachedService<CurrentReasonRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CurrentReasonService).FullName);

    readonly IDictionary<int, IEnumerable<ICurrentReasonExtension>> m_extensions = new Dictionary<int, IEnumerable<ICurrentReasonExtension>> ();
    readonly SemaphoreSlim m_semaphore = new SemaphoreSlim (1, 1);

    /// <summary>
    /// 
    /// </summary>
    public CurrentReasonService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }

    CurrentReasonPeriod ConvertPeriodParameter (string periodParameter)
    {
      if (string.IsNullOrEmpty (periodParameter)) {
        return CurrentReasonPeriod.None;
      }
      switch (periodParameter.ToLowerInvariant ()) {
      case "none":
      case "":
        return CurrentReasonPeriod.None;
      case "reason_machinemodecategory":
        return CurrentReasonPeriod.Reason | CurrentReasonPeriod.MachineModeCategory;
      case "running":
        return CurrentReasonPeriod.Running;
      case "running_machinemodecategory":
        return CurrentReasonPeriod.Running | CurrentReasonPeriod.MachineModeCategory;
      default:
        log.Error ($"ConvertPeriodParameter: {periodParameter} is not a supported parameter");
        throw new ArgumentException ("Not supported period", "periodParameter");
      }
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (CurrentReasonRequestDTO request)
    {
      CurrentReasonResponseDTO response = new CurrentReasonResponseDTO ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineId = request.MachineId;
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (machineId);
        if (null == machine) {
          log.Error ($"GetWithoutCache: unknown machine with ID {machineId}");
          return new ErrorDTO ("No monitored machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        var period = ConvertPeriodParameter (request.Period);
        var notRunningOnlyDuration = request.NotRunningOnlyDuration;

        var currentReasonBusinessRequest = new CurrentReason (machine, period, notRunningOnlyDuration);
        var currentReasonBusinessResponse = Lemoine.Business.ServiceProvider
          .Get (currentReasonBusinessRequest);

        if ((null != currentReasonBusinessResponse) && (null != currentReasonBusinessResponse.Reason)) {
          response.CurrentDateTime = ConvertDTO.DateTimeUtcToIsoString (currentReasonBusinessResponse.CurrentDateTime);
          response.MachineMode = new MachineModeDTOAssembler ().Assemble (currentReasonBusinessResponse.MachineMode);
          response.Reason = new ReasonDTOAssembler ().Assemble (currentReasonBusinessResponse.Reason, currentReasonBusinessResponse.JsonData);
          response.DateTime = ConvertDTO.DateTimeUtcToIsoString (currentReasonBusinessResponse.DateTime);
          response.ReasonScore = currentReasonBusinessResponse.ReasonScore;
          response.ReasonSource = currentReasonBusinessResponse.ReasonSource.HasValue ? new ReasonSourceDTO (currentReasonBusinessResponse.ReasonSource.Value) : null;
          response.AutoReasonNumber = currentReasonBusinessResponse.AutoReasonNumber;
          if (currentReasonBusinessResponse.PeriodStart.HasValue) {
            response.PeriodStart = ConvertDTO.DateTimeUtcToIsoString (currentReasonBusinessResponse.PeriodStart.Value);
          }

          var extensions = GetExtensions (machine);
          var severity = extensions
            .Select (ext => ext.GetSeverity (currentReasonBusinessResponse))
            .Where (ext => null != ext)
            .OrderBy (ext => ext.LevelValue)
            .FirstOrDefault ();
          if (null != severity) {
            response.Severity = new SeverityDTO (severity);
          }
        }
        else {
          log.Error ($"GetWithoutCache: unable to get a current reason for machine {machineId}");
          return new ErrorDTO ("No current reason was recorded",
                               ErrorStatus.ProcessingDelay);
        }
      } // session

      return response;
    }

    IEnumerable<ICurrentReasonExtension> GetExtensions (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      if (m_extensions.ContainsKey (machine.Id)) {
        return m_extensions[machine.Id];
      }
      else {
        IEnumerable<ICurrentReasonExtension> extensions;
        using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (m_semaphore)) {
          extensions = Lemoine.Business.ServiceProvider
            .Get (new Lemoine.Business.Extension
            .MonitoredMachineExtensions<ICurrentReasonExtension> (machine, (ext, m) => ext.Initialize (m)));
          m_extensions[machine.Id] = extensions;
        }
        return extensions;
      }
    }
  }
}
