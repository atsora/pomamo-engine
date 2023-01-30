// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Core.Hosting.LctrChecker
{
  /// <summary>
  /// Check the current server is Lctr without using the database,
  /// but for example using the Pfrdata directory
  /// </summary>
  public class PfrDataDirectoryLctrChecker : ILctrChecker
  {
    readonly ILog log = LogManager.GetLogger (typeof (PfrDataDirectoryLctrChecker).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public PfrDataDirectoryLctrChecker ()
    {
    }

    /// <summary>
    /// <see cref="ILctrChecker"/>
    /// </summary>
    /// <returns></returns>
    public bool IsLctr ()
    {
      var pfrDataDirectory = Lemoine.Info.PulseInfo.PfrDataDir;
      if (!Directory.Exists (pfrDataDirectory)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsLctr: directory {pfrDataDirectory} does not exist => return false");
        }
        return false;
      }
      else { // Check there is at least one CNC configuration file in it
        var cncConfigsDirectory = Path.Combine (pfrDataDirectory, "cncconfigs");
        if (!Directory.Exists (cncConfigsDirectory)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsLctr: directory {cncConfigsDirectory} does not exist => return false");
          }
          return false;
        }
        var xmlConfigurationFiles = Directory.GetFiles (cncConfigsDirectory, "*.xml", SearchOption.TopDirectoryOnly);
        if (xmlConfigurationFiles.Any ()) {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsLctr: there is at least one XML file in {cncConfigsDirectory} => return true");
          }
          return true;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsLctr: no XML file in {cncConfigsDirectory} => return false");
          }
          return false;
        }
      }
    }
  }
}
