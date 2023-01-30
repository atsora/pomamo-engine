// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Lemoine.Core.Log;

namespace Lemoine.WebMiddleware.Assemblies
{
  /// <summary>
  /// HandlerAssemblies
  /// </summary>
  public class ServiceAssembliesFromList: IServiceAssembliesResolver
  {
    readonly ILog log = LogManager.GetLogger (typeof (ServiceAssembliesFromList).FullName);

    #region Getters / Setters
    readonly IEnumerable<Assembly> m_assemblies;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ServiceAssembliesFromList (IEnumerable<Assembly> assemblies)
    {
      m_assemblies = assemblies;
    }

    /// <summary>
    /// <see cref="IServiceAssembliesResolver"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Assembly> GetServiceAssemblies ()
    {
      return m_assemblies;
    }
    #endregion // Constructors
  }
}
