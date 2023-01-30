// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Interface of an item that can be used with DisplayPriorityComparer
  /// </summary>
  public interface IDisplayPriorityComparerItem
  {
    /// <summary>
    /// Display priority
    /// </summary>
    int? DisplayPriority { get; set;  }
  }

  /// <summary>
  /// IComparer for display priority
  /// </summary>
  public class DisplayPriorityComparer<I, T>
    : IComparer<I>
    where T : class, I, IDisplayPriorityComparerItem
  {
    readonly ILog log = LogManager.GetLogger<DisplayPriorityComparer<I, T>> ();

    /// <summary>
    /// Implementation of a Comparer that only takes into account the display priority
    /// 
    /// Use the following properties to sort the items:
    /// <item>Display priority</item>
    /// 
    /// null items come at the end
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public int Compare (I x, I y)
    {
      if (null == x) {
        if (null == y) {
          return 0;
        }
        else {
          return int.MaxValue;
        }
      }
      else if (null == y) {
        return int.MinValue;
      }

      Debug.Assert (null != x);
      Debug.Assert (null != y);

      var ix = x as IDisplayPriorityComparerItem;
      var iy = y as IDisplayPriorityComparerItem;

      if ((ix is null) || (iy is null)) {
        log.Fatal ("Compare: one of the item is not a IDisplayPriorityComparerItem, which is unexpected");
        Debug.Assert (false);
      }

      if (!ix.DisplayPriority.HasValue
          && !iy.DisplayPriority.HasValue) {
        return 0;
      }
      else if (!ix.DisplayPriority.HasValue) {
        Debug.Assert (iy.DisplayPriority.HasValue);
        return int.MaxValue;
      }
      else if (!iy.DisplayPriority.HasValue) {
        Debug.Assert (ix.DisplayPriority.HasValue);
        Debug.Assert (int.MinValue < 0);
        return int.MinValue;
      }
      else {
        Debug.Assert (ix.DisplayPriority.HasValue);
        Debug.Assert (iy.DisplayPriority.HasValue);
        return ix.DisplayPriority.Value - iy.DisplayPriority.Value;
      }
    }

    /// <summary>
    /// Implementation of DisplayComparer
    /// 
    /// Use the following properties to sort the machines:
    /// <item>Display priority</item>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    int IComparer<I>.Compare (I x, I y)
    {
      return (new DisplayPriorityComparer<I, T> ())
        .Compare (x, y);
    }
  }
}
