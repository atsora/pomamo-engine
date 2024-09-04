// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.MachineState
{
  /// <summary>
  /// Description of BarDataMachineState.
  /// </summary>
  public class MachineStateBarData: IBarObjectFactory
  {
    #region Members
    readonly IMachine m_machine;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="machine"></param>
    public MachineStateBarData(IMachine machine)
    {
      m_machine = machine;
    }
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
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction())
        {
          ModelDAOHelper.DAOFactory.MachineDAO.Lock(m_machine);
          IList<IMachineStateTemplateSlot> slots = ModelDAOHelper.DAOFactory.MachineStateTemplateSlotDAO
            .FindOverlapsRange(m_machine, new UtcDateTimeRange (start, end));
          
          foreach (var slot in slots) {
            if (slot.MachineStateTemplate != null) {
              DateTime? startDateTime = null;
              if (slot.DateTimeRange.Lower.HasValue) {
                startDateTime = slot.DateTimeRange.Lower.Value.ToLocalTime();
              }

              DateTime? endDateTime = null;
              if (slot.DateTimeRange.Upper.HasValue) {
                endDateTime = slot.DateTimeRange.Upper.Value.ToLocalTime();
              }

              barObjects.Add(new BarSegment(startDateTime, endDateTime,
                                            slot.MachineStateTemplate, slot.MachineStateTemplate.Display));
            }
          }
        }
      }
      
      return barObjects;
    }
    #endregion // Methods
  }
}
