// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Analysis;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Extensions.Analysis.StateMachine;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lem_ActivityAnalysis.Console
{
  /// <summary>
  /// ConsoleRunner
  /// </summary>
  public class ConsoleRunner : IConsoleRunner<Options>
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConsoleRunner).FullName);

    readonly IApplicationInitializer m_applicationInitializer;
    bool m_global;
    int m_machineId;
    int m_sleepTime;
    Options m_options;

    /// <summary>
    /// Constructor
    /// </summary>
    public ConsoleRunner (IApplicationInitializer applicationInitializer)
    {
      m_applicationInitializer = applicationInitializer;
    }

    public void SetOptions (Options options)
    {
      m_global = options.Global;
      m_machineId = options.MachineId;
      m_sleepTime = options.SleepTime;
      m_options = options;
    }

    public async Task RunConsoleAsync (CancellationToken cancellationToken = default)
    {
      await m_applicationInitializer.InitializeApplicationAsync (cancellationToken);

      if (cancellationToken.IsCancellationRequested) {
        return;
      }

      Lemoine.GDBPersistentClasses.DaySlotCache.Activate ();

      if (cancellationToken.IsCancellationRequested) {
        return;
      }

      Lemoine.GDBPersistentClasses.DaySlotCache.Activate ();

      Action<string> printStep = (s) => { if (m_options.Verbose) { System.Console.WriteLine (s); } };

      if (m_options.TurnOnCatchUpMode) {
        printStep ("Turn on the catch-up mode in the analysis service");
        Lemoine.Analysis.SwitchCatchUpMode.TurnOn ();
        if (int.MinValue == m_machineId) {
          log.Debug ($"RunConsoleAsync: no machine was set, nothing else to do");
          return;
        }
      }

      // machine initialization
      IMachine machine = null;
      IMonitoredMachine monitoredMachine = null;
      if (!m_global) {
        if (int.MinValue == m_machineId) {
          printStep ($"-g or -i is not set, please set one of them");
          log.Error ($"RunConsoleAsync: -g or -i was not set");
          return;
        }
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindByIdWithMachineModules (m_machineId);
          if (null != monitoredMachine) {
            ModelDAOHelper.DAOFactory.Initialize (monitoredMachine.MachineModules);
            machine = monitoredMachine;
          }
          else {
            machine = ModelDAOHelper.DAOFactory.MachineDAO
              .FindById (m_machineId);
          }
        }
        if (null == machine) {
          printStep ($"Invalid machine id {m_machineId}");
          log.Error ($"Invalid machine with id {m_machineId}");
          return;
        }
      } // !global, machine initialization

      if (m_options.StateMachine) {
        if (m_global) {
          log.Info ("RunConsoleAsync: run the global analysis");
          printStep ($"Global state machine analysis");
          RunGlobalAnalysis (m_sleepTime, m_options.Loop);
        }
        else {
          log.Info ($"RunConsoleAsync: run the analysis for machine {m_machineId}");
          printStep ($"State machine analysis of machine {m_machineId}");
          if (null != monitoredMachine) {
            RunMonitoredMachineAnalysis (monitoredMachine, m_sleepTime, m_options.Loop);
          }
          else if (null != machine) {
            RunNotMonitoredMachineAnalysis (machine, m_sleepTime, m_options.Loop);
          }
        }
      } // StateMachine
      else {
        if (m_options.CatchUp) {
          if (m_options.Global) {
            m_options.ModificationAnalysis = true;
            m_options.DayTemplateAnalysis = true;
            m_options.ShiftTemplateAnalysis = true;
            m_options.WeekNumberAnalysis = true;
            m_options.CleanFlaggedModifications = true;
          }
          else if (null != machine) {
            m_options.MachineStateTemplateAnalysis = true;
            m_options.OperationSlotSplitAnalysis = true;
            m_options.ProductionAnalysis = true;
            m_options.ModificationAnalysis = true;
            m_options.ActivityAnalysis = -1;
            m_options.ProcessingReasonSlotsAnalysis = -1;
            m_options.DetectionAnalysis = -1;
            m_options.AutoSequenceAnalysis = -1;
            m_options.CleanFlaggedModifications = true;
          }
        }

        printStep ($"Initialization");
        IGlobalAnalysis globalContext = null;
        if (m_options.Global) {
          globalContext = new GlobalAnalysis (null);
          globalContext.Initialize ();
          globalContext.FirstRun = true;
        }
        IMachineActivityAnalysis machineContext = null;
        if (null != machine) {
          machineContext = new MachineActivityAnalysis (machine);
          machineContext.Initialize ();
        }
        IMonitoredMachineActivityAnalysis monitoredMachineContext = null;
        if (null != monitoredMachine) {
          monitoredMachineContext = new MonitoredMachineActivityAnalysis (monitoredMachine);
          monitoredMachineContext.Initialize ();
        }
        TimeSpan maxTime = TimeSpan.FromDays (7);
        while (true) {
          if (log.IsDebugEnabled) {
            log.Debug ($"RunConsoleAsync: run one more time the loop");
          }
          if (m_options.MachineStateTemplateAnalysis && (null != machineContext)) {
            printStep ($"Machine state template analysis");
            machineContext.ManageMachineStateTemplates (CancellationToken.None, maxTime, maxTime);
          }
          if (m_options.OperationSlotSplitAnalysis && (null != machineContext)) {
            printStep ($"Operation slot split analysis");
            machineContext.RunOperationSlotSplitAnalysis (CancellationToken.None, maxTime, maxTime, TimeSpan.FromDays (31));
          }
          if (m_options.ProductionAnalysis && (null != machineContext)) {
            printStep ($"Production analysis");
            machineContext.RunProductionAnalysis (CancellationToken.None, maxTime, maxTime);
          }
          if ((0 != m_options.ActivityAnalysis) && (null != monitoredMachineContext)) {
            var factNumber = 0 < m_options.ActivityAnalysis
            ? m_options.ActivityAnalysis
            : int.MaxValue;
            //Lemoine.Info.ConfigSet.ForceValue ("Analysis.Activity.Facts.MaxNumber", factNumber);
            printStep ($"Activity analysis");
            monitoredMachineContext.RunActivityAnalysis (CancellationToken.None, factNumber);
          }
          if (m_options.ModificationAnalysis) {
            printStep ($"Modification analysis");
            if (null != globalContext) {
              globalContext.RunPendingModificationsAnalysis (CancellationToken.None, maxTime, 0, 0);
              if (log.IsDebugEnabled) {
                log.Debug ("RunConsoleAsync: RunPendingModificationAnalysis context=global completed");
              }
            }
            if (null != monitoredMachineContext) {
              monitoredMachineContext.RunPendingModificationsAnalysis (CancellationToken.None, maxTime, maxTime, 0, 0);
              if (log.IsDebugEnabled) {
                log.Debug ("RunConsoleAsync: RunPendingModificationAnalysis context=monitored completed");
              }
            }
            else if (null != machineContext) {
              machineContext.RunPendingModificationsAnalysis (CancellationToken.None, maxTime, maxTime, 0, 0);
              if (log.IsDebugEnabled) {
                log.Debug ("RunConsoleAsync: RunPendingModificationAnalysis context=machine completed");
              }
            }
          }
          if ((0 != m_options.ProcessingReasonSlotsAnalysis) && (null != monitoredMachineContext)) {
            printStep ($"Processing reason slot analysis");
            var maxLoopNumber = 0 < m_options.ProcessingReasonSlotsAnalysis
            ? m_options.ProcessingReasonSlotsAnalysis
            : int.MaxValue;
            //Lemoine.Info.ConfigSet.ForceValue ("Analysis.ProcessingReasonSlots.MaxLoopNumber", maxLoopNumber);
            monitoredMachineContext.RunProcessingReasonSlotsAnalysis (CancellationToken.None, maxTime, maxTime, maxLoopNumber: maxLoopNumber);
          }
          if ((0 != m_options.DetectionAnalysis) && (null != monitoredMachineContext)) {
            printStep ("Detection analysis");
            var maxDetectionNumber = 0 < m_options.DetectionAnalysis
            ? m_options.DetectionAnalysis
            : int.MaxValue;
            // Lemoine.Info.ConfigSet.ForceValue ("Analysis.Activity.Detections.MaxNumber", maxDetectionNumber);
            monitoredMachineContext.RunDetectionAnalysis (CancellationToken.None, maxTime, maxTime, maxDetectionNumber);
          }
          if ((0 != m_options.AutoSequenceAnalysis) && (null != monitoredMachineContext)) {
            printStep ("Auto-sequence analysis");
            var number = 0 < m_options.AutoSequenceAnalysis
            ? m_options.AutoSequenceAnalysis
            : int.MaxValue;
            //Lemoine.Info.ConfigSet.ForceValue ("Analysis.Activity.AutoSequences.MaxNumber", number);
            monitoredMachineContext.RunAutoSequenceAnalysis (CancellationToken.None, maxTime, maxTime, number);
          }
          if (m_options.ShiftTemplateAnalysis && (null != globalContext)) {
            printStep ("Global shift template analysis");
            globalContext.ManageShiftTemplates (CancellationToken.None, maxTime);
          }
          if (m_options.DayTemplateAnalysis && (null != globalContext)) {
            printStep ("Day template analysis");
            globalContext.ManageDayTemplates (CancellationToken.None, maxTime);
          }
          if (m_options.WeekNumberAnalysis && (null != globalContext)) {
            printStep ("Week number analysis");
            globalContext.ManageWeekNumbers (CancellationToken.None);
          }
          if (m_options.CleanFlaggedModifications) {
            printStep ("Clean flagged modifications");
            if (null != globalContext) {
              globalContext.CleanFlaggedModifications (CancellationToken.None, maxTime);
            }
            if (null != monitoredMachineContext) {
              monitoredMachineContext.CleanFlaggedModifications (CancellationToken.None, maxTime, maxTime);
            }
            else if (null != machineContext) {
              machineContext.CleanFlaggedModifications (CancellationToken.None, maxTime, maxTime);
            }
          }
          printStep ("One pass completed");
          if (!m_options.Loop) {
            if (log.IsDebugEnabled) {
              log.Debug ($"RunConsoleAsync: option loop is not set => return");
            }
            Environment.ExitCode = 0;
            return;
          }

          Thread.Sleep (m_sleepTime);
        }
      }
    }

    // disable once FunctionNeverReturns
    void RunGlobalAnalysis (int sleepTime, bool loop)
    {
      GlobalAnalysis analysis = new GlobalAnalysis (null);
      while (true) {
        Debug.Assert (analysis.CanRun ());
        analysis.RunDirectly (CancellationToken.None);
        if (!loop) {
          return;
        }
        Thread.Sleep (sleepTime);
      }
    }

    // disable once FunctionNeverReturns
    void RunMonitoredMachineAnalysis (IMonitoredMachine monitoredMachine, int sleepTime, bool loop)
    {
      var analysis = new MonitoredMachineActivityAnalysis (monitoredMachine);
      while (true) {
        Debug.Assert (analysis.CanRun ());
        analysis.RunDirectly (CancellationToken.None);
        if (!loop) {
          return;
        }
        Thread.Sleep (sleepTime);
      }
    }

    // disable once FunctionNeverReturns
    void RunNotMonitoredMachineAnalysis (IMachine machine, int sleepTime, bool loop)
    {
      var analysis = new MachineActivityAnalysis (machine);
      while (true) {
        Debug.Assert (analysis.CanRun ());
        analysis.RunDirectly (CancellationToken.None);
        if (!loop) {
          return;
        }
        Thread.Sleep (sleepTime);
      }
    }
  }
}
