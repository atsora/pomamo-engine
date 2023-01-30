// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
using System.Threading.Tasks;

namespace Lemoine.Plugin.Web.DefaultCache
{
  public class CacheDomainExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , ICacheDomainExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (CacheDomainExtension).FullName);

    IEnumerable<string> GetCacheKeyRegex (string domain)
    {
      var prefixes = GetPrefixes (domain);
      return prefixes
        .Select (p => p + "[?/].*|$");
    }

    IEnumerable<string> GetPrefixes (string domain)
    {
      switch (domain.ToLowerInvariant ()) {
      case "daychange":
        return new List<string> {
            "MachineMode/?ColorSlots",
            "(MachineMode/)?RunningSlots",
            "(MachineMode/)?Utilization",
            "Reason/?ColorSlots",
            "(Time/)?RangeAround"};
      case "shiftchange":
        return new List<string> { "(Time/)?RangeAround" };
      case "shifttemplateassociation":
        return new List<string> { };
      case "componentintermediateworkpieceupdate":
      case "intermediateworkpieceoperationupdate":
      case "projectcomponentupdate":
      case "workorderprojectupdate":
      case "workorderlineassociation":
        return new List<string> { "Operation/?Slots" };
      case "pulseinfo":
        return new List<string> { "(Info/)?PulseVersions" };
      case "operationinformationchange":
        return new List<string>
          {
            "(Operation/)?CycleProgress",
            "(Operation/)?OperationProgress",
            "(Tool/)?CurrentMachinesWithExpiredTools",
            "(Tool/)?ToolLivesByMachine",
            "GetProductionMachiningStatus"
          };
      default:
        if (domain.Contains ('.')) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetPrefixes: unknown domain {domain}");
          }
          return new List<string> ();
        }
        else { // main domain
          log.Error ($"GetPrefixes: unknown main domain {domain}");
          Debug.Assert (false);
          throw new NotImplementedException ($"Unknown main domain {domain}");
        }
      }
    }

    IEnumerable<string> GetCacheKeyRegexByMachineModule (string domain, int machineModuleId)
    {
      var prefixes = GetPrefixesByMachineModule (domain);
      var patterns1 = prefixes
        .Select (p => p + "([?](.*&)?|/.*[&?])MachineModuleId=" + machineModuleId + "(&.*)|$");
      var patterns2 = prefixes
        .Select (p => p + "/(.*/)?" + machineModuleId + "(/.*)|$");
      return patterns1.Concat (patterns2);
    }

    IEnumerable<string> GetPrefixesByMachineModule (string domain)
    {
      switch (domain.ToLowerInvariant ()) {
      case "isofileassociation":
        return new List<string> { "IsoFile/?Slots" };
      default:
        if (domain.Contains ('.')) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetPrefixesByMachineModule: unknown domain {domain}");
          }
          return new List<string> ();
        }
        else { // main domain
          log.Error ($"GetPrefixesByMachineModule: unknown main domain {domain}");
          Debug.Assert (false);
          throw new NotImplementedException ($"Unknown main domain {domain}");
        }
      }
    }

    IEnumerable<string> GetCacheKeyRegexByMachine (string domain, int machineId)
    {
      var prefixes = GetPrefixesByMachine (domain);
      var patterns1 = prefixes
        .Select (p => p + "([?](.*&)?|/.*[&?])MachineId=" + machineId + "(&.*)|$");
      var patterns2 = prefixes
        .Select (p => p + "/(.*/)?" + machineId + "(/.*)|$");
      var patterns3 = prefixes
        .Select (p => p + "([?](.*&)?|/.*[&?])GroupId=" + machineId + "(&.*)|$");
      var patterns4 = prefixes
        .Select (p => p + @"([?](.*&)?|/.*[&?])GroupId=\d*[a-zA-Z].*(&.*)|$"); // Any group id which is not numeric
      return patterns1.Concat (patterns2).Concat (patterns3).Concat (patterns4);
    }

    IEnumerable<string> GetPrefixesByMachine (string domain)
    {
      switch (domain.ToLowerInvariant ()) {
      case "machinemodeassociation":
        return new List<string> {
            "MachineMode/?ColorSlots",
            "(MachineMode/)?RunningSlots",
            "(MachineMode/)?Utilization"
          };
      case "machinestatetemplateassociation":
        return new List<string> { "MachineStateTemplate/?Slots" };
      case "processmachinestatetemplate":
      case "machineobservationstateassociation":
        return new List<string> {
            "(MachineStateTemplate/)?ObservationStateSlots",
            "Reason/?ColorSlots",
            "(Reason/)?ReasonOnlySlots",
            "CurrentMachineStateTemplateOperation"
          };
      case "operationassociation":
        return new List<string> {
            "Operation/?Slots",
            "(Operation/)?CycleProgress",
            "(Operation/)?OperationProgress",
            "GetProductionMachiningStatus",
            "(Operation/)?SequenceSlots"
          };
      case "startcycle":
      case "stopcycle":
        return new List<string> {
            "Operation/?Slots",
            "(Operation/)?CycleProgress",
            "(Tool/)?CurrentMachinesWithExpiredTools",
            "(Tool/)?ToolLivesByMachine",
            "GetProductionMachiningStatus"
          };
      case "reasonassociation":
        return new List<string> {
            "Reason/?ColorSlots",
            "(Reason/)?ReasonOnlySlots"
          };
      case "shiftassociation":
        return new List<string> { "(MachineStateTemplate/)?ObservationStateSlots" };
      case "serialnumberstamp":
        return new List<string> { };
      case "taskassociation":
        return new List<string> {
            "Operation/?Slots",
            "(Operation/)?CycleProgress",
            "(Operation/)?OperationProgress",
            "GetProductionMachiningStatus"
          };
      case "productioninformation":
      case "nonconformancereport":
        return new List<string> { };
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

    public bool ClearDomain (ICacheClient cacheClient, string domain)
    {
      var regexes = GetCacheKeyRegex (domain);
      return cacheClient.ClearDomainFromRegexes (regexes);
    }

    public bool ClearDomainByMachine (ICacheClient cacheClient, string domain, int machineId)
    {
      var regexes = GetCacheKeyRegexByMachine (domain, machineId);
      return cacheClient.ClearDomainFromRegexes (regexes);
    }

    public bool ClearDomainByMachineModule (ICacheClient cacheClient, string domain, int machineModuleId)
    {
      var regexes = GetCacheKeyRegexByMachineModule (domain, machineModuleId);
      return cacheClient.ClearDomainFromRegexes (regexes);
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
