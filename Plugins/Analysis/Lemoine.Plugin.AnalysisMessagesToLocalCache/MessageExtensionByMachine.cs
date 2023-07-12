// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Database;
using System.Text.RegularExpressions;
using Lemoine.Business.Cache;

namespace Lemoine.Plugin.AnalysisMessagesToLocalCache
{
  public class MessageExtensionByMachine
    : Lemoine.Extensions.NotConfigurableExtension
    , IMessageExtension
  {
    static readonly string PREFIX = "Cache/ClearDomainByMachine/";
    readonly Regex REGEX = new Regex ($@"{PREFIX}([A-Za-z]+)/([0-9]+)(\?Broadcast=true)?$");

    readonly ILog log = LogManager.GetLogger (typeof (MessageExtensionByMachine).FullName);

    public void ProcessMessage (string message)
    {
      if (message.StartsWith (PREFIX)) {
        var match = REGEX.Match (message);
        if (match.Success) {
          var domain = match.Groups[1].Value;
          var id = int.Parse (match.Groups[2].Value);
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessMessage: clear domain {domain} id {id}");
          }
          try {
            var result = Lemoine.Core.Cache.CacheManager.CacheClient.ClearDomainByMachine (domain, id);
            if (log.IsDebugEnabled) {
              log.Debug ($"ProcessMessage: completed with resul={result}");
            }
          }
          catch (Exception ex) {
            log.Exception (ex, "ProcessMessage");
            throw;
          }
        }
      }
    }

    public async System.Threading.Tasks.Task ProcessMessageAsync (string message)
    {
      if (message.StartsWith (PREFIX)) {
        var match = REGEX.Match (message);
        if (match.Success) {
          var domain = match.Groups[1].Value;
          var id = int.Parse (match.Groups[2].Value);
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessMessageAsync: clear domain {domain} id {id}");
          }
          try {
            var result = await Lemoine.Core.Cache.CacheManager.CacheClient.ClearDomainByMachineAsync (domain, id);
            if (log.IsDebugEnabled) {
              log.Debug ($"ProcessMessageAsync: completed with resul={result}");
            }
          }
          catch (Exception ex) {
            log.Exception (ex, "ProcessMessageAsync");
            throw;
          }
        }
      }
    }
  }
}
