// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Extensions.Analysis
{
  /// <summary>
  /// Extension to determine if an operation cycle can be considered full
  /// </summary>
  public interface IOperationCycleFullDetectionExtension: IDetectionExtension, Pulse.Extensions.Database.IOperationCycleFullExtension
  {
  }
}
