// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Interface to follow to use PeriodAssociationInsertImplementation
  /// </summary>
  public interface IPeriodAssociationInsert: IPeriodAssociation, Lemoine.Threading.IChecked
  {
    /// <summary>
    /// Return the logger
    /// </summary>
    /// <returns></returns>
    ILog GetLogger ();
    
    /// <summary>
    /// Convert the Association to the given Slot
    /// 
    /// If no slot can be created from the association,
    /// null is returned
    /// </summary>
    /// <returns></returns>
    TSlot ConvertToSlot<TSlot> ()
      where TSlot: Slot;
    
    /// <summary>
    /// Merge the data of the current association
    /// with the data of an old slot.
    /// 
    /// The slot and the association must reference the same reference data.
    /// 
    /// oldSlot can't be null.
    /// 
    /// The returned slot has no specific period set and is never null
    /// 
    /// The merge period is given in parameter and can't be used for advanced process
    /// like updating some summary analysis tables
    /// </summary>
    /// <param name="oldSlot">It can't be null and must reference the same machine</param>
    /// <param name="range">merge period range</param>
    /// <returns></returns>
    TSlot MergeDataWithOldSlot<TSlot>(TSlot oldSlot,
                                      UtcDateTimeRange range)
      where TSlot: Slot;
    
    /// <summary>
    /// Get the impacted slots without considering any pre-fetched slot
    /// </summary>
    /// <param name="slotDAO">DAO to use to update the slot</param>
    /// <param name="pastOnly">Apply the association on the existing past slots only</param>
    IList<I> GetImpactedSlots<TSlot, I, TSlotDAO> (TSlotDAO slotDAO,
                                                   bool pastOnly)
      where TSlot: Slot, I
      where I: ISlot
      where TSlotDAO: SlotDAO<TSlot, I>;
    
    /// <summary>
    /// Modification date/time
    /// </summary>
    DateTime DateTime {
      get;
    }
    
    /// <summary>
    /// Modification option
    /// </summary>
    AssociationOption? Option {
      get;
    }
    
    /// <summary>
    /// Check if the step timeout is reached.
    /// 
    /// If it is, raise the StepTimeoutException
    /// </summary>
    /// <returns></returns>
    void CheckStepTimeout ();
  }
}
