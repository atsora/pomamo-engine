// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Analysis.Detection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemoine.Analysis
{
  /// <summary>
  /// Detection analysis interface
  /// </summary>
  public interface IDetectionAnalysis : ISingleAnalysis
  {
    /// <summary>
    /// Operation detection
    /// </summary>
    Lemoine.Extensions.Analysis.Detection.IOperationDetection OperationDetection { get; }
  }
}
