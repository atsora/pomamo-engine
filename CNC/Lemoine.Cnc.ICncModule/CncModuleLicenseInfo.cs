// Copyright (c) 2023 Nicolas Relange

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Cnc module license
  /// </summary>
  public enum CncModuleLicenseInfo
  {
    /// <summary>
    /// No specific information
    /// </summary>
    None = 0,
    /// <summary>
    /// Gpl
    /// </summary>
    Gpl = 1,
    /// <summary>
    /// Propriatory
    /// </summary>
    Propriatory = 2,
  }
}
