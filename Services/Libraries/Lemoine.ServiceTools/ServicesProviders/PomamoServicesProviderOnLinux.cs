// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using Lemoine.Core.Log;
using Lemoine.Info;

namespace Lemoine.ServiceTools.ServicesProviders
{
  /// <summary>
  /// Return the Pomamo services that are in Auto mode
  /// </summary>
  [SupportedOSPlatform ("linux")]
  public class PomamoServicesProviderOnLinux : ServicesProviderOnLinux, IServiceControllersProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (PomamoServicesProviderOnLinux).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public PomamoServicesProviderOnLinux ()
      : base ($"{PulseInfo.LinuxPackageName}-alert", $"{PulseInfo.LinuxPackageName}-analysis", $"{PulseInfo.LinuxPackageName}-autoreason", $"{PulseInfo.LinuxPackageName}-asp", $"{PulseInfo.LinuxPackageName}-cnccore", $"{PulseInfo.LinuxPackageName}-ocnccore", $"{PulseInfo.LinuxPackageName}-cncdata", $"{PulseInfo.LinuxPackageName}-mtconnectadapter", $"{PulseInfo.LinuxPackageName}-stamping", $"{PulseInfo.LinuxPackageName}-stampfilewatch")
    {
    }
  }
}
