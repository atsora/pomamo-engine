// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using Lemoine.Collections;
using Lemoine.Core.Log;

namespace Lemoine.ServiceTools.ServicesProviders
{
  /// <summary>
  /// Get additional services from a config key
  /// </summary>
  [SupportedOSPlatform ("linux")]
  public class ConfigServicesProviderOnLinux : IServiceControllersProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConfigServicesProviderOnLinux).FullName);

    static readonly string ADDITIONAL_SERVICES_KEY = "Services.Additional";
    static readonly string ADDITIONAL_SERVICES_DEFAULT = ",";

    IEnumerable<string>? m_serviceNames = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public ConfigServicesProviderOnLinux ()
    {
    }

    /// <summary>
    /// Config key
    /// </summary>
    public static string ConfigKey => ADDITIONAL_SERVICES_KEY;

    IEnumerable<string> GetServiceNames ()
    {
      if (m_serviceNames is null) {
        var additionalServices = Lemoine.Info.ConfigSet
          .LoadAndGet (ADDITIONAL_SERVICES_KEY, ADDITIONAL_SERVICES_DEFAULT);
        m_serviceNames = EnumerableString.ParseListString (additionalServices)
          .Where (x => !string.IsNullOrEmpty (x?.Trim ()));
      }
      return m_serviceNames;
    }

    /// <summary>
    /// <see cref="IServiceControllersProvider"/>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<IServiceController> GetServiceControllers ()
    {
      return GetServiceNames ()
        .Select (x => new SystemdServiceController (x))
        .Where (x => x.IsInstalled);
    }
  }
}
