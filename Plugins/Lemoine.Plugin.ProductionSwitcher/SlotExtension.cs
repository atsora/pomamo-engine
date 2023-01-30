// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.ProductionSwitcher
{
  /// <summary>
  /// Track the changes in observation state slots
  /// </summary>
  public class SlotExtension
    : Pulse.Extensions.Database.ISlotExtension
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (SlotExtension).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public SlotExtension ()
    {
    }
    #endregion // Constructors

    #region ISlotExtension implementation
    /// <summary>
    /// <see cref="Lemoine.Extensions.IExtension"/>
    /// </summary>
    public bool UniqueInstance {
      get { return true; }
    }

    public void InsertNewSlotsBegin(IPeriodAssociation association, UtcDateTimeRange range, System.Collections.Generic.IEnumerable<ISlot> existingSlots, System.Collections.Generic.IEnumerable<ISlot> newSlots)
    { }
    
    public void InsertNewSlotsEnd()
    { }
    
    public void AddSlot(Lemoine.Model.ISlot slot)
    {
      if (slot is IObservationStateSlot) {
        ObservationStateSlotChangeNotifier.NotifyChanges (slot as IObservationStateSlot);
      }
    }

    public void RemoveSlot(Lemoine.Model.ISlot slot)
    {
    }

    public void ModifySlot(Lemoine.Model.ISlot oldSlot, Lemoine.Model.ISlot newSlot)
    {
      // ModifySlot usually only changes the applicable range (and some consolidated data)
      // => there is nothing to do here
    }

    #endregion
  }
}
