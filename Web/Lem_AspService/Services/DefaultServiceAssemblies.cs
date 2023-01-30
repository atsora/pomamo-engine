// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Web;
using Lemoine.WebMiddleware.Assemblies;

namespace Lem_AspService.Services
{
  /// <summary>
  /// HandlerAssemblies
  /// </summary>
  public class DefaultServiceAssemblies : IServiceAssembliesResolver
  {
    readonly ILog log = LogManager.GetLogger (typeof (DefaultServiceAssemblies).FullName);

    #region Getters / Setters
    readonly IEnumerable<Assembly> m_assemblies;
    readonly IExtensionsLoader? m_extensionsLoader;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public DefaultServiceAssemblies (IEnumerable<Assembly> assemblies, IExtensionsLoader? extensionsLoader = null)
    {
      m_assemblies = assemblies;
      m_extensionsLoader = extensionsLoader;
    }

    /// <summary>
    /// <see cref="IServiceAssembliesResolver"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Assembly> GetServiceAssemblies ()
    {
      if (null != m_extensionsLoader) {
        System.Threading.Tasks.Task.Run (async() => await m_extensionsLoader.LoadExtensionsAsync ()).Wait ();
      }

      IEnumerable<IWebExtension> extensions = ExtensionManager
        .GetExtensions<IWebExtension> ();
      var extensionAssemblies = extensions
        .Select (ext => ext.GetAssembly ())
        .ToList ();
      if (log.IsInfoEnabled) {
        foreach (var extensionAssembly in extensionAssemblies) {
          log.Info ($"GetServiceAssemblies: add {extensionAssembly} from extension");
        }
      }

      return m_assemblies.Concat (extensionAssemblies);
    }
    #endregion // Constructors
  }
}
