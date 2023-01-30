// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Threading;

namespace Lemoine.Analysis
{
  /// <summary>
  /// WeekNumberAnalysis
  /// </summary>
  public class WeekNumberAnalysis
    : IChecked
  {
    readonly ILog log = LogManager.GetLogger (typeof (WeekNumberAnalysis).FullName);

    #region Members
    readonly IChecked m_checkedThread = null;
    readonly TransactionLevel m_restrictedTransactionLevel = TransactionLevel.Serializable;
    #endregion // Members

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected WeekNumberAnalysis ()
    {
      Debug.Assert (false);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="restrictedTransactionLevel"></param>
    /// <param name="checkedThread"></param>
    public WeekNumberAnalysis (TransactionLevel restrictedTransactionLevel, IChecked checkedThread)
    {
      m_restrictedTransactionLevel = restrictedTransactionLevel;
      m_checkedThread = checkedThread;
    }
    #endregion // Constructors

    #region IChecked
    /// <summary>
    /// SetActive method
    /// </summary>
    public void SetActive ()
    {
      if (null != m_checkedThread) {
        m_checkedThread.SetActive ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void PauseCheck ()
    {
      if (null != m_checkedThread) {
        m_checkedThread.PauseCheck ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void ResumeCheck ()
    {
      if (null != m_checkedThread) {
        m_checkedThread.ResumeCheck ();
      }
    }
    #endregion // IChecked

    public void Run (CancellationToken cancellationToken = default)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("Analysis.WeekNumber", m_restrictedTransactionLevel)) {
          var daySlots = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindWithNoWeekNumber ();
          foreach (var daySlot in daySlots) {
            cancellationToken.ThrowIfCancellationRequested ();
            SetActive ();
            ((Lemoine.GDBPersistentClasses.DaySlot)daySlot).ComputeWeekNumber ();
            ModelDAOHelper.DAOFactory.DaySlotDAO.MakePersistent (daySlot);
          }
          transaction.Commit ();
        }
      }
    }
  }
}
