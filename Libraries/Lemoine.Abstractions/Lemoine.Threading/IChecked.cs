// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;

namespace Lemoine.Threading
{
  /// <summary>
  /// Interface that must be implemented by
  /// any class that may return to the top class that the thread is still active
  /// </summary>
  public interface IChecked
  {
    /// <summary>
    /// Method to call regularly to keep the thread active
    /// </summary>
    void SetActive ();

    /// <summary>
    /// Pause the check of a thread
    /// </summary>
    void PauseCheck ();

    /// <summary>
    /// Resume the check of a thread
    /// </summary>
    void ResumeCheck ();
  }
}
