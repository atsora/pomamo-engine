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
  /// Persistent class of table SerialNumberMachineStamp
  /// </summary>
  [Serializable]
  public class SerialNumberMachineStamp
    : MachineStamp, ISerialNumberMachineStamp
  {
    #region Members
    string m_serialNumber;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "SerialNumberMachineStamp"; }
    }

    /// <summary>
    /// Reference to the SerialNumber
    /// </summary>
    [XmlElement ("SerialNumber")]
    public virtual string SerialNumber {
      get { return m_serialNumber; }
      set { m_serialNumber = value; }
    }
    
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected SerialNumberMachineStamp ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <returns></returns>
    public SerialNumberMachineStamp (IMachine machine, string serialNumber, DateTime dateTime)
      : base (machine, dateTime)
    {
      SerialNumber = serialNumber;
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
        return;
      }

      // try to find matching operation slots and cycles
      IOperationSlot operationSlotAtScan =
        ModelDAOHelper.DAOFactory
        .OperationSlotDAO.FindAt(this.Machine, this.DateTime);

      if ((operationSlotAtScan == null) || (operationSlotAtScan.Component == null)) {
        // no operation slot found, or no component yet: analysis is pending
        MarkAsPending (null);
        return;
      }

      // also, if serialNumber is associated to a WorkOrder (through a Revision)
      // check that this WorkOrder of the operation slot is the same
      if (this.Revision != null) {
        IWorkOrder revisionWorkOrder = null;
        foreach (IMachineModification modification in this.Revision.MachineModifications) {
          if (modification is IWorkOrderMachineStamp)
          {
            revisionWorkOrder = (modification as IWorkOrderMachineStamp).WorkOrder;
            break;
          }
        }
        if ((revisionWorkOrder != null) && (revisionWorkOrder != operationSlotAtScan.WorkOrder)) {
          // not a matching operation slot: analysis remains pending
          MarkAsPending (null);
          return;
        }
      }

      // search for last cycle of slot not yet associated with a deliverable piece
      IList<IOperationCycle> allCyclesForDebug =
        ModelDAOHelper.DAOFactory
        .OperationCycleDAO.FindAllWithOperationSlot(operationSlotAtScan);
      
      IOperationCycle lastForDebug;
      IList<IOperationCycleDeliverablePiece> ocdpDebug;
      if (allCyclesForDebug.Count > 0) {
        lastForDebug = allCyclesForDebug[0];
        ocdpDebug =
          ModelDAOHelper.DAOFactory
          .OperationCycleDeliverablePieceDAO.FindAllWithOperationCycle(lastForDebug);
      }
      
      IOperationCycle lastFullCycle =
        ModelDAOHelper.DAOFactory
        .OperationCycleDAO.FindLastFullNotAssociated(operationSlotAtScan, this.DateTime);
      
      IList<IOperationCycle> listCyclesToAssociate;
      
      if ((lastFullCycle == null) || !(lastFullCycle.Begin.HasValue)) {
        // no such cycle
        // fetch the last cycles (which are partial) with no associated serial number
        // and begin date <= endDate (limit to 10)
        listCyclesToAssociate =
          ModelDAOHelper.DAOFactory
          .OperationCycleDAO
          .FindAllNotAssociated(operationSlotAtScan,
                                this.DateTime);
      } else {
        // also fetch full / partial cycles before
        // the last full cycle (i.e. their begin date is <= lastFullCycle.Begin)
        // that have no associated serial number
        // (already includes lastFullCycle)
        listCyclesToAssociate =
          ModelDAOHelper.DAOFactory
          .OperationCycleDAO
          .FindAllNotAssociated(operationSlotAtScan,
                                lastFullCycle.Begin.Value);
      }
      
      // cycles have been found to associate with the serial number
      if (listCyclesToAssociate.Count > 0) {
        // if deliverable piece with same component/same serial already exists,
        // use it
        // otherwise create a new deliverable piece
        
        IDeliverablePiece deliverablePiece =
          ModelDAOHelper.DAOFactory.DeliverablePieceDAO
          .FindByCodeAndComponent(this.SerialNumber,
                                  operationSlotAtScan.Component);
        
        if (deliverablePiece == null) {
          deliverablePiece =
            ModelDAOHelper.ModelFactory.CreateDeliverablePiece(this.SerialNumber);
          
          deliverablePiece.Component = operationSlotAtScan.Component;
          ModelDAOHelper.DAOFactory.DeliverablePieceDAO.MakePersistent(deliverablePiece);
        }
        
        // associate deliverable piece with matching operation cycles
        foreach(IOperationCycle operationCycle in listCyclesToAssociate) {
          IOperationCycleDeliverablePiece ocdp =
            new OperationCycleDeliverablePiece(deliverablePiece,
                                               operationCycle);
          
          ModelDAOHelper.DAOFactory
            .OperationCycleDeliverablePieceDAO.MakePersistent(ocdp);
        }
        
        // analysis done
        MarkAsCompleted ("Cache/ClearDomainByMachine/SerialNumberStamp/" + this.Machine.Id + "?Broadcast=true");
        
      }
      else {
        // can be a problem (no operation cycle detected yet
        // but could also be a repeated scan)
        log.ErrorFormat("No free operation cycle found at {0} on machine {1}",
                        this.DateTime.ToString(),
                        this.Machine.ToString());
        MarkAsPending (null);
        return;
      }
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