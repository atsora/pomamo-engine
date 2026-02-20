// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Business.Cache;
using Lemoine.Model;
using Lemoine.Core.Cache;
using Lemoine.Business.Cache;
using System.Threading.Tasks;

namespace Lemoine.Plugin.Business.DefaultCache
{
  public class CacheDomainExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , ICacheDomainExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (CacheDomainExtension).FullName);

    OperationSlotSplitOption m_operationSlotSplitOption;

    public CacheDomainExtension ()
    {
      m_operationSlotSplitOption =
        (OperationSlotSplitOption)Lemoine.Info.ConfigSet
        .LoadAndGet<int> (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationSlotSplitOption),
          (int)OperationSlotSplitOption.None);
    }

    IEnumerable<string> GetCacheKeyRegex (string domain)
    {
      switch (domain.ToLowerInvariant ()) {
        case "daychange":
          return new List<string> {
          @"Business\.Time\..*"
        };
        case "operationinformationchange":
          return new List<string> {
            @"Business\.Operation\..*Progress\..*",
            @"Business\.Operation\.DefaultSequenceTime\..*",
            @"Business\.Operation\.OperationCurrentShiftTarget\..*",
            @"Business\.Operation\.PartProductionRange\..*",
            @"Business\.Operation\.SequenceStandardTime\..*",
            @"Business\.Operation\.StandardCycleDuration\..*",
            @"Business\.Operation\.StandardProductionTargetPerHour\..*",
            @"Business\.Tool\.MachinesWithExpiringTools\..*",
            @"Business\.Tool\.OperationTools\..*",
            @"Business\.Tool\.OperationToolSequences\..*",
            @"Business\.Tool\.ToolLivesByMachine\..*"
          };
        case "intermediateworkpieceoperationupdate":
          return new List<string> {
            @"Business\.Operation\.StandardProductionTargetPerHour\..*"
          };
        case "shiftchange":
        case "shifttemplateassociation":
        case "componentintermediateworkpieceupdate":
        case "projectcomponentupdate":
        case "workorderprojectupdate":
        case "workorderlineassociation":
        case "pulseinfo":
          return new List<string> { };
        case "business.operation.effectiveoperationcurrentshift":
          return new List<string> {
            @"Business\.Operation\.OperationCurrentShiftTarget\..*",
            @"Business\.Operation\.EffectiveOperationCurrentShift\..*",
            @"Business\.Operation\.ReserveCapacityCurrentShift\..*"
        };
        case "business.operation.standardproductiontargetperhour":
          return new List<string> {
            @"Business\.Operation\.OperationCurrentShiftTarget\..*",
            @"Business\.Operation\.StandardProductionTargetPerHour\..*",
            @"Business\.Operation\.ReserveCapacityCurrentShift\..*"
        };
        default:
          if (domain.Contains ('.')) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetCacheKeyRegex: unknown domain {domain}");
            }
            return new List<string> ();
          }
          else { // main domain
            log.Error ($"GetCacheKeyRegex: unknown main domain {domain}");
            Debug.Assert (false);
            throw new NotImplementedException ($"Unknown main domain {domain}");
          }
      }
    }

    IEnumerable<string> GetSubDomains (string domain)
    {
      switch (domain.ToLowerInvariant ()) {
        case "daychange":
          if (m_operationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByDay)) {
            return new List<string>
            {
            "business.operation.effectiveoperationcurrentshift",
          };
          }
          else {
            return new List<string> { };
          }
        case "shiftchange":
          if (m_operationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByGlobalShift)) {
            return new List<string>
            {
            "business.operation.effectiveoperationcurrentshift",
          };
          }
          else {
            return new List<string> { };
          }
        case "intermediateworkpieceoperationupdate":
          return new List<string>
          {
            "business.operation.standardproductiontargetperhour"
          };
        case "operationinformationchange":
        case "shifttemplateassociation":
        case "componentintermediateworkpieceupdate":
        case "projectcomponentupdate":
        case "workorderprojectupdate":
        case "workorderlineassociation":
        case "pulseinfo":
          return new List<string> { };
        default:
          if (domain.Contains ('.')) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetSubDomains: unknown domain {domain}");
            }
            return new List<string> ();
          }
          else { // main domain
            log.Error ($"GetSubDomains: unknown main domain {domain}");
            Debug.Assert (false);
            throw new NotImplementedException ($"Unknown main domain {domain}");
          }
      }
    }

    IEnumerable<string> GetCacheKeyRegexByMachine (string domain, int machineId)
    {
      var prefixes = GetPrefixesByMachine (domain);
      var patterns = prefixes
        .Select (p => p + @".*\." + machineId + @"(\..*)|$");
      return patterns;
    }

    IEnumerable<string> GetPrefixesByMachine (string domain)
    {
      switch (domain.ToLowerInvariant ()) {
        case "machinemodeassociation":
          return new List<string> {
            @"Business\.MachineMode\.RunningDuration",
            @"Business\.MachineMode\.UtilizationPercentage",
          };
        case "machinestatetemplateassociation":
        case "processmachinestatetemplate":
        case "machineobservationstateassociation": {
            var result = new List<string>
            {
          @"Business\.Reason\.AppliedAutoReasonMachineAssociations",
          @"Business\.Reason\.AppliedManualReasonMachineAssociations",
          @"Business\.MachineState\.MachineShiftSlotAt",
          @"Business\.MachineState\.MachineShiftSlotEnd",
        };
            return result;
          }
        case "operationassociation":
          return new List<string> {
          };
        case "stopcycle":
          return new List<string> {
          @"Business\.Operation\.CycleCounter",
          @"Business\.Operation\.CycleDetectionStatus",
          @"Business\.Operation\.PartProductionRange",
          @"Business\.Tool\.MachinesWithExpiringTools",
        };
        case "reasonassociation":
          return new List<string> {
            @"Business\.Reason\.AppliedAutoReasonMachineAssociations",
            @"Business\.Reason\.AppliedManualReasonMachineAssociations",
            @"Business\.Reason\.Current",
          };
        case "startcycle":
          return new List<string> {
          @"Business\.Operation\.CycleDetectionStatus",
        };
        case "taskassociation":
          return new List<string> {
          @"Business\.Operation\.PartProductionCurrentShiftTask",
        };
        case "productioninformation":
        case "nonconformancereport":
        case "shiftassociation":
        case "serialnumberstamp":
          return new List<string> { };
        case "business.operation.cyclecounter":
          return new List<string> {
          @"Business\.Operation\.CycleCounter",
        };
        case "business.operation.cycleprogress":
          return new List<string> {
          @"Business\.Operation\.CycleProgress",
        };
        case "business.operation.effectiveoperationcurrentshift":
          return new List<string> {
          @"Business\.Operation\.EffectiveOperationCurrentShift",
        };
        case "business.operation.operationcurrentshifttarget":
          return new List<string> {
          @"Business\.Operation\.OperationCurrentShiftTarget",
        };
        case "business.operation.operationprogress":
          return new List<string> {
          @"Business\.Operation\.OperationProgress",
        };
        case "business.operation.partproductioncurrentshiftoperation":
          return new List<string> {
          @"Business\.Operation\.PartProductionCurrentShiftOperation",
        };
        case "business.operation.partproductioncurrentshifttask":
          return new List<string> {
          @"Business\.Operation\.PartProductionCurrentShiftTask",
        };
        case "business.operation.partproductionrange":
          return new List<string> {
          @"Business\.Operation\.PartProductionRange",
        };
        case "business.operation.productioncapacityperhour":
          return new List<string> {
          @"Business\.Operation\.ProductionCapacityPerHour",
          };
        case "business.operation.reservecapacitycurrentshift":
          return new List<string> {
          @"Business\.Operation\.ReserveCapacityCurrentShift",
        };
        case "business.operation.standardproductiontargetperhour":
          return new List<string> {
          @"Business\.Operation\.StandardProductionTargetPerHour",
          };
        case "business.shift.operationshift":
          return new List<string> {
            @"Business\.Shift\.OperationShift",
          };
        case "business.tool.machineswithexpiringtools":
          return new List<string> {
          @"Business\.Tool\.MachinesWithExpiringTools",
        };
        case "business.tool.toollivesbymachine":
          return new List<string>
          {
          @"Business\.Tool\.ToolLivesByMachine",
        };
        default:
          if (domain.Contains ('.')) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetPrefixesByMachine: unknown domain {domain}");
            }
            return new List<string> ();
          }
          else { // main domain
            log.Error ($"GetPrefixesByMachine: unknown main domain {domain}");
            Debug.Assert (false);
            throw new NotImplementedException ($"Unknown main domain {domain}");
          }
      }
    }

    IEnumerable<string> GetSubDomainsByMachine (string domain)
    {
      switch (domain.ToLowerInvariant ()) {
        case "operationassociation":
          return new List<string>
          {
          "business.operation.cyclecounter", // Not sure this is required
          "business.operation.cycleprogress",
          "business.operation.effectiveoperationcurrentshift",
          "business.operation.operationprogress",
        };
        case "stopcycle":
        case "startcycle":
          return new List<string>
          {
          "business.operation.cyclecounter", // Not sure this is required
          "business.operation.cycleprogress",
        };
        case "processmachinestatetemplate":
        case "machineobservationstateassociation":
          if (m_operationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByMachineShift)) {
            return new List<string>
            {
            "business.operation.effectiveoperationcurrentshift",
          };
          }
          else {
            return new List<string> { };
          }
        case "machinemodeassociation":
        case "machinestatetemplateassociation":
        case "cyclecounter":
        case "reasonassociation":
        case "productioninformation":
        case "nonconformancereport":
        case "shiftassociation":
        case "serialnumberstamp":
        case "taskassociation":
          return new List<string> { };
        case "business.operation.cyclecounter":
          return new List<string>
          {
          "business.operation.partproductionrange"
        };
        case "business.operation.cycleprogress":
          return new List<string>
          {
          "business.operation.partproductioncurrentshiftoperation",
          "business.operation.partproductioncurrentshifttask",
          "business.tool.toollivesbymachine",
        };
        case "business.operation.effectiveoperationcurrentshift":
          return new List<string> {
          "business.operation.operationcurrentshifttarget",
          "business.operation.partproductioncurrentshiftoperation",
          "business.operation.partproductioncurrentshifttask",
        };
        case "business.operation.operationprogress":
          return new List<string>
          {
          "business.tool.toollivesbymachine",
        };
        case "business.operation.partproductioncurrentshiftoperation":
          return new List<string> {
            "business.operation.reservecapacitycurrentshift"
          };
        case "business.operation.productioncapacityperhour":
          return new List<string> {
            "business.operation.reservecapacitycurrentshift;"
          };
        case "business.operation.standardproductiontargetperhour":
          return new List<string> {
            "business.operation.operationcurrentshifttarget",
            "business.operation.reservecapacitycurrentshift"
          };
        case "business.operation.toollivesbymachine":
          return new List<string>
          {
          "business.tool.machineswithexpiringtools",
        };
        case "business.shift.operationshift":
          return new List<string> {
            "business.operation.operationcurrentshifttarget",
            "business.operation.reservecapacitycurrentshift"
          };
        default:
          if (domain.Contains ('.')) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetSubDomainsByMachine: unknown domain {domain}");
            }
            return new List<string> ();
          }
          else { // main domain
            log.Error ($"GetSubDomainsByMachine: unknown main domain {domain}");
            Debug.Assert (false);
            throw new NotImplementedException ($"Unknown main domain {domain}");
          }
      }
    }

    public bool ClearDomain (ICacheClient cacheClient, string domain)
    {
      var result = false;

      {
        var subDomains = GetSubDomains (domain);
        foreach (var subDomain in subDomains) {
          result |= cacheClient.ClearDomain (subDomain);
        }
      }

      {
        var regexes = GetCacheKeyRegex (domain);
        result |= cacheClient.ClearDomainFromRegexes (regexes);
      }

      return result;
    }

    public bool ClearDomainByMachine (ICacheClient cacheClient, string domain, int machineId)
    {
      var result = false;

      {
        var subDomains = GetSubDomainsByMachine (domain);
        foreach (var subDomain in subDomains) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ClearDomainByMachine: clear sub-domain {subDomain}");
          }
          result |= cacheClient.ClearDomain (subDomain);
        }
      }

      {
        var regexes = GetCacheKeyRegexByMachine (domain, machineId);
        if (regexes.Any ()) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ClearDomainByMachine: clear domain from {regexes.Count ()} regexes");
          }
          result |= cacheClient.ClearDomainFromRegexes (regexes);
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"ClearDomainByMachine: result is {result} for domain {domain} machineId {machineId}");
      }

      return result;
    }

    public bool ClearDomainByMachineModule (ICacheClient cacheClient, string domain, int machineModuleId)
    {
      return false;
    }

    public async Task<bool> ClearDomainAsync (ICacheClient cacheClient, string domain)
    {
      return await Task.Run (() => ClearDomain (cacheClient, domain));
    }

    public async Task<bool> ClearDomainByMachineAsync (ICacheClient cacheClient, string domain, int machineId)
    {
      return await Task.Run (() => ClearDomainByMachine (cacheClient, domain, machineId));
    }

    public async Task<bool> ClearDomainByMachineModuleAsync (ICacheClient cacheClient, string domain, int machineModuleId)
    {
      return await Task.Run (() => ClearDomainByMachineModule (cacheClient, domain, machineModuleId));
    }
  }
}
