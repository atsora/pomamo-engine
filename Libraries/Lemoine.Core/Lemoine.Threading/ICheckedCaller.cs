// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Threading
{
  /// <summary>
  /// Interface for objects with a checked caller
  /// </summary>
  public interface ICheckedCaller
  {
    /// <summary>
    /// Add the checked caller
    /// </summary>
    /// <param name="caller"></param>
    void SetCheckedCaller (IChecked caller);
  }
}
