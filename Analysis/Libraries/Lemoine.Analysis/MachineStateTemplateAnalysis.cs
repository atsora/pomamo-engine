// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Linq;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Analysis
{
  /// <summary>
  /// Analysis of the MachineStateTemplate in ObservationStateSlot
  /// 
  /// Note: instead of applying the MachineStateTemplate items sequentially,
  ///       this may be more efficient (not really sure though)
  ///       to pre-process the individual period of times when a machine observation state applies
  ///       first
  /// </summary>
  internal class MachineStateTemplateAnalysis : TemplateAnalysis<ObservationStateSlot, IObservationStateSlot>
  {
    static readonly string MAX_SLOTS_BY_ITERATION_KEY = "Analysis.MachineStateTemplateAnalysis.MaxSlotsByIteration";
    static readonly int MAX_SLOTS_BY_ITERATION_DEFAULT = 2;

    static readonly string MAX_ANALYSIS_TIME_RANGE_KEY = "Analysis.MachineStateTemplateAnalysis.MaxAnalysisTimeRange";
    static readonly TimeSpan MAX_ANALYSIS_TIME_RANGE_DEFAULT = TimeSpan.FromHours (8);

    #region Members
    IMachine m_machine;
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (MachineStateTemplateAnalysis).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated machine
    /// </summary>
    public IMachine Machine
    {
      get { return m_machine; }
      private set
      {
        if (null == value) {
          log.Fatal ("Machine: " +
                     "it can't be null");
          throw new ArgumentNullException ();
        }
        else {
          m_machine = value;
          log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                    this.GetType ().FullName,
                                                    value.Id));
        }
      }
    }

    /// <summary>
    /// Max number of slots to process by iteration
    /// </summary>
    protected override int MaxSlotsByIteration
    {
      get
      {
        return Lemoine.Info.ConfigSet.LoadAndGet<int> (MAX_SLOTS_BY_ITERATION_KEY,
                                                       MAX_SLOTS_BY_ITERATION_DEFAULT);
      }
    }

    /// <summary>
    /// Max analysis time range to process when all the templates are processed
    /// </summary>
    protected override TimeSpan MaxAnalysisTimeRange
    {
      get
      {
        return Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (MAX_ANALYSIS_TIME_RANGE_KEY,
                                                            MAX_ANALYSIS_TIME_RANGE_DEFAULT);
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected MachineStateTemplateAnalysis ()
    {
      Debug.Assert (false);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="restrictedTransactionLevel"></param>
    /// <param name="checkedThread"></param>
    public MachineStateTemplateAnalysis (IMachine machine, TransactionLevel restrictedTransactionLevel, IChecked checkedThread)
      : base (restrictedTransactionLevel, checkedThread)
    {
      this.Machine = machine;
    }
    #endregion // Constructors

    /// <summary>
    /// Get a logger
    /// </summary>
    /// <returns></returns>
    protected override ILog GetLogger ()
    {
      return log;
    }

    /// <summary>
    /// Get the analysis rows in the specified range which template has not been processed yet
    /// </summary>
    /// <param name="range"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    protected override IEnumerable<IObservationStateSlot> GetNotProcessedTemplate (UtcDateTimeRange range, int limit)
    {
      Debug.Assert (range.Upper.HasValue);

      if (range.Lower.HasValue) {
        return ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindWithNoMachineObservationStateInRange (this.Machine, range.Lower.Value, range.Upper.Value, limit);
      }
      else {
        return ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindWithNoMachineObservationState (this.Machine, range.Upper.Value, limit);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    protected override IWithTemplate ReloadSlot (IObservationStateSlot slot)
    {
      return (IWithTemplate)ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.FindById (slot.Id, slot.Machine);
    }
  }
}
