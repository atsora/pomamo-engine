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
  /// Analysis of the DayTemplate in DaySlot
  /// </summary>
  internal class DayTemplateAnalysis : TemplateAnalysis<DaySlot, IDaySlot>
  {
    static readonly string MAX_SLOTS_BY_ITERATION_KEY = "Analysis.DayTemplateAnalysis.MaxSlotsByIteration";
    static readonly int MAX_SLOTS_BY_ITERATION_DEFAULT = 2;

    static readonly string MAX_ANALYSIS_TIME_RANGE_KEY = "Analysis.DayTemplateAnalysis.MaxAnalysisTimeRange";
    static readonly TimeSpan MAX_ANALYSIS_TIME_RANGE_DEFAULT = TimeSpan.FromDays (3);

    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (DayTemplateAnalysis).FullName);

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
    protected DayTemplateAnalysis ()
    {
      Debug.Assert (false);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="restrictedTransactionLevel"></param>
    /// <param name="checkedThread"></param>
    public DayTemplateAnalysis (TransactionLevel restrictedTransactionLevel, IChecked checkedThread)
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
    protected override IEnumerable<IDaySlot> GetNotProcessedTemplate (UtcDateTimeRange range, int limit)
    {
      Debug.Assert (range.Upper.HasValue);

      return ModelDAOHelper.DAOFactory.DaySlotDAO
        .GetNotProcessTemplate (range, limit);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    protected override IWithTemplate ReloadSlot (IDaySlot slot)
    {
      return (IWithTemplate)ModelDAOHelper.DAOFactory.DaySlotDAO.FindById (slot.Id);
    }
  }
}
