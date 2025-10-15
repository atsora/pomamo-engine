// Copyright (C) 2009-2023 Lemoine Automation Technologies, 2023 Nicolas Relange
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using System.Xml.Serialization;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Extensions to OperationSlotCompatibility
  /// </summary>
  internal static class OperationSlotCompatibilityExtensions
  {
    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this OperationCycle.OperationSlotCompatibility t, OperationCycle.OperationSlotCompatibility other)
    {
      return other == (t & other);
    }
  }


  /// <summary>
  /// Persistent class of table OperationCycle
  /// </summary>
  [Serializable]
  public class OperationCycle : IOperationCycle, ICloneable
  {
    /// <summary>
    /// Disposable class to call only once before the change AnalysisAccumulator.RemoveCycle
    /// and only once after the change AnalysisAccumulator.AddCycle
    /// </summary>
    internal class ChangeTracker : IDisposable
    {
      readonly OperationCycle m_operationCycle;
      readonly OperationCycle m_previous;
      readonly bool m_active;

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="operationCycle">not null</param>
      public ChangeTracker (OperationCycle operationCycle)
      {
        if (!ModelDAOHelper.DAOFactory.IsInitialized (operationCycle)) {
          log.ErrorFormat ("ChangeTracker: operationCycle is not initialized. StackTrace={0}",
            System.Environment.StackTrace);
        }

        Debug.Assert (null != operationCycle);
        if (!operationCycle.m_changeTrackerActive) {
          operationCycle.m_changeTrackerActive = true;
          m_operationCycle = operationCycle;
          if ((0 != operationCycle.Id) && !operationCycle.m_deleted) {
            m_previous = (OperationCycle)operationCycle.Clone ();
          }
          else {
            m_previous = null; // Transient
          }
          m_active = true;
        }
      }

      /// <summary>
      /// Implements IDisposable
      /// </summary>
      public void Dispose ()
      {
        if (m_active) {
          if ((0 != m_operationCycle.Id) && !m_operationCycle.m_deleted) {
            if (m_previous is null) {
              using (var analysisAccumulatorCallerHolder2 = new AnalysisAccumulatorCallerHolder (m_operationCycle)) {
                AnalysisAccumulator.OperationCycleUpdated (null,
                                                           m_operationCycle);
              }
            }
            else { // m_previous is not null 
              using (var analysisAccumulatorCallerHolder1 = new AnalysisAccumulatorCallerHolder (m_previous)) {
                using (var analysisAccumulatorCallerHolder2 = new AnalysisAccumulatorCallerHolder (m_operationCycle)) {
                  AnalysisAccumulator.OperationCycleUpdated (m_previous,
                                                             m_operationCycle);
                }
              }
            }
          }
          else if (null != m_previous) {
            using (var analysisAccumulatorCallerHolder = new AnalysisAccumulatorCallerHolder (m_previous)) {
              AnalysisAccumulator.OperationCycleUpdated (m_previous,
                                                         null);
            }
          }
          m_operationCycle.m_changeTrackerActive = false;
        }
      }
    }

    int m_id = 0;
    int m_version = 0;
    IMachine m_machine;
    DateTime? m_begin;
    DateTime? m_end;
#pragma warning disable 169
    DateTime m_dateTime; // Note: this is only used by the mapping
#pragma warning restore 169
    double? m_offsetDuration;
    IOperationSlot m_operationSlot;
    OperationCycleStatus m_status = 0;
    bool m_full = false;
    int? m_quantity = null;

    bool m_deleted = false;
    bool m_changeTrackerActive = false;

    Lemoine.Threading.IChecked m_caller = null;

    static readonly ILog log = LogManager.GetLogger (typeof (OperationCycle).FullName);

    #region Getters / Setters
    /// <summary>
    /// OperationCycle Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// OperationCycle Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Reference to the Machine
    /// </summary>
    public virtual IMachine Machine
    {
      get { return m_machine; }
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IOperationCycle.Begin">IOperationCycle.Begin</see>
    /// </summary>
    public virtual DateTime? Begin
    {
      get { return m_begin; }
      set {
        if (!object.Equals (m_begin, value)) {
          if (value.HasValue && End.HasValue && (value.Value.CompareTo (End.Value) > 0)) {
            log.ErrorFormat ("Cycle begin set to {0} > cycle end {1}",
                            value.Value, End.Value);
          }
          else {
            m_begin = value;
          }
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IOperationCycle.End">IOperationCycle.End</see>
    /// 
    /// Note: use SetRealEnd or SetEstimatedEnd to set the End
    /// </summary>
    public virtual DateTime? End
    {
      get { return m_end; }
      protected set {
        if (!object.Equals (m_end, value)) {
          if (value.HasValue && Begin.HasValue && (value.Value.CompareTo (Begin.Value) < 0)) {
            log.ErrorFormat ("Cycle end set to {0} < cycle begin {1}",
                            value.Value, Begin.Value);
          }
          else {
            using (ChangeTracker changeTracker = new ChangeTracker (this)) {
              m_end = value;
            }
          }
        }
      }
    }

    /// <summary>
    /// Date/time of the operation cycle to use
    /// to sort the operation cycles
    /// 
    /// This is:
    /// <item>End in case of a full cycle</item>
    /// <item>Begin in case of a partial cycle</item>
    /// </summary>
    public virtual DateTime DateTime
    {
      get {
        if (m_end.HasValue && !this.Status.HasFlag (OperationCycleStatus.EndEstimated)) {
          return m_end.Value;
        }
        else if (m_end.HasValue && this.Full) {
          return m_end.Value;
        }
        else if (m_begin.HasValue && !this.Status.HasFlag (OperationCycleStatus.BeginEstimated)) {
          return m_begin.Value;
        }
        else if (m_end.HasValue) { // estimated
          log.Warn ("DateTime.get: both date/time are estimated");
          return m_end.Value;
        }
        else if (m_begin.HasValue) { // estimated
          log.Warn ("DateTime.get: both date/time are estimated");
          return m_begin.Value;
        }
        else {
          log.Fatal ("DateTime.get: no date/time, should not happen");
          throw new InvalidOperationException ();
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IOperationCycle.OperationSlot">IOperationCycle.OperationSlot</see>
    /// </summary>
    public virtual IOperationSlot OperationSlot
    {
      get { return m_operationSlot; }
      set {
        if (object.Equals (m_operationSlot, value)) {
          // Nothing to do
          return;
        }
        else {
          using (ChangeTracker changeTracker = new ChangeTracker (this)) {
            m_operationSlot = value;
          }
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IOperationCycle.OperationSlot">IOperationCycle.OffsetDuration</see>
    /// </summary>
    public virtual Double? OffsetDuration
    {
      get { return m_offsetDuration; }
      set {
        if (object.Equals (m_offsetDuration, value)) {
          // Nothing to do
          return;
        }
        else {
          using (ChangeTracker changeTracker = new ChangeTracker (this)) {
            m_offsetDuration = value;
          }
        }
      }
    }

    /// <summary>
    /// Status of the bounds (is begin or end estimated)
    /// </summary>
    public virtual OperationCycleStatus Status
    {
      get { return m_status; }
      set { m_status = value; }
    }

    /// <summary>
    /// Can this operation cycle be considered as a full cycle,
    /// meaning it made some parts
    /// </summary>
    public virtual bool Full
    {
      get { return m_full; }
      set {
        if (object.Equals (m_full, value)) {
          // Nothing to do
          return;
        }
        else {
          using (ChangeTracker changeTracker = new ChangeTracker (this)) {
            m_full = value;
          }
        }
      }
    }

    /// <summary>
    /// Quantity
    /// 
    /// null: consider the default quantity
    /// </summary>
    public virtual int? Quantity
    {
      get { return m_quantity; }
      set {
        if (object.Equals (m_quantity, value)) {
          // Nothing to do
          return;
        }
        else {
          using (ChangeTracker changeTracker = new ChangeTracker (this)) {
            m_quantity = value;
          }
        }
      }
    }
    #endregion // Getters / Setters

    #region IChecked implementation
    /// <summary>
    /// <see cref="Lemoine.Threading.ICheckedCaller"/>
    /// </summary>
    /// <param name="caller"></param>
    public virtual void SetCheckedCaller (Lemoine.Threading.IChecked caller)
    {
      m_caller = caller;
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked">IChecked implementation</see>
    /// </summary>
    public virtual void SetActive ()
    {
      if (null != m_caller) {
        m_caller.SetActive ();
        m_operationSlot?.SetActive ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public virtual void PauseCheck ()
    {
      if (null != m_caller) {
        m_caller.PauseCheck ();
        m_operationSlot?.PauseCheck ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public virtual void ResumeCheck ()
    {
      if (null != m_caller) {
        m_caller.ResumeCheck ();
        m_operationSlot?.ResumeCheck ();
      }
    }
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    internal OperationCycle (IMachine machine)
    {
      m_machine = machine;
    }

    /// <summary>
    /// Protected constructor for NHibernate
    /// </summary>
    internal protected OperationCycle ()
    {
    }

    #region Members
    /// <summary>
    /// Has the operation cycle a real begin (defined and not estimated) ?
    /// </summary>
    /// <returns></returns>
    public virtual bool HasRealBegin ()
    {
      return this.Begin.HasValue && !this.Status.HasFlag (OperationCycleStatus.BeginEstimated);
    }

    /// <summary>
    /// Has the operation cycle a real end (defined and not estimated) ?
    /// </summary>
    /// <returns></returns>
    public virtual bool HasRealEnd ()
    {
      return this.End.HasValue && !this.Status.HasFlag (OperationCycleStatus.EndEstimated);
    }

    /// <summary>
    /// Set the operation cycle as persistent
    /// </summary>
    internal protected virtual void SetPersistent ()
    {
      m_deleted = false;
    }

    /// <summary>
    /// Set the operation cycle as transient
    /// </summary>
    internal protected virtual void SetTransient ()
    {
      m_deleted = true;
    }

    /// <summary>
    /// Operation slot compatibility
    /// 
    /// Returned by IsCompatibleWithOperationSlot
    /// </summary>
    [Flags]
    public enum OperationSlotCompatibility
    {
      /// <summary>
      /// No compatibility at all
      /// </summary>
      None = 0,
      /// <summary>
      /// The operation slot is compatible (Full or AdjustEstimatedBegin or AdjustEstimatedEnd)
      /// </summary>
      Compatible = 1,
      /// <summary>
      /// Adjust some estimated date/times
      /// </summary>
      AdjustEstimated = 2,
      /// <summary>
      /// Splitting the operation cycle is required
      /// </summary>
      Split = 4,
      /// <summary>
      /// The (real) begin of the operation cycle matches the operation slot
      /// </summary>
      BeginMatch = 8,
      /// <summary>
      /// The (real) end of the operation cycle matches the operation slot
      /// </summary>
      EndMatch = 16,
      /// <summary>
      /// Full compatibility, no change is required
      /// </summary>
      Full = Compatible | BeginMatch | EndMatch,
      /// <summary>
      /// Only the (real) begin matches the operation slot period
      /// The operation cycle may need to be split
      /// </summary>
      BeginOnly = Split | BeginMatch,
      /// <summary>
      /// Only the (real) end matches the operation slot period
      /// The operation cycle may need to be split
      /// </summary>
      EndOnly = Split | EndMatch,
      /// <summary>
      /// The end (real) is compatible with the operation slot,
      /// but the estimated begin needs to be adjusted
      /// </summary>
      AdjustEstimatedBegin = Compatible | AdjustEstimated | EndMatch,
      /// <summary>
      /// The begin (real) is compatible with the operation slot,
      /// but the estimated end needs to be adjusted
      /// </summary>
      AdjustEstimatedEnd = Compatible | AdjustEstimated | BeginMatch,
    };

    /// <summary>
    /// Check if the operation cycle may be associated with the specified operation slot
    /// </summary>
    /// <param name="operationSlot">not null</param>
    /// <returns></returns>
    public virtual OperationSlotCompatibility IsCompatibleWithOperationSlot (IOperationSlot operationSlot)
    {
      Debug.Assert (null != operationSlot);

      if (this.Machine.Id != operationSlot.Machine.Id) {
        log.ErrorFormat ("IsCompatibleWithOperationSlot: " +
                         "not the same machine");
        return OperationSlotCompatibility.None;
      }

      if (this.End.HasValue
          && !this.Status.HasFlag (OperationCycleStatus.EndEstimated)) { // real end
        if (this.Begin.HasValue
            && !this.Status.HasFlag (OperationCycleStatus.BeginEstimated)) { // real begin
          UtcDateTimeRange operationCycleRange = new UtcDateTimeRange (this.Begin.Value,
                                                                       this.End.Value);
          if (operationSlot.DateTimeRange.ContainsRange (operationCycleRange)) {
            return OperationSlotCompatibility.Full;
          }
          else {
            if (operationSlot.DateTimeRange.ContainsElement (this.End.Value)
              || Bound.Equals (operationSlot.DateTimeRange.Upper, this.End.Value)) {
              UtcDateTimeRange extendedOperationSlotRange = operationSlot.DateTimeRange;
              if (operationSlot.DateTimeRange.Lower.HasValue) {
                var operationCycleAssociationMargin = AnalysisConfigHelper.OperationCycleAssociationMargin;
                extendedOperationSlotRange =
                  new UtcDateTimeRange (operationSlot.DateTimeRange.Lower.Value.Subtract (operationCycleAssociationMargin),
                                        operationSlot.DateTimeRange.Upper);
              }
              if (extendedOperationSlotRange.ContainsElement (this.Begin.Value)) {
                return OperationSlotCompatibility.Full;
              }
              else {
                // Check if this is a same operation
                using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
                  bool isContinousOperation = ModelDAOHelper.DAOFactory.OperationSlotDAO
                    .IsContinuousOperationInRange (this.Machine,
                                                   new UtcDateTimeRange (this.Begin.Value,
                                                                         operationSlot.DateTimeRange.Lower.Value),
                                                   operationSlot.Operation);
                  return isContinousOperation
                    ? OperationSlotCompatibility.Full
                    : OperationSlotCompatibility.EndOnly;
                }
              }
            }
            else if (operationSlot.DateTimeRange.ContainsElement (this.Begin.Value)) {
              UtcDateTimeRange restrictedOperationSlotRange = operationSlot.DateTimeRange;
              if (operationSlot.DateTimeRange.Upper.HasValue) {
                var operationCycleAssociationMargin = AnalysisConfigHelper.OperationCycleAssociationMargin;
                restrictedOperationSlotRange =
                  new UtcDateTimeRange (operationSlot.DateTimeRange.Lower,
                                        operationSlot.DateTimeRange.Upper.Value.Subtract (operationCycleAssociationMargin));
              }
              if (restrictedOperationSlotRange.ContainsElement (this.Begin.Value)) {
                // Check if this is a same operation
                using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
                  bool isContinousOperation = ModelDAOHelper.DAOFactory.OperationSlotDAO
                    .IsContinuousOperationInRange (this.Machine,
                                                   new UtcDateTimeRange (operationSlot.DateTimeRange.Upper.Value,
                                                                         this.End.Value),
                                                   operationSlot.Operation);
                  return isContinousOperation
                    ? OperationSlotCompatibility.Full
                    : OperationSlotCompatibility.BeginOnly;
                }
              }
              else {
                return OperationSlotCompatibility.None;
              }
            }
            else {
              return OperationSlotCompatibility.None;
            }
          }
        }
        else { // no real begin
          if (operationSlot.DateTimeRange.ContainsElement (this.End.Value)) {
            return OperationSlotCompatibility.AdjustEstimatedBegin;
          }
          else if (Bound.Equals (operationSlot.DateTimeRange.Upper, this.End.Value)) {
            return OperationSlotCompatibility.AdjustEstimatedBegin;
          }
          else {
            return OperationSlotCompatibility.None;
          }
        }
      }
      else { // no real end
        if (this.Begin.HasValue
            && !this.Status.HasFlag (OperationCycleStatus.BeginEstimated)) { // real begin
          if (operationSlot.DateTimeRange.ContainsElement (this.Begin.Value)) {
            return OperationSlotCompatibility.AdjustEstimatedEnd;
          }
          else {
            return OperationSlotCompatibility.None;
          }
        }
        else { // Nor real begin, nor real end
          log.FatalFormat ("IsCompatibleWithOperationSlot: " +
                           "no real date/time in operation cycle");
          Debug.Assert (false);
          return OperationSlotCompatibility.None;
        }
      }
    }

    /// <summary>
    /// Set the real begin of a cycle
    /// </summary>
    /// <param name="dateTime"></param>
    public virtual void SetRealBegin (DateTime dateTime)
    {
      if (this.Status.HasFlag (OperationCycleStatus.BeginEstimated)
          || !this.Begin.HasValue
          || !this.Begin.Value.Equals (dateTime)) {
        this.Status = this.Status.Remove (OperationCycleStatus.BeginEstimated);
        this.Begin = dateTime;
      }
    }

    /// <summary>
    /// Set the end of a full cycle
    /// </summary>
    /// <param name="dateTime"></param>
    public virtual void SetRealEnd (DateTime dateTime)
    {
      if (this.Status.HasFlag (OperationCycleStatus.EndEstimated)
          || !this.End.HasValue
          || !this.End.Value.Equals (dateTime)) {
        if (Begin.HasValue && (dateTime.CompareTo (Begin.Value) < 0)) {
          log.ErrorFormat ("SetRealEnd: Cycle end set to {0} < cycle begin {1}",
                          dateTime, Begin.Value);
          return;
        }

        using (ChangeTracker changeTracker = new ChangeTracker (this)) { // because the day depends on the end date/time
          this.Status = this.Status.Remove (OperationCycleStatus.EndEstimated);
          this.End = dateTime;
          this.Full = true;
        } // End using change tracker
      }
    }

    /// <summary>
    /// set end and flag as estimated end
    /// </summary>
    /// <param name="dateTime"></param>
    public virtual void SetEstimatedEnd (DateTime? dateTime)
    {
      if (!this.Status.HasFlag (OperationCycleStatus.EndEstimated)
          || !object.Equals (m_end, dateTime)) {
        if (Begin.HasValue && dateTime.HasValue && (dateTime.Value.CompareTo (Begin.Value) < 0)) {
          log.ErrorFormat ("SetEstimatedEnd:" +
                          "Cycle end set to {0} < cycle begin {1}",
                          dateTime, Begin.Value);
          return;
        }

        using (ChangeTracker changeTracker = new ChangeTracker (this)) { // because the day depends on the end date/time
          this.End = dateTime;
          this.Status = this.Status.Add (OperationCycleStatus.EndEstimated);
        } // End using change tracker
      }
    }

    /// <summary>
    /// set begin and flag as estimated begin
    /// </summary>
    /// <param name="dateTime"></param>
    public virtual void SetEstimatedBegin (DateTime dateTime)
    {
      this.Begin = dateTime; // The offset duration is re-computed there
      this.Status = this.Status.Add (OperationCycleStatus.BeginEstimated);
    }
    #endregion // Members

    #region ICloneable interface
    /// <summary>
    /// Make a shallow copy
    /// <see cref="ICloneable.Clone" />
    /// </summary>
    /// <returns></returns>
    public virtual object Clone ()
    {
      object clone = this.MemberwiseClone ();
      var cloneCycle = clone as OperationCycle;
      cloneCycle.m_id = 0;
      cloneCycle.m_version = 0;
      return clone;
    }
    #endregion // ICloneable

    #region Methods
    /// <summary>
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[OperationCycle {this.Id} {this.Machine?.ToStringIfInitialized ()} Begin={this.Begin} End={this.End} Status={this.Status}]";
      }
      else {
        return $"[OperationCycle Id={this.Id}]";
      }
    }

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IOperationCycle other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      OperationCycle other = obj as OperationCycle;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode ();
        }
        return hashCode;
      }
      else {
        return base.GetHashCode ();
      }
    }
    #endregion // Methods
  }
}
