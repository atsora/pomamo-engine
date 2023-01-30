// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;

namespace Lemoine.Stamping.Impl
{
  /// <summary>
  /// StampingApplicationBuilder
  /// </summary>
  public class StampingApplicationBuilder
    : IStampingApplicationBuilder
  {
    readonly ILog log = LogManager.GetLogger (typeof (StampingApplicationBuilder).FullName);

    readonly IServiceProvider m_serviceProvider;
    readonly TypeLoader m_typeLoader;
    IStampingEventHandler? m_firstEventHandler;
    IStampingEventHandler? m_lastEventHandler;

    /// <summary>
    /// <see cref="IStampingApplicationBuilder"/>
    /// </summary>
    public IStampingEventHandler EventHandler => m_firstEventHandler ?? new StampingEventHandlers.DummyEventHandler ();

    /// <summary>
    /// Constructor
    /// </summary>
    public StampingApplicationBuilder (IServiceProvider serviceProvider, TypeLoader typeLoader)
    {
      m_serviceProvider = serviceProvider;
      m_typeLoader = typeLoader;
    }

    /// <summary>
    /// Add a new stamping event handler that will be create through dependency injection
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns></returns>
    public IStampingApplicationBuilder UseEventHandler<T> ()
      where T : IStampingEventHandler
    {
      return UseEventHandler (typeof (T));
    }

    /// <summary>
    /// <see cref="IStampingApplicationBuilder"/>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public IStampingApplicationBuilder UseEventHandler (Type type)
    {
      var stampingEventHandler = (IStampingEventHandler?)m_serviceProvider.GetService (type);
      if (stampingEventHandler is null) {
        log.Error ($"UseEventHandler: {type} not found");
        throw new ArgumentException ("Invalid type");
      }
      if (m_firstEventHandler is null) {
        m_firstEventHandler = stampingEventHandler;
      }
      if (m_lastEventHandler is not null) {
        m_lastEventHandler.Next = stampingEventHandler;
      }
      m_lastEventHandler = stampingEventHandler;
      return this;
    }

    public IStampingApplicationBuilder UseEventHandler (string typeFullName)
    {
      var type = m_typeLoader.GetType (typeFullName);
      return UseEventHandler (type);
    }
  }
}
