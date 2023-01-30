// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Model;

using Lemoine.Database.Persistent;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Threading;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ProductionInformationShift
  /// </summary>
  [Serializable]
  public class ProductionInformationShift: MachineModification, IProductionInformationShift
  {
    #region Members
    DateTime m_day;
    IShift m_shift;
    IWorkOrder m_workOrder;
    IIntermediateWorkPiece m_intermediateWorkPiece;
    int m_checked = 0;
    int m_scrapped = 0;
    bool? m_inProgress = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "ProductionInformationShift"; }
    }

    /// <summary>
    /// Associated day
    /// </summary>
    [XmlIgnore]
    public virtual DateTime Day
    {
      get { return m_day; }
    }
    
    /// <summary>
    /// Day in SQL string for XML serialization
    /// </summary>
    [XmlAttribute("Day")]
    public virtual string SqlDay {
      get
      {
        return this.Day.ToString("yyyy-MM-dd");
      }
      set
      {
        m_day = System.DateTime.Parse (value);
      }
    }

    /// <summary>
    /// Associated shift
    /// 
    /// not null
    /// </summary>
    [XmlIgnore]
    public virtual IShift Shift
    {
      get { return m_shift; }
    }
    
    /// <summary>
    /// Reference to the shift for XML serialization
    /// </summary>
    [XmlElement("Shift")]
    public virtual Shift XmlSerializationShift {
      get { return this.Shift as Shift; }
      set { m_shift = value; }
    }
    
    /// <summary>
    /// Associated work order
    /// 
    /// nullable
    /// </summary>
    [XmlIgnore]
    public virtual IWorkOrder WorkOrder
    {
      get { return m_workOrder; }
      set { m_workOrder = value; }
    }
    
    /// <summary>
    /// Reference to the work order for XML serialization
    /// </summary>
    [XmlElement("WorkOrder")]
    public virtual WorkOrder XmlWorkOrder {
      get { return this.WorkOrder as WorkOrder; }
      set { this.WorkOrder = value; }
    }
    
    /// <summary>
    /// Associated intermediate work piece
    /// 
    /// not null
    /// </summary>
    [XmlIgnore]
    public virtual IIntermediateWorkPiece IntermediateWorkPiece
    {
      get { return m_intermediateWorkPiece; }
    }
    
    /// <summary>
    /// Reference to the intermediate work piece for XML serialization
    /// </summary>
    [XmlElement("IntermediateWorkPiece")]
    public virtual IntermediateWorkPiece XmlIntermediateWorkPiece {
      get { return this.IntermediateWorkPiece as IntermediateWorkPiece; }
      set { m_intermediateWorkPiece = value; }
    }
    
    /// <summary>
    /// Number of produced pieces
    /// </summary>
    [XmlAttribute("Checked")]
    public virtual int Checked
    {
      get { return m_checked; }
      set { m_checked = value; }
    }
    
    /// <summary>
    /// Number of scrapped parts
    /// 
    /// Default: 0
    /// </summary>
    [XmlAttribute("Scrapped")]
    public virtual int Scrapped
    {
      get { return m_scrapped; }
      set { m_scrapped = value; }
    }
    
    /// <summary>
    /// At the end of the shift, was it the production in progress ?
    /// </summary>
    [XmlIgnore]
    public virtual bool? InProgress
    {
      get { return m_inProgress; }
      set { m_inProgress = value; }
    }
    
    /// <summary>
    /// InProgress for XML serialization
    /// </summary>
    [XmlAttribute("InProgress")]
    public virtual string XmlInProgress {
      get
      {
        if (!this.InProgress.HasValue) {
          return null;
        }
        else {
          return this.InProgress.Value.ToString();
        }
      }
      set
      {
        if (string.IsNullOrEmpty (value)) {
          this.InProgress = null;
        }
        else {
          this.InProgress = bool.Parse (value);
        }
      }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected ProductionInformationShift ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day"></param>
    /// <param name="shift">not null</param>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="checkedValue"></param>
    internal protected ProductionInformationShift (IMachine machine,
                                                   DateTime day,
                                                   IShift shift,
                                                   IIntermediateWorkPiece intermediateWorkPiece,
                                                   int checkedValue)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != shift);
      Debug.Assert (null != intermediateWorkPiece);
      
      this.Machine = machine;
      m_day = day;
      m_shift = shift;
      m_intermediateWorkPiece = intermediateWorkPiece;
      m_checked = checkedValue;
    }
    #endregion // Constructors
    
    #region Modification implementation
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      // TODO: cancellationToken
      var cancellationToken = CancellationToken.None;

      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      // Get the end date/time of the period
      DateTime? end = null;
      IList<IObservationStateSlot> observationStateSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
        .FindAllInDayRange (this.Machine, new DayRange (m_day, m_day));
      foreach (IObservationStateSlot observationStateSlot in observationStateSlots) {
        if (null == observationStateSlot.MachineObservationState) {
          if (null == observationStateSlot.MachineStateTemplate) {
            string message = string.Format ("Observation state slot {0} with no machine state template " +
                                            "and no machine observation state",
                                            observationStateSlot);
            log.ErrorFormat ("ProductionInformationShift: " +
                             "{0}",
                             message);
            AddAnalysisLog (LogLevel.ERROR, message);
            MarkAsError ();
            return;
          }
          log.InfoFormat ("ProductionInformationShift: " +
                          "process the machine state template {0}",
                          observationStateSlot.MachineStateTemplate);
          IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindProcessedByDay (m_day);
          if (null == daySlot) { // give up
            string message = string.Format ("No day slot at {0}",
                                            m_day);
            log.ErrorFormat ("ProductionInformationShift: " +
                             "{0}",
                             message);
            AddAnalysisLog (LogLevel.ERROR, message);
            MarkAsError ();
            return;
          }
          ((ObservationStateSlot)observationStateSlot).ProcessTemplate (cancellationToken, daySlot.DateTimeRange,
                                                                        this,
                                                                        false,
                                                                        this.Caller,
                                                                        null);
          MarkAsInProgress (null);
          return;
        }
        if (object.Equals (m_shift, observationStateSlot.Shift)) {
          if (!observationStateSlot.EndDateTime.HasValue) {
            string message = string.Format ("No observation state slot at {0} for shift {1} has no end",
                                            observationStateSlot.BeginDateTime, observationStateSlot.Shift);
            log.ErrorFormat ("ProductionInformationShift: " +
                             "{0}",
                             message);
            AddAnalysisLog (LogLevel.ERROR, message);
            MarkAsError ();
            return;
          }
          if (Bound.Compare<DateTime> (this.DateTime,
                                       observationStateSlot.DateTimeRange.Upper) < 0) {
            string message = string.Format ("Observation state slot end {0} is after the modification end date/time " +
                                            "=> the data can not be reliable",
                                            observationStateSlot.EndDateTime);
            log.ErrorFormat ("ProductionInformationShift: " +
                             "{0}",
                             message);
            AddAnalysisLog (LogLevel.ERROR, message);
            MarkAsError ();
            return;
          }
          Debug.Assert (observationStateSlot.EndDateTime.HasValue);
          end = observationStateSlot.EndDateTime.Value;
        }
      }
      
      if (!end.HasValue) {
        string message = string.Format ("No period for {0} {1} was detected",
                                        this.Day, this.Shift);
        log.ErrorFormat ("ProductionInformationShift: " +
                         "{0}",
                         message);
        AddAnalysisLog (LogLevel.ERROR, message);
        MarkAsError ();
        return;
      }
      
      // Give the work to ProductionInformation
      IProductionInformation subModification = new ProductionInformation (this.Machine,
                                                                          end.Value,
                                                                          this.IntermediateWorkPiece,
                                                                          this.Checked);
      subModification.WorkOrder = this.WorkOrder;
      subModification.Scrapped = this.Scrapped;
      subModification.InProgress = this.InProgress;
      ModelDAOHelper.DAOFactory.ProductionInformationDAO.MakePersistent (subModification);
      subModification.Parent = this.MainModification ?? this;
      subModification.Priority = this.StatusPriority;
      // Analysis is done
      MarkAsCompleted ("");
      return;
    }
    
    /// <summary>
    /// Apply the modification while keeping it transient
    /// 
    /// It should be never called, because there is no transient modification to process.
    /// Use a persistent entity instead and MakeAnalysis.
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (false);
      log.FatalFormat ("Apply: not implemented/supported");
      throw new NotImplementedException ();
    }
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IShift> (ref m_shift);
      NHibernateHelper.Unproxy<IWorkOrder> (ref m_workOrder);
      NHibernateHelper.Unproxy<IIntermediateWorkPiece> (ref m_intermediateWorkPiece);
    }
    #endregion // Modification implementation
  }
}
