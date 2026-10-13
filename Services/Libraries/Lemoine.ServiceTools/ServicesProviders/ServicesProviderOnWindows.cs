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
    readonly IEnumerable<string> m_productServiceNames = new List<string> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public ServicesProviderOnWindows(params string[] serviceNames)
    {
      m_serviceNames = serviceNames;
    }

    /// <summary>
    /// Constructor that distinguishes the services of the product from the external services
    /// </summary>
    /// <param name="productServiceNames">Services of the product, whose name gets the suffix of the instance, -dev in a dev instance (see <see cref="Lemoine.Info.InstanceInfo"/>)</param>
    /// <param name="externalServiceNames">External services, whose name does not depend on the instance</param>
    public ServicesProviderOnWindows (IEnumerable<string> productServiceNames, IEnumerable<string> externalServiceNames)
    {
      m_productServiceNames = productServiceNames;
      m_serviceNames = externalServiceNames;
    }

    /// <summary>
    /// Get the set of <see cref="WindowsServiceController"/>
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerable<WindowsServiceController> GetWindowsServiceControllers ()
    {
      // The suffix of the instance is applied here, once the configuration is loaded
      return m_productServiceNames
        .Select (x => Lemoine.Info.InstanceInfo.GetServiceName (x))
        .Concat (m_serviceNames)
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
