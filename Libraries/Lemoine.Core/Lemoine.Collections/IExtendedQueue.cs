// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Collections
{
  /// <summary>
  /// Queue interface that extends IQueue with:
  /// <item>Peek (n elements)</item>
  /// <item>UnsafeDequeue</item>
  /// <item>VacuumIfNeeded</item>
  /// </summary>
  public interface IExtendedQueue<T>: IQueue<T>, IDisposable
  {
    /// <summary>
    /// Peek up to n elements in the queue in the same time.
    /// 
    /// Given the implementation of the queue, less elements may be returned.
    /// </summary>
    /// <param name="nbElements"></param>
    /// <returns></returns>
    IList<T> Peek (int nbElements);
    
    /// <summary>
    /// Dequeue without checking for the queue emptyness
    /// Only use if one is sure the queue contains an element
    /// </summary>
    void UnsafeDequeue();
    
    /// <summary>
    /// Dequeue n elements without checking the queue has at least n elements
    /// Only use if one is sure the queue contains at least n elements
    /// </summary>
    void UnsafeDequeue(int n);
       
    /// <summary>
    /// Vacuum queue if needed (useful for DB implementation);
    /// returns true if a vacuum did occur
    /// </summary>
    bool VacuumIfNeeded();    
  }
}
