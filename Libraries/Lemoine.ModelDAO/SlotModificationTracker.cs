// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Disposable class to track the modifications in slots with 'using'
  /// 
  /// T is the type of the slot
  /// </summary>
  public class SlotModificationTracker<T> : IDisposable
    where T : class, ISlot
  {
    #region Members
    readonly int m_initialLevel = 0;
    readonly bool m_active = false;
    readonly T m_oldSlot;
    readonly T m_slot;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (SlotModificationTracker<T>).FullName);

    #region Getters / Setters
    /// <summary>
    /// Old slot
    /// </summary>
    public T OldSlot
    {
      get { return m_oldSlot; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor to track all the modifications done in the slot in parameter
    /// </summary>
    /// <param name="slot">If null, do nothing</param>
    public SlotModificationTracker (T slot)
      : this (slot, true)
    {
    }

    /// <summary>
    /// Constructor to track all the modifications done in the slot in parameter
    /// </summary>
    /// <param name="slot">If null, do nothing</param>
    /// <param name="track">Option not to track the modifications</param>
    public SlotModificationTracker (T slot, bool track)
    {
      if (default (T) == slot) { // null => do nothing
        m_active = false;
        return;
      }

      m_initialLevel = slot.ModificationTrackerLevel;
      ++slot.ModificationTrackerLevel;
      m_oldSlot = (T)slot.Clone ();
      m_slot = slot;

      if (!track && 0 != m_initialLevel) {
        log.Fatal ($"SlotModificationTracker: do not track is on but initial level is not 0");
      }

      m_active = track && (0 == m_initialLevel) && (0 != slot.Id);
    }

    /// <summary>
    /// Alternative constructor to track but with some delays all the modifications done in the slot in parameter
    /// </summary>
    /// <param name="oldSlot">not null</param>
    /// <param name="slot">not null</param>
    public SlotModificationTracker (T oldSlot, T slot)
    {
      Debug.Assert (default(T) != oldSlot);
      Debug.Assert (default(T) != slot);

      m_initialLevel = slot.ModificationTrackerLevel;
      ++slot.ModificationTrackerLevel;
      m_oldSlot = oldSlot;
      m_slot = slot;

      m_active = (0 == m_initialLevel) && (0 != slot.Id);
    }
    #endregion // Constructors

    /// <summary>
    /// IDisposable implementation
    /// </summary>
    public void Dispose ()
    {
      if (default (T) == m_slot) { // null => do nothing
        return;
      }

      try {
        if (default (T) == m_oldSlot) {
          log.Fatal ($"Dispose: old slot null, which is unexpected");
          Debug.Assert (default (T) != m_oldSlot, $"Old slot is null");
          return;
        }

        if (m_active && (0 == m_initialLevel)) {
          Debug.Assert (1 == m_slot.ModificationTrackerLevel, $"Wrong modification tracker level {m_slot.ModificationTrackerLevel}, not 1");
          m_slot.HandleModifiedSlot (m_oldSlot);
        }
      }
      catch (Exception ex) {
        log.Fatal ($"Dispose: exception", ex);
        throw;
      }
      finally {
        --m_slot.ModificationTrackerLevel;
      }

      if (m_initialLevel != m_slot.ModificationTrackerLevel) {
        log.Fatal ($"Dispose: initial level {m_initialLevel} is different than the modification tracker level {m_slot.ModificationTrackerLevel}");
      }
      Debug.Assert (m_initialLevel == m_slot.ModificationTrackerLevel, $"Unattended modification level");
    }
  }
}
