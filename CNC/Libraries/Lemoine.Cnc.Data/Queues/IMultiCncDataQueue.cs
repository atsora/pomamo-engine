// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Collections;
namespace Lemoine.Cnc.Data
{
  /// <summary>
  /// ICncDataQueue multiple queues implementation
  /// </summary>
  public interface IMultiCncDataQueue : ICncDataQueue
  {
    /// <summary>
    /// Index of current queue
    /// </summary>
    int CurrentQueueIndex { get; }
    
    /// <summary>
    /// Move to next queue (modulo number of queues)
    /// </summary>
    void MoveNextQueue();
    
    /// <summary>
    /// Go to the first internal single queue
    /// </summary>
    void Reset ();
  }
}
