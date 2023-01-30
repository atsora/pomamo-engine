// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Linq;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Common DAO methods for all the Slots
  /// </summary>
  public abstract class SlotDAO<TSlot, I>
    : VersionableNHibernateDAO<TSlot, I, int>, Lemoine.Threading.IChecked
    where TSlot : Slot, I, Lemoine.Threading.IChecked
    where I : ISlot
  {
    readonly ILog log = LogManager.GetLogger (typeof (SlotDAO<TSlot, I>).FullName);

    #region Members
    Lemoine.Threading.IChecked m_caller = null;
    bool m_dayColumn = true;

    IEnumerable<Pulse.Extensions.Database.ISlotExtension> m_slotExtensions = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Add a caller to this class to correctly redirect the SetActive calls
    /// </summary>
    public virtual Lemoine.Threading.IChecked Caller
    {
      get { return m_caller; }
      set { m_caller = value; }
    }

    /// <summary>
    /// There is a Day column
    /// </summary>
    protected virtual bool DayColumn
    {
      get { return m_dayColumn; }
      set { m_dayColumn = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dayColumn"></param>
    protected SlotDAO (bool dayColumn)
    {
      m_dayColumn = dayColumn;
    }
    #endregion // Constructors

    #region IChecked implementation
    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked">IChecked implementation</see>
    /// </summary>
    public virtual void SetActive ()
    {
      if (null != m_caller) {
        m_caller.SetActive ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void PauseCheck ()
    {
      if (null != m_caller) {
        m_caller.PauseCheck ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void ResumeCheck ()
    {
      if (null != m_caller) {
        m_caller.ResumeCheck ();
      }
    }
    #endregion // IChecked implementation

    /// <summary>
    /// Get the extensions and load them if needed
    /// </summary>
    /// <returns></returns>
    IEnumerable<Pulse.Extensions.Database.ISlotExtension> GetExtensions ()
    {
      LoadExtensions ();
      return m_slotExtensions;
    }

    /// <summary>
    /// Load the extensions
    /// </summary>
    void LoadExtensions ()
    {
      if (null == m_slotExtensions) { // Initialization
        var request = new Lemoine.Business.Extension
          .GlobalExtensions<Pulse.Extensions.Database.ISlotExtension> ();
        m_slotExtensions = Lemoine.Business.ServiceProvider
          .Get (request);
      }
    }

    bool IsMergeApplicableWithNext (ConsecutiveSlotList<TSlot> slots, TSlot nextSlot, IPeriodAssociationInsert association)
    {
      if (slots.Count <= 0) {
        return false;
      }
      var lastNewSlot = slots.Last;
      Debug.Assert (default (TSlot) != lastNewSlot);
      return IsMergeApplicableWithNext (lastNewSlot, nextSlot, association);
    }

    bool IsMergeApplicableWithNext (TSlot slot, ConsecutiveSlotList<TSlot> nextSlots, IPeriodAssociationInsert association)
    {
      if (nextSlots.Count <= 0) {
        return false;
      }
      var firstNextSlot = nextSlots.First;
      Debug.Assert (default (TSlot) != firstNextSlot);
      return IsMergeApplicableWithNext (slot, firstNextSlot, association);
    }

    bool IsMergeApplicableWithNext (TSlot slot, TSlot nextSlot, IPeriodAssociationInsert association)
    {
      if (default (TSlot) == slot) {
        return false;
      }
      if (Bound.Compare<DateTime> (slot.EndDateTime, nextSlot.BeginDateTime) < 0) {
        return false;
      }
      if (!slot.Consolidated && slot.IsEarlyConsolidateRequiredForDataReference ()) {
        log.Debug ("IsMergeApplicableWithNext: consolidate slot");
        using (var modificationTracker = new SlotModificationTracker<TSlot> (slot)) {
          slot.Consolidate (null, association);
        }
      }
      if (!nextSlot.Consolidated && nextSlot.IsEarlyConsolidateRequiredForDataReference ()) {
        log.Debug ("IsMergeApplicableWithNext: consolidate nextSlot");
        using (var modificationTracker = new SlotModificationTracker<TSlot> (nextSlot)) {
          nextSlot.Consolidate (null, association);
        }
      }
      return slot.ReferenceDataEquals (nextSlot);
    }

    /// <summary>
    /// Given the list of impacted existing slots
    /// and the list of coming new slots:
    /// <item>update if needed the existing slots</item>
    /// <item>insert the new slots</item>
    /// 
    /// If an existing slot already matches a new slot,
    /// the existing slot is kept
    /// </summary>
    /// <param name="association">Reference to the association that is in the source of the change</param>
    /// <param name="existingSlots">Correctly ordered list of impacted existing slots</param>
    /// <param name="newSlots">List of consecutive new slots to insert</param>
    /// <param name="range">New association range</param>
    /// <param name="preFetchedSlots"></param>
    internal void InsertNewSlots (IPeriodAssociationInsert association,
                                  IList<I> existingSlots,
                                  ConsecutiveSlotList<TSlot> newSlots,
                                  UtcDateTimeRange range,
                                  IList<I> preFetchedSlots)
    {
      LoadExtensions ();

      foreach (var extension in GetExtensions ()) {
        extension.InsertNewSlotsBegin (association, range,
                                       existingSlots.Cast<ISlot> (),
                                       newSlots.Cast<ISlot> ());
      }

      TSlot initialStateExtendedSlot = default (TSlot);
      TSlot extendedSlot = default (TSlot);
      IList<TSlot> impactedSlotsToConsolidate = new List<TSlot> ();

      // If there are some new slots to insert,
      // modify the slots that overlap
      // the new association: delete them or cut them
      foreach (I existingSlot in existingSlots) { // Note: they are correctly ordered
        if (log.IsDebugEnabled) {
          log.DebugFormat ("InsertNewSlots: " +
                           "modifiying impacted existing slot {0} with association range {1}",
                           existingSlot, range);
        }
        SetActive ();
        var impactedSlot = (TSlot)existingSlot;

        if (default (TSlot) != extendedSlot) {
          bool isMergeApplicableWithNext;
          using (var noTrackExtendedSlot = new SlotModificationTracker<TSlot> (extendedSlot, false)) { // Do not track it now, because it is done later (see below)
            isMergeApplicableWithNext = IsMergeApplicableWithNext (extendedSlot, impactedSlot, association);
          }
          if (isMergeApplicableWithNext) {
            if (extendedSlot.DateTimeRange.ContainsRange (impactedSlot.DateTimeRange)) {
              if (log.IsDebugEnabled) {
                log.DebugFormat ("InsertNewSlots: " +
                                 "delete slot {0} because the extended slot already contains the impacted slot",
                                 impactedSlot);
              }
              impactedSlot.HandleRemovedSlot ();
              foreach (var extension in GetExtensions ()) {
                extension.RemoveSlot (impactedSlot);
              }
              MakeTransient (impactedSlot);
              if (null != preFetchedSlots) {
                preFetchedSlots.Remove (impactedSlot);
              }
              continue;
            }
          }
        }

        if (Bound.Compare<DateTime> (range.Lower, impactedSlot.BeginDateTime) < 0) {
          if (Bound.Compare<DateTime> (range.Upper, impactedSlot.BeginDateTime) < 0) {
            // 1> Association:  xxxx
            //    ImpactedSlot:       xxxxx
            //    => Do nothing
          }
          else if (Bound.Compare<DateTime> (impactedSlot.EndDateTime, range.Upper) <= 0) {
            // 2> Association:  xxxxxxxxx
            //    ImpactedSlot:   xxxxx..
            //    => delete
            log.DebugFormat ("InsertNewSlots: " +
                             "delete slot {0}",
                             impactedSlot);
            impactedSlot.HandleRemovedSlot ();
            foreach (var extension in GetExtensions ()) {
              extension.RemoveSlot (impactedSlot);
            }
            MakeTransient (impactedSlot);
            if (null != preFetchedSlots) {
              preFetchedSlots.Remove (impactedSlot);
            }
          }
          else {
            // 3> Association    xxxxxx
            //    ImpactedSlot:     ...xxxx
            //    => update / new or merge
            Debug.Assert (range.Upper.HasValue);
            using (var noTrackExtendedSlot = new SlotModificationTracker<TSlot> (extendedSlot, false)) {
              // This is done later
              if (IsMergeApplicableWithNext (newSlots, impactedSlot, association)) { // merge
                newSlots.Last.MergeWithNextSlot<TSlot> (impactedSlot);
                impactedSlot.HandleRemovedSlot ();
                foreach (var extension in GetExtensions ()) {
                  extension.RemoveSlot (impactedSlot);
                }
                MakeTransient (impactedSlot);
                if (null != preFetchedSlots) {
                  preFetchedSlots.Remove (impactedSlot);
                }
              }
              else if ((default (TSlot) != extendedSlot)
                && IsMergeApplicableWithNext (extendedSlot, impactedSlot, association)) { // 2nd merge
                // - Remove impactedSlot: to do first because of the accumulators
                impactedSlot.HandleRemovedSlot ();
                foreach (var extension in GetExtensions ()) {
                  extension.RemoveSlot (impactedSlot);
                }
                MakeTransient (impactedSlot);
                if (null != preFetchedSlots) {
                  preFetchedSlots.Remove (impactedSlot);
                }
                // - Extend extendedSlot: to do next
                extendedSlot.MergeWithNextSlot<TSlot> (impactedSlot);
                // Note: ModifySlot process is postponed as much as possible because of the accumulators
                //       so that the slots are removed before adding new ones
              }
              else if (Bound.Compare<DateTime> (impactedSlot.BeginDateTime, range.Upper) < 0) { // update / new
                Debug.Assert (range.Overlaps (impactedSlot.DateTimeRange));
                using (var modificationTracker = new SlotModificationTracker<TSlot> (impactedSlot)) {
                  var oldSlot = modificationTracker.OldSlot;
                  oldSlot.Caller = this;
                  impactedSlot.UpdateDateTimeRange (new UtcDateTimeRange (range.Upper.Value, impactedSlot.EndDateTime));
                  ModifySlot (oldSlot, impactedSlot, association);
                }
              }
            }
          }
        }
        else { // impactedSlot.BeginDateTime <= range.Lower
          if (Bound.Compare<DateTime> (impactedSlot.EndDateTime, range.Lower) < 0) {
            // 4> Association:          xxxx
            //    ImpactedSlot: xxxx
            //    => do nothing
          }
          else if (Bound.Compare<DateTime> (impactedSlot.EndDateTime, range.Upper) < 0) {
            // 5> Association:  ....xxxxxx
            //    ImpactedSlot: xxxx...
            //    => update / new or merge
            Debug.Assert (impactedSlot.EndDateTime.HasValue);
            Debug.Assert ((newSlots.Count == 0)
                          || (Bound.Compare<DateTime> (range.Lower, newSlots.First.BeginDateTime) <= 0));
            if (IsMergeApplicableWithNext (impactedSlot, newSlots, association)) { // merge
              if (!Bound.Equals<DateTime> (impactedSlot.EndDateTime, newSlots.First.EndDateTime)) {
                if (default (TSlot) != extendedSlot) {
                  Debug.Assert (default (TSlot) != initialStateExtendedSlot);
                  using (var modificationTracker = new SlotModificationTracker<TSlot> (initialStateExtendedSlot, extendedSlot)) {
                    // Note: ModifySlot process is postponed as much as possible because of the accumulators
                    //       so that the slots are removed before adding new ones
                    ModifySlot (initialStateExtendedSlot, extendedSlot, association);
                  }
                }
                initialStateExtendedSlot = (TSlot)impactedSlot.Clone ();
                initialStateExtendedSlot.Caller = this;
                extendedSlot = impactedSlot;
                using (var noTrackExtendedSlot = new SlotModificationTracker<TSlot> (extendedSlot, false)) { // Do not track it now, because it is done later (see below)
                  extendedSlot.MergeWithNextSlot<TSlot> (newSlots.First);
                }
              }
              else if (newSlots.Any (s => !s.Consolidated)) {
                using (var modificationTracker = new SlotModificationTracker<TSlot> (impactedSlot)) {
                  foreach (var newSlot in newSlots.Where (s => !s.Consolidated)) {
                    impactedSlot.MergeSamePeriodAdditionalProperties (newSlot);
                  }
                }
                impactedSlotsToConsolidate.Add (impactedSlot);
              }
              newSlots.RemoveFirst ();
            }
            else if (range.Overlaps (impactedSlot.DateTimeRange)) { // update / new
              if (Bound.Equals<DateTime> (impactedSlot.BeginDateTime, range.Lower)) {
                impactedSlot.HandleRemovedSlot ();
                foreach (var extension in GetExtensions ()) {
                  extension.RemoveSlot (impactedSlot);
                }
                MakeTransient (impactedSlot);
                if (null != preFetchedSlots) {
                  preFetchedSlots.Remove (impactedSlot);
                }
              }
              else { // impactedSlot.BeginDateTime < range.Lower => make it shorter
                Debug.Assert (Bound.Compare<DateTime> (impactedSlot.BeginDateTime, range.Lower) < 0);
                Debug.Assert (range.Lower.HasValue);
                if (!Bound.Equals<DateTime> (impactedSlot.EndDateTime,
                                             range.Lower)) {
                  using (var modificationTracker = new SlotModificationTracker<TSlot> (impactedSlot)) {
                    var oldSlot = modificationTracker.OldSlot;
                    oldSlot.Caller = this;
                    impactedSlot.UpdateDateTimeRange (new UtcDateTimeRange (impactedSlot.BeginDateTime, range.Lower.Value));
                    Debug.Assert (!Bound.Equals<DateTime> (impactedSlot.BeginDateTime,
                                                           impactedSlot.EndDateTime));
                    ModifySlot (oldSlot, impactedSlot, association);
                  }
                }
              }
            }
          }
          else {
            // 6> Association:  ....xxxxxx....
            //    ImpactedSlot: xxxxxxxxxxxxxx
            //    => update / cut / new or merge
            if (IsMergeApplicableWithNext (impactedSlot, newSlots, association)) {
              Debug.Assert (1 == newSlots.Count);
              // Really nothing to do, no item to add in the database
              // => clear newSlots
              if (newSlots.Any (s => !s.Consolidated)) {
                using (var modificationTracker = new SlotModificationTracker<TSlot> (impactedSlot)) {
                  foreach (var newSlot in newSlots.Where (s => !s.Consolidated)) {
                    impactedSlot.MergeSamePeriodAdditionalProperties (newSlot);
                  }
                }
                impactedSlotsToConsolidate.Add (impactedSlot);
              }
              newSlots.Clear ();
            }
            else {
              var oldSlot = (TSlot)impactedSlot.Clone ();
              // Left slot
              if (Bound.Equals<DateTime> (range.Lower,
                                          impactedSlot.DateTimeRange.Lower)) { // Remove the left slot
                impactedSlot.HandleRemovedSlot ();
                MakeTransient (impactedSlot);
                if (null != preFetchedSlots) {
                  preFetchedSlots.Remove (impactedSlot);
                }
                // Right slot
                if (!Bound.Equals<DateTime> (range.Upper,
                                             oldSlot.DateTimeRange.Upper)) {
                  Debug.Assert (range.Upper.HasValue);
                  var rightSlot =
                    (TSlot)oldSlot.Clone (new UtcDateTimeRange (range.Upper.Value, oldSlot.EndDateTime));
                  Debug.Assert (!Bound.Equals<DateTime> (rightSlot.BeginDateTime,
                                                         rightSlot.EndDateTime));
                  rightSlot.Caller = this;
                  this.Insert (rightSlot, oldSlot, association);
                  if (null != preFetchedSlots) {
                    InsertInSortedSlotList (preFetchedSlots, rightSlot);
                  }
                  /* Or, alternative:
                newSlots.Insert (rightSlot);
                   */
                }
                else {
                  foreach (var extension in GetExtensions ()) {
                    extension.RemoveSlot (impactedSlot);
                  }
                }
              }
              else { // Make the left slot shorter
                Debug.Assert (Bound.Compare<DateTime> (impactedSlot.DateTimeRange.Lower, range.Lower) < 0);
                Debug.Assert (range.Lower.HasValue);
                Debug.Assert (!Bound.Equals<DateTime> (impactedSlot.EndDateTime,
                                                       range.Lower));
                using (var modificationTracker = new SlotModificationTracker<TSlot> (impactedSlot)) {
                  impactedSlot.Caller = this;
                  var impactedSlotNewRange = new UtcDateTimeRange (impactedSlot.BeginDateTime, range.Lower.Value);
                  if ((default (TSlot) != extendedSlot)
                    && impactedSlotNewRange.ContainsElement (extendedSlot.DateTimeRange.Upper)) {
                    impactedSlotNewRange = new UtcDateTimeRange (extendedSlot.DateTimeRange.Upper.Value,
                      range.Lower.Value);
                    if (log.IsDebugEnabled) {
                      log.DebugFormat ("InsertNewSlots: impactedSlotNewRange is {0} because of extendedSlot", impactedSlotNewRange);
                    }
                  }
                  impactedSlot.UpdateDateTimeRange (impactedSlotNewRange);
                  ModifySlot (oldSlot, impactedSlot, association);
                }
                // Right slot
                if (!Bound.Equals<DateTime> (range.Upper,
                                             oldSlot.DateTimeRange.Upper)) {
                  Debug.Assert (range.Upper.HasValue);
                  var rightSlot =
                    (TSlot)oldSlot.Clone (new UtcDateTimeRange (range.Upper.Value, oldSlot.EndDateTime));
                  Debug.Assert (!Bound.Equals<DateTime> (rightSlot.BeginDateTime,
                                                         rightSlot.EndDateTime));
                  rightSlot.Caller = this;
                  this.Insert (rightSlot, null, association);
                  if (null != preFetchedSlots) {
                    InsertInSortedSlotList (preFetchedSlots, rightSlot);
                  }
                  /* Or, alternative:
                newSlots.Insert (rightSlot);
                   */
                }
              }
            }
          }
        }
      }
      if (default (TSlot) != extendedSlot) {
        Debug.Assert (default (TSlot) != initialStateExtendedSlot);
        // Note: ModifySlot process is postponed as much as possible because of the accumulators
        //       so that the slots are removed before adding new ones
        using (var modificationTracker = new SlotModificationTracker<TSlot> (initialStateExtendedSlot, extendedSlot)) {
          ModifySlot (initialStateExtendedSlot, extendedSlot, association);
        }
      }

      foreach (var impactedSlotToConsolidate in impactedSlotsToConsolidate
        .Distinct ()
        .Where (s => !s.Consolidated)) {
        var oldSlot = (ISlot)impactedSlotToConsolidate.Clone ();
        using (var modificationTracker = new SlotModificationTracker<TSlot> (impactedSlotToConsolidate)) {
          impactedSlotToConsolidate.Consolidate (oldSlot, association);
        }
      }

      // Insert the new modified slots in two steps:
      // first run a basic MakePersistent, then insert them
      if (log.IsDebugEnabled) {
        log.Debug ($"InsertNewSlots: there are {newSlots.Count} new slots to insert");
      }
      foreach (TSlot slotToInsert in newSlots) {
        if (log.IsDebugEnabled) {
          log.Debug ($"InsertNewSlots: insert the new modified slot {slotToInsert}");
        }
        Debug.Assert (!slotToInsert.IsEmpty ());
        SetActive ();
        slotToInsert.Caller = this;
        this.Insert (slotToInsert, null, association);
        if (null != preFetchedSlots) {
          InsertInSortedSlotList (preFetchedSlots, slotToInsert);
        }
      }

      foreach (var extension in GetExtensions ()) {
        extension.InsertNewSlotsEnd ();
      }
    }

    void ModifySlot (TSlot old, TSlot modified, IPeriodAssociation association)
    {
      modified.Consolidate (old, association);
      MakePersistent (modified);
      foreach (var extension in GetExtensions ()) {
        extension.ModifySlot (old, modified);
      }
    }

    void InsertInSortedSlotList (IList<I> list, I newElement)
    {
      for (int i = 0; i < list.Count; i++) {
        I element = list[i];
        if (Bound.Compare<DateTime> (newElement.BeginDateTime, element.BeginDateTime) < 0) {
          // Insert it before element
          list.Insert (i, newElement);
          return;
        }
      }
    }

    /// <summary>
    /// Insert a slot in database after:
    /// checking it is not empty (else do not insert it) and consolidating it
    /// </summary>
    /// <param name="slot">not null</param>
    /// <param name="oldSlot"></param>
    /// <param name="association"></param>
    void Insert (TSlot slot, TSlot oldSlot, IPeriodAssociation association)
    {
      Debug.Assert (null != slot);
      if (0 != slot.Id) {
        log.Fatal ($"Insert: slot has already a positive id={slot.Id} which is unexpected");
      }

      if (slot.DateTimeRange.IsEmpty ()) {
        log.Fatal ($"Insert: empty date/time range in slot => do nothing {System.Environment.StackTrace}");
        return;
      }

      if (slot.IsEmpty ()) {
        log.Debug ("Insert: empty slot, skip it");
        return;
      }

      LoadExtensions ();

      slot.Consolidate (oldSlot, association);
      if (log.IsDebugEnabled) {
        log.Debug ($"Insert: about to save {this}");
      }
      MakePersistent (slot);
      if (0 != slot.ModificationTrackerLevel) {
        log.Fatal ($"Insert: modification tracker level is {slot.ModificationTrackerLevel} != 0, which is unexpected {System.Environment.StackTrace}");
      }
      slot.HandleAddedSlot ();
      foreach (var extension in GetExtensions ()) {
        if (null == oldSlot) {
          extension.AddSlot (slot);
        }
        else {
          extension.ModifySlot (oldSlot, slot);
        }
      }
    }

    /// <summary>
    /// Get the list of impacted slots for the analysis
    /// </summary>
    /// <param name="range"></param>
    /// <param name="dateTime"></param>
    /// <param name="pastOnly"></param>
    /// <param name="leftMerge">Get the data to try to merge on the left too</param>
    /// <param name="rightMerge">Get the data to try to merge on the right too</param>
    /// <returns></returns>
    public abstract IList<I> GetImpactedSlotsForAnalysis (UtcDateTimeRange range,
                                                          DateTime dateTime,
                                                          bool pastOnly,
                                                          bool leftMerge,
                                                          bool rightMerge);

    /// <summary>
    /// Get the list of impacted machine slots for the analysis
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="dateTime"></param>
    /// <param name="pastOnly"></param>
    /// <param name="leftMerge">Get the data to try to merge on the left too</param>
    /// <param name="rightMerge">Get the data to try to merge on the right too</param>
    /// <returns></returns>
    public abstract IList<I> GetImpactedMachineSlotsForAnalysis (IMachine machine,
                                                                 UtcDateTimeRange range,
                                                                 DateTime dateTime,
                                                                 bool pastOnly,
                                                                 bool leftMerge,
                                                                 bool rightMerge);

    /// <summary>
    /// Get the list of impacted machine module slots for the analysis
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <param name="dateTime"></param>
    /// <param name="pastOnly"></param>
    /// <param name="leftMerge">Get the data to try to merge on the left too</param>
    /// <param name="rightMerge">Get the data to try to merge on the right too</param>
    /// <returns></returns>
    public abstract IList<I> GetImpactedMachineModuleSlotsForAnalysis (IMachineModule machineModule,
                                                                       UtcDateTimeRange range,
                                                                       DateTime dateTime,
                                                                       bool pastOnly,
                                                                       bool leftMerge,
                                                                       bool rightMerge);

    /// <summary>
    /// Get the list of impacted line slots for the analysis
    /// </summary>
    /// <param name="line"></param>
    /// <param name="range"></param>
    /// <param name="dateTime"></param>
    /// <param name="pastOnly"></param>
    /// <param name="leftMerge">Get the data to try to merge on the left too</param>
    /// <param name="rightMerge">Get the data to try to merge on the right too</param>
    /// <returns></returns>
    public abstract IList<I> GetImpactedLineSlotsForAnalysis (ILine line,
                                                              UtcDateTimeRange range,
                                                              DateTime dateTime,
                                                              bool pastOnly,
                                                              bool leftMerge,
                                                              bool rightMerge);

    /// <summary>
    /// Get the list of impacted user slots for the analysis
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    /// <param name="dateTime"></param>
    /// <param name="pastOnly"></param>
    /// <param name="leftMerge">Get the data to try to merge on the left too</param>
    /// <param name="rightMerge">Get the data to try to merge on the right too</param>
    /// <returns></returns>
    public abstract IList<I> GetImpactedUserSlotsForAnalysis (IUser user,
                                                              UtcDateTimeRange range,
                                                              DateTime dateTime,
                                                              bool pastOnly,
                                                              bool leftMerge,
                                                              bool rightMerge);

  }
}
