// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Model;
using System.Diagnostics;

namespace Lemoine.WebDataAccess
{
  class ComponentMachineAssociation: Lemoine.Model.IComponentMachineAssociation
  {
    #region Constructors
    internal ComponentMachineAssociation (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      this.Machine = machine;
      this.Range = range;
    }
    #endregion // Constructors

    public bool ActivityAnalysis {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public DateTime? AnalysisAppliedDateTime {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public DateTime? AnalysisBegin {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public long? AnalysisCompletionOrder {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public DateTime? AnalysisEnd {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public int AnalysisIterations {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public TimeSpan? AnalysisLastDuration {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public AnalysisStatus AnalysisStatus {
      get
      {
        throw new NotImplementedException ();
      }
    }

    public TimeSpan? AnalysisStepSpan {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public bool AnalysisSubGlobalModifications {
      get
      {
        throw new NotImplementedException ();
      }
    }

    public bool AnalysisSubMachineModifications {
      get
      {
        throw new NotImplementedException ();
      }
    }

    public bool AnalysisSubModifications {
      get
      {
        throw new NotImplementedException ();
      }
    }

    public TimeSpan AnalysisTotalDuration {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public bool Auto {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public LowerBound<DateTime> Begin {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public IComponent Component {
      get; set;
    }

    public DateTime DateTime {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public UpperBound<DateTime> End {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public long Id {
      get; set;
    }

    public DateTime LastAnalysisBegin {
      get
      {
        throw new NotImplementedException ();
      }
    }

    public IMachine Machine {
      get; set;
    }

    public AssociationOption? Option {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public IModification Parent {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public IGlobalModification ParentGlobal {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public IMachineModification ParentMachine {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public int Priority {
      get
      {
        throw new NotImplementedException ();
      }

      set
      {
        throw new NotImplementedException ();
      }
    }

    public UtcDateTimeRange Range {
      get; set;
    }

    public IRevision Revision {
      get; set;
    }

    public ICollection<IGlobalModification> SubGlobalModifications {
      get
      {
        throw new NotImplementedException ();
      }
    }

    public ICollection<IMachineModification> SubMachineModifications {
      get
      {
        throw new NotImplementedException ();
      }
    }

    public IEnumerable<IModification> SubModifications {
      get
      {
        throw new NotImplementedException ();
      }
    }

    public void Apply ()
    {
      throw new NotImplementedException ();
    }

    public void CheckStepTimeout ()
    {
      throw new NotImplementedException ();
    }

    public IList<IMachine> GetImpactedActivityAnalysis ()
    {
      throw new NotImplementedException ();
    }

    public bool IsMainModificationTransient ()
    {
      throw new NotImplementedException ();
    }

    public void MarkAllSubModificationsCompleted ()
    {
      throw new NotImplementedException ();
    }

    public void MarkAsConstraintIntegrityViolation ()
    {
      throw new NotImplementedException ();
    }

    public void MarkAsStepTimeout ()
    {
      throw new NotImplementedException ();
    }

    public void MarkAsTimeout (DateTime startDateTime)
    {
      throw new NotImplementedException ();
    }

    public void MarkAsDatabaseTimeout ()
    {
      throw new NotImplementedException ();
    }

    public void Unproxy ()
    {
      throw new NotImplementedException ();
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

    public string ModificationType
    {
      get { throw new NotImplementedException (); }
    }

    public AnalysisStatus? NextAnalysisStatus
    {
      get
      {
        throw new NotImplementedException ();
      }
    }

    public int StatusPriority { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
  }
}
