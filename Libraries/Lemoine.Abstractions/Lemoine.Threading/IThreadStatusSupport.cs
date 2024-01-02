// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;

namespace Lemoine.Threading
{
  /// <summary>
  /// Object with an internal status to be used to run the execute the object only once
  /// (for example in a thread pool)
  /// </summary>
  public interface IThreadStatusSupport
  {
    /// <summary>
    /// Request an analysis
    /// 
    /// To be called before trying to run the analysis in a thread pool
    /// </summary>
    /// <returns>the object could be set in the Request status</returns>
    bool Request ();

    /// <summary>
    /// Reset the request status (back to Available status)
    /// for example in case of error
    /// </summary>
    /// <returns>Success: the status was Requested</returns>
    bool ResetRequested ();

    /// <summary>
    /// Reset the status to Available
    /// for example in case of error
    /// </summary>
    void ResetToAvailable ();

    /// <summary>
    /// Check if the analysis can be run,
    /// this it is not currently running on an another thread
    /// </summary>
    /// <returns></returns>
    bool CanRun ();

    /// <summary>
    /// Run in pool
    /// </summary>
    /// <param name="stateInfo">cancellation token</param>
    /// <param name="action">action</param>
    void RunInPool (Object stateInfo, Action<CancellationToken> action);
  }
}
