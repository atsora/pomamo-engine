// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Cache;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.ProductionTracker
{
  public class CacheDomainExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , ICacheDomainExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (CacheDomainExtension).FullName);

    public bool ClearDomain (ICacheClient cacheClient, string domain)
    {
      return false;
    }

    public bool ClearDomainByMachine (ICacheClient cacheClient, string domain, int machineId)
    {
      var regexes = GetCacheKeyRegexByMachine (domain, machineId);
      return cacheClient.ClearDomainFromRegexes (regexes);
    }

    public bool ClearDomainByMachineModule (ICacheClient cacheClient, string domain, int machineModuleId)
    {
      return false;
    }

    public IEnumerable<string> GetCacheKeyRegexByMachine (string domain, int machineId)
    {
      switch (domain.ToLowerInvariant ()) {
      case "processmachinestatetemplate":
      case "machineobservationstateassociation":
      case "business.operation.partproductionrange":
        return new List<string> {
          $@"ProductionTracker([?](.*&)?|/.*[&?])GroupId={machineId}(&.*)|$",
          $@"ProductionTracker([?](.*&)?|/.*[&?])GroupId=\d*[a-zA-Z].*(&.*)|$", // All the group ids that are not numeric
        };
      default:
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCacheKeyRegexByMachine: unknown domain {domain}");
        }
        return new List<string> { };
      }
    }

    public async Task<bool> ClearDomainAsync (ICacheClient cacheClient, string domain)
    {
      return await Task .Run (() => ClearDomain (cacheClient, domain));
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
