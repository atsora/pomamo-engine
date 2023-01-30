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
  /// Analysis of the ShiftTemplate in ShiftSlot
  /// </summary>
  internal class ShiftTemplateAnalysis : TemplateAnalysis<ShiftSlot, IShiftSlot>
  {
    static readonly string MAX_SLOTS_BY_ITERATION_KEY = "Analysis.ShiftTemplateAnalysis.MaxSlotsByIteration";
    static readonly int MAX_SLOTS_BY_ITERATION_DEFAULT = 2;

    static readonly string MAX_ANALYSIS_TIME_RANGE_KEY = "Analysis.ShiftTemplateAnalysis.MaxAnalysisTimeRange";
    static readonly TimeSpan MAX_ANALYSIS_TIME_RANGE_DEFAULT = TimeSpan.FromDays (3);

    #region Members
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (ShiftTemplateAnalysis).FullName);

    #region Getters
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
    #endregion // Getters

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected ShiftTemplateAnalysis ()
    {
      Debug.Assert (false);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="restrictedTransactionLevel"></param>
    /// <param name="checkedThread"></param>
    public ShiftTemplateAnalysis (TransactionLevel restrictedTransactionLevel, IChecked checkedThread)
      : base (restrictedTransactionLevel, checkedThread)
    {
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
    protected override IEnumerable<IShiftSlot> GetNotProcessedTemplate (UtcDateTimeRange range, int limit)
    {
      Debug.Assert (range.Upper.HasValue);

      return ModelDAOHelper.DAOFactory.ShiftSlotDAO
        .GetNotProcessTemplate (range, limit);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    protected override IWithTemplate ReloadSlot (IShiftSlot slot)
    {
      return (IWithTemplate)ModelDAOHelper.DAOFactory.ShiftSlotDAO.FindById (slot.Id);
    }
  }
}
