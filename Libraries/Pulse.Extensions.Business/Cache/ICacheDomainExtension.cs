// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lemoine.Extensions.Business.Cache
{
  /// <summary>
  /// Extension to manage the cache
  /// </summary>
  public interface ICacheDomainExtension : IExtension
  {
    /// <summary>
    /// In addition to the regex method, run the additional actions to clear a domain
    /// </summary>
    /// <param name="cacheClient">not null</param>
    /// <param name="domain"></param>
    /// <returns>Some cache data was cleared</returns>
    bool ClearDomain (ICacheClient cacheClient, string domain);

    /// <summary>
    /// In addition to the regex method, run the additional actions to clear a domain by machine
    /// </summary>
    /// <param name="cacheClient">not null</param>
    /// <param name="domain"></param>
    /// <param name="machineId"></param>
    /// <returns>Some cache data was cleared</returns>
    bool ClearDomainByMachine (ICacheClient cacheClient, string domain, int machineId);

    /// <summary>
    /// In addition to the regex method, run the additional actions to clear a domain by machine module
    /// </summary>
    /// <param name="cacheClient">not null</param>
    /// <param name="domain"></param>
    /// <param name="machineModuleId"></param>
    /// <returns>Some cache data was cleared</returns>
    bool ClearDomainByMachineModule (ICacheClient cacheClient, string domain, int machineModuleId);

    /// <summary>
    /// In addition to the regex method, run the additional actions to clear a domain
    /// </summary>
    /// <param name="cacheClient">not null</param>
    /// <param name="domain"></param>
    /// <returns>Some cache data was cleared</returns>
    Task<bool> ClearDomainAsync (ICacheClient cacheClient, string domain);

    /// <summary>
    /// In addition to the regex method, run the additional actions to clear a domain by machine
    /// </summary>
    /// <param name="cacheClient">not null</param>
    /// <param name="domain"></param>
    /// <param name="machineId"></param>
    /// <returns>Some cache data was cleared</returns>
    Task<bool> ClearDomainByMachineAsync (ICacheClient cacheClient, string domain, int machineId);

    /// <summary>
    /// In addition to the regex method, run the additional actions to clear a domain by machine module
    /// </summary>
    /// <param name="cacheClient">not null</param>
    /// <param name="domain"></param>
    /// <param name="machineModuleId"></param>
    /// <returns>Some cache data was cleared</returns>
    Task<bool> ClearDomainByMachineModuleAsync (ICacheClient cacheClient, string domain, int machineModuleId);
  }

  /// <summary>
  /// Extensions to interface <see cref="ICacheDomainExtension"/>
  /// </summary>
  public static class CacheDomainExtensionExtensions
  {
    static ILog log = LogManager.GetLogger (typeof (CacheDomainExtensionExtensions).FullName);

    /// <summary>
    /// Clear a domain from regexes
    /// </summary>
    /// <param name="cacheClient">not null</param>
    /// <param name="regexes"></param>
    /// <returns>Some cache data was cleared</returns>
    public static bool ClearDomainFromRegexes (this ICacheClient cacheClient, IEnumerable<string> regexes)
    {
      try {
        if (!regexes.Any ()) {
          if (log.IsDebugEnabled) {
            log.Debug ("ClearDomainFromRegexes: no regex");
          }
          return false;
        }
        foreach (var regex in regexes.Distinct ()) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ClearDomainFromRegexes: remove cache for regex {regex}");
          }
          cacheClient.RemoveByRegex (regex);
        }
        return true;
      }
      catch (Exception ex) {
        log.Error ("ClearDomainFromRegexes: exception", ex);
        throw;
      }
    }
  }
}
