// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Config;
using Lemoine.Model;

namespace Lemoine.Plugin.AnalysisConfigDefault
{
  public class ConfigExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IConfigExtension
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
    static readonly int DEFAULT_OPERATION_SLOT_SPLIT_OPTION = (int)OperationSlotSplitOption.None;
    static readonly DateTime DEFAULT_MIN_TEMPLATE_PROCESS_DATE_TIME = new DateTime (2020, 01, 01, 00, 00, 00, DateTimeKind.Utc);
    static readonly TimeSpan DEFAULT_MAX_DAY_SLOT_PROCESS = TimeSpan.FromDays (365);
    static readonly TimeSpan DEFAULT_MAX_SHIFT_SLOT_PROCESS = TimeSpan.FromDays (365);
    static readonly bool DEFAULT_SPLIT_CYCLE_SUMMARY_BY_SHIFT = true;
    static readonly int DEFAULT_EXTEND_OPERATION_PROPAGATION = (int)PropagationOption.All;
    static readonly int DEFAULT_AUTO_OPERATION_PROPAGATION = (int)PropagationOption.All;
    static readonly bool DEFAULT_LINE_MANAGEMENT = false;
    static readonly bool DEFAULT_MANUFACTURING_ORDER_MANAGEMENT = false;
    static readonly bool DEFAULT_LINE_FROM_MACHINE_OPERATION = true;
    static readonly bool DEFAULT_OPERATION_SLOT_RUN_TIME = true;
    static readonly bool DEFAULT_OPERATION_SLOT_PRODUCTION_DURATION = true;
    static readonly int DEFAULT_MEMORY_PERCENTAGE_STOP_NEW_THREADS = 50;
    static readonly int DEFAULT_MEMORY_PERCENTAGE_EXIT = 68;
    static readonly int DEFAULT_AUTO_MODIFICATION_PRIORITY = 5;
    static readonly TimeSpan DEFAULT_AUTO_MODIFICATION_PURGE_DELAY = TimeSpan.FromDays (2);
    static readonly TimeSpan DEFAULT_SEQUENCE_GAP_LIMIT = TimeSpan.FromHours (24);
    static readonly bool DEFAULT_EXTEND_FULL_CYCLE_WHEN_NEW_CYCLE_END = false;

    readonly ILog log = LogManager.GetLogger (typeof (ConfigExtension).FullName);

    public double Priority => 10.0;

    object Get (string key)
    {
      if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ActivityAnalysisFrequency))) {
        return DEFAULT_ACTIVITY_ANALYSIS_FREQUENCY;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.AutoComponentSame))) {
        return DEFAULT_AUTO_COMPONENT_SAME;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.AutoModificationPriority))) {
        return DEFAULT_AUTO_MODIFICATION_PRIORITY;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.AutoModificationPurgeDelay))) {
        return DEFAULT_AUTO_MODIFICATION_PURGE_DELAY;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.AutoOperationMargin))) {
        return DEFAULT_AUTO_OPERATION_MARGIN;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.AutoOperationPropagation))) {
        return DEFAULT_AUTO_OPERATION_PROPAGATION;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.AutoOperationSame))) {
        return DEFAULT_AUTO_OPERATION_SAME;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.AutoWorkOrderSame))) {
        return DEFAULT_AUTO_WORKORDER_SAME;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.CleanDeletedModifications))) {
        return DEFAULT_CLEAN_DELETED_MODIFICATIONS;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.CreateComponentOperationLink))) {
        return DEFAULT_CREATECOMPONENTOPERATIONLINK;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.CreateWorkOrderComponentLink))) {
        return DEFAULT_CREATEWORKORDERCOMPONENTLINK;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.CurrentTimeFrameDuration))) {
        return DEFAULT_CURRENT_TIME_FRAME_DURATION;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.Every))) {
        return DEFAULT_EVERY;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ExtendFullCycleWhenNewCycleEnd))) {
        return DEFAULT_EXTEND_FULL_CYCLE_WHEN_NEW_CYCLE_END;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ExtendOperationPropagation))) {
        return DEFAULT_EXTEND_OPERATION_PROPAGATION;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.LineFromMachineOperation))) {
        return DEFAULT_LINE_FROM_MACHINE_OPERATION;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.LineManagement))) {
        return DEFAULT_LINE_MANAGEMENT;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.LinkOperationMaximumTime))) {
        return DEFAULT_LINK_OPERATION_MAXIMUM_TIME;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.MaxDaySlotProcess))) {
        return DEFAULT_MAX_DAY_SLOT_PROCESS;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.MaxRunningMachineThreads))) {
        return DEFAULT_MAX_RUNNING_MACHINE_THREADS;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.MaxShiftSlotProcess))) {
        return DEFAULT_MAX_SHIFT_SLOT_PROCESS;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.MaxThreadsInPool))) {
        return DEFAULT_MAX_THREADS_IN_POOL;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.MemoryPercentageExit))) {
        return DEFAULT_MEMORY_PERCENTAGE_EXIT;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.MemoryPercentageStopNewThreads))) {
        return DEFAULT_MEMORY_PERCENTAGE_STOP_NEW_THREADS;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.MinTemplateProcessDateTime))) {
        return DEFAULT_MIN_TEMPLATE_PROCESS_DATE_TIME;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ModificationStepTimeout))) {
        return DEFAULT_MODIFICATION_STEP_TIMEOUT;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ModificationTimeout))) {
        return DEFAULT_MODIFICATION_TIMEOUT;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ObsoleteTime))) {
        return DEFAULT_OBSOLETE_TIME;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationCycleAssociationMargin))) {
        return DEFAULT_OPERATIONCYCLE_ASSOCIATION_MARGIN;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationSlotProductionDuration))) {
        return DEFAULT_OPERATION_SLOT_PRODUCTION_DURATION;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationSlotRunTime))) {
        return DEFAULT_OPERATION_SLOT_RUN_TIME;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationSlotSplitOption))) {
        return DEFAULT_OPERATION_SLOT_SPLIT_OPTION;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.SequenceGapLimit))) {
        return DEFAULT_SEQUENCE_GAP_LIMIT;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.SplitCycleSummaryByShift))) {
        return DEFAULT_SPLIT_CYCLE_SUMMARY_BY_SHIFT;
      }
      else if (key.Equals (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ManufacturingOrderManagement))) {
        return DEFAULT_MANUFACTURING_ORDER_MANAGEMENT;
      }
      else {
        throw new Lemoine.Info.ConfigKeyNotFoundException (key);
      }
    }

    public T Get<T> (string key)
    {
      var v = Get (key);
      if (v is T) {
        return (T)v;
      }
      else {
        log.Error ($"Get: invalid type for {v} VS {typeof (T)}");
        throw new InvalidCastException ();
      }
    }

    public bool Initialize ()
    {
      return true;
    }
  }
}
