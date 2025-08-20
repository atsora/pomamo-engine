// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.AutoReason
{
  /// <summary>
  /// Action to update an auto-reason state
  /// </summary>
  public interface IStateAction: IAutoReasonAction
  {
    /// <summary>
    /// Reset some internal values in case of failure
    /// </summary>
    void Reset ();
  }
}
