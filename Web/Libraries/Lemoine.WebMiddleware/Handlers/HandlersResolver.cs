// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Lemoine.Core.Log;
using Lemoine.WebMiddleware.Assemblies;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;

namespace Lemoine.WebMiddleware.Handlers
{
  /// <summary>
  /// HandlersResolver
  /// </summary>
  public class HandlersResolver: IHandlersResolver
  {
    readonly ILog log = LogManager.GetLogger (typeof (HandlersResolver).FullName);

    readonly IEnumerable<Assembly> m_assemblies;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="assembliesResolver">not null</param>
    public HandlersResolver (IServiceAssembliesResolver assembliesResolver)
    {
      Debug.Assert (null != assembliesResolver);

      m_assemblies = assembliesResolver.GetServiceAssemblies ();
    }

    public IEnumerable<Type> GetHandlers ()
    {
      return m_assemblies
        .SelectMany (a => a.GetTypes ())
        .Where (t => t.GetInterfaces ().Contains (typeof (IHandler)))
        .Where (t => !t.IsAbstract);
    }
    #endregion // Constructors

  }
}
