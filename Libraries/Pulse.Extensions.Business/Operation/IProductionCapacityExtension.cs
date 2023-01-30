// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.Extensions.Business.Operation
{
  /// <summary>
  /// 
  /// </summary>
  public interface IProductionCapacityExtension : IExtension
  {
    /// <summary>
    /// Initialize the plugin. If false is returned, do not consider the plugin
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    bool Initialize (IMachine machine);

    /// <summary>
    /// Score of the extension (a plugin with a higher score is considered first)
    /// </summary>
    double Score { get; }

    /// <summary>
    /// Get a Capacity per hour
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<double> GetCapacityPerHourAsync (IIntermediateWorkPiece intermediateWorkPiece);
  }
}
