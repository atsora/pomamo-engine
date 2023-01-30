// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Operation;
using Lemoine.Model;

namespace Lemoine.Extensions.Business.Group.Impl
{
  /// <summary>
  /// Class to combine IPartProductionDataRange data
  /// considering they are run in a concurrent way.
  /// </summary>
  public class PartProductionDataRangeConcurrent
    : IPartProductionDataRange
  {
    readonly ILog log = LogManager.GetLogger (typeof (PartProductionDataRangeConcurrent).FullName);

    readonly IEnumerable<IPartProductionDataRange> m_items;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="items">not empty</param>
    public PartProductionDataRangeConcurrent (IEnumerable<IPartProductionDataRange> items)
    {
      m_items = items;
    }

    /// <summary>
    /// <see cref="IPartProductionDataRange"/>
    /// </summary>
    public double NbPieces
    {
      get
      {
        if (m_items.Any ()) {
          return m_items
            .Sum (i => i.NbPieces);
        }
        else {
          return 0;
        }
      }
    }

    /// <summary>
    /// <see cref="IPartProductionDataRange"/>
    /// </summary>
    public double? Goal
    {
      get
      {
        var withGoal = m_items
          .Where (i => i.Goal.HasValue);
        if (withGoal.Any ()) {
          return withGoal.Sum (i => i.Goal.Value);
        }
        else {
          return null;
        }
      }
    }

    /// <summary>
    /// <see cref="IPartProductionDataRange"/>
    /// </summary>
    public bool InProgress
    {
      get
      {
        return m_items.Any (i => i.InProgress);
      }
    }

    /// <summary>
    /// <see cref="IPartProductionDataRange"/>
    /// </summary>
    public UtcDateTimeRange Range
    {
      get
      {
        if (!m_items.Any ()) {
          return new UtcDateTimeRange ();
        }
        else if (m_items.Any (i => i.Range.IsEmpty ())) {
          return new UtcDateTimeRange ();
        }
        else { // All are not empty
          var firstItem = m_items.First ();
          if (m_items.Skip (1)
            .All (i => LowerBound.Equals (i.Range.Lower, firstItem.Range.Lower))) { // Same lower for everyone
            return m_items.Skip (1)
              .Aggregate (firstItem.Range, (a, i) => new UtcDateTimeRange (a.Union (i.Range)));
          }
          else {
            return new UtcDateTimeRange ();
          }
        }
      }
    }
  }
}
