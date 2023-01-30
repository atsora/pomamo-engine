// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.Model;
using Lemoine.Stamping.Config;

namespace Lemoine.Stamping.StampingEventHandlersProviders
{
  /// <summary>
  /// StampingEventHandlersProvider
  /// </summary>
  public class EventHandlersProviderFromStampingConfig: IStampingEventHandlersProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (EventHandlersProviderFromStampingConfig).FullName);

    readonly IList<Type> m_eventHandlerTypes = new List<Type> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public EventHandlersProviderFromStampingConfig (TypeLoader typeLoader, StampingConfig stampingConfig)
    {
      foreach (var eventHandler in stampingConfig.EventHandlers) {
        try {
          var type = typeLoader.GetType (eventHandler);
          m_eventHandlerTypes.Add (type);
        }
        catch (Exception ex) {
          log.Error ($"EventHandlersProviderFromStampingConfig: event handler {eventHandler} could not be loaded, skip it", ex);
        }
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandlersProvider"/>
    /// </summary>
    public IEnumerable<Type> EventHandlerTypes => m_eventHandlerTypes;

  }
}
