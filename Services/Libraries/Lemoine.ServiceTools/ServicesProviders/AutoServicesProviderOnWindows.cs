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
  /// returning only the installed services that are set to start Automatically
  /// </summary>
  [SupportedOSPlatform ("windows")]
  public class AutoServicesProviderOnWindows : ServicesProviderOnWindows, IServiceControllersProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (AutoServicesProviderOnWindows).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoServicesProviderOnWindows (params string[] serviceNames)
      : base (serviceNames)
    {
    }

    /// <summary>
    /// Constructor that distinguishes the services of the product from the external services
    /// </summary>
    /// <param name="productServiceNames">Services of the product, whose name gets the suffix of the instance, -dev in a dev instance</param>
    /// <param name="externalServiceNames">External services, whose name does not depend on the instance</param>
    public AutoServicesProviderOnWindows (IEnumerable<string> productServiceNames, IEnumerable<string> externalServiceNames)
      : base (productServiceNames, externalServiceNames)
    {
    }

    bool IsAuto (WindowsServiceController service)
    {
      try {
        return service.StartupType.Equals (StartupType.Auto);
      }
      catch (Exception ex) {
        log.Error ($"IsAuto: start up type of {service} Path={service.WmiPath} unknown", ex);
        return false;
      }
    }

    /// <summary>
    /// Keep only the services with StartupType="Automatic"
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<WindowsServiceController> GetWindowsServiceControllers ()
    {
      return base.GetWindowsServiceControllers ()
        .Where (x => IsAuto (x));
    }
  }
}
