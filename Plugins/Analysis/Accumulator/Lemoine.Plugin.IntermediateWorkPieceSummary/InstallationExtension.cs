// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Config;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.IntermediateWorkPieceSummary
{
  /// <summary>
  /// This is only done so that the plugin is upgraded and the tables are created by Lem_PackageManager.Console -c
  /// before given the rights to the reports
  /// </summary>
  public class InstallationExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IInstallationExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (InstallationExtension).FullName);

    public double Priority => 0.0;

    public bool CheckConfig ()
    {
      return true;
    }

    public bool RemoveConfig ()
    {
      return true;
    }
  }
}
