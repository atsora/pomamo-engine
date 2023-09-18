// Copyright (c) 2023 Nicolas Relange

#if NET40
using Lemoine.Cnc.Engine;
using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lem_CncService
{
  /// <summary>
  /// Implementation of <see cref="ICncEngineConfig"/> for .Net Framework 4.0
  /// </summary>
  public class CncEngineConfig40: ICncEngineConfig
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncEngineConfig40).FullName);

    public string ConsoleProgramName => "Lem_CncConsole";

    /// <summary>
    /// Constructor
    /// </summary>
    public CncEngineConfig40 ()
    {
    }
  }
}
#endif // NET40
