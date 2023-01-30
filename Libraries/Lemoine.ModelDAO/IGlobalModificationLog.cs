// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// GlobalModificationLog model
  /// </summary>
  public interface IGlobalModificationLog: IBaseLog
  {
    /// <summary>
    /// Reference to the modification
    /// </summary>
    IGlobalModification Modification { get; }
  }
}
