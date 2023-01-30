// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Reference data interface
  /// 
  /// This is implemented by most Type classes
  /// </summary>
  public interface IReferenceData
  {
    /// <summary>
    /// Is the reference data undefined ?
    /// </summary>
    /// <returns></returns>
    bool IsUndefined ();
  }
}
