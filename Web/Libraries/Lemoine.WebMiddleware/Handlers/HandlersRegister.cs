// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Microsoft.Extensions.DependencyInjection;

namespace Lemoine.WebMiddleware.Handlers
{
  /// <summary>
  /// HandlersRegister
  /// </summary>
  public class HandlersRegister
  {
    readonly ILog log = LogManager.GetLogger (typeof (HandlersRegister).FullName);

    readonly IHandlersResolver m_handlersResolver;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="handlersResolver">not null</param>
    public HandlersRegister (IHandlersResolver handlersResolver)
    {
      Debug.Assert (null != handlersResolver);

      m_handlersResolver = handlersResolver;
    }

    /// <summary>
    /// Register the handlers in serviceCollection
    /// </summary>
    /// <param name="serviceCollection">not null</param>
    public void Register (IServiceCollection serviceCollection)
    {
      Debug.Assert (null != serviceCollection);

      var handlers = m_handlersResolver.GetHandlers ();
      foreach (var handler in handlers.Where (t => !t.IsAbstract)) {
        serviceCollection.AddTransient (handler);
      }
    }
    #endregion // Constructors

  }
}
