// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Interface for all the slot analysis by machine
  /// 
  /// This is used when a global slot must be refined by machine
  /// </summary>
  public interface ISlotAnalysisByMachine: ISlotAnalysis
  {
    /// <summary>
    /// Associated machine
    /// </summary>
    IMachine Machine { get; }
  }
}
