// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// MachineModificationLog model
  /// </summary>
  public interface IMachineModificationLog: IBaseLog
  {
    /// <summary>
    /// Reference to the modification
    /// </summary>
    IMachineModification Modification { get; set; }
  }
}
