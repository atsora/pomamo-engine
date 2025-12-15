// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of MachineStateTemplateMachineAssociation.
  /// </summary>
  public class MachineStateTemplateAssociation: IMachineStateTemplateAssociation
  {
    #region Constructors
    internal MachineStateTemplateAssociation (IMachine machine, IMachineStateTemplate machineStateTemplate, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);
      
      this.Machine = machine;
      this.Range = range;
      this.MachineStateTemplate = machineStateTemplate;
    }
    #endregion // Constructors

    #region IModification implementation
    public string ModificationType
    {
      get { throw new NotImplementedException (); }
    }
    public IMachineStateTemplateAssociation Clone (UtcDateTimeRange range)
    {
      throw new NotImplementedException ();
    }
    public void Apply()
    {
      throw new NotImplementedException();
    }
    public System.Collections.Generic.IList<Lemoine.Model.IMachine> GetImpactedActivityAnalysis()
    {
      throw new NotImplementedException();
    }
    public void MarkAsTimeout (DateTime startDateTime)
    {
      throw new NotImplementedException ();
    }

    public void MarkAsDatabaseTimeout ()
    {
      throw new NotImplementedException ();
    }

    public void MarkAsStepTimeout()
    {
      throw new NotImplementedException();
    }
    public void MarkAsConstraintIntegrityViolation()
    {
      throw new NotImplementedException();
    }
    public void MarkAllSubModificationsCompleted()
    {
      throw new NotImplementedException();
    }
    public bool IsMainModificationTransient()
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IRevision Revision {
      get; set;
    }
    public Lemoine.Model.IModification Parent {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public Lemoine.Model.IGlobalModification ParentGlobal {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public Lemoine.Model.IMachineModification ParentMachine {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public bool ActivityAnalysis {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public System.Collections.Generic.ICollection<Lemoine.Model.IGlobalModification> SubGlobalModifications {
      get {
        throw new NotImplementedException();
      }
    }
    public System.Collections.Generic.ICollection<Lemoine.Model.IMachineModification> SubMachineModifications {
      get {
        throw new NotImplementedException();
      }
    }
    public System.Collections.Generic.IEnumerable<Lemoine.Model.IModification> SubModifications {
      get {
        throw new NotImplementedException();
      }
    }
    public DateTime DateTime {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public int Priority {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public Lemoine.Model.AnalysisStatus AnalysisStatus {
      get {
        throw new NotImplementedException();
      }
    }
    public DateTime? AnalysisAppliedDateTime {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public bool Auto {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public DateTime? AnalysisBegin {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public DateTime? AnalysisEnd {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public DateTime LastAnalysisBegin {
      get {
        throw new NotImplementedException();
      }
    }
    public int AnalysisIterations {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public TimeSpan AnalysisTotalDuration {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public TimeSpan? AnalysisLastDuration {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public long? AnalysisCompletionOrder {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public TimeSpan? AnalysisStepSpan {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public bool AnalysisSubModifications {
      get {
        throw new NotImplementedException();
      }
    }
    public bool AnalysisSubGlobalModifications {
      get {
        throw new NotImplementedException();
      }
    }
    public bool AnalysisSubMachineModifications {
      get {
        throw new NotImplementedException();
      }
    }

    /// <summary>
    /// Check if the step timeout is reached.
    /// 
    /// If it is, raise the StepTimeoutException
    /// 
    /// It must be called by the main modification
    /// </summary>
    /// <returns></returns>
    public virtual void CheckStepTimeout ()
    {
      throw new NotImplementedException();
    }
    #endregion
    #region ISerializableModel implementation
    public void Unproxy()
    {
      throw new NotImplementedException();
    }

    public void Cancel ()
    {
      throw new NotImplementedException ();
    }

    public void MarkAsParentInError (bool cancel)
    {
      throw new NotImplementedException ();
    }

    public string GetTransactionNameSuffix ()
    {
      throw new NotImplementedException ();
    }
    #endregion
    #region IMachineStateTemplateMachineAssociation implementation
    public Lemoine.Model.IMachineStateTemplate MachineStateTemplate {
      get; set;
    }
    public Lemoine.Model.AssociationOption? Option {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public IUser User {
      get; set;
    }
    public IShift Shift {
      get; set;
    }
    public bool Force {
      get; set;
    }
    #endregion
    #region IPeriodAssociation implementation
    public Lemoine.Model.LowerBound<DateTime> Begin {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public Lemoine.Model.UpperBound<DateTime> End {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public Lemoine.Model.UtcDateTimeRange Range {
      get; set;
    }
    #endregion
    #region IPartitionedByMachine implementation
    public Lemoine.Model.IMachine Machine {
      get; set;
    }
    #endregion
    #region IDataWithId implementation
    public long Id {
      get; set;
    }

    public AnalysisStatus? NextAnalysisStatus
    {
      get
      {
        throw new NotImplementedException ();
      }
    }

    public int StatusPriority { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
    #endregion
  }
}
