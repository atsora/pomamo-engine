// Copyright (c) 2023 Atsora Solutions

using Lemoine.Cnc.Engine;
using Lemoine.Core.Log;
using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lem_CncGUI
{
  /// <summary>
  /// CncEngineConfig
  /// </summary>
  public class CncEngineConfig: ICncEngineConfig
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncEngineConfig).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncEngineConfig ()
    {
    }

    public string ConsoleProgramName => "Lem_CncConsole";

    public string RepositoryCacheFileName => null;

    public bool FilterCncAcquisition (ICncAcquisition cncAcquisition) => !cncAcquisition.License.HasFlag (CncModuleLicense.Proprietary);
  }
}
