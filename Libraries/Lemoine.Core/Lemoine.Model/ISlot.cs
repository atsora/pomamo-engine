// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Interface for all the analysis slot models
  /// 
  /// A slot is made of the following properties:
  /// <item>UTC begin date/time</item>
  /// <item>Begin day (from cut-off time)</item>
  /// <item>Optionally a UTC end date/time</item>
  /// <item>Optionally an end day (from cut-off time)</item>
  /// </summary>
  public interface ISlot: IVersionable, IWithRange, IMergeableItem<ISlot>, IComparable, ICloneable
  {
    /// <summary>
    /// Modification tracker level
    /// 
    /// 0: no modification tracker active
    /// 1: first level: do the changes
    /// &gt;2: do nothing
    /// </summary>
    int ModificationTrackerLevel { get; set; }
    
    /// <summary>
    /// Is the slot consolidated ?
    /// </summary>
    bool Consolidated { get; set; }
    
    /// <summary>
    /// Begin date/time of the slot
    /// </summary>
    LowerBound<DateTime> BeginDateTime { get; }
    
    /// <summary>
    /// Optionally end date/time of the slot
    /// </summary>
    UpperBound<DateTime> EndDateTime { get; set; } // set for the unit tests. TODO: remove it in the future
    
    /// <summary>
    /// Begin day (from cut-off time) of the slot
    /// </summary>
    LowerBound<DateTime> BeginDay { get; }
    
    /// <summary>
    /// Optionally end day of the slot
    /// </summary>
    UpperBound<DateTime> EndDay { get; }
    
    /// <summary>
    /// Duration of the slot
    /// </summary>
    TimeSpan? Duration { get; }
    
    /// <summary>
    /// Is the machine slot empty ?
    /// 
    /// If the slot is empty, it will not be inserted in the database.
    /// </summary>
    /// <returns></returns>
    bool IsEmpty ();
    
    /// <summary>
    /// Merge the next slot with this when:
    /// <item>it comes right after this</item>
    /// <item>the reference data are the same</item>
    /// </summary>
    /// <param name="nextSlot"></param>
    void Merge (ISlot nextSlot);
    
    /// <summary>
    /// Handle here the specific tasks for a modified slot
    /// 
    /// This can be for example an update of a summary analysis table
    /// </summary>
    /// <param name="oldSlot"></param>
    void HandleModifiedSlot (ISlot oldSlot);

    /// <summary>
    /// Consolidate the slot manually
    /// </summary>
    /// <param name="oldSlot"></param>
    /// <param name="association"></param>
    void Consolidate (ISlot oldSlot, IPeriodAssociation association);
  }
}
