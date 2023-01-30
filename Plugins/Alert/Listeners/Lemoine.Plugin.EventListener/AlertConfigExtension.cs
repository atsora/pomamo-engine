// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Alert;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.EventListener
{
  public class AlertConfigExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IAlertConfigExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (AlertConfigExtension).FullName);

    public bool Initialize ()
    {
      return true;
    }

    public IEnumerable<IListener> Listeners => new List<IListener> () { new Lemoine.Alert.GDBListeners.EventListener () };

    public IEnumerable<TriggeredAction> TriggeredActions => null;
  }
}
