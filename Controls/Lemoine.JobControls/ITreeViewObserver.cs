// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.JobControls
{
  /// <summary>
  /// Description of ITreeViewObserver.
  /// </summary>
  public interface ITreeViewObserver
  {
    /// <summary>
    ///   Update state of this observer according to the change 
    ///   on observable
    /// </summary>
    void UpdateInfo (ITreeViewObservable observable);
  }

}
