// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Utilities to get the config keys
  /// </summary>
  public class ConfigKeys
  {
    /// <summary>
    /// Get an calendar config key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetCalendarConfigKey (CalendarConfigKey key)
    {
      return ConfigPrefix.Global.ToString () + "." + GlobalConfigPrefix.Calendar.ToString () + "." + key.ToString ();
    }

    /// <summary>
    /// Get an analysis config key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetAnalysisConfigKey (AnalysisConfigKey key)
    {
      return ConfigPrefix.Analysis.ToString () + "." + key.ToString ();
    }

    /// <summary>
    /// Get an CNC config key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetCncConfigKey (CncConfigKey key)
    {
      return ConfigPrefix.Cnc.ToString () + "." + key.ToString ();
    }

    /// <summary>
    /// Get a Webservice config key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetWebServiceConfigKey (WebServiceConfigKey key)
    {
      return ConfigPrefix.WebService.ToString () + "." + key.ToString ();
    }

    /// <summary>
    /// Get a Data Structure config key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetDataStructureConfigKey (DataStructureConfigKey key)
    {
      return ConfigPrefix.DataStructure.ToString () + "." + key.ToString ();
    }

    /// <summary>
    /// Get a config key for the DB_Monitor or UAT
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetDbmConfigKey (DbmConfigKey key)
    {
      return ConfigPrefix.Gui.ToString () + "." + GuiConfigPrefix.Dbm.ToString () + "." + key.ToString ();
    }

    /// <summary>
    /// Get an Operation Explorer config key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetOperationExplorerConfigKey (OperationExplorerConfigKey key)
    {
      return ConfigPrefix.Gui.ToString () + "." + GuiConfigPrefix.OperationExplorer.ToString () + "." + key.ToString ();
    }
  }
  
  /// <summary>
  /// Set of Analysis config prefixes
  /// </summary>
  public enum ConfigPrefix
  {
    /// <summary>
    /// Prefix for some global configs
    /// </summary>
    Global,
    /// <summary>
    /// Prefix for the Analysis configs
    /// </summary>
    Analysis,
    /// <summary>
    /// Prefix for the CNC configs
    /// </summary>
    Cnc,
    /// <summary>
    /// Prefix for the DataStructure configs
    /// </summary>
    DataStructure,
    /// <summary>
    /// Prefix for the graphical user interface configs
    /// </summary>
    Gui,
    /// <summary>
    /// Prefix for the web service configs
    /// </summary>
    WebService,
  }
  
  /// <summary>
  /// Set of global config prefixes
  /// </summary>
  public enum GlobalConfigPrefix
  {
    /// <summary>
    /// Calendar configs
    /// </summary>
    Calendar,
  }

  /// <summary>
  /// Set of calendar config keys
  /// 
  /// These keys are prefixed by ConfigPrefix.Global and GlobalConfigPrefix.Calendar
  /// </summary>
  public enum CalendarConfigKey
  {
    /// <summary>
    /// First day of week
    /// </summary>
    FirstDayOfWeek,
    /// <summary>
    /// Calendar week rule:
    /// <item>FirstDay</item>
    /// <item>FirstFourDayWeek (almost ISO, see https://blogs.msdn.microsoft.com/shawnste/2006/01/24/iso-8601-week-of-year-format-in-microsoft-net/)</item>
    /// <item>FirstFullWeek</item>
    /// <item>Iso (for future use)</item>
    /// </summary>
    CalendarWeekRule,
  }
  
  /// <summary>
  /// Set of Analysis Config keys
  /// 
  /// These keys are prefixed by ConfigPrefix.Analysis
  /// </summary>
  public enum AnalysisConfigKey
  {
    /// <summary>
    /// Obsolete time
    /// </summary>
    ObsoleteTime,
    // Note: ActivitySequenceDetectionMargin is now deprecated
    /// <summary>
    /// Margin for auto-operation
    /// </summary>
    AutoOperationMargin,
    /// <summary>
    /// Time after which the link operation is not applied any more
    /// </summary>
    LinkOperationMaximumTime,
    /// <summary>
    /// Operation cycle association margin
    /// </summary>
    OperationCycleAssociationMargin,
    /// <summary>
    /// Clean deleted modifications option
    /// </summary>
    CleanDeletedModifications,
    /// <summary>
    /// Every parameter
    /// </summary>
    Every,
    /// <summary>
    /// Frequency at which the activity analysis is run
    /// </summary>
    ActivityAnalysisFrequency,
    /// <summary>
    /// Maximum number of threads to use in the thread pool
    /// </summary>
    MaxThreadsInPool,
    /// <summary>
    /// Maximum number of monitored machine analysis threads that can be run simultaneously
    /// </summary>
    MaxRunningMachineThreads,
    /// <summary>
    /// Key used for "Should a link between a workOrder with no component
    /// and the component of an operation be created on operation slots analysis ?"
    /// </summary>
    CreateWorkOrderComponentLink,
    /// <summary>
    /// Key used for "Should a link between an operation with no component
    /// and the component of a work order be created on operation slots analysis ?"
    /// </summary>
    CreateComponentOperationLink,
    /// <summary>
    /// Option to split the operation slot:
    /// <item>0: none</item>
    /// <item>1: by day</item>
    /// <item>2: by global shift (as defined in shiftslot)</item>
    /// <item>4: by machine shift (as defined in observationstateslot)</item>
    /// <item></item>
    /// </summary>
    OperationSlotSplitOption,
    /// <summary>
    /// Option to split the cycle summaries by shift (else, only by day)
    /// 
    /// This option is only active if OperationSlotSplitOption has flag 2 or 4.
    /// 
    /// Default: true (if OperationSlotSplitOption is active)
    /// </summary>
    SplitCycleSummaryByShift,
    /// <summary>
    /// Get the time after which no auto operation should be set
    /// between two same operations
    /// </summary>
    AutoOperationSame,
    /// <summary>
    /// Get the time after which no auto component should be set
    /// between two operations that refer to the same components
    /// </summary>
    AutoComponentSame,
    /// <summary>
    /// Get the time after which no auto work order should be set
    /// between two operations that refer to the same work orders
    /// </summary>
    AutoWorkOrderSame,
    // Note: AutoOperationDifferent is deprecated, the feature has been removed
    /// <summary>
    /// Timeout for the modification analysis
    /// </summary>
    ModificationTimeout,
    /// <summary>
    /// Timeout for one step of a modification analysis
    /// </summary>
    ModificationStepTimeout,
    /// <summary>
    /// Extend a sequence slot if the gap between the two sequences (activities) is less than this value
    /// </summary>
    SequenceGapLimit,
    /// <summary>
    /// Extend an already full cycle when a new cycle end is detected
    /// </summary>
    ExtendFullCycleWhenNewCycleEnd,
    /// <summary>
    /// Current time frame duration that must be considered to determine current slots
    /// </summary>
    CurrentTimeFrameDuration,
    /// <summary>
    /// Min date/time to consider to process the templates
    /// </summary>
    MinTemplateProcessDateTime,
    /// <summary>
    /// Duration from now for which the day slots must be processed
    /// </summary>
    MaxDaySlotProcess,
    /// <summary>
    /// Duration from now for which the shift slots must be processed
    /// </summary>
    MaxShiftSlotProcess,
    /// <summary>
    /// Extend operation propagation option
    /// </summary>
    ExtendOperationPropagation,
    /// <summary>
    /// Auto-operation propagation option
    /// </summary>
    AutoOperationPropagation,
    /// <summary>
    /// Option to activate the line management
    /// </summary>
    LineManagement,
    /// <summary>
    /// Option to activate the manufacturing order management
    /// </summary>
    ManufacturingOrderManagement,
    /// <summary>
    /// Give the possibility to dynamically determine the line from the machine and the operation
    /// </summary>
    LineFromMachineOperation,
    /// <summary>
    /// Track the run time in the operation slots
    /// </summary>
    OperationSlotRunTime,
    /// <summary>
    /// Track the production duration in the operation slots
    /// </summary>
    OperationSlotProductionDuration,
    /// <summary>
    /// Percentage of the used memory after which no new thread is created
    /// </summary>
    MemoryPercentageStopNewThreads,
    /// <summary>
    /// Percentage of the used memory after which the service is stopped
    /// </summary>
    MemoryPercentageExit,
    /// <summary>
    /// Priority of the auto modifications
    /// </summary>
    AutoModificationPriority,
    /// <summary>
    /// Purge delay of the completed auto modifications
    /// </summary>
    AutoModificationPurgeDelay,
  }
  
  /// <summary>
  /// Possible OperationSlot split options
  /// </summary>
  [Flags]
  public enum OperationSlotSplitOption
  {
    /// <summary>
    /// No operation slot split
    /// </summary>
    None = 0,
    /// <summary>
    /// Split the operation slots by day
    /// </summary>
    ByDay = 1, // 1 << 0
    /// <summary>
    /// Split the operation slots by global shift (as defined in shiftslot)
    /// </summary>
    ByGlobalShift = 2, // 1 << 1
    /// <summary>
    /// Split the operation slots by machine shift (as defined in observationstateslot)
    /// </summary>
    ByMachineShift = 4, // 1 << 2
  }
  
  /// <summary>
  /// Extensions to OperationSlotSplitOption
  /// </summary>
  public static class OperationSlotSplitOptionExtensions
  {
    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this OperationSlotSplitOption t, OperationSlotSplitOption other)
    {
      return other == (t & other);
    }
    
    /// <summary>
    /// Are the operation slots split by shift (global or by machine)
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static bool IsSplitByShift (this OperationSlotSplitOption t)
    {
      return t.HasFlag (OperationSlotSplitOption.ByGlobalShift) || t.HasFlag (OperationSlotSplitOption.ByMachineShift);
    }
    
    /// <summary>
    /// Are the operation slots split by day
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static bool IsSplitByDay (this OperationSlotSplitOption t)
    {
      return t.HasFlag (OperationSlotSplitOption.ByDay) || t.HasFlag (OperationSlotSplitOption.ByGlobalShift);
    }
    
    /// <summary>
    /// Is the operation slot split active ?
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static bool IsActive (this OperationSlotSplitOption t)
    {
      return t.IsSplitByShift () || t.IsSplitByDay ();
    }
  }
  
  /// <summary>
  /// Propagation options for extend-operation and auto-operation
  /// </summary>
  [Flags]
  public enum PropagationOption
  {
    /// <summary>
    /// No propagation
    /// </summary>
    None = 0,
    /// <summary>
    /// Propagate the work order
    /// </summary>
    WorkOrder = 1, // 1 << 0
    /// <summary>
    /// Propagate the component
    /// </summary>
    Component = 2, // 1 << 1
    /// <summary>
    /// Propagate the line
    /// </summary>
    Line = 4, // 1 << 2
    /// <summary>
    /// Propgate the task
    /// </summary>
    ManufacturingOrder = 8, // 1 << 3
    /// <summary>
    /// Propage the work order, the component, the line, the manufacturing order
    /// </summary>
    All = 15, // = SUM
  }

  /// <summary>
  /// Extensions to PropagationOption
  /// </summary>
  public static class PropagationOptionExtensions
  {
    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this PropagationOption t, PropagationOption other)
    {
      return other == (t & other);
    }
    
    /// <summary>
    /// Is any of the option active ?
    /// </summary>
    /// <returns></returns>
    public static bool IsActive (this PropagationOption t)
    {
      return t != PropagationOption.None;
    }
  }
  
  /// <summary>
  /// Set of CNC Config keys
  /// 
  /// These keys are prefixed by ConfigPrefix.Cnc
  /// </summary>
  public enum CncConfigKey
  {
    /// <summary>
    /// Use processes instead of threads in the Lem_CncDataService
    /// </summary>
    CncDataUseProcess,
    /// <summary>
    /// Use processes instead of threads in the Lem_CncDataService
    /// </summary>
    ToolLifeDataObsoleteTime,
    /// <summary>
    /// Try to determine a stamp from the program name and block number
    /// </summary>
    StampFromProgramNameBlock,
    /// <summary>
    /// Spindle load peak limit
    /// </summary>
    SpindleLoadPeakLimit,
  }
  
  /// <summary>
  /// Set of Operation Explorer config keys
  /// 
  /// The keys are prefixed by ConfigPrefix.OperationExplorer
  /// </summary>
  public enum OperationExplorerConfigKey
  {
    /// <summary>
    /// Part at the top ?
    /// </summary>
    PartAtTheTop,
  }
  
  /// <summary>
  /// Set of Webservice config keys
  /// 
  /// The keys are prefixed by ConfigPrefix.WebService
  /// </summary>
  public enum WebServiceConfigKey
  {
    /// <summary>
    /// If not operation information currently available, consider
    /// last operation slot having one if its enddatetime is within this gap
    /// w.r.t. current date time
    /// </summary>
    GapLastOperationSlot,
  }
  
  /// <summary>
  /// Set of Data Structure config keys
  /// 
  /// The keys are prefixed by ConfigPrefix.DataStructure
  /// </summary>
  public enum DataStructureConfigKey
  {
    /// <summary>
    /// Option WorkOrder + Project = Job
    /// </summary>
    WorkOrderProjectIsJob,
    /// <summary>
    /// Option Project + Component = Part
    /// </summary>
    ProjectComponentIsPart,
    /// <summary>
    /// Option Intermediate Work Piece + Operation = Simple Operation
    /// </summary>
    IntermediateWorkPieceOperationIsSimpleOperation,
    /// <summary>
    /// Option Project/Component/Part => 1 Work Order
    /// </summary>
    UniqueWorkOrderFromProjectOrComponent,
    /// <summary>
    /// Option Operation => 1 Project/Component/Part
    /// </summary>
    UniqueComponentFromOperation,
    /// <summary>
    /// Option Work Order => 1 Project/Part
    /// </summary>
    UniqueProjectOrPartFromWorkOrder,
    /// <summary>
    /// Option Project/Component/Part &lt;= Operation only
    /// </summary>
    ComponentFromOperationOnly,
    /// <summary>
    /// Option Work Order &lt;= Project/Component/Part only
    /// </summary>
    WorkOrderFromComponentOnly,
    /// <summary>
    /// Operation Line => 1 Project/Part
    /// </summary>
    UniqueComponentFromLine,
    /// <summary>
    /// Key used for "Does an operation only have a single path ?"
    /// </summary>
    SinglePath,
  }
  
  /// <summary>
  /// Set of graphicale user interface Config keys
  /// 
  /// These keys are prefixed by ConfigPrefix.Gui
  /// </summary>
  public enum GuiConfigPrefix
  {
    /// <summary>
    /// Prefix to target the DB_Monitor or UAT
    /// </summary>
    Dbm,
    /// <summary>
    /// Prefix for OperationExplorer configs
    /// </summary>
    OperationExplorer,
  }

  /// <summary>
  /// Set of Config keys for the DB_Monitor or UAT
  /// 
  /// These keys are prefixed by ConfigPrefix.Cnc and GuiConfigPrefix.Dbm
  /// </summary>
  public enum DbmConfigKey
  {
    /// <summary>
    /// Input the Machine Observation State with the Reason
    /// </summary>
    MOSWithReason,
  }
  

}
