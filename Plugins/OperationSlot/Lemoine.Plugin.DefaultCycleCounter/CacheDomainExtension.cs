// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Business.Cache;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Cache;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.OperationSlotByMachineShift
{
  public class CacheDomainExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , ICacheDomainExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (CacheDomainExtension).FullName);

    public CacheDomainExtension ()
    {
    }

    public bool ClearDomain (ICacheClient cacheClient, string domain)
    {
      return false;
    }

    public bool ClearDomainByMachine (ICacheClient cacheClient, string domain, int machineId)
    {
      switch (domain.ToLowerInvariant ()) {
      case "operationassociation":
      case "productioninformation":
      case "stopcycle":
      case "taskassociation":
        return cacheClient.ClearDomainByMachine ("business.operation.cyclecounter", machineId);
      default:
        if (log.IsDebugEnabled) {
          log.Debug ($"ClearDomainByMachine: domain {domain} with no action");
        }
        return false;
      }
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
