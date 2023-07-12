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
  public class MessageExtensionGlobal
    : Lemoine.Extensions.NotConfigurableExtension
    , IMessageExtension
  {
    static readonly string PREFIX = "Cache/ClearDomain/";
    readonly Regex REGEX = new Regex ($@"{PREFIX}([A-Za-z]+)(\?Broadcast=true)?$");

    readonly ILog log = LogManager.GetLogger (typeof (MessageExtensionGlobal).FullName);

    public void ProcessMessage (string message)
    {
      if (message.StartsWith (PREFIX)) {
        var match = REGEX.Match (message);
        if (match.Success) {
          var domain = match.Groups[1].Value;
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessMessage: clear domain {domain}");
          }
          Lemoine.Core.Cache.CacheManager.CacheClient.ClearDomain (domain);
        }
      }
    }

    public async System.Threading.Tasks.Task ProcessMessageAsync (string message)
    {
      if (message.StartsWith (PREFIX)) {
        var match = REGEX.Match (message);
        if (match.Success) {
          var domain = match.Groups[1].Value;
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessMessageAsync: clear domain {domain}");
          }
          await Lemoine.Core.Cache.CacheManager.CacheClient.ClearDomainAsync (domain);
        }
      }
    }
  }
}
