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
  public class MessageExtensionByMachineModule
    : Lemoine.Extensions.NotConfigurableExtension
    , IMessageExtension
  {
    readonly Regex REGEX = new Regex (@"Cache/ClearDomainByMachineModule/([A-Za-z]+)/([0-9]+)(\?Broadcast=true)?$");

    readonly ILog log = LogManager.GetLogger (typeof (MessageExtensionByMachineModule).FullName);

    public async System.Threading.Tasks.Task ProcessMessageAsync (string message)
    {
      var match = REGEX.Match (message);
      if (match.Success) {
        var domain = match.Groups[1].Value;
        var id = int.Parse (match.Groups[2].Value);
        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessMessageAsync: clear domain {domain} id {id}");
        }
        await Lemoine.Core.Cache.CacheManager.CacheClient.ClearDomainByMachineModuleAsync (domain, id);
      }
      await System.Threading.Tasks.Task.Delay (0);
    }
  }
}
