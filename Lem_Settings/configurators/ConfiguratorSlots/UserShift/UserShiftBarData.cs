// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.UserShift
{
  /// <summary>
  /// Description of UserShiftBarData.
  /// </summary>
  public class UserShiftBarData: IBarObjectFactory
  {
    #region Members
    readonly IUser m_user;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="user"></param>
    public UserShiftBarData(IUser user)
    {
      m_user = user;
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
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ModelDAOHelper.DAOFactory.UserDAO.Lock(m_user);
          IList<IUserShiftSlot> slots = ModelDAOHelper.DAOFactory.UserShiftSlotDAO
            .FindOverlapsRange(m_user, new UtcDateTimeRange(start, end));
          
          foreach (IUserShiftSlot slot in slots) {
            if (slot.Shift != null) {
              DateTime? startDateTime = null;
              if (slot.BeginDateTime.HasValue) {
                startDateTime = slot.BeginDateTime.Value.ToLocalTime();
              }

              DateTime? endDateTime = null;
              if (slot.EndDateTime.HasValue) {
                endDateTime = slot.EndDateTime.Value.ToLocalTime();
              }

              barObjects.Add(new BarSegment(startDateTime, endDateTime,
                                            slot.Shift, slot.Shift.Display));
            }
          }
        }
      }
      
      return barObjects;
    }
    #endregion // Methods
  }
}
