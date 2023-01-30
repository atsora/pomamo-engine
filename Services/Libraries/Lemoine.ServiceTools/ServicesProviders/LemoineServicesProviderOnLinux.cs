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
  /// Return the Lemoine services that are in Auto mode
  /// </summary>
  [SupportedOSPlatform ("linux")]
  public class LemoineServicesProviderOnLinux : ServicesProviderOnLinux, IServiceControllersProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (LemoineServicesProviderOnLinux).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public LemoineServicesProviderOnLinux ()
      : base ("lpulse-alert", "lpulse-analysis", "lpulse-autoreason", "lpulse-asp", "lpulse-cnccore", "lpulse-cncdata", "lpulse-mtconnectadapter")
    {
    }
  }
}
