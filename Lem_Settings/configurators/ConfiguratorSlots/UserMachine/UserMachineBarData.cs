// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.UserMachine
{
  /// <summary>
  /// Description of UserMachineBarData.
  /// </summary>
  public class UserMachineBarData: IBarObjectFactory
  {
    #region Members
    readonly IUser m_user;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="user"></param>
    public UserMachineBarData(IUser user)
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
          IList<IUserMachineSlot> slots = ModelDAOHelper.DAOFactory.UserMachineSlotDAO
            .FindOverlapsRange(m_user, new UtcDateTimeRange(start, end));
          
          foreach (IUserMachineSlot slot in slots) {
            if (slot.Machines != null && slot.Machines.Count > 0) {
              DateTime? startDateTime = null;
              if (slot.BeginDateTime.HasValue) {
                startDateTime = slot.BeginDateTime.Value.ToLocalTime();
              }

              DateTime? endDateTime = null;
              if (slot.EndDateTime.HasValue) {
                endDateTime = slot.EndDateTime.Value.ToLocalTime();
              }

              // Display and legend
              IList<string> listStr = new List<string>();
              IList<IMachineStateTemplate> msts = new List<IMachineStateTemplate>();
              foreach (IUserMachineSlotMachine umsm in slot.Machines.Values) {
                listStr.Add(String.Format("{0} \u2192 {1}", umsm.Machine.Display, umsm.MachineStateTemplate.Display));
                if (!msts.Contains(umsm.MachineStateTemplate)) {
                  msts.Add(umsm.MachineStateTemplate);
                }
              }
              
              object obj = "";
              string legend = "multiple states";
              if (msts.Count == 1) {
                obj = msts[0];
                legend = msts[0].Display;
              }
              
              barObjects.Add(new BarSegment(startDateTime, endDateTime, obj,
                                            String.Join("\n", listStr.ToArray()), legend));
            }
          }
        }
      }
      
      return barObjects;
    }
    #endregion // Methods
  }
}
