// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using Lemoine.Core.Log;

namespace Lemoine.ServiceTools.ServicesProviders
{
  /// <summary>
  /// Implements <see cref="IServiceControllersProvider"/> using a list of service names,
  /// returning only the installed services
  /// </summary>
  [SupportedOSPlatform ("windows")]
  public class ServicesProviderOnWindows: IServiceControllersProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (ServicesProviderOnWindows).FullName);

    readonly IEnumerable<string> m_serviceNames;

    /// <summary>
    /// Constructor
    /// </summary>
    public ServicesProviderOnWindows(params string[] serviceNames)
    {
      m_serviceNames = serviceNames;
    }

    /// <summary>
    /// Get the set of <see cref="WindowsServiceController"/>
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerable<WindowsServiceController> GetWindowsServiceControllers ()
    {
      return m_serviceNames
        .Select (x => new WindowsServiceController (x))
        .Where (x => x.IsInstalled);
    }

    /// <summary>
    /// <see cref="IServiceControllersProvider"/>
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerable<IServiceController> GetServiceControllers ()
    {
      return GetWindowsServiceControllers ();
    }
  }
}
