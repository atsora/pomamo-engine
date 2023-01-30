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
  [SupportedOSPlatform ("linux")]
  public class ServicesProviderOnLinux : IServiceControllersProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (ServicesProviderOnLinux).FullName);

    readonly IEnumerable<string> m_serviceNames;

    /// <summary>
    /// Constructor
    /// </summary>
    public ServicesProviderOnLinux (params string[] serviceNames)
    {
      m_serviceNames = serviceNames;
    }

    /// <summary>
    /// Get the set of <see cref="SystemdServiceController"/>
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerable<SystemdServiceController> GetSystemdServiceControllers ()
    {
      return m_serviceNames
        .Select (x => new SystemdServiceController (x))
        .Where (x => x.IsInstalled);
    }

    /// <summary>
    /// <see cref="IServiceControllersProvider"/>
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerable<IServiceController> GetServiceControllers ()
    {
      return GetSystemdServiceControllers ();
    }
  }
}
