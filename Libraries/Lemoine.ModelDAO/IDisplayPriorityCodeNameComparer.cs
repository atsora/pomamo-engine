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
  /// Interface of an item that can be used with DisplayPriorityCodeNameComparer
  /// </summary>
  public interface IDisplayPriorityCodeNameComparerItem : IDisplayPriorityComparerItem
  {
    /// <summary>
    /// Code
    /// </summary>
    string Code { get; set; }
    /// <summary>
    /// Name
    /// </summary>
    string Name { get; set; }
  }

  /// <summary>
  /// IComparer for display priority
  /// </summary>
  public class DisplayPriorityCodeNameComparer<I, T>
    : IComparer<I>
    where T: class, I, IDisplayPriorityCodeNameComparerItem
  {
    readonly ILog log = LogManager.GetLogger<DisplayPriorityCodeNameComparer<I, T>> ();

    /// <summary>
    /// Implementation of a Comparer that takes into the following properties to sort the items:
    /// <item>Display priority</item>
    /// <item>Code</item>
    /// <item>Name</item>
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

      int displayPriorityComparerResult =
        (new DisplayPriorityComparer<I, T> ()).Compare (x, y);
      if (0 != displayPriorityComparerResult) {
        return displayPriorityComparerResult;
      }
      else {
        var ix = x as IDisplayPriorityCodeNameComparerItem;
        var iy = y as IDisplayPriorityCodeNameComparerItem;

        if ((ix is null) || (iy is null)) {
          log.Fatal ("Compare: one of the item is not a IDisplayPriorityCodeNameComparerItem, which is unexpected");
          Debug.Assert (false);
        }

        int codeComparerResult =
          string.Compare (ix.Code, iy.Code);
        if (0 != codeComparerResult) {
          return codeComparerResult;
        }
        else {
          return string.Compare (ix.Name, iy.Name);
        }
      }
    }

    /// <summary>
    /// Implementation of DisplayComparer
    /// 
    /// Use the following properties to sort the machines:
    /// <item>Display priority</item>
    /// <item>Code</item>
    /// <item>Name</item>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    int IComparer<I>.Compare (I x, I y)
    {
      return (new DisplayPriorityCodeNameComparer<I, T> ())
        .Compare (x, y);
    }
  }
}
