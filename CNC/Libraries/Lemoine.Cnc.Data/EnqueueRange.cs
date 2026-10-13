// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Collections;

namespace Lemoine.Cnc.Data
{
  /// <summary>
  /// Queue that can add several data at once, for example in a single transaction
  /// </summary>
  public interface IEnqueueRange
  {
    /// <summary>
    /// Add several data at the end of the queue, in this order
    /// </summary>
    /// <param name="items">not null</param>
    void EnqueueRange (IList<ExchangeData> items);
  }

  /// <summary>
  /// Extensions to add several data in a queue
  /// </summary>
  public static class EnqueueRangeExtensions
  {
    /// <summary>
    /// Add several data at the end of the queue, in this order:
    /// at once if the queue implements <see cref="IEnqueueRange"/>, else one by one
    /// </summary>
    /// <param name="queue">not null</param>
    /// <param name="items">not null</param>
    public static void EnqueueRange (this IQueue<ExchangeData> queue, IList<ExchangeData> items)
    {
      if (0 == items.Count) {
        return;
      }
      if (queue is IEnqueueRange enqueueRange) {
        enqueueRange.EnqueueRange (items);
      }
      else {
        foreach (var item in items) {
          queue.Enqueue (item);
        }
      }
    }

    /// <summary>
    /// Add several data in the sub-queues, keeping the order in each sub-queue
    /// </summary>
    /// <param name="items">not null</param>
    /// <param name="getQueue">sub-queue of a data, null if none</param>
    public static void EnqueueRangeInSubQueues (IList<ExchangeData> items, Func<ExchangeData, IQueue<ExchangeData>> getQueue)
    {
      var byQueue = new List<(IQueue<ExchangeData> queue, IList<ExchangeData> items)> ();
      foreach (var item in items) {
        var queue = getQueue (item);
        if (null == queue) {
          continue;
        }
        var index = byQueue.FindIndex (x => object.ReferenceEquals (x.queue, queue));
        if (index < 0) {
          byQueue.Add ((queue, new List<ExchangeData> { item }));
        }
        else {
          byQueue[index].items.Add (item);
        }
      }
      foreach (var (queue, queueItems) in byQueue) {
        queue.EnqueueRange (queueItems);
      }
    }
  }
}
