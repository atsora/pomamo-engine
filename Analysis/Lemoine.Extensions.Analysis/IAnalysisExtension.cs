// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.Extensions.Analysis
{
  /// <summary>
  /// Common interface for all the detection extensions
  /// </summary>
  public interface IAnalysisExtension: Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the machine
    /// </summary>
    /// <param name="machine"></param>
    bool Initialize (IMonitoredMachine machine);    
  }
}
