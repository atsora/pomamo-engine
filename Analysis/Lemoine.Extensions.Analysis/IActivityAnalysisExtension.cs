// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Collections.Generic;

namespace Lemoine.Extensions.Analysis
{
  /// <summary>
  /// Track all changes in the observed and planned states of a monitored machine.
  /// Entry points in MonitoredMachineActivityAnalysis, Lemoine.Analysis
  /// </summary>
  public interface IActivityAnalysisExtension: IAnalysisExtension
  {
    /// <summary>
    /// Before processing the activities (not in a transaction)
    /// </summary>
    /// <param name="lastActivityAnalysisDateTime"></param>
    /// <param name="facts"></param>
    void BeforeProcessingActivities (DateTime lastActivityAnalysisDateTime, IList<IFact> facts);

    /// <summary>
    /// Before processing a new activity period
    /// </summary>
    /// <param name="machineStatus"></param>
    void BeforeProcessingNewActivityPeriod (IMachineStatus machineStatus);
    
    /// <summary>
    /// Process a new activity period
    /// </summary>
    /// <param name="activityRange"></param>
    /// <param name="machineMode"></param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="shift"></param>
    void AfterProcessingNewActivityPeriod (UtcDateTimeRange activityRange,
                                           IMachineMode machineMode,
                                           IMachineStateTemplate machineStateTemplate,
                                           IMachineObservationState machineObservationState,
                                           IShift shift);
    
    /// <summary>
    /// Run before the commit of the Activities transaction
    /// </summary>
    void BeforeActivitiesCommit ();
    
    /// <summary>
    /// Run after a rollback of the Activities transaction
    /// </summary>
    void AfterActivitiesRollback ();
  }
}
