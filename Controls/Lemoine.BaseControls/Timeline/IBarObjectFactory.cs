// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Interface for a factory of BarObjects
  /// </summary>
  public interface IBarObjectFactory
  {
    /// <summary>
    /// Create all BarObjects included in a period
    /// </summary>
    /// <param name="start">beginning of the period</param>
    /// <param name="end">end of the period</param>
    /// <returns></returns>
    IList<BarObject> CreateBarObjects(DateTime start, DateTime end);  }
}
