// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// Description of ISlotExtension.
  /// </summary>
  public interface ISlotExtension: Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Begin the insertion of new slots
    /// </summary>
    /// <param name="association"></param>
    /// <param name="range"></param>
    /// <param name="existingSlots"></param>
    /// <param name="newSlots"></param>
    void InsertNewSlotsBegin (IPeriodAssociation association, UtcDateTimeRange range,
                              IEnumerable<ISlot> existingSlots,
                              IEnumerable<ISlot> newSlots);
    
    /// <summary>
    /// Add a new slot with some new data
    /// </summary>
    /// <param name="slot"></param>
    void AddSlot (ISlot slot);
    
    /// <summary>
    /// Remove an existing slot
    /// </summary>
    /// <param name="slot"></param>
    void RemoveSlot (ISlot slot);
    
    /// <summary>
    /// Modify an existing slot. Usually only the applicable date/time is changed
    /// (and sometimes some associated consolidated data)
    /// 
    /// It is run only once per oldSlot
    /// </summary>
    /// <param name="oldSlot"></param>
    /// <param name="newSlot"></param>
    void ModifySlot (ISlot oldSlot, ISlot newSlot);
    
    /// <summary>
    /// End the insertion of new slots
    /// </summary>
    void InsertNewSlotsEnd ();
  }
}
