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
  /// Return the connector services that are in Auto mode
  /// </summary>
  [SupportedOSPlatform ("windows")]
  public class ConnectorServicesProviderOnWindows : AutoServicesProviderOnWindows, IServiceControllersProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (PomamoServicesProviderOnWindows).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ConnectorServicesProviderOnWindows ()
      : base ("AconnectorCncService", "AconnectorCncCoreService", "AconnectorOpenCncService", "AconnectorOpenCncCoreService")
    {
    }
  }
}
