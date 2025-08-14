// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ProductionInformationDateTime
  /// </summary>
  [Serializable]
  public class ProductionInformation: MachineModification, IProductionInformation
  {
    #region Members
    DateTime m_informationDateTime;
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
      get { return "ProductionInformation"; }
    }

    /// <summary>
    /// Associated day
    /// </summary>
    [XmlIgnore]
    public virtual DateTime InformationDateTime
    {
      get { return m_informationDateTime; }
    }
    
    /// <summary>
    /// Information date/time in SQL string for XML serialization
    /// </summary>
    [XmlAttribute("Day")]
    public virtual string SqlInformationDateTime {
      get
      {
        return this.InformationDateTime.ToString("yyyy-MM-dd HH:mm:ss");
      }
      set
      {
        m_informationDateTime = System.DateTime.Parse (value);
      }
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
    /// At the end of the DateTime, was it the production in progress ?
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
    protected ProductionInformation ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="informationDateTime">in UTC</param>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="checkedValue"></param>
    internal protected ProductionInformation (IMachine machine,
                                              DateTime informationDateTime,
                                              IIntermediateWorkPiece intermediateWorkPiece,
                                              int checkedValue)
      : base (machine)
    {
      Debug.Assert (null != intermediateWorkPiece);
      
      m_informationDateTime = informationDateTime;
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
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      // Step 1: update operationslot
      
      // - Work order in progress
      if (this.InProgress.HasValue && this.InProgress.Value) {
        // - Compute the number of completed cycles
        int nbCycles = (this.Checked + this.Scrapped) / this.IntermediateWorkPiece.OperationQuantity;
        if (nbCycles < 1) {
          if (log.IsDebugEnabled) {
            log.Debug ($"MakeAnalysis: the number of cycles for the in progress production is {nbCycles} => no manufactuirng order is associated");
          }
        }
        else { // 1 <= nbCycles
          // - Get the begin of the period
          IList<IOperationCycle> cycles = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .GetLastFullCyclesBefore (this.Machine, this.InformationDateTime, nbCycles);
          if (cycles.Count < nbCycles) {
            string message = string.Format ("no {0} full cycles could be fetched before {1}",
                                            nbCycles, this.InformationDateTime);
            log.Error ($"MakeAnalysis: {message}");
            AddAnalysisLog (LogLevel.ERROR, message);
            MarkAsError ();
            return;
          }
          DateTime begin;
          IOperationCycle firstCycle = cycles[cycles.Count - 1];
          if (firstCycle.Begin.HasValue) {
            begin = firstCycle.Begin.Value;
          }
          else { // !cycleBegin.HasValue
            string message = string.Format ("cycle {0} has no begin => fallback to end",
                                            firstCycle);
            log.WarnFormat ("MakeAnalysis: " +
                            "{0}",
                            message);
            AddAnalysisLog (LogLevel.WARN, message);
            Debug.Assert (firstCycle.End.HasValue);
            if (firstCycle.End.HasValue) {
              begin = firstCycle.End.Value;
            }
            else {
              string message2 = string.Format ("cycle {0} has no end => unexpected",
                                              firstCycle);
              log.FatalFormat ("MakeAnalysis: " +
                               "{0}",
                               message2);
              AddAnalysisLog (LogLevel.CRIT, message2);
              MarkAsError ();
              return;
            }
          }
          // Note: begin of the period is known
          // - Check the existing operations
          UpperBound<DateTime> end = new UpperBound<DateTime> (null);

          // - Begin is known, apply the work order / manufacturing order

        }
      }
      
      // Step 2: update the number of scrap parts
      
      // TODO: ....
      MarkAsCompleted ("Cache/ClearDomainByMachine/ProductionInformation/" + this.Machine.Id + "?Broadcast=true");
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
      NHibernateHelper.Unproxy<IWorkOrder> (ref m_workOrder);
      NHibernateHelper.Unproxy<IIntermediateWorkPiece> (ref m_intermediateWorkPiece);
    }
    #endregion // Modification implementation
  }
}
