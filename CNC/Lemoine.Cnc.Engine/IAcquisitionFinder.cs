// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.CncEngine;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// Interface for set the rules to find the right acquisition
  /// </summary>
  public interface IAcquisitionFinder
  {
    /// <summary>
    /// Return true if an acquisition matches an identifier
    /// </summary>
    /// <param name="acquisition"></param>
    /// <param name="identifier"></param>
    /// <returns></returns>
    bool IsMatch (Acquisition acquisition, string identifier);
  }
}
