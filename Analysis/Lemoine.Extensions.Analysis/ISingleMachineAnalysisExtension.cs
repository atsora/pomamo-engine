// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using Lemoine.Model;

namespace Lemoine.Extensions.Analysis
{
  /// <summary>
  /// Basic interface to extend the analysis of a single machine
  /// </summary>
  public interface ISingleMachineAnalysisExtension: Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialization phase
    /// </summary>
    /// <param name="machine"></param>
    bool Initialize (IMachine machine);
    
    /// <summary>
    /// Run the analysis once
    /// </summary>
    void RunOnce (CancellationToken cancellationToken);
  }
}
