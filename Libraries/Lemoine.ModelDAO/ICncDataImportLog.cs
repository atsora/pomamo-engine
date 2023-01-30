// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// CncDataImportLog model
  /// </summary>
  public interface ICncDataImportLog : IBaseLog
  {
    /// <summary>
    /// Reference to the Monitored Machine
    /// </summary>
    IMachine Machine { get; }

    /// <summary>
    /// Reference to the Machine Module
    /// (may be null)
    /// </summary>
    IMachineModule MachineModule { get; }
  }
}
