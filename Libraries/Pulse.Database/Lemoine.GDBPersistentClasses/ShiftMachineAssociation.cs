// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Analysis class for the ShiftChange modification table that targets only one machine
  /// </summary>
  public class ShiftMachineAssociation: MachineAssociation, IShiftMachineAssociation // Note: internal because it is used in Lemoine.Analysis
    // Note: public else it is not serializable for the alert service
  {
    #region Members
    DateTime? m_day;
    IShift m_shift;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected ShiftMachineAssociation ()
    {
      m_activityAnalysis = true;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="range">UTC begin date/time</param>
    public ShiftMachineAssociation (IMachine machine,
                                    DateTime? day,
                                    IShift shift,
                                    UtcDateTimeRange range) // Note: public because it is used in Lemoine.Analysis
      : base (machine, range)
    {
      m_activityAnalysis = true;
      m_day = day;
      m_shift = shift;
    }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="range">UTC begin date/time</param>
    /// <param name="mainModification"></param>
    internal protected ShiftMachineAssociation (IMachine machine,
                                                DateTime? day,
                                                IShift shift,
                                                UtcDateTimeRange range,
                                                IModification mainModification)
      : base (machine, range, mainModification)
    {
      m_activityAnalysis = true;
      m_day = day;
      m_shift = shift;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    public override string ModificationType
    {
      get { return "ShiftMachineAssociation"; }
    }

    /// <summary>
    /// Reference to the day
    /// </summary>
    public virtual DateTime? Day {
      get { return m_day; }
    }
    
    /// <summary>
    /// Reference to the Shift
    /// </summary>
    public virtual IShift Shift {
      get { return m_shift; }
    }
    #endregion // Getters / Setters
    
    #region MachineAssociation implementation
    /// <summary>
    /// <see cref="PeriodAssociation.ConvertToSlot" />
    /// </summary>
    /// <returns></returns>
    public override TSlot ConvertToSlot<TSlot>()
    {
      if (typeof (TSlot).Equals (typeof (OperationSlot))) {
        IOperationSlot operationSlot = ModelDAO.ModelDAOHelper.ModelFactory
          .CreateOperationSlot (this.Machine,
                                null,
                                null,
                                null,
                                null,
                                null,
                                this.Day,
                                this.Shift,
                                this.Range);
        Debug.Assert (operationSlot.IsEmpty ()); // A shiftchange does not create a new OperationSlot
        return (TSlot) operationSlot;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        throw new NotImplementedException ("Slot type not implemented");
      }
    }
    
    /// <summary>
    /// <see cref="PeriodAssociation.MergeDataWithOldSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    /// <param name="range">merge period range</param>
    /// <returns></returns>
    public override TSlot MergeDataWithOldSlot<TSlot>(TSlot oldSlot,
                                                      UtcDateTimeRange range)
    {
      Debug.Assert (null != oldSlot);
      // disable once CSharpWarnings::CS0183
      Debug.Assert (oldSlot is Slot);
      Debug.Assert (this.Machine.Equals ((oldSlot as IPartitionedByMachine).Machine));
      
      if (oldSlot is IOperationSlot) {
        IOperationSlot oldOperationSlot = oldSlot as IOperationSlot;

        IOperationSlot newOperationSlot = (IOperationSlot) oldOperationSlot.Clone ();
        Debug.Assert (object.Equals (newOperationSlot.WorkOrder, oldOperationSlot.WorkOrder));
        Debug.Assert (object.Equals (newOperationSlot.Task, oldOperationSlot.Task));
        Debug.Assert (object.Equals (newOperationSlot.Operation, oldOperationSlot.Operation));
        ((OperationSlot)newOperationSlot).Day = this.Day;
        ((OperationSlot)newOperationSlot).Shift = this.Shift;

        return (TSlot) newOperationSlot;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        log.FatalFormat ("MergeData: " +
                         "trying to merge the association with a not supported slot {0}",
                         typeof (TSlot));
        throw new ArgumentException ("Not supported machine slot");
      }
    }
    #endregion // MachineAssociation implementation
    
    #region Modification implementation
    /// <summary>
    /// Get the impacted activity analysis
    /// so that the activity analysis makes a pause
    /// </summary>
    /// <returns></returns>
    public override IList<IMachine> GetImpactedActivityAnalysis ()
    {
      IList<IMachine> list = new List<IMachine> ();
      
      if (AnalysisConfigHelper.OperationSlotSplitOption.IsActive ()) {
        list.Add (this.Machine);
      }
      
      return list;
    }
    
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsAnalysisCompleted ());
      
      // Note: for the moment ShiftMachineAssociation impacts only OperationSlot
      //       This is then wise to skip any process if OperationSlotSplitOption is not active
      if (!AnalysisConfigHelper.OperationSlotSplitOption.IsActive ()) {
        log.Debug ("MakeAnalysis: OperationSlotSplitOption is not active => mark as completed");
        MarkAsCompleted ("");
        return;
      }
      
      if ((null != this.Shift)
          && !this.Day.HasValue) { // Create one sub-modification by day, for each shift
        Debug.Assert (this.Range.Upper.HasValue);
        
        // Limit the range to operationSlotSplit.End
        UtcDateTimeRange correctedRange = this.Range;
        IOperationSlotSplit operationSlotSplit = ModelDAOHelper.DAOFactory.OperationSlotSplitDAO
          .FindById (this.Machine.Id);
        if ( (null != operationSlotSplit)
            && (Bound.Compare<DateTime> (operationSlotSplit.End, correctedRange.Upper) < 0)) {
          correctedRange = new UtcDateTimeRange (correctedRange.Lower, operationSlotSplit.End);
          log.Debug ($"MakeAnalysis: no day set, the corrected range is {correctedRange}");
        }
        
        SetActive ();
        IList<IDaySlot> daySlots = ModelDAOHelper.DAOFactory.DaySlotDAO
          .FindProcessedInRange (correctedRange);
        foreach (IDaySlot daySlot in daySlots) {
          SetActive ();
          Debug.Assert (daySlot.Day.HasValue);
          if (!daySlot.Day.HasValue) {
            log.Error ($"MakeAnalysis: in daySlot {daySlot} the day is not defined");
            continue;
          }
          UtcDateTimeRange intersection = new UtcDateTimeRange (correctedRange.Intersects (daySlot.DateTimeRange));
          Debug.Assert (!intersection.IsEmpty ());
          ShiftMachineAssociation association = new ShiftMachineAssociation (this.Machine,
                                                                             daySlot.Day.Value,
                                                                             this.Shift,
                                                                             intersection);
          association.Auto = this.Auto;
          association.Parent = this.MainModification ?? this;
          association.Priority = this.StatusPriority;
          (new ShiftMachineAssociationDAO ()).MakePersistent (association);
        }
        MarkAsCompleted ("");
        return;
      }
      
      Debug.Assert (this.Day.HasValue || (null == this.Shift));
      {
        // Get the adjusted step range
        UtcDateTimeRange range = GetNotAppliedRange ();

        // Analyze ! with the adjusted begin and end
        ShiftMachineAssociation association = new ShiftMachineAssociation (this.Machine,
                                                                           this.Day,
                                                                           this.Shift,
                                                                           range,
                                                                           this);
        association.DateTime = this.DateTime;
        association.Caller = this;
        association.ProcessAssociation ();
        
        // Analysis is done
        MarkAsCompleted ("Cache/ClearDomainByMachine/ShiftAssociation/" + this.Machine.Id + "?Broadcast=true",
                         (DateTime?)range.Upper); // => InProgress or Done
      }
    }
    
    /// <summary>
    /// Apply the modification while keeping it transient
    /// </summary>
    public override void Apply ()
    {
      if (!this.Day.HasValue && !(this.Shift is null)) {
        log.Error ($"Apply: case with no day and a not null shift is not supported");
        throw new NotImplementedException ("ShiftMachineAssociation direct apply without day not implemented");
      }
      ProcessAssociation ();
    }

    /// <summary>
    /// Process the association
    /// </summary>
    public virtual void ProcessAssociation ()
    {
      // Update operation slot
      // if the option is selected...
      if (AnalysisConfigHelper.OperationSlotSplitOption.IsActive ()) {
        OperationSlotDAO operationSlotDAO = new OperationSlotDAO ();
        operationSlotDAO.Caller = this;
        this.Insert<OperationSlot, IOperationSlot, OperationSlotDAO> (operationSlotDAO);
      }
    }
    #endregion // Modification implementation
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      NHibernateHelper.Unproxy<IShift> (ref m_shift);
      base.Unproxy ();
    }
  }
}
