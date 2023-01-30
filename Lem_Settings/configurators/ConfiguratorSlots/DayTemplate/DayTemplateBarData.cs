// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.DayTemplate
{
  /// <summary>
  /// Description of DayTemplateBarData.
  /// </summary>
  public class DayTemplateBarData: IBarObjectFactory
  {
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DayTemplateBarData() {}
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Create all BarObjects included in a period
    /// </summary>
    /// <param name="start">beginning of the period</param>
    /// <param name="end">end of the period</param>
    /// <returns></returns>
    public IList<BarObject> CreateBarObjects(DateTime start, DateTime end)
    {
      IList<BarObject> barObjects = new List<BarObject>();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        IList<IDayTemplateSlot> slots = ModelDAOHelper.DAOFactory.DayTemplateSlotDAO
          .FindOverlapsRange(new UtcDateTimeRange(start, end));
        
        foreach (IDayTemplateSlot slot in slots) {
          if (slot.DayTemplate != null) {
            DateTime? startDateTime = null;
            if (slot.BeginDateTime.HasValue) {
              startDateTime = slot.BeginDateTime.Value.ToLocalTime();
            }

            DateTime? endDateTime = null;
            if (slot.EndDateTime.HasValue) {
              endDateTime = slot.EndDateTime.Value.ToLocalTime();
            }

            barObjects.Add(new BarSegment(startDateTime, endDateTime,
                                          slot.DayTemplate, slot.DayTemplate.Display));
          }
        }
      }
      
      return barObjects;
    }
    #endregion // Methods
  }
}
