// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Business.Reason
{
  /// <summary>
  /// DAO for ReasonColorSlot
  /// </summary>
  public class ReasonColorSlotDAO
    : IReasonColorSlotDAO
  {
    static readonly string FILTER_PROCESSING_AGE_KEY = "ReasonColorSlot.FilterProcessing.Age";
    static readonly TimeSpan FILTER_PROCESSING_AGE_DEFAULT = TimeSpan.FromHours (8);
    
    readonly TimeSpan m_filterProcessingAge;
    
    /// <summary>
    /// Constructor
    /// </summary>
    public ReasonColorSlotDAO ()
    {
      m_filterProcessingAge = Lemoine.Info.ConfigSet
        .LoadAndGet (FILTER_PROCESSING_AGE_KEY, FILTER_PROCESSING_AGE_DEFAULT);
    }

    readonly ILog log = LogManager.GetLogger(typeof (ReasonColorSlotDAO).FullName);
    
    #region IReasonColorSlotDAO implementation
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <param name="extend">Try to extend the slots</param>
    /// <returns></returns>
    public IReasonColorSlot FindAt(IMachine machine, DateTime dateTime, bool extend)
    {
      Debug.Assert (null != machine);
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ReasonColorSlotDAO.FindAt3"))
        {
          IReasonColorSlot slot = new ReasonColorSlot (
            ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindAt (machine, dateTime));
          if (extend && (null != slot)) {
            slot = ExtendLeft (slot);
            slot = ExtendRight (slot);
          }
          return slot;
        }
      }
    }
    
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="extend">Try to extend the slots</param>
    /// <returns></returns>
    public IList<IReasonColorSlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range, bool extend)
    {
      Debug.Assert (null != machine);
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.Reason.ReasonColorSlotDAO.FindOverlapsRange3"))
        {
          IList<IReasonSlot> reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRange (machine, range)
            .Where (s => FilterProcessing (s))
            .ToList ();
          return Merge (reasonSlots, extend);
        }
      }
    }

    /// <summary>
    /// False is returned if a processing period must be excluded
    /// </summary>
    /// <returns></returns>
    bool FilterProcessing (IReasonSlot reasonSlot)
    {
      if (!reasonSlot.IsProcessing ()) {
        return true;
      }
      else { // Processing
        if (0 != m_filterProcessingAge.Ticks) {
          return DateTime.UtcNow < reasonSlot.DateTimeRange.Upper.Value.Add (m_filterProcessingAge);
        }
        else { // Filter all processing
          return false;
        }
      }
    }

    /// <summary>
    /// Check if two IMergeable object can be effectively be merged
    /// </summary>
    /// <param name="left">not null</param>
    /// <param name="right">not null</param>
    /// <returns></returns>
    bool IsMergeable (IReasonColorSlot left, IReasonColorSlot right)
    {
      if (left.DateTimeRange.IsEmpty ()) {
        log.Error ($"IsMergeable: left range is empty, which is unexpected => return true because it can be dismissed");
        Debug.Assert (!left.DateTimeRange.IsEmpty ());
        return true;
      }
      if (right.DateTimeRange.IsEmpty ()) {
        log.Error ($"IsMergeable: right range is empty, which is unexpected => return true because it can be dismissed");
        Debug.Assert (!right.DateTimeRange.IsEmpty ());
        return true;
      }
      
      return left.ReferenceDataEquals (right)
        && left.DateTimeRange.IsAdjacentTo (right.DateTimeRange);
    }
    
    /// <summary>
    /// Merge two mergeable items
    /// </summary>
    /// <param name="left">not null</param>
    /// <param name="right">not null</param>
    /// <returns></returns>
    IReasonColorSlot Merge (IReasonColorSlot left, IReasonColorSlot right)
    {
      Debug.Assert (IsMergeable (left, right));
      Debug.Assert (left.DateTimeRange.IsStrictlyLeftOf (right.DateTimeRange));
      
      // Note: the Union function supports empty ranges
      UtcDateTimeRange newRange =
        new UtcDateTimeRange (left.DateTimeRange.Union (right.DateTimeRange));
      DayRange newDayRange =
        new DayRange (left.DayRange.Union (right.DayRange));
      
      IReasonColorSlot result = new ReasonColorSlot (left.Machine,
                                                     left.Processing,
                                                     left.Color,
                                                     left.OverwriteRequired,
                                                     left.Auto,
                                                     left.Running,
                                                     left.NotRunning,
                                                     newRange,
                                                     newDayRange);
      return result;
    }
    
    IList<IReasonColorSlot> Merge (IList<IReasonSlot> reasonSlots, bool extend)
    {
      IList<IReasonColorSlot> result = new List<IReasonColorSlot> ();
      foreach (var reasonSlot in reasonSlots) {
        IReasonColorSlot newSlot = new ReasonColorSlot (reasonSlot);
        if ( (1 <= result.Count)
            && IsMergeable (result [result.Count - 1], newSlot)) {
          result [result.Count - 1] =
            Merge (result [result.Count - 1], newSlot);
        }
        else {
          result.Add (newSlot);
        }
      }
      if (extend && (0 < result.Count)) {
        result [0] = ExtendLeft (result [0]);
        result [result.Count - 1] = ExtendRight (result [result.Count - 1]);
      }
      return result;
    }
    
    /// <summary>
    /// Extend a slot to the left
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    IReasonColorSlot ExtendLeft (IReasonColorSlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Lower.HasValue) {
        return slot;
      }
      else {
        IReasonSlot leftReasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindWithEnd (slot.Machine, slot.DateTimeRange.Lower.Value);
        if (null == leftReasonSlot) {
          return slot;
        }
        else { // null != leftReasonSlot
          if (leftReasonSlot.IsEmpty ()){
            Debug.Assert (false);
            log.Error ($"ExtendLeft: empty left reason slot {leftReasonSlot}");
            return slot;
          }
          IReasonColorSlot leftReasonColorSlot = new ReasonColorSlot (leftReasonSlot);
          if (IsMergeable (leftReasonColorSlot, slot)) { // Extend it
            IReasonColorSlot merged =
              Merge (leftReasonColorSlot, slot);
            return this.ExtendLeft (merged);
          }
          else {
            return slot;
          }
        }
      }
    }

    /// <summary>
    /// Extend a slot to the right
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    IReasonColorSlot ExtendRight (IReasonColorSlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Upper.HasValue) {
        return slot;
      }
      else {
        IReasonSlot rightReasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindAt (slot.Machine, slot.DateTimeRange.Upper.Value);
        if (null == rightReasonSlot) {
          return slot;
        }
        else { // null != rightReasonSlot
          if (rightReasonSlot.IsEmpty ()){
            Debug.Assert (false);
            log.Error ($"ExtendRight: empty right reason slot {rightReasonSlot}");
            return slot;
          }
          IReasonColorSlot rightReasonColorSlot = new ReasonColorSlot (rightReasonSlot);
          if (IsMergeable (slot, rightReasonColorSlot)) { // Extend it
            IReasonColorSlot merged = Merge (slot, rightReasonColorSlot);
            return this.ExtendRight (merged);
          }
          else {
            return slot;
          }
        }
      }
    }
    #endregion // IReasonColorSlotDAO implementation
  }
}
