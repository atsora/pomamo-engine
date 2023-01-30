// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table productionanalysisstatus
  /// </summary>
  public interface IProductionAnalysisStatus: IVersionable
  {
    /// <summary>
    /// Reference to the Machine
    /// </summary>
    IMachine Machine { get; }

    /// <summary>
    /// Date/time up to which the production analysis is completed (close to now)
    /// </summary>
    DateTime AnalysisDateTime { get; set; }
  }
}
