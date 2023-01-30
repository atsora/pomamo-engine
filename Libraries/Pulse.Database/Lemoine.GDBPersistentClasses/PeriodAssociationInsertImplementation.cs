// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of PeriodAssociationImplementation.
  /// </summary>
  internal class PeriodAssociationInsertImplementation
  {
    #region Members
    readonly IPeriodAssociationInsert m_periodAssociation;
    #endregion // Members
    
    #region Getters / Setters
    LowerBound<DateTime> Begin {
      get { return m_periodAssociation.Begin; }
    }

    UpperBound<DateTime> End {
      get { return m_periodAssociation.End; }
    }
    
    UtcDateTimeRange Range {
      get { return m_periodAssociation.Range; }
    }
    
    DateTime DateTime {
      get { return m_periodAssociation.DateTime; }
    }
    
    AssociationOption? Option {
      get { return m_periodAssociation.Option; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="periodAssociation"></param>
    public PeriodAssociationInsertImplementation (IPeriodAssociationInsert periodAssociation)
    {
      m_periodAssociation = periodAssociation;
    }
    #endregion // Constructors
    
    #region Methods
    void SetActive ()
    {
      m_periodAssociation.SetActive ();
    }

    ILog GetLogger ()
    {
      return m_periodAssociation.GetLogger ();
    }
    
    TSlot ConvertToSlot<TSlot> ()
      where TSlot: Slot
    {
      return m_periodAssociation.ConvertToSlot<TSlot> ();
    }
    
    /// <summary>
    /// Insert a period association in database
    /// considering all the existing slots,
    /// cutting them or joining them if necessary
    /// </summary>
    /// <param name="slotDAO">DAO to use to update the slot</param>
    /// <param name="pastOnly">Apply the association on the existing past slots only</param>
    /// <param name="preFetchedImpactedSlots">Pre-fetched list of impacted slots (to be filter). The list is sorted</param>
    public void Insert<TSlot, I, TSlotDAO> (TSlotDAO slotDAO,
                                            bool pastOnly,
                                            IList<I> preFetchedImpactedSlots)
      where TSlot: Slot, I
      where I: ISlot
      where TSlotDAO: SlotDAO<TSlot, I>
    {
      if (Bound.Equals<DateTime> (Begin,
                                  End)) {
        GetLogger ().Info ("Insert: " +
                           "empty association, do nothing");
        return;
      }
      Debug.Assert (Bound.Compare<DateTime> (Begin, End) <= 0);

      // A. Get the impacted existing slots
      IList<I> impactedSlots =
        GetImpactedSlots<TSlot, I, TSlotDAO> (slotDAO, pastOnly, preFetchedImpactedSlots);
      GetLogger ().DebugFormat ("Insert: " +
                                "got impacted slots",
                                impactedSlots.Count);
      SetActive ();
      
      // B. Merge the data of the existing slots with newMachineSlot
      //    This results in getting several new machine slots
      TSlot newSlot = ConvertToSlot<TSlot> (); // may be null !
      GetLogger ().DebugFormat ("Insert: " +
                                "new slot is {0}",
                                newSlot);
      Debug.Assert ( (null == newSlot)
                    || (Bound.Equals<DateTime> (newSlot.BeginDateTime, Begin)
                        && Bound.Equals<DateTime> (newSlot.EndDateTime, End)));
      // + Same Machine / MachineModule / Line / User
      ConsecutiveSlotList<TSlot> newSlots = new ConsecutiveSlotList<TSlot> ();
      if ((0 == impactedSlots.Count) && (null != newSlot) && !newSlot.IsEmpty ()) {
        GetLogger ().Debug ("Insert: " +
                            "no impacted slot, " +
                            "push the not null new slot");
        newSlots.Push (newSlot);
      }
      else { // There are some impacted slots => merge
        // Check if a change is really required
        if (1 == impactedSlots.Count) {
          var impactedSlot = impactedSlots [0];
          if (impactedSlot.DateTimeRange.Equals (this.Range)
              && impactedSlot.ReferenceDataEquals (newSlot)) {
            GetLogger ().DebugFormat ("Insert: " +
                                      "no change is required on slot {0}, " +
                                      "new slot {1} " +
                                      "=> return",
                                      impactedSlot, newSlot);
            return;
          }
        }
        
        Bound<DateTime> endLatestProcessedPeriod = Begin;
        foreach (TSlot impactedSlot in impactedSlots) {
          CheckStepTimeout ();
          if (!impactedSlot.DateTimeRange.Overlaps (this.Range)) {
            continue;
          }
          // From now, impactedSlot intersects association
          if ( (null != newSlot)
              && (Bound.Compare<DateTime> (endLatestProcessedPeriod, impactedSlot.BeginDateTime) < 0)
              && !newSlot.IsEmpty ()) {
            Debug.Assert (impactedSlot.BeginDateTime.HasValue);
            Debug.Assert (!Bound.Equals<DateTime> (new UpperBound<DateTime> (null), endLatestProcessedPeriod));// Not +oo
            Debug.Assert (null != newSlot);
            // There is a period where newMachineSlot does not intersect
            // with impactedSlots
            TSlot newPartialSlot =
              (TSlot) newSlot.Clone (new UtcDateTimeRange ((LowerBound<DateTime>)endLatestProcessedPeriod,
                                                           impactedSlot.BeginDateTime.Value));
            newSlots.Push (newPartialSlot);
            GetLogger ().DebugFormat ("Insert: " +
                                      "push the partial slot {0}",
                                      newPartialSlot);
          }
          UtcDateTimeRange mergedRange = new UtcDateTimeRange (this.Range.Intersects (impactedSlot.DateTimeRange));
          if (!mergedRange.IsEmpty ()) {
            TSlot mergedSlot = m_periodAssociation.MergeDataWithOldSlot<TSlot> (impactedSlot,
                                                                                mergedRange);
            if (null != mergedSlot) {
              mergedSlot.UpdateDateTimeRange (mergedRange);
              if (!mergedSlot.IsEmpty ()) {
                newSlots.Push (mergedSlot);
                GetLogger ().DebugFormat ("Insert: " +
                                          "push the merged slot {0}",
                                          mergedSlot);
              }
            }
          }
          endLatestProcessedPeriod = impactedSlot.EndDateTime;
        }
        if ( (null != newSlot)
            && (Bound.Compare<DateTime> (endLatestProcessedPeriod,
                                         End) < 0)
            && !newSlot.IsEmpty ()) {
          Debug.Assert (!Bound.Equals<DateTime> (new UpperBound<DateTime> (), endLatestProcessedPeriod));
          Debug.Assert (null != newSlot);
          // There is a period to insert after all the impacted slots
          newSlot.UpdateDateTimeRange (new UtcDateTimeRange ((LowerBound<DateTime>)endLatestProcessedPeriod,
                                                             newSlot.EndDateTime));
          newSlots.Push (newSlot);
          GetLogger ().DebugFormat ("Insert: " +
                                    "push the a new  slot {0} after all the impacted slots",
                                    newSlot);
        }
      }
      
      // C. If there are some new slots to insert,
      //    modify the slots that overlap
      //    the new association: delete them or cut them
      //    Then insert the new modified slots
      GetLogger ().DebugFormat ("Insert: " +
                                "insert {0} new slots between {1} and {2} " +
                                "with {3} impacted slots",
                                newSlots.Count, Begin, End,
                                impactedSlots.Count);
      slotDAO.InsertNewSlots (m_periodAssociation,
                              impactedSlots,
                              newSlots,
                              this.Range,
                              preFetchedImpactedSlots);
    }
    
    /// <summary>
    /// Get the impacted slots
    /// </summary>
    /// <param name="slotDAO">DAO to use to update the slot</param>
    /// <param name="pastOnly">Apply the association on the existing past slots only</param>
    /// <param name="preFetchedImpactedSlots">Pre-fetched list of impacted slots (to be filter). The list is sorted</param>
    public IList<I> GetImpactedSlots<TSlot, I, TSlotDAO> (TSlotDAO slotDAO,
                                                          bool pastOnly,
                                                          IList<I> preFetchedImpactedSlots)
      where TSlot: Slot, I
      where I: ISlot
      where TSlotDAO: SlotDAO<TSlot, I>
    {
      if (null == preFetchedImpactedSlots) {
        return GetImpactedSlots<TSlot, I, TSlotDAO> (slotDAO, pastOnly);
      }
      else { // Filter preFetchedImpactedSlots
        IList<I> impactedSlots = new List<I> ();
        foreach (I slot in preFetchedImpactedSlots) {
          if (Bound.Compare<DateTime> (slot.EndDateTime, this.Begin) < 0) {
            continue;
          }
          if (this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.NoLeftMerge)
              && (Bound.Compare<DateTime> (slot.EndDateTime, this.Begin) <= 0)) {
            continue;
          }
          if (Bound.Compare<DateTime> (this.End, slot.BeginDateTime) < 0) {
            // All the next slots are after this.End because the list is sorted => break
            break;
          }
          if (this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.NoRightMerge)
              && (Bound.Compare<DateTime> (this.End, slot.BeginDateTime) <= 0)) {
            break;
          }
          if (pastOnly && (Bound.Compare<DateTime> (this.DateTime, this.Begin) <= 0)) {
            // All the next slots are in the future => break
            break;
          }
          GetLogger ().DebugFormat ("GetImpactedSlots: " +
                                    "Add slot with range {0}",
                                    slot.DateTimeRange);
          impactedSlots.Add (slot);
        }
        return impactedSlots;
      }
    }
    
    /// <summary>
    /// Get the impacted slots without considering any pre-fetched slot
    /// </summary>
    /// <param name="slotDAO">DAO to use to update the slot</param>
    /// <param name="pastOnly">Apply the association on the existing past slots only</param>
    IList<I> GetImpactedSlots<TSlot, I, TSlotDAO> (TSlotDAO slotDAO,
                                                   bool pastOnly)
      where TSlot: Slot, I
      where I: ISlot
      where TSlotDAO: SlotDAO<TSlot, I>
    {
      return m_periodAssociation.GetImpactedSlots<TSlot, I, TSlotDAO> (slotDAO, pastOnly);
    }
    
    /// <summary>
    /// Check if the step timeout is reached.
    /// 
    /// If it is, raise the StepTimeoutException
    /// </summary>
    /// <returns></returns>
    void CheckStepTimeout ()
    {
      m_periodAssociation.CheckStepTimeout ();
    }
    #endregion // Methods
  }
}
