// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
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
  /// Return the Atsora Tracking services that are in Auto mode
  /// </summary>
  [SupportedOSPlatform ("windows")]
  public class TrackingServicesProviderOnWindows : AutoServicesProviderOnWindows, IServiceControllersProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (PomamoServicesProviderOnWindows).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public TrackingServicesProviderOnWindows ()
      : base ("Lem_AlertService", "AtrackingAnalysisService", "Lem_AutoReasonService", "Lem_SynchronizationService", "AtrackingAspService", "Lem_CncService", "Lem_CncCoreService", "AtrackingCncService", "AtrackingCncCoreService", "Lem_CncDataService", "Lem_StampingService", "Lem_StampFileWatchService", "TAO_NT_Naming_Service", "omninames", "Tomcat9", "nscp", "MTConnect Agent", "MTConnectAdapterService")
    {
    }
  }
}
