// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Core.Cache;
using System.Threading.Tasks;

namespace Lemoine.Business.Cache
{
  /// <summary>
  /// CacheClientExtension
  /// </summary>
  public static class CacheClientExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CacheClientExtension).FullName);

    static IEnumerable<Lemoine.Extensions.Business.Cache.ICacheDomainExtension> GetExtensions
       ()
    {
      var extensionRequest = new Lemoine.Business.Extension
        .GlobalExtensions<Lemoine.Extensions.Business.Cache.ICacheDomainExtension> ();
      var extensions = Lemoine.Business.ServiceProvider
        .Get (extensionRequest);
      return extensions;
    }

    static async Task<IEnumerable<Lemoine.Extensions.Business.Cache.ICacheDomainExtension>> GetExtensionsAsync
       ()
    {
      var extensionRequest = new Lemoine.Business.Extension
        .GlobalExtensions<Lemoine.Extensions.Business.Cache.ICacheDomainExtension> ();
      var extensions = await Lemoine.Business.ServiceProvider
        .GetAsync (extensionRequest);
      return extensions;
    }

    /// <summary>
    /// Clear a domain
    /// </summary>
    /// <param name="cacheClient"></param>
    /// <param name="domain"></param>
    /// <returns>Some cache data was cleared</returns>
    public static bool ClearDomain (this ICacheClient cacheClient, string domain)
    {
      try {
        var extensions = GetExtensions ();
        if (log.IsDebugEnabled) {
          log.Debug ($"ClearDomain: {extensions.Count ()} extensions");
        }
        var result = false;
        using (var cacheClientBatchUpdater = new CacheClientBatchUpdater (cacheClient)) {
          foreach (var extension in extensions) {
            if (log.IsDebugEnabled) {
              log.Debug ($"ClearDomain: about to process {extension} domain {domain}");
            }
            bool localResult;
            try {
              localResult = extension.ClearDomain (cacheClient, domain);
              if (log.IsDebugEnabled) {
                log.Debug ($"ClearDomain: result is {localResult} for {extension} domain {domain}");
              }
            }
            catch (Exception ex) {
              log.Error ($"ClearDomain: exception in {extension} domain {domain}", ex);
              localResult = false;
            }
            result |= localResult;
          }
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"ClearDomain: completed, result={result}");
        }
        return result;
      }
      catch (Exception ex) {
        log.Error ("ClearDomain: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Clear a domain asynchronously
    /// </summary>
    /// <param name="cacheClient"></param>
    /// <param name="domain"></param>
    /// <returns>Some cache data was cleared</returns>
    public static async Task<bool> ClearDomainAsync (this ICacheClient cacheClient, string domain)
    {
      try {
        var extensions = await GetExtensionsAsync ();
        if (log.IsDebugEnabled) {
          log.Debug ($"ClearDomainAsync: {extensions.Count ()} extensions");
        }
        using (var cacheClientBatchUpdater = new CacheClientBatchUpdater (cacheClient)) {
          var tasks = extensions
            .Select (async ext => await ClearDomainAsync (cacheClient, domain, ext));
          var results = await Task.WhenAll (tasks);
          var result = results.Any (x => x);
          if (log.IsDebugEnabled) {
            log.Debug ($"ClearDomainAsync: completed, result={result}");
          }
          return result;
        }
      }
      catch (Exception ex) {
        log.Error ("ClearDomainAsync: exception", ex);
        throw;
      }
    }

    static async Task<bool> ClearDomainAsync (ICacheClient cacheClient, string domain, Extensions.Business.Cache.ICacheDomainExtension extension)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"ClearDomainAsync: about to process {extension} domain {domain}");
      }
      try {
        var result = await extension.ClearDomainAsync (cacheClient, domain);
        if (log.IsDebugEnabled) {
          log.Debug ($"ClearDomainAsync: result is {result} for {extension} domain {domain}");
        }
        return result;
      }
      catch (Exception ex) {
        log.Error ($"ClearDomainAsync: exception in {extension} domain {domain}", ex);
        return false;
      }
    }

    /// <summary>
    /// Clear a domain
    /// </summary>
    /// <param name="cacheClient"></param>
    /// <param name="domain"></param>
    /// <param name="machineId"></param>
    /// <returns>Some cache data was cleared</returns>
    public static bool ClearDomainByMachine (this ICacheClient cacheClient, string domain, int machineId)
    {
      try {
        var extensions = GetExtensions ();
        if (log.IsDebugEnabled) {
          log.Debug ($"ClearDomainByMachine: {extensions.Count ()} extensions");
        }
        var result = false;
        using (var cacheClientBatchUpdater = new CacheClientBatchUpdater (cacheClient)) {
          foreach (var extension in extensions) {
            if (log.IsDebugEnabled) {
              log.Debug ($"ClearDomainByMachine: about to process {extension} domain {domain} machine id {machineId}");
            }
            bool localResult;
            try {
              localResult = extension.ClearDomainByMachine (cacheClient, domain, machineId);
              if (log.IsDebugEnabled) {
                log.Debug ($"ClearDomainByMachine: result is {localResult} for {extension} domain {domain} machine id {machineId}");
              }
            }
            catch (Exception ex) {
              log.Error ($"ClearDomainByMachine: exception in {extension} domain {domain}", ex);
              localResult = false;
            }
            result |= localResult;
          }
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"ClearDomainByMachine: completed, result={result}");
        }
        return result;
      }
      catch (Exception ex) {
        log.Error ("ClearDomainByMachine: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Clear a domain by machine asynchronously
    /// </summary>
    /// <param name="cacheClient"></param>
    /// <param name="domain"></param>
    /// <param name="machineId"></param>
    /// <returns>Some cache data was cleared</returns>
    public static async Task<bool> ClearDomainByMachineAsync (this ICacheClient cacheClient, string domain, int machineId)
    {
      try {
        var extensions = await GetExtensionsAsync ();
        if (log.IsDebugEnabled) {
          log.Debug ($"ClearDomainByMachineAsync: {extensions.Count ()} extensions");
        }
        if (!extensions.Any ()) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ClearDomainByMachineAsync: no extension => return false");
          }
          return false;
        }
        using (var cacheClientBatchUpdater = new CacheClientBatchUpdater (cacheClient)) {
          var tasks = extensions
            .Select (ext => ClearDomainByMachineAsync (cacheClient, domain, machineId, ext));
          var results = await Task.WhenAll (tasks);
          var result = results.Any (x => x);
          if (log.IsDebugEnabled) {
            log.Debug ($"ClearDomainByMachineAsync: completed, result={result}");
          }
          return result;
        }
      }
      catch (Exception ex) {
        log.Error ("ClearDomainByMachineAsync: exception", ex);
        throw;
      }
    }

    static async Task<bool> ClearDomainByMachineAsync (this ICacheClient cacheClient, string domain, int machineId, Lemoine.Extensions.Business.Cache.ICacheDomainExtension extension)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"ClearDomainByMachineAsync: about to process {extension} domain {domain} machine id {machineId}");
      }
      try {
        var result = await extension.ClearDomainByMachineAsync (cacheClient, domain, machineId);
        if (log.IsDebugEnabled) {
          log.Debug ($"ClearDomainByMachineAsync: result is {result} for {extension} domain {domain} machine id {machineId}");
        }
        return result;
      }
      catch (Exception ex) {
        log.Error ($"ClearDomainByMachineAsync: exception in {extension} domain {domain}", ex);
        return false;
      }
    }

    /// <summary>
    /// Clear a domain
    /// </summary>
    /// <param name="cacheClient"></param>
    /// <param name="domain"></param>
    /// <param name="machineModuleId"></param>
    /// <returns>Some cache data was cleared</returns>
    public static bool ClearDomainByMachineModule (this ICacheClient cacheClient, string domain, int machineModuleId)
    {
      try {
        var extensions = GetExtensions ();
        if (log.IsDebugEnabled) {
          log.Debug ($"ClearDomainByMachineModule: {extensions.Count ()} extensions");
        }
        var result = false;
        using (var cacheClientBatchUpdater = new CacheClientBatchUpdater (cacheClient)) {
          foreach (var extension in extensions) {
            if (log.IsDebugEnabled) {
              log.Debug ($"ClearDomainByMachineModule: about to process {extension} domain {domain} machine id {machineModuleId}");
            }
            bool localResult;
            try {
              localResult = extension.ClearDomainByMachineModule (cacheClient, domain, machineModuleId);
              if (log.IsDebugEnabled) {
                log.Debug ($"ClearDomainByMachineModule: result is {localResult} for {extension} domain {domain} machine id {machineModuleId}");
              }
            }
            catch (Exception ex) {
              log.Error ($"ClearDomainByMachineModule: exception in {extension} domain {domain}", ex);
              localResult = false;
            }
            result |= localResult;
          }
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"ClearDomainByMachineModule: completed, result={result}");
        }
        return result;
      }
      catch (Exception ex) {
        log.Error ("ClearDomainByMachineModule: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Clear a domain by machine module asynchronously
    /// </summary>
    /// <param name="cacheClient"></param>
    /// <param name="domain"></param>
    /// <param name="machineModuleId"></param>
    /// <returns>Some cache data was cleared</returns>
    public static async Task<bool> ClearDomainByMachineModuleAsync (this ICacheClient cacheClient, string domain, int machineModuleId)
    {
      try {
        var extensions = GetExtensions ();
        if (log.IsDebugEnabled) {
          log.Debug ($"ClearDomainByMachineModuleAsync: {extensions.Count ()} extensions");
        }
        using (var cacheClientBatchUpdater = new CacheClientBatchUpdater (cacheClient)) {
          var tasks = extensions
            .Select (ext => ClearDomainByMachineModuleAsync (cacheClient, domain, machineModuleId, ext));
          var results = await Task.WhenAll (tasks);
          var result = results.Any (x => x);
          if (log.IsDebugEnabled) {
            log.Debug ($"ClearDomainByMachineModuleAsync: completed, result={result}");
          }
          return result;
        }
      }
      catch (Exception ex) {
        log.Error ("ClearDomainByMachineModuleAsync: exception", ex);
        throw;
      }
    }

    static async Task<bool> ClearDomainByMachineModuleAsync (this ICacheClient cacheClient, string domain, int machineModuleId, Extensions.Business.Cache.ICacheDomainExtension extension)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"ClearDomainByMachineModuleAsync: about to process {extension} domain {domain} machine module id {machineModuleId}");
      }
      try {
        var result = await extension.ClearDomainByMachineModuleAsync (cacheClient, domain, machineModuleId);
        if (log.IsDebugEnabled) {
          log.Debug ($"ClearDomainByMachineModuleAsync: result is {result} for {extension} domain {domain} machine module id {machineModuleId}");
        }
        return result;
      }
      catch (Exception ex) {
        log.Error ($"ClearDomainByMachineModuleAsync: exception in {extension} domain {domain}", ex);
        return false;
      }
    }
  }
}
