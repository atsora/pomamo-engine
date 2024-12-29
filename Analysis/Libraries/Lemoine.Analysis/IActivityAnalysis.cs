// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Lemoine.Core.ExceptionManagement;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using Lemoine.Collections;
using Lemoine.Core.Performance;

namespace Lemoine.Analysis
{
  /// <summary>
  /// This interface takes care of the activity analysis
  /// for all the machine modules
  /// </summary>
  public interface IActivityAnalysis : IThreadClass
  {
    /// <summary>
    /// Request a pause in the activity analysis
    /// </summary>
    /// <param name="machine">Not null</param>
    /// <param name="modificationId"></param>
    /// <returns>The pause could be set</returns>
    bool RequestPause (IMachine machine, long modificationId);

    /// <summary>
    /// Release a pause in the activity analysis
    /// </summary>
    /// <param name="machine">Not null</param>
    /// <param name="modificationId"></param>
    void ReleasePause (IMachine machine, long modificationId);

    /// <summary>
    /// Check if the activity analysis is already in pause
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="modificationId"></param>
    /// <returns></returns>
    bool IsInPause (IMachine machine, int modificationId);

    /// <summary>
    /// Wait for the pause of the activity analysis
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="modificationId"></param>
    /// <param name="cancellationToken"></param>
    void WaitPause (IMachine machine, long modificationId, CancellationToken cancellationToken);
  }
}
