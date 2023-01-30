// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.CycleCsv
{
  public class OperationCycleDetectionExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , Lemoine.Extensions.Analysis.IOperationCycleDetectionExtension
    , Lemoine.Extensions.Database.Impl.TransactionNotifier.ITransactionListener
  {
    ILog log = LogManager.GetLogger (typeof (OperationCycleDetectionExtension).FullName);

    Configuration m_configuration = null;
    IMonitoredMachine m_machine;
    IOperationCycle m_operationCycle;
    readonly IList<string> m_header = new List<string> ();
    readonly IList<string> m_values = new List<string> ();
    volatile int m_threadId = 0; // Thread id, 0 means do not store it

    public void AfterCommit ()
    {
      if (null != m_operationCycle) {
        int currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        if (currentThreadId == m_threadId) {
          GenerateCsvFile ();
          Clear ();
        }
      }
    }

    public void AfterRollback ()
    {
      if (null != m_operationCycle) {
        int currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        if (currentThreadId == m_threadId) {
          Clear ();
        }
      }
    }

    public void BeforeCommit ()
    {
      if (null != m_operationCycle) {
        int currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        if (currentThreadId == m_threadId) {
          GetValues ();
        }
      }
    }

    public void CreateBetweenCycle (Lemoine.Model.IBetweenCycles betweenCycles)
    {
    }

    public void DetectionProcessComplete ()
    {
    }

    public void DetectionProcessError (Lemoine.Model.IMachineModule machineModule, Exception ex)
    {
    }

    public void DetectionProcessStart ()
    {
    }

    public bool Initialize (Lemoine.Model.IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      log = LogManager.GetLogger (typeof (OperationCycleDetectionExtension).FullName + "." + machine.Id);

      if (!LoadConfiguration (out m_configuration)) {
        log.ErrorFormat ("Initialize: " +
                         "the configuration is wrong, skip this instance");
        return false;
      }

      m_machine = machine;
      Lemoine.Extensions.Database.Impl.TransactionNotifier.TransactionNotifier.AddListener (this);
      return true;
    }

    public void StartCycle (Lemoine.Model.IOperationCycle operationCycle)
    {
    }

    public void StopCycle (Lemoine.Model.IOperationCycle operationCycle)
    {
      if (null != m_machine) {
        m_threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        m_operationCycle = operationCycle;
      }
    }

    void Clear ()
    {
      m_header.Clear ();
      m_values.Clear ();
      m_threadId = 0;
      m_operationCycle = null;
    }

    void GetValues ()
    {
      Debug.Assert (null != m_configuration);
      Debug.Assert (null != m_operationCycle);
      Debug.Assert (m_operationCycle.End.HasValue);

      if (0 < m_header.Count) { // Already set, do nothing
        return;
      }

      // - OperationCycle.Begin
      if (m_operationCycle.Begin.HasValue) {
        AddValue (m_configuration.OperationCycleUtcStartHeader,
          m_operationCycle.Begin.Value);
        AddValue (m_configuration.OperationCycleLocalStartHeader,
          m_operationCycle.Begin.Value.ToLocalTime ());
      }
      // - OperationCycle.End
      AddValue (m_configuration.OperationCycleUtcStopHeader,
        m_operationCycle.End.Value);
      AddValue (m_configuration.OperationCycleLocalStopHeader,
        m_operationCycle.End.Value.ToLocalTime ());

      // - Machine
      AddValue (m_configuration.MachineCodeHeader,
        m_machine.Code);
      AddValue (m_configuration.MachineDisplayHeader,
        m_machine.Display);
      AddValue (m_configuration.MachineExternalCodeHeader,
        m_machine.ExternalCode);
      AddValue (m_configuration.MachineNameHeader,
        m_machine.Name);

      if (null != m_operationCycle.OperationSlot) {
        // - Component
        if (null != m_operationCycle.OperationSlot.Component) {
          var component = m_operationCycle.OperationSlot.Component;
          AddValue (m_configuration.ComponentNameHeader,
            component.Name);
          AddValue (m_configuration.ComponentCodeHeader,
            component.Code);
        }
        // - Operation
        if (null != m_operationCycle.OperationSlot.Operation) {
          var operation = m_operationCycle.OperationSlot.Operation;
          AddValue (m_configuration.OperationCodeHeader,
            operation.Code);
          AddValue (m_configuration.OperationNameHeader,
            operation.Name);
        }
        // - Task
        if (null != m_operationCycle.OperationSlot.Task) {
          var task = m_operationCycle.OperationSlot.Task;
          AddValue (m_configuration.TaskExternalCodeHeader,
            task.ExternalCode);
          AddValue (m_configuration.TaskIdHeader,
            ((Lemoine.Collections.IDataWithId<int>)task).Id.ToString ());
        }
      }

      // Get the running %
      if (IsRunningTrack () && m_operationCycle.Begin.HasValue) {
        UtcDateTimeRange cycleRange = new UtcDateTimeRange (m_operationCycle.Begin.Value, m_operationCycle.End.Value);
        if (!cycleRange.IsEmpty ()) {
          IEnumerable<IReasonSlot> reasonSlots;
          using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (var transaction = session.BeginReadOnlyTransaction ()) {
              reasonSlots = ModelDAO.ModelDAOHelper.DAOFactory.ReasonSlotDAO
                .FindAllInUtcRangeWithMachineMode (m_machine, cycleRange);
            }
          }
          if (!reasonSlots.Any ()) {
            log.WarnFormat ("GetValues: no reason slots in range {0}",
              cycleRange);
          }
          else {
            var t = reasonSlots.First ().DateTimeRange.Upper;
            var reasonSlotsUpper = reasonSlots.Max (s => (Bound<DateTime>)s.DateTimeRange.Upper); // Because UpperBound<T> did not implement IComparable<UpperBound<T>>
            Debug.Assert (reasonSlotsUpper.HasValue);
            if (Bound.Compare<DateTime> (reasonSlotsUpper, cycleRange.Upper.Value.AddSeconds (-10)) <= 0) { // Ok to loose 10s
              log.WarnFormat ("GetValues: missing period {0}-{1} in reason slots",
                reasonSlotsUpper, m_operationCycle.End);
            }
            var runningDurationSeconds = reasonSlots
          .Where (s => s.MachineMode.Running.HasValue && s.MachineMode.Running.Value)
          .Select (s => s.DateTimeRange.Intersects (cycleRange))
          .Where (r => !r.IsEmpty ())
          .Select (r => new UtcDateTimeRange (r).Duration.Value.TotalSeconds)
          .Sum ();
            AddValue (m_configuration.RunningDurationHeader,
              ((int)runningDurationSeconds).ToString ());
            var totalDurationSeconds = cycleRange.Duration.Value.TotalSeconds;
            var notRunningDurationSeconds = totalDurationSeconds - runningDurationSeconds;
            AddValue (m_configuration.NotRunningDurationHeader,
              ((int)notRunningDurationSeconds).ToString ());
          }
        }
      }

      return;
    }

    bool IsRunningTrack ()
    {
      return !string.IsNullOrEmpty (m_configuration.RunningDurationHeader)
        || !string.IsNullOrEmpty (m_configuration.NotRunningDurationHeader);
    }

    void AddValue (string header, DateTime v)
    {
      AddValue (header, v.ToString ("yyyy-MM-dd HH:mm:ss"));
    }

    void AddValue (string header, string v)
    {
      if (string.IsNullOrEmpty (header)) {
        log.ErrorFormat ("AddValue: header {0} is null or empty", header);
      }
      else {
        if (null == v) {
          log.FatalFormat ("AddValue: null v is unexpected");
          Debug.Assert (false);
        }
        else {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("AddValue: {0}={1}", header, v);
          }
          m_header.Add (header);
          m_values.Add (v);
        }
      }
    }

    string GetContent ()
    {
      string header = string.Join (m_configuration.Separator, m_header.ToArray ());
      string values = string.Join (m_configuration.Separator, m_values.ToArray ());
      return header + "\n" + values;
    }

    void GenerateCsvFile ()
    {
      Debug.Assert (null != m_operationCycle);
      Debug.Assert (m_operationCycle.End.HasValue);

      string fileName = string.Format ("PulseCycle-M{0}-{1}.csv",
        m_machine.Id,
        m_operationCycle.End.Value.ToString ("yyyy-MM-ddTHH-mm-ss"));
      string filePath = Path.Combine (m_configuration.Path, fileName);
      File.WriteAllText (filePath, GetContent ());
    }
  }
}
