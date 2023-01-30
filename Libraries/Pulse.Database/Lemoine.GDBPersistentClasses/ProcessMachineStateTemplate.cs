// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Lemoine.Info;
using Lemoine.Model;

using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using System.Threading;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ProcessMachineStateTemplate
  /// </summary>
  [Serializable]
  public class ProcessMachineStateTemplate
    : MachineModification
    , IProcessMachineStateTemplate
  {
    static readonly string DEFAULT_MINIMUM_STEP_SPAN_KEY = "Analysis.StepSpan.ProcessMachineStateTemplate.DefaultMinimum";
    static readonly TimeSpan DEFAULT_MINIMUM_STEP_SPAN_DEFAULT = TimeSpan.FromHours (1);
    
    static readonly string DEFAULT_STEP_SPAN_NO_END_KEY = "Analysis.StepSpan.NoEnd";
    static readonly TimeSpan DEFAULT_STEP_SPAN_NO_END_DEFAULT = TimeSpan.FromDays (1);

    static readonly string STEP_TIMEOUT_EXCEPTION_DELTA_KEY = "Analysis.StepSpan.ProcessMachineStateTemplate.StepTimeoutExceptionDelta";
    static readonly TimeSpan STEP_TIMEOUT_EXCEPTION_DELTA_DEFAULT = TimeSpan.FromSeconds (10);
    
    #region Members
    UtcDateTimeRange m_range;
    bool m_lowerStepSpanPossible = true;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "ProcessMachineStateTemplate"; }
    }

    /// <summary>
    /// Range
    /// </summary>
    [XmlIgnore]
    public virtual UtcDateTimeRange Range
    {
      get { return this.m_range; }
    }

    /// <summary>
    /// Range for Xml serialization
    /// </summary>
    [XmlAttribute("Range")]
    public virtual string XmlSerializationRange
    {
      get
      {
        if (null == this.m_range) {
          log.WarnFormat ("XmlSerializationRange.get: " +
                          "range null");
          return "";
        }
        else {
          return this.m_range.ToString (dt => dt.ToString ("yyyy-MM-dd HH:mm:ss"));
        }
      }
      set
      {
        m_range = new UtcDateTimeRange (value);
      }
    }
    #endregion // Getters / Setters
    
    /// <summary>
    /// Forbidden constructor
    /// </summary>
    protected ProcessMachineStateTemplate ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range">not null</param>
    internal ProcessMachineStateTemplate (IMachine machine, UtcDateTimeRange range)
      : base (machine)
    {
      Debug.Assert (null != range);
      
      m_range = range;
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range">not null</param>
    /// <param name="mainModification"></param>
    internal ProcessMachineStateTemplate (IMachine machine, UtcDateTimeRange range, IModification mainModification)
      : base (machine, mainModification)
    {
      Debug.Assert (null != range);

      m_range = range;
    }
    
    #region Modification implementation
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      Debug.Assert (null != this.Range);
      
      if (Range.IsEmpty ()) {
        log.DebugFormat ("MakeAnalysis: " +
                         "empty range => nothing to do");
        MarkAsCompleted ("");
        return;
      }
      
      // Get the adjusted step range
      UtcDateTimeRange range = GetStepRange ();
      log.DebugFormat ("MakeAnalysis: " +
                       "consider step {0}",
                       range);
      if (range.IsEmpty ()) {
        log.DebugFormat ("MakeAnalysis: " +
                         "step range is empty => completed");
        MarkAsCompleted ("");
        return;
      }

      // Analyze ! with the adjusted begin and end
      var association = new ProcessMachineStateTemplate (this.Machine,
                                                         range,
                                                         this);
      association.DateTime = this.DateTime;
      association.Caller = this;
      association.Analyze ();
      
      Debug.Assert (!range.IsEmpty ());
      var noProcessedSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
        .FindWithNoMachineObservationState (this.Machine,
                                            new UtcDateTimeRange (range.Lower,
                                                                  this.Range.Upper));
      if (noProcessedSlots.Any ()) {
        var lowerDateTime = noProcessedSlots.First ().BeginDateTime;
        if (Bound.Compare<DateTime> (lowerDateTime, range.Lower) <= 0) {
          log.ErrorFormat ("MakeAnalysis: " +
                           "process of range {0} has not done anything",
                           range);
          Debug.Assert (false);
          MarkAsError ();
        }
        else {
          Debug.Assert (lowerDateTime.HasValue);
          MarkAsCompleted ("Cache/ClearDomainByMachine/ProcessMachineStateTemplate/" + this.Machine.Id + "?Broadcast=true",
                            (DateTime?)lowerDateTime.Value);
        }
      }
      else {
        MarkAsCompleted ("Cache/ClearDomainByMachine/ProcessMachineStateTemplate/" + this.Machine.Id + "?Broadcast=true");
      }
    }
    
    /// <summary>
    /// Apply the modifications
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (false);
      log.FatalFormat ("Apply: not implemented/supported");
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Process the association itself,
    /// without updating any Modification property
    /// </summary>
    /// <returns>completed</returns>
    bool Analyze ()
    {
      // TODO: cancellationToken
      Debug.Assert (null != this.Range);

      if (this.Range.IsEmpty ()) {
        log.DebugFormat ("Analyze: " +
                         "empty range => nothing to do");
        return true;
      }
      
      DateTime maxAnalysisDateTime = this.LastAnalysisBegin.Add (AnalysisConfigHelper.ModificationStepTimeout);
      
      var slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
        .FindWithNoMachineObservationState (this.Machine, this.Range);
      foreach (var slot in slots) {
        if (false == ((ObservationStateSlot)slot).ProcessTemplate (CancellationToken.None, this.Range,
                                                                   this.MainModification ?? this,
                                                                   false,
                                                                   this,
                                                                   maxAnalysisDateTime)) {
          log.WarnFormat ("Analyze: " +
                          "max analysis date/time {0} is reached and not everything was completed",
                          maxAnalysisDateTime);
          return false;
        }
      }
      
      return true;
    }
    #endregion // Modification implementation
    
    #region Step utility methods
    /// <summary>
    /// Is a lower step span possible ?
    /// </summary>
    protected virtual bool IsLowerStepSpanPossible ()
    {
      return m_lowerStepSpanPossible;
    }

    /// <summary>
    /// Check if the step timeout is reached.
    /// 
    /// If it is, raise the StepTimeoutException
    /// 
    /// It must be called by the main modification
    /// </summary>
    /// <returns></returns>
    public override void CheckStepTimeout ()
    {
      // Use a delta to give a chance to complete at least some data without raising an exception
      TimeSpan delta = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (STEP_TIMEOUT_EXCEPTION_DELTA_KEY,
                               STEP_TIMEOUT_EXCEPTION_DELTA_DEFAULT);
      CheckStepTimeout (delta);
    }

    /// <summary>
    /// Check if the step timeout is reached.
    /// 
    /// If it is, raise the StepTimeoutException
    /// </summary>
    /// <returns></returns>
    public virtual void CheckStepTimeout (TimeSpan delta)
    {
      if (IsStepTimeoutReached (delta)) {
        log.InfoFormat ("CheckStepTimeout: " +
                        "delta={0} step timeout is reached",
                        delta);
        throw new StepTimeoutException ();
      }
    }
    
    /// <summary>
    /// Check if the step timeout is reached.
    /// </summary>
    /// <returns></returns>
    public virtual bool IsStepTimeoutReached ()
    {
      return IsStepTimeoutReached (TimeSpan.FromTicks (0));
    }
    
    /// <summary>
    /// Check if the step timeout is reached.
    /// </summary>
    /// <param name="delta"></param>
    /// <returns></returns>
    public virtual bool IsStepTimeoutReached (TimeSpan delta)
    {
      if (IsStepActive (this.Range)
          && IsLowerStepSpanPossible ()
          && (this.LastAnalysisBegin.Add (AnalysisConfigHelper.ModificationStepTimeout.Add (delta)) < DateTime.UtcNow)) {
        TimeSpan minimumStepSpan = this.GetMinimumStepSpan ();
        if (this.AnalysisStepSpan.HasValue && (this.AnalysisStepSpan.Value <= minimumStepSpan)) {
          // Second check here just in case. The call to IsLowerStepSpanPossible is normally sufficient
          log.DebugFormat ("IsStepTimeoutReached: " +
                           "modification step timeout {0} is reached, " +
                           "but the analysis step span is already less than the minimum {1} " +
                           "=> return false",
                           AnalysisConfigHelper.ModificationStepTimeout, minimumStepSpan);
        }
        else {
          log.ErrorFormat ("IsStepTimeoutReached: " +
                           "elapsed time is {0} while modification step time out is {1}" +
                           "=> return true",
                           DateTime.UtcNow.Subtract (this.LastAnalysisBegin), AnalysisConfigHelper.ModificationStepTimeout);
          return true;
        }
      }
      
      return false;
    }
    
    /// <summary>
    /// Get the minimum step span
    /// </summary>
    /// <returns></returns>
    protected virtual TimeSpan GetMinimumStepSpan ()
    {
      return ConfigSet.LoadAndGet<TimeSpan> (DEFAULT_MINIMUM_STEP_SPAN_KEY, DEFAULT_MINIMUM_STEP_SPAN_DEFAULT); // By default: 30s
    }
    
    /// <summary>
    /// Check if the step process should be active or not for the specified range
    /// 
    /// By default, it is not active in the future or when the main modification is transient
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected virtual bool IsStepActive (UtcDateTimeRange range)
    {
      return !IsMainModificationTransient ()
        && (Bound.Compare<DateTime> (range.Lower, DateTime.UtcNow) < 0);
    }
    
    /// <summary>
    /// Compute a new step span after a step timeout
    /// 
    /// By default, the current step span is divided by 2
    /// or one day is considered in case end is null.
    /// 
    /// In case it needs to be overriden, override
    /// ComputeNewStepSpan(DateTime begin, DateTime? end)
    /// instead
    /// </summary>
    /// <returns></returns>
    protected override TimeSpan? ComputeNewStepSpan ()
    {
      UtcDateTimeRange range = GetStepDefaultRange ();
      return ComputeNewStepSpan (range);
    }
    
    /// <summary>
    /// Compute a new step span after a step timeout
    /// 
    /// By default, the current step span is divided by 2
    /// or one day is considered in case end is null.
    /// 
    /// But this can be overridden by the child classes
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected virtual TimeSpan? ComputeNewStepSpan (UtcDateTimeRange range)
    {
      if (this.AnalysisStepSpan.HasValue) {
        return TimeSpan.FromSeconds (TimeSpan.FromTicks (this.AnalysisStepSpan.Value.Ticks / 2).TotalSeconds);
      }
      else if (range.Duration.HasValue) {
        return TimeSpan.FromSeconds (TimeSpan.FromTicks (range.Duration.Value.Ticks / 2).TotalSeconds);
      }
      else { // No AnalysisStepSpan and no end
        return ConfigSet.LoadAndGet<TimeSpan> (DEFAULT_STEP_SPAN_NO_END_KEY, DEFAULT_STEP_SPAN_NO_END_DEFAULT); // 1 day by default
      }
    }
    
    /// <summary>
    /// Get the default step range after a step time out but before adjusting it with the step span
    /// </summary>
    /// <returns></returns>
    protected virtual UtcDateTimeRange GetStepDefaultRange ()
    {
      return new UtcDateTimeRange (this.AnalysisAppliedDateTime.HasValue
                                   ? this.AnalysisAppliedDateTime.Value
                                   : this.Range.Lower,
                                   this.Range.Upper);
    }
    
    /// <summary>
    /// Get the adjusted step range from the already applied date/time, the analysis status and the step span
    /// </summary>
    /// <returns></returns>
    protected virtual UtcDateTimeRange GetStepRange ()
    {
      // - Default applicable range
      UtcDateTimeRange range = GetStepDefaultRange ();
      m_lowerStepSpanPossible = true;

      // - Get a new step span in case of Step timeout
      if (AnalysisStatus.StepTimeout == this.AnalysisStatus) {
        TimeSpan? newStepSpan = ComputeNewStepSpan (range);
        if (newStepSpan.HasValue
            && (!this.AnalysisStepSpan.HasValue || (newStepSpan < this.AnalysisStepSpan.Value))) {
          log.DebugFormat ("GetStepRange: " +
                           "step span is going from {0} to {1}",
                           this.AnalysisStepSpan, newStepSpan);
          this.AnalysisStepSpan = newStepSpan.Value;
          ModelDAOHelper.DAOFactory.ModificationDAO.MakePersistent (this);
        }
        else {
          log.DebugFormat ("GetStepRange: " +
                           "no better step span could be computed");
          m_lowerStepSpanPossible = false;
        }
        log.DebugFormat ("GetStepRange: " +
                         "the new step span is {0}",
                         this.AnalysisStepSpan);
      }
      
      // - Check the minimum step span has not been already reached
      //   This is necessary to check it here because the analysis status may be InProgress
      if (m_lowerStepSpanPossible && this.AnalysisStepSpan.HasValue) {
        TimeSpan minimumStepSpan = GetMinimumStepSpan ();
        if (this.AnalysisStepSpan.Value <= minimumStepSpan) {
          log.InfoFormat ("GetStepRange: " +
                          "minimum step range {0} reached",
                          minimumStepSpan);
          if (this.AnalysisStepSpan.Value < minimumStepSpan) {
            this.AnalysisStepSpan = minimumStepSpan;
            ModelDAOHelper.DAOFactory.ModificationDAO.MakePersistent (this);
          }
          m_lowerStepSpanPossible = false;
        }
      }
      
      // - adjust the new end from the step span
      if (range.Lower.HasValue && IsStepActive (range) && this.AnalysisStepSpan.HasValue) {
        DateTime stepEnd = range.Lower.Value.Add (this.AnalysisStepSpan.Value);
        if (Bound.Compare<DateTime> (stepEnd, range.Upper) < 0) {
          range = new UtcDateTimeRange (range.Lower, stepEnd);
          log.DebugFormat ("GetStepRange: " +
                           "the adjusted end from step is {0}",
                           stepEnd);
        }
      }
      
      return range;
    }
    
    /// <summary>
    /// Mark the modification as completed (Done) or partially completed (InProgress)
    /// </summary>
    /// <param name="message">Message to send to the web service (nullable)</param>
    /// <param name="effectiveEnd"></param>
    protected override void MarkAsCompleted (string message, DateTime? effectiveEnd)
    {
      if (IsStepTimeoutReached ()) {
        // Compute a new step timeout
        // This step timeout was not small enough
        TimeSpan? newStepSpan = ComputeNewStepSpan ();
        if (newStepSpan.HasValue) {
          // Check the new value is smaller
          Debug.Assert (!this.AnalysisStepSpan.HasValue || (newStepSpan.Value < this.AnalysisStepSpan.Value));
          
          log.DebugFormat ("MarkAsStepTimeout: " +
                           "adjust the analysis step span from {0} to {1}",
                           this.AnalysisStepSpan, newStepSpan);
          this.AnalysisStepSpan = newStepSpan;
        }
      }
      
      if (effectiveEnd.HasValue && (Bound.Compare<DateTime> (effectiveEnd.Value, this.Range.Upper) < 0)) { // Partially done... continue later
        log.DebugFormat ("MakeAsCompleted: " +
                         "step completed, but the period {0}-{1} still needs to be processed",
                         effectiveEnd, this.Range.Upper);
        base.MarkAsInProgress (effectiveEnd);
      }
      else { // Completed
        log.DebugFormat ("MakeAsCompleted: " +
                         "completed !");
        base.MarkAsCompleted (message);
      }
    }
    #endregion // Step utilities
  }
}
