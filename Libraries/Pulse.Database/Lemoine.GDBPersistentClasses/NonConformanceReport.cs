// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;
using Lemoine.Extensions.Database;
using Pulse.Extensions.Database;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table NonConformanceReport
  /// 
  /// This table tracks the nonconformance reason detected
  /// on deliverable piece during operation
  /// 
  /// </summary>
  [Serializable]
  public class NonConformanceReport: MachineModification, INonConformanceReport
  {
    #region Members
    IDeliverablePiece m_deliverablePiece;
    IIntermediateWorkPiece m_intermediateWorkPiece;
    INonConformanceReason m_nonConformanceReason;
    bool? m_fixable;
    DateTime? m_operationDateTime;
    string m_nonConformanceDetails;
    #endregion // Members

    #region constructor
    /// <summary>
    /// Default Constructor
    /// </summary>
    protected NonConformanceReport()
    {
    }
    
    /// <summary>
    ///   Constructor
    /// </summary>
    /// <param name="deliverablePiece"></param>
    /// <param name="machine"></param>
    protected internal NonConformanceReport(IDeliverablePiece deliverablePiece, IMachine machine)
      : base (machine)
    {
      this.m_deliverablePiece = deliverablePiece;
    }
    #endregion // constructor


    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "NonConformanceReport"; }
    }

    /// <summary>
    /// Intermediate work piece
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IIntermediateWorkPiece IntermediateWorkPiece {
      get { return m_intermediateWorkPiece; }
      set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.ErrorFormat ("IntermediateWorkPiece.set: " +
                           "null value");
          throw new ArgumentNullException ("NonConformanceReport.IntermediateWorkPiece");
        }
        m_intermediateWorkPiece = value;
      }
    }
    
    /// <summary>
    /// Intermediate work piece for XML serialization
    /// </summary>
    [XmlElement("IntermediateWorkPiece")]
    public virtual IntermediateWorkPiece XmlSerializationIntermediateWorkPiece {
      get { return this.IntermediateWorkPiece as IntermediateWorkPiece; }
      set { this.IntermediateWorkPiece = value; }
    }

    
    /// <summary>
    /// Deliverable piece
    /// </summary>
    [XmlIgnore]
    public virtual IDeliverablePiece DeliverablePiece
    {
      get { return this.m_deliverablePiece; }
      set {this.m_deliverablePiece = value; }
    }
    
    /// <summary>
    /// Deliverable piece for XML serialization
    /// </summary>
    [XmlElement("DeliverablePiece")]
    public virtual DeliverablePiece XmlSerializationDeliverablePiece
    {
      get { return this.DeliverablePiece as DeliverablePiece; }
      set {this.DeliverablePiece = value; }
    }
    
    
    /// <summary>
    /// Nonconformance reason
    /// </summary>
    [XmlIgnore]
    public virtual INonConformanceReason NonConformanceReason
    {
      get { return this.m_nonConformanceReason; }
      set {this.m_nonConformanceReason = value; }
    }

    
    /// <summary>
    /// Nonconformance reason for XML serialization
    /// </summary>
    [XmlElement("NonConformanceReason")]
    public virtual NonConformanceReason XmlSerializationNonConformanceReason
    {
      get { return this.NonConformanceReason as NonConformanceReason; }
      set {this.NonConformanceReason = value; }
    }
    
    /// <summary>
    /// Nonconformance reason details for XML serialization
    /// </summary>
    [XmlElement("NonConformanceDetails")]
    public virtual string XmlSerializationNonConformanceDetails
    {
      get { return this.NonConformanceDetails as string; }
      set {this.NonConformanceDetails = value; }
    }
    
    /// <summary>
    /// Nonconformance reason details
    /// </summary>
    [XmlIgnore]
    public virtual string NonConformanceDetails
    {
      get { return this.m_nonConformanceDetails; }
      set { this.m_nonConformanceDetails = value; }
    }
    
    /// <summary>
    /// Tells if nonconformance is fixable
    /// </summary>
    [XmlIgnore]
    public virtual bool? NonConformanceFixable
    {
      get { return this.m_fixable; }
      set { this.m_fixable = value; }
    }

    /// <summary>
    /// End date of operation cyle associated with this nonconformance report
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? NonConformanceOperationDateTime
    {
      get { return this.m_operationDateTime; }
      set { this.m_operationDateTime = value; }
    }
    #endregion // Getters / Setters
    
    
    #region Methods
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      Debug.Assert (null != this.IntermediateWorkPiece);
      Debug.Assert (null != this.Machine);
      Debug.Assert(null != this.DeliverablePiece);
      
      IMonitoredMachine monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById(this.Machine.Id);
      if(null != monitoredMachine){
        IOperationCycle operationCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindWithEndEqualTo(monitoredMachine, this.NonConformanceOperationDateTime.Value);
        if(null != operationCycle){
          IOperationCycleDeliverablePiece operationCycleDeliverablePiece =
            ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO
            .FindWithOperationCycleDeliverablePiece(operationCycle, this.DeliverablePiece);
          
          operationCycleDeliverablePiece.NonConformanceReason = this.NonConformanceReason;
          operationCycleDeliverablePiece.NonConformanceDetails = this.m_nonConformanceDetails;
          
          ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO
            .MakePersistent(operationCycleDeliverablePiece);

          var extensionsRequest = new Lemoine.Business.Extension
            .MachineExtensions<INonConformanceReportExtension> (monitoredMachine, (ext, m) => ext.Initialize (m));
          var extensions = Lemoine.Business.ServiceProvider
            .Get (extensionsRequest);
          foreach (var extension in extensions) {
            extension.ReportCycleNonConformance (this.DeliverablePiece, operationCycle, this.NonConformanceReason, m_nonConformanceDetails);
          }
          
          MarkAsCompleted ("Cache/ClearDomainByMachine/NonConformanceReport/" + this.Machine.Id + "?Broadcast=true");
        }
        else {
          string message = string.Format ("MakeAnalysis: No operation cycle detected");
          log.ErrorFormat ("MakeAnalysis: {0} ",message);
          AddAnalysisLog (LogLevel.ERROR, message);
          MarkAsError ();
          return;
        }
      }
      else {
        string message = string.Format ("MakeAnalysis: there is no monitored machine with id {0} ",this.Machine.Id);
        log.ErrorFormat ("MakeAnalysis: {0} ",message);
        AddAnalysisLog (LogLevel.ERROR, message);
        MarkAsError ();
        return;
      }
      
    }
    
    /// <summary>
    /// Apply the modification while keeping it transient
    /// 
    /// It should be never called, because there is no transient NonConformanceReport to process.
    /// Use a persistent entity instead and MakeAnalysis.
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (false);
      log.FatalFormat ("Apply: not implemented/supported");
      throw new NotImplementedException ();
    }
    #endregion // Methods
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IIntermediateWorkPiece> (ref m_intermediateWorkPiece);
      NHibernateHelper.Unproxy<IDeliverablePiece> (ref m_deliverablePiece);
    }  
  }
}
