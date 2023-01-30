// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.Extensions.Business.Cache;
using Lemoine.Core.Cache;
using System.Threading.Tasks;

namespace Lemoine.Plugin.ApplicableAtMachineStateShift
{
  public class CacheDomainExtension
    : MultipleInstanceConfigurableExtension<Configuration>
    , ICacheDomainExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (CacheDomainExtension).FullName);

    Configuration m_configuration = null;

    public string Name
    {
      get
      {
        if (null == m_configuration) {
          if (!LoadConfiguration (out m_configuration)) {
            log.ErrorFormat ("Name.get: LoadConfiguration failed");
            return "";
          }
        }
        Debug.Assert (null != m_configuration); // Load successful
        return m_configuration.Name;
      }
    }

    IEnumerable<string> GetCacheKeyRegexByMachine (string domain, int machineId)
    {
      var prefixes = GetPrefixexByMachine (domain);
      var patterns = prefixes
        .Select (p => p + @".*\." + machineId + @"(\..*)|$");
      return patterns;
    }

    IEnumerable<string> GetPrefixexByMachine (string domain)
    {
      switch (domain.ToLowerInvariant ()) {
      case "machinestatetemplateassociation":
      case "processmachinestatetemplate":
      case "machineobservationstateassociation":
        return new List<string>
        {
          $@"DynamicTime\.{this.Name}\..*"
        };
      case "machinemodeassociation":
      case "operationassociation":
      case "reasonassociation":
      case "shiftassociation":
      case "serialnumberstamp":
      case "taskassociation":
      case "productioninformation":
      case "nonconformancereport":
        return new List<string> { };
      default:
        log.ErrorFormat ("GetPrefixexByMachine: " +
                         "unknown domain {0}",
                         domain);
        Debug.Assert (false);
        throw new NotImplementedException ("Domain " + domain);
      }
    }

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

    public Task<bool> ClearDomainAsync (ICacheClient cacheClient, string domain)
    {
      return Task.Run (() => ClearDomain (cacheClient, domain));
    }

    public Task<bool> ClearDomainByMachineAsync (ICacheClient cacheClient, string domain, int machineId)
    {
      return Task.Run (() => ClearDomainByMachine (cacheClient, domain, machineId));
    }

    public Task<bool> ClearDomainByMachineModuleAsync (ICacheClient cacheClient, string domain, int machineModuleId)
    {
      return Task.Run (() => ClearDomainByMachineModule (cacheClient, domain, machineModuleId));
    }
  }
}
