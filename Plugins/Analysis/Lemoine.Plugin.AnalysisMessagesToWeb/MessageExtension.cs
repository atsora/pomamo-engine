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

namespace Lemoine.Plugin.AnalysisMessagesToWeb
{
  public class MessageExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IMessageExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (MessageExtension).FullName);

    readonly Lemoine.WebClient.Query m_webQuery = new Lemoine.WebClient.Query ();

    public MessageExtension ()
    {
      m_webQuery.Timeout = TimeSpan.FromSeconds (5);
    }

    public void ProcessMessage (string message)
    {
      DateTime requestStart = DateTime.UtcNow;
      try {
        m_webQuery.Execute (message);
      }
      catch (Exception ex) {
        log.Error ($"ProcessMessage: exception for {message} (timeout={m_webQuery.Timeout} reached, requestStart={requestStart}?)", ex);
      }
    }

    public async System.Threading.Tasks.Task ProcessMessageAsync (string message)
    {
      DateTime requestStart = DateTime.UtcNow;
      try {
        await m_webQuery.ExecuteAsync (message);
      }
      catch (Exception ex) {
        log.Error ($"ProcessMessageAsync: exception for {message} (timeout={m_webQuery.Timeout} reached, requestStart={requestStart}?)", ex);
      }
    }
  }
}
