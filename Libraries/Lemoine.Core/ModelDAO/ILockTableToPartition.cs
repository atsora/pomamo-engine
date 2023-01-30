// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// LockTableToPartition
  /// </summary>
  public interface ILockTableToPartition: IDisposable
  {
    /// <summary>
    /// Execute the requests
    /// </summary>
    void Run ();

    /// <summary>
    /// Reset the locked table
    /// </summary>
    void Reset ();
  }
}
