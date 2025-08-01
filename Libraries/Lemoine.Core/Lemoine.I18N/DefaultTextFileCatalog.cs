// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.I18N
{
  /// <summary>
  /// DefaultTextFileCatalog
  /// </summary>
  public sealed class DefaultTextFileCatalog: TextFileCatalog
  {
    readonly ILog log = LogManager.GetLogger (typeof (DefaultTextFileCatalog).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DefaultTextFileCatalog ()
      : base ("Catalog", Lemoine.Info.PulseInfo.CommonConfigurationDirectory)
    {
    }
  }
}
