// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Threading
{
  /// <summary>
  /// Interface for objects with a checked caller
  /// </summary>
  public interface ICheckedCallers
  {
    /// <summary>
    /// Add a checked caller
    /// </summary>
    /// <param name="caller"></param>
    void AddCheckedCaller (IChecked caller);
  }
}
