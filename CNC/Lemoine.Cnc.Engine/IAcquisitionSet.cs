// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// Get an acquisition list
  /// </summary>
  public interface IAcquisitionSet
  {
    /// <summary>
    /// Get an acquisition list
    /// </summary>
    /// <returns></returns>
    IEnumerable<Acquisition> GetAcquisitions (CancellationToken cancellationToken);
  }
}
