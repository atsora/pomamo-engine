// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Collections
{
  /// <summary>
  /// Interface to represent a first-in, first-out collection of objects (FIFO)
  /// </summary>
  public interface IQueue<T>
  {
    /// <summary>
    /// Gets the number of elements contained in the Queue. 
    /// </summary>
    int Count { get; }
    
    /// <summary>
    /// Removes all objects from the queue
    /// </summary>
    void Clear ();
    
    /// <summary>
    /// Removes and returns the object at the beginning of the Queue. 
    /// </summary>
    /// <returns>The object that is removed from the beginning of the Queue.</returns>
    T Dequeue ();
    
    /// <summary>
    /// Adds an object to the end of the Queue. 
    /// </summary>
    /// <param name="item">The object to add to the Queue. The value can be a null reference for reference types.</param>
    void Enqueue (T item);
    
    /// <summary>
    /// Returns the object at the beginning of the Queue without removing it.
    /// 
    /// InvalidOperationException is returned in case there is no item in the queue
    /// </summary>
    /// <returns>The object at the beginning of the Queue. </returns>
    T Peek ();

    /// <summary>
    /// Close the queue
    /// </summary>
    void Close ();

    /// <summary>
    /// Delete the queue
    /// </summary>
    void Delete();
  }
}
