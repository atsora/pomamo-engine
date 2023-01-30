// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Business.Config
{
  /// <summary>
  /// Class to get directly some configuration values for the analysis service
  /// 
  /// This is deprecated. Use now directly ConfigSet.LoadAndGet
  /// </summary>
  public sealed class AnalysisConfigHelper
  {
    static readonly TimeSpan DEFAULT_OBSOLETE_TIME = TimeSpan.FromDays (2);
    static readonly TimeSpan DEFAULT_AUTO_OPERATION_SAME = TimeSpan.FromDays (1);
    static readonly TimeSpan DEFAULT_AUTO_COMPONENT_SAME = TimeSpan.FromDays (1);
    static readonly TimeSpan DEFAULT_AUTO_WORKORDER_SAME = TimeSpan.FromDays (1);
    static readonly TimeSpan DEFAULT_AUTO_OPERATION_MARGIN = TimeSpan.FromSeconds (40);
    static readonly TimeSpan DEFAULT_LINK_OPERATION_MAXIMUM_TIME = TimeSpan.FromDays (1);
    static readonly TimeSpan DEFAULT_OPERATIONCYCLE_ASSOCIATION_MARGIN = TimeSpan.FromSeconds (20);
    static readonly bool DEFAULT_CLEAN_DELETED_MODIFICATIONS = true;
    static readonly TimeSpan DEFAULT_EVERY = TimeSpan.FromSeconds (1);
    static readonly TimeSpan DEFAULT_ACTIVITY_ANALYSIS_FREQUENCY = TimeSpan.FromSeconds (3);
    static readonly int DEFAULT_MAX_THREADS_IN_POOL = 1024;
    static readonly int DEFAULT_MAX_RUNNING_MACHINE_THREADS = 16;
    static readonly bool DEFAULT_CREATEWORKORDERCOMPONENTLINK = false;
    static readonly bool DEFAULT_CREATECOMPONENTOPERATIONLINK = false;
    static readonly TimeSpan DEFAULT_MODIFICATION_TIMEOUT = TimeSpan.FromSeconds (40);
    static readonly TimeSpan DEFAULT_MODIFICATION_STEP_TIMEOUT = TimeSpan.FromSeconds (8);
    static readonly TimeSpan DEFAULT_CURRENT_TIME_FRAME_DURATION = TimeSpan.FromMinutes (1);
    static readonly OperationSlotSplitOption DEFAULT_OPERATION_SLOT_SPLIT_OPTION = OperationSlotSplitOption.None;
    static readonly DateTime DEFAULT_MIN_TEMPLATE_PROCESS_DATE_TIME = new DateTime (2020, 01, 01, 00, 00, 00, DateTimeKind.Utc);
    static readonly TimeSpan DEFAULT_MAX_DAY_SLOT_PROCESS = TimeSpan.FromDays (365);
    static readonly TimeSpan DEFAULT_MAX_SHIFT_SLOT_PROCESS = TimeSpan.FromDays (365);
    static readonly bool DEFAULT_SPLIT_CYCLE_SUMMARY_BY_SHIFT = true;
    static readonly PropagationOption DEFAULT_EXTEND_OPERATION_PROPAGATION = PropagationOption.All;
    static readonly PropagationOption DEFAULT_AUTO_OPERATION_PROPAGATION = PropagationOption.All;
    static readonly bool DEFAULT_LINE_MANAGEMENT = false;
    static readonly bool DEFAULT_TASK_MANAGEMENT = false;
    static readonly bool DEFAULT_LINE_FROM_MACHINE_OPERATION = true;
    static readonly bool DEFAULT_OPERATION_SLOT_RUN_TIME = true;
    static readonly bool DEFAULT_OPERATION_SLOT_PRODUCTION_DURATION = true;
    static readonly int DEFAULT_MEMORY_PERCENTAGE_STOP_NEW_THREADS = 50;
    static readonly int DEFAULT_MEMORY_PERCENTAGE_EXIT = 68;
    static readonly int DEFAULT_AUTO_MODIFICATION_PRIORITY = 5;
    static readonly TimeSpan DEFAULT_AUTO_MODIFICATION_PURGE_DELAY = TimeSpan.FromDays (2);

    static readonly ILog log = LogManager.GetLogger (typeof (AnalysisConfigHelper).FullName);

    #region Getters / Setters
    /// <summary>
    /// Default time after which a undone modification table
    /// is considered as obsolete
    /// </summary>
    public static TimeSpan ObsoleteTime
    {
      get
      {
        return GetConfig (AnalysisConfigKey.ObsoleteTime, DEFAULT_OBSOLETE_TIME);
      }
    }

    /// <summary>
    /// Get the time after which no auto operation should be set
    /// between two same operations
    /// </summary>
    public static TimeSpan AutoOperationSame
    {
      get
      {
        return GetConfig (AnalysisConfigKey.AutoOperationSame, DEFAULT_AUTO_OPERATION_SAME);
      }
    }

    /// <summary>
    /// Time after which a small auto-sequence period after the
    /// last operation slot is not taken into account.
    /// 
    /// It is very useful to manage the small unidentifier periods
    /// once the variable of the CNC is resetted at the end of the
    /// ISO file.
    /// 
    /// Default value is 40 s
    /// </summary>
    /// <returns></returns>
    public static TimeSpan AutoOperationMargin
    {
      get
      {
        return GetConfig (AnalysisConfigKey.AutoOperationMargin, DEFAULT_AUTO_OPERATION_MARGIN);
      }
    }

    /// <summary>
    /// Get the time after which no auto component should be set
    /// between two same components
    /// </summary>
    public static TimeSpan AutoComponentSame
    {
      get
      {
        return GetConfig (AnalysisConfigKey.AutoComponentSame, DEFAULT_AUTO_COMPONENT_SAME);
      }
    }

    /// <summary>
    /// Get the time after which no auto work order should be set
    /// between two same work orders
    /// </summary>
    public static TimeSpan AutoWorkOrderSame
    {
      get
      {
        return GetConfig (AnalysisConfigKey.AutoWorkOrderSame, DEFAULT_AUTO_WORKORDER_SAME);
      }
    }

    /// <summary>
    /// Maximum time after which a link operation is not applied any more
    /// 
    /// Default value is one day
    /// </summary>
    /// <returns></returns>
    public static TimeSpan LinkOperationMaximumTime
    {
      get
      {
        return GetConfig (AnalysisConfigKey.LinkOperationMaximumTime, DEFAULT_LINK_OPERATION_MAXIMUM_TIME);
      }
    }

    /// <summary>
    /// Margin an operation cycle association is given
    /// to match an operation slot
    /// 
    /// Default value is 20 s
    /// </summary>
    /// <returns></returns>
    public static TimeSpan OperationCycleAssociationMargin
    {
      get
      {
        return GetConfig (AnalysisConfigKey.OperationCycleAssociationMargin, DEFAULT_OPERATIONCYCLE_ASSOCIATION_MARGIN);
      }
    }

    /// <summary>
    /// Once all the modifications has been processed,
    /// this option tells if the modifications that have been flagged
    /// Delete must be deleted or not (for debug purpose for example)
    /// 
    /// Default is true
    /// </summary>
    public static bool CleanDeletedModifications
    {
      get
      {
        return GetConfig (AnalysisConfigKey.CleanDeletedModifications, DEFAULT_CLEAN_DELETED_MODIFICATIONS);
      }
    }

    /// <summary>
    /// This value indicates how often is run the analysis.
    /// 
    /// This is a minimum value. There is no sleep time
    /// if the process takes more time than this value.
    /// </summary>
    public static TimeSpan Every
    {
      get
      {
        return GetConfig (AnalysisConfigKey.Every, DEFAULT_EVERY);
      }
    }

    /// <summary>
    /// This value indicates how often is run the activity analysis.
    /// </summary>
    public static TimeSpan ActivityAnalysisFrequency
    {
      get
      {
        return GetConfig (AnalysisConfigKey.ActivityAnalysisFrequency, DEFAULT_ACTIVITY_ANALYSIS_FREQUENCY);
      }
    }

    /// <summary>
    /// This value indicates what is the maximum number of threads to use in the thread pool.
    /// </summary>
    public static int MaxThreadsInPool
    {
      get
      {
        return GetConfig (AnalysisConfigKey.MaxThreadsInPool, DEFAULT_MAX_THREADS_IN_POOL);
      }
    }

    /// <summary>
    /// This value indicates what is the maximum number of monitored machine threads
    /// that can be run simultaneously.
    /// </summary>
    public static int MaxRunningMachineThreads
    {
      get
      {
        return GetConfig (AnalysisConfigKey.MaxRunningMachineThreads, DEFAULT_MAX_RUNNING_MACHINE_THREADS);
      }
    }

    /// <summary>
    /// Should a link between a workOrder with no component
    /// and the component of an operation be created on operation slots analysis ?
    /// </summary>
    public static bool CreateWorkOrderComponentLink {
      get
      {
        return GetConfig (AnalysisConfigKey.CreateWorkOrderComponentLink, DEFAULT_CREATEWORKORDERCOMPONENTLINK);
      }
    }

    /// <summary>
    /// Should a link between an operation with no component
    /// and the component of a work order be created on operation slots analysis ?
    /// </summary>
    public static bool CreateComponentOperationLink {
      get
      {
        return GetConfig (AnalysisConfigKey.CreateComponentOperationLink, DEFAULT_CREATECOMPONENTOPERATIONLINK);
      }
    }

    /// <summary>
    /// Timeout for the modifications
    /// </summary>
    public static TimeSpan ModificationTimeout
    {
      get
      {
        return GetConfig (AnalysisConfigKey.ModificationTimeout, DEFAULT_MODIFICATION_TIMEOUT);
      }
    }

    /// <summary>
    /// Timeout for one step of a modification
    /// </summary>
    public static TimeSpan ModificationStepTimeout
    {
      get
      {
        return GetConfig (AnalysisConfigKey.ModificationStepTimeout, DEFAULT_MODIFICATION_STEP_TIMEOUT);
      }
    }

    /// <summary>
    /// Current time frame duration.
    /// 
    /// The current time frame is the period of time before now that can be considered
    /// for current operation / work order slots
    /// </summary>
    public static TimeSpan CurrentTimeFrameDuration
    {
      get
      {
        return GetConfig (AnalysisConfigKey.CurrentTimeFrameDuration, DEFAULT_CURRENT_TIME_FRAME_DURATION);
      }
    }

    /// <summary>
    /// OperationSlot split option
    /// </summary>
    public static OperationSlotSplitOption OperationSlotSplitOption {
      get
      {
        return (OperationSlotSplitOption) GetConfig<int> (AnalysisConfigKey.OperationSlotSplitOption,
                                                          (int)DEFAULT_OPERATION_SLOT_SPLIT_OPTION);
      }
    }

    /// <summary>
    /// Min template process date/time.
    /// 
    /// This is the minimum date/time to consider when the day slots are processed
    /// </summary>
    public static DateTime MinTemplateProcessDateTime
    {
      get
      {
        return GetConfig (AnalysisConfigKey.MinTemplateProcessDateTime, DEFAULT_MIN_TEMPLATE_PROCESS_DATE_TIME);
      }
    }

    /// <summary>
    /// Duration from now for which the day slots must be processed
    /// </summary>
    public static TimeSpan MaxDaySlotProcess
    {
      get
      {
        return GetConfig (AnalysisConfigKey.MaxDaySlotProcess, DEFAULT_MAX_DAY_SLOT_PROCESS);
      }
    }

    /// <summary>
    /// Duration from now for which the shift slots must be processed
    /// </summary>
    public static TimeSpan MaxShiftSlotProcess
    {
      get
      {
        return GetConfig (AnalysisConfigKey.MaxShiftSlotProcess, DEFAULT_MAX_SHIFT_SLOT_PROCESS);
      }
    }

    /// <summary>
    /// Split the cycle summary by shift ?
    /// 
    /// This is valid only if the operation slots are split by shift
    /// </summary>
    public static bool SplitCycleSummaryByShift {
      get
      {
        return GetConfig (AnalysisConfigKey.SplitCycleSummaryByShift, DEFAULT_SPLIT_CYCLE_SUMMARY_BY_SHIFT);
      }
    }

    /// <summary>
    /// Extend operation propgation option
    /// </summary>
    public static PropagationOption ExtendOperationPropagation {
      get
      {
        return (PropagationOption) GetConfig<int> (AnalysisConfigKey.ExtendOperationPropagation,
                                                   (int)DEFAULT_EXTEND_OPERATION_PROPAGATION);
      }
    }

    /// <summary>
    /// Auto-operation propgation option
    /// </summary>
    public static PropagationOption AutoOperationPropagation {
      get
      {
        return (PropagationOption) GetConfig<int> (AnalysisConfigKey.AutoOperationPropagation,
                                                   (int)DEFAULT_AUTO_OPERATION_PROPAGATION);
      }
    }

    /// <summary>
    /// Option to validate the line management
    /// </summary>
    public static bool LineManagement {
      get
      {
        return GetConfig (AnalysisConfigKey.LineManagement,
                          DEFAULT_LINE_MANAGEMENT);
      }
    }

    /// <summary>
    /// Option to validate the task management
    /// </summary>
    public static bool TaskManagement {
      get
      {
        return GetConfig (AnalysisConfigKey.TaskManagement,
                          DEFAULT_TASK_MANAGEMENT);
      }
    }

    /// <summary>
    /// Give the possibility to determine the line from the operation and the machine
    /// </summary>
    public static bool LineFromMachineOperation {
      get
      {
        return GetConfig (AnalysisConfigKey.LineFromMachineOperation,
                          DEFAULT_LINE_FROM_MACHINE_OPERATION);
      }
    }

    /// <summary>
    /// Track the run time in operation slots
    /// </summary>
    public static bool OperationSlotRunTime {
      get
      {
        return GetConfig (AnalysisConfigKey.OperationSlotRunTime,
                          DEFAULT_OPERATION_SLOT_RUN_TIME);
      }
    }

    /// <summary>
    /// Track the production duration in operation slots
    /// </summary>
    public static bool OperationSlotProductionDuration {
      get
      {
        return GetConfig (AnalysisConfigKey.OperationSlotProductionDuration,
                          DEFAULT_OPERATION_SLOT_PRODUCTION_DURATION);
      }
    }

    /// <summary>
    /// Percentage of the used memory after which no new thread is created
    /// </summary>
    public static int MemoryPercentageStopNewThreads
    {
      get
      {
        return GetConfig (AnalysisConfigKey.MemoryPercentageStopNewThreads,
                          DEFAULT_MEMORY_PERCENTAGE_STOP_NEW_THREADS);
      }
    }

    /// <summary>
    /// Percentage of the used memory after which the service is stopped
    /// </summary>
    public static int MemoryPercentageExit
    {
      get
      {
        return GetConfig (AnalysisConfigKey.MemoryPercentageExit,
                          DEFAULT_MEMORY_PERCENTAGE_EXIT);
      }
    }

    /// <summary>
    /// Priority for the auto-modifications
    /// </summary>
    public static int AutoModificationPriority
    {
      get
      {
        return GetConfig (AnalysisConfigKey.AutoModificationPriority,
                          DEFAULT_AUTO_MODIFICATION_PRIORITY);
      }
    }

    /// <summary>
    /// Get the auto-modification purge delay
    /// </summary>
    public static TimeSpan AutoModificationPurgeDelay
    {
      get
      {
        return GetConfig (AnalysisConfigKey.AutoModificationPurgeDelay,
                          DEFAULT_AUTO_MODIFICATION_PURGE_DELAY);
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private AnalysisConfigHelper ()
    {
    }
    #endregion // Constructors

    #region Methods
    static T GetConfig<T> (AnalysisConfigKey key, T defaultValue)
    {
      string configKey = ConfigKeys.GetAnalysisConfigKey (key);
      return Lemoine.Info.ConfigSet
        .LoadAndGet<T> (configKey,
                        defaultValue);
    }
    #endregion // Methods

    #region Instance
    static AnalysisConfigHelper Instance
    {
      get { return Nested.instance; }
    }

    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested ()
      {
      }

      internal static readonly AnalysisConfigHelper instance = new AnalysisConfigHelper ();
    }
    #endregion // Instance
  }
}
