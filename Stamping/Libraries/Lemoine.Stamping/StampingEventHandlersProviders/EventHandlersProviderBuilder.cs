// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.StampingEventHandlersProviders
{
  /// <summary>
  /// EventHandlersProviderBuilder
  /// </summary>
  public class EventHandlersProviderBuilder: IStampingEventHandlersProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (EventHandlersProviderBuilder).FullName);

    readonly IList<Type> m_eventHandlerTypes = new List<Type> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public EventHandlersProviderBuilder ()
    {
    }

    /// <summary>
    /// Add a IStampingEventHandler
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public EventHandlersProviderBuilder Add<T> ()
      where T : IStampingEventHandler
    {
      m_eventHandlerTypes.Add (typeof (T));
      return this;
    }

    /// <summary>
    /// <see cref="IStampingEventHandlersProvider"/>
    /// </summary>
    public IEnumerable<Type> EventHandlerTypes => m_eventHandlerTypes;
  }
}
