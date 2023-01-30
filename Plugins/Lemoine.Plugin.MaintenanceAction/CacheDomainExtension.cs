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

namespace Lemoine.Plugin.MaintenanceAction
{
  public class CacheDomainExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , ICacheDomainExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (CacheDomainExtension).FullName);

    public bool ClearDomain (ICacheClient cacheClient, string domain)
    {
      switch (domain.ToLowerInvariant ()) {
      case "plugin.maintenanceaction":
        return cacheClient.ClearDomainFromRegexes (new List<string> { "MaintenanceAction.*" });
      default:
        return false;
      }
    }

    public bool ClearDomainByMachine (ICacheClient cacheClient, string domain, int machineId)
    {
      return false;
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
