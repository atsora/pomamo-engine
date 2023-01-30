// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ViewReasons
{
  /// <summary>
  /// Description of ReasonManager.
  /// </summary>
  public class ReasonManager
  {
    #region Members
    readonly List<ReasonsByMOS> m_reasons = new List<ReasonsByMOS>();
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Get the number of reason groups
    /// </summary>
    public int Count { get { return m_reasons.Count; } }
    #endregion // Getters / Setters

    #region Methods
    /// <summary>
    /// Load all reasons for a particular machine mode
    /// </summary>
    /// <param name="machineMode">can be null</param>
    public void LoadReasons(IMachineMode machineMode)
    {
      m_reasons.Clear();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction())
        {
          // For each machine observation states
          IList<IMachineObservationState> moss = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindAll();
          foreach (var mos in moss) {
            var reasonsByMOS = new ReasonsByMOS();
            reasonsByMOS.MOSS.Add(mos);

            // Recursively browse the machine mode and its parents until default reasons are found
            var machineModeTmp = machineMode;
            bool inherited = false;
            bool found = false;
            while (!found && machineModeTmp != null) {
              // All default reasons for the pair {MOS ; machine mode}
              var mmdrs = ModelDAOHelper.DAOFactory
                .MachineModeDefaultReasonDAO.FindWithForConfig(machineModeTmp, mos);
              foreach (var mmdr in mmdrs) {
                reasonsByMOS.Reasons.Add(new OrderedReason(mmdr, inherited));
                found = true;
              }
              
              if (!found) {
                machineModeTmp = machineModeTmp.Parent;
                inherited = true;
              }
            }
            
            // Recursively browse the machine mode and its parents until selectable reasons are found
            machineModeTmp = machineMode;
            inherited = false;
            found = false;
            while (!found && machineModeTmp != null) {
              // All selectable reasons for the pair {MOS ; machine mode}
              var rss = ModelDAOHelper.DAOFactory
                .ReasonSelectionDAO.FindWithForConfig(machineModeTmp, mos);
              foreach (var rs in rss) {
                if (rs.Selectable) {
                  reasonsByMOS.Reasons.Add(new OrderedReason(rs, inherited));
                  found = true;
                }
              }
              
              if (!found) {
                machineModeTmp = machineModeTmp.Parent;
                inherited = true;
              }
            }
            
            // Sort reasons for the pair {MOS ; machine mode}
            reasonsByMOS.Sort();
            m_reasons.Add(reasonsByMOS);
          }
        }
      }
      
      // Factorize reasons
      int count = m_reasons.Count;
      for (int i = count - 1; i > 0; i--) {
        if (m_reasons[i-1].Factorize(m_reasons[i])) {
          m_reasons.RemoveAt(i);
        }
      }

      // Order the reason groups
      m_reasons.Sort();
    }
    
    /// <summary>
    /// Get all reasons sorted and factorized by MOS
    /// </summary>
    /// <returns></returns>
    public IList<ReasonsByMOS> GetReasons()
    {
      return m_reasons;
    }
    #endregion // Methods
  }
}
