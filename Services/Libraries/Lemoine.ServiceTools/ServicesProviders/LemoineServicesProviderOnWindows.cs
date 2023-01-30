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
  [SupportedOSPlatform ("windows")]
  public class LemoineServicesProviderOnWindows : AutoServicesProviderOnWindows, IServiceControllersProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (LemoineServicesProviderOnWindows).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public LemoineServicesProviderOnWindows()
      : base ("Lem_AlertService", "Lem_AnalysisService", "Lem_AutoReasonService", "Lem_PFR", "Lem_SynchronizationService", "Lem_AspService", "Lem_CncService", "Lem_CncCoreService", "Lem_CncDataService", "TAO_NT_Naming_Service", "omninames", "Tomcat9", "nscp", "MTConnect Agent", "MTConnectAdapterService")
    {
    }
  }
}
