// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using Lemoine.Business.Config;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table SerialNumberModification
  /// </summary>
  [Serializable]
  public class SerialNumberModification : MachineStamp, ISerialNumberModification
  {
    #region Members
    string m_serialNumber;
    DateTime m_beginOrEndDateTime;
    bool m_isBegin;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "SerialNumberModification"; }
    }

    /// <summary>
    /// Reference to the SerialNumber
    /// </summary>
    [XmlElement ("SerialNumber")]
    public virtual string SerialNumber {
      get { return m_serialNumber; }
      set { m_serialNumber = value; }
    }

    /// <summary>
    /// Begin or end date time of related operation cycle (should be a non-estimated one)
    /// </summary>
    public virtual DateTime BeginOrEndDateTime {
      get { return m_beginOrEndDateTime; }
      set { m_beginOrEndDateTime = value; }
    }
    
    /// <summary>
    /// Is BeginOrEndDateTime refering to begin ?
    /// </summary>
    public virtual bool IsBegin {
      get { return m_isBegin; }
      set { m_isBegin = value; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected SerialNumberModification ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <returns></returns>
    public SerialNumberModification (IMachine machine,
                                     string serialNumber,
                                     DateTime beginOrEndDateTime,
                                     bool isBegin,
                                     DateTime dateOfModification)
      : base (machine, dateOfModification)
    {
      SerialNumber = serialNumber;
      BeginOrEndDateTime = beginOrEndDateTime;
      IsBegin = isBegin;
    }
    #endregion // Constructors
    
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());
      
      // handle obsolescence
      if (DateTime.UtcNow.Subtract(AnalysisConfigHelper.ObsoleteTime) > this.DateTime) {
        MarkAsObsolete ();
        string warnMsg = String.Format("SerialNumberModification {0} is obsolete",
                                       this.Id);
        log.Warn(warnMsg);
        ModelDAOHelper.DAOFactory.MachineModificationLogDAO.Add (this, LogLevel.WARN, warnMsg);
        return;
      }

      IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById(this.Machine.Id);
      
      if (machine == null) {
        MarkAsError ();
        string errMsg = String.Format("SerialNumberModification {0}: no matching monitored machine",
                                      this.Id);
        log.Error(errMsg);
        ModelDAOHelper.DAOFactory.MachineModificationLogDAO.Add (this, LogLevel.ERROR, errMsg);
        return;
      }
      
      IOperationCycle operationCycle = null;
      if (this.IsBegin) {
        operationCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO.FindAt(machine, this.BeginOrEndDateTime);
      } else {
        operationCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO.FindAtEnd(machine, this.BeginOrEndDateTime);
      }
      
      if (operationCycle == null) {
        MarkAsError ();
        string errMsg = String.Format("SerialNumberModification {0}: no matching operation cycle",
                                      this.Id);
        log.Error(errMsg);
        ModelDAOHelper.DAOFactory.MachineModificationLogDAO.Add (this, LogLevel.ERROR, errMsg);
        return;
      }
      
      int cycleID = operationCycle.Id;
      
      if ((operationCycle.OperationSlot == null) || (operationCycle.OperationSlot.Component == null)) {
        MarkAsError ();
        string errMsg = String.Format("SerialNumberModification {0}: no operation slot or component for cycle id {1}",
                                      this.Id, cycleID);
        log.Error(errMsg);
        ModelDAOHelper.DAOFactory.MachineModificationLogDAO.Add (this, LogLevel.ERROR, errMsg);
        return;

      }

      IDeliverablePiece deliverablePiece =
        ModelDAOHelper.DAOFactory.DeliverablePieceDAO
        .FindByCodeAndComponent(this.SerialNumber,
                                operationCycle.OperationSlot.Component);
      
      if (deliverablePiece == null) {
        deliverablePiece =
          ModelDAOHelper.ModelFactory.CreateDeliverablePiece(this.SerialNumber);
        
        deliverablePiece.Component = operationCycle.OperationSlot.Component;
        ModelDAOHelper.DAOFactory.DeliverablePieceDAO.MakePersistent(deliverablePiece);
      }

      IList<IOperationCycleDeliverablePiece> opCyclePieces =
        ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.FindAllWithOperationCycle(operationCycle);
      
      bool alreadyScanned = false; // to avoid remove/insert at the same time
      foreach(IOperationCycleDeliverablePiece opCyclePiece in opCyclePieces) {
        SetActive ();
        if (opCyclePiece.DeliverablePiece.Id != deliverablePiece.Id) {
          ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.MakeTransient(opCyclePiece);
          string warnMsg = String.Format("Serial Number {0} on Cycle {1} is superseeded by {2}",
                                         opCyclePiece.DeliverablePiece.Display,
                                         operationCycle.Id,
                                         this.SerialNumber);
          log.Warn(warnMsg);
          ModelDAOHelper.DAOFactory.MachineModificationLogDAO.Add (this, LogLevel.WARN, warnMsg);
        } else {
          alreadyScanned = true;
        }
      }
      
      if (alreadyScanned == false) {
        IOperationCycleDeliverablePiece ocdp =
          new OperationCycleDeliverablePiece(deliverablePiece,
                                             operationCycle);
        
        ModelDAOHelper.DAOFactory
          .OperationCycleDeliverablePieceDAO.MakePersistent(ocdp);
      }
      
      // analysis done
      MarkAsCompleted ("Cache/ClearDomainByMachine/SerialNumberStamp/" + this.Machine.Id + "?Broadcast=true");
    }

    /// <summary>
    /// Apply the modification while keeping it transient
    /// 
    /// It should be never called, because there is no transient ReasonMachineAssociation to process.
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
    }
  }
}