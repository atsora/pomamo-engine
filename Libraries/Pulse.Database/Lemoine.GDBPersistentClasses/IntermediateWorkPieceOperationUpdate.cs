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
using NHibernate;
using NHibernate.Criterion;
using Lemoine.Business.Config;
using System.Linq;
using Lemoine.Extensions.Database;
using Pulse.Extensions.Database;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table IntermediateWorkPieceOperationUpdate
  /// 
  /// This table tracks the modifications that are made
  /// in the relations between an IntermediateWorkPiece and an Operation
  /// in table IntermediateWorkPiece.
  /// 
  /// It is necessary to allow the Analyzer service to update correctly
  /// all the Analysis tables.
  /// </summary>
  [Serializable]
  public class IntermediateWorkPieceOperationUpdate : GlobalModification, IIntermediateWorkPieceOperationUpdate
  {
    #region Members
    IIntermediateWorkPiece m_intermediateWorkPiece;
    IOperation m_oldOperation;
    IOperation m_newOperation;
    int m_oldQuantity;
    int m_newQuantity;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (IntermediateWorkPieceOperationUpdate).FullName);

    #region constructor
    /// <summary>
    /// Default Constructor
    /// </summary>
    protected IntermediateWorkPieceOperationUpdate ()
    {
    }

    /// <summary>
    ///   Constructor
    /// </summary>
    /// <param name="intermediateworkpiece"></param>
    /// <param name="oldOperation"></param>
    /// <param name="newOperation"></param>
    internal protected IntermediateWorkPieceOperationUpdate (IIntermediateWorkPiece intermediateworkpiece, IOperation oldOperation, IOperation newOperation)
    {
      this.m_intermediateWorkPiece = intermediateworkpiece;
      this.m_oldOperation = oldOperation;
      this.m_newOperation = newOperation;
    }
    #endregion

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "IntermediateWorkPieceOperationUpdate"; }
    }

    /// <summary>
    /// Intermediate work piece
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IIntermediateWorkPiece IntermediateWorkPiece
    {
      get { return m_intermediateWorkPiece; }
      set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.ErrorFormat ("IntermediateWorkPiece.set: " +
                           "null value");
          throw new ArgumentNullException ("IntermediateWorkPieceOperationUpdate.IntermediateWorkPiece");
        }
        m_intermediateWorkPiece = value;
      }
    }

    /// <summary>
    /// Intermediate work piece for XML serialization
    /// </summary>
    [XmlElement ("IntermediateWorkPiece")]
    public virtual IntermediateWorkPiece XmlSerializationIntermediateWorkPiece
    {
      get { return this.IntermediateWorkPiece as IntermediateWorkPiece; }
      set { this.IntermediateWorkPiece = value; }
    }

    /// <summary>
    /// Old operation
    /// 
    /// null in case a new intermediate work piece / operation relation is set
    /// </summary>
    [XmlIgnore]
    public virtual IOperation OldOperation
    {
      get { return m_oldOperation; }
      set { m_oldOperation = value; }
    }

    /// <summary>
    /// Old operation for XML serialization
    /// </summary>
    [XmlElement ("OldOperation")]
    public virtual Operation XmlSerializationOldOperation
    {
      get { return this.OldOperation as Operation; }
      set { this.OldOperation = value; }
    }

    /// <summary>
    /// New operation
    /// 
    /// null in case the intermediate work piece / operation is deleted
    /// </summary>
    [XmlIgnore]
    public virtual IOperation NewOperation
    {
      get { return m_newOperation; }
      set { m_newOperation = value; }
    }

    /// <summary>
    /// New operation for XML serialization
    /// </summary>
    [XmlElement ("NewOperation")]
    public virtual Operation XmlSerializationNewOperation
    {
      get { return this.NewOperation as Operation; }
      set { this.NewOperation = value; }
    }

    /// <summary>
    /// Old quantity of intermediate work pieces to make with the operation
    /// </summary>
    public virtual int OldQuantity
    {
      get { return m_oldQuantity; }
      set { m_oldQuantity = value; }
    }

    /// <summary>
    /// New quantity of intermediate work pieces to make with the operation
    /// </summary>
    public virtual int NewQuantity
    {
      get { return m_newQuantity; }
      set { m_newQuantity = value; }
    }
    #endregion // Getters / Setters

    #region Methods
    bool GetDataStructureOption (DataStructureConfigKey key)
    {
      return Lemoine.Info.ConfigSet
        .Get<bool> (ConfigKeys.GetDataStructureConfigKey (key));
    }

    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      Debug.Assert (null != this.IntermediateWorkPiece);

      // - If only the operation quantity changed (because for example because of a previous mistake):
      if (object.Equals (this.OldOperation, this.NewOperation)) {
        Debug.Assert (!object.Equals (this.OldQuantity, this.NewQuantity)); // Else, what else for ?
        log.DebugFormat ("MakeAnalysis: " +
                         "only the quantity changed from {0} to {1}",
                         this.OldQuantity, this.NewQuantity);
        var extensions = Lemoine.Business.ServiceProvider
        .Get (new Lemoine.Business.Extension.GlobalExtensions<IIntermediateWorkPieceExtension> ());
        foreach (var extension in extensions) {
          extension.UpdateQuantity (this.IntermediateWorkPiece, this.OldQuantity, this.NewQuantity);
        }
        return;
      }

      if (null != this.OldOperation) {
        // - Get the impacted old operations in slots
        IList<IOperationSlot> impactedOldOperationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindByOperation (this.OldOperation);
        foreach (IOperationSlot impactedOldOperationSlot in impactedOldOperationSlots) {
          // Remove the component from the operation slot in case
          // - the component is only determined by the operation
          // - the component is not compatible any more with the operation
          if ((null != impactedOldOperationSlot.Component)
              && (GetDataStructureOption (DataStructureConfigKey.ComponentFromOperationOnly)
                  || (!Lemoine.Business.Operation.OperationRelations.IsComponentCompatibleWithOperation (impactedOldOperationSlot.Component,

                                                                         this.OldOperation)))) {
            impactedOldOperationSlot.Component = null;
            AddAnalysisLog (LogLevel.INFO,
                            "Reset the component " +
                            "because it is not compatible with the old operation");
          }

          // Remove the work order from the operation slot and work order slot in case
          // - the work order is only determined by the operation
          // - the work order is not compatible any more with the operation
          if ((null != impactedOldOperationSlot.WorkOrder)
              && ((GetDataStructureOption (DataStructureConfigKey.ComponentFromOperationOnly)
                   && GetDataStructureOption (DataStructureConfigKey.WorkOrderFromComponentOnly))
                  || (!Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithOperation (impactedOldOperationSlot.WorkOrder,

                                                                         this.OldOperation)))) {
            impactedOldOperationSlot.WorkOrder = null;
            AddAnalysisLog (LogLevel.INFO,
                            string.Format ("Reset the work order between {0} and {1}",
                                           impactedOldOperationSlot.BeginDateTime,
                                           impactedOldOperationSlot.EndDateTime));
          }

          // Remove the line in case
          // - the line is not compatible any more with the operation
          if (!Lemoine.Business.Operation.OperationRelations.IsLineCompatibleWithMachineOperation (impactedOldOperationSlot.Line,
                                                                   impactedOldOperationSlot.Machine,

                                                                   this.OldOperation)) {
            impactedOldOperationSlot.Line = null;
            AddAnalysisLog (LogLevel.INFO,
                            string.Format ("Reset the line between {0} and {1}",
                                           impactedOldOperationSlot.BeginDateTime,
                                           impactedOldOperationSlot.EndDateTime));
          }
        }
      }

      // - Get the impacted new operations in the slots
      if ((null != this.NewOperation)
        && GetDataStructureOption (DataStructureConfigKey.UniqueComponentFromOperation)) {
        // Associate the component to the operation slot
        // in case the component is unique for an operation
        Debug.Assert (this.IntermediateWorkPiece.ComponentIntermediateWorkPieces.Count <= 1);
        if (this.IntermediateWorkPiece.ComponentIntermediateWorkPieces.Count > 1) {
          log.FatalFormat ("MakeAnalysis: " +
                           "more than one component for intermediate work piece {0} " +
                           "although there should be only one according to the options",
                           this.IntermediateWorkPiece);
        }
        IComponent component = null;
        if (this.IntermediateWorkPiece.ComponentIntermediateWorkPieces.Count > 0) {
          component =
            this.IntermediateWorkPiece.ComponentIntermediateWorkPieces.First ().Component;
        }
        IList<IOperationSlot> impactedNewOperationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindByOperation (this.NewOperation);
        foreach (IOperationSlot impactedNewOperationSlot in impactedNewOperationSlots) {
          impactedNewOperationSlot.Component = component;
          // Associate the work order
          // in case the work order is unique for a component
          IWorkOrder workOrder = null;
          if ((null != component) // workOrder is already null by default
              && GetDataStructureOption (DataStructureConfigKey.UniqueWorkOrderFromProjectOrComponent)) {
            workOrder =
              Lemoine.Business.Operation.OperationRelations.GuessUniqueWorkOrderFromComponent (component);
          }
          if (!object.Equals (impactedNewOperationSlot.WorkOrder, workOrder)) {
            AddAnalysisLog (LogLevel.INFO,
                            string.Format ("Update or reset also the work order " +
                                           "from {0} to {1}",
                                           impactedNewOperationSlot.WorkOrder,
                                           workOrder));
            impactedNewOperationSlot.WorkOrder = workOrder;
          }
          ModelDAOHelper.DAOFactory.OperationSlotDAO
            .MakePersistent (impactedNewOperationSlot);
        }
      }

      // Analysis is done
      MarkAsCompleted ("Cache/ClearDomain/ComponentIntermediateWorkPieceUpdate?Broadcast=true");
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
    #endregion // Methods

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      string modificationType = this.ModificationType;
      NHibernateHelper.Unproxy<IIntermediateWorkPiece> (ref m_intermediateWorkPiece);
      NHibernateHelper.Unproxy<IOperation> (ref m_oldOperation);
      NHibernateHelper.Unproxy<IOperation> (ref m_newOperation);
    }
  }
}
