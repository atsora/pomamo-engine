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

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ComponentIntermediateWorkPieceUpdate
  /// 
  /// This table tracks the modifications in the relations
  /// between a Component and an IntermediateWorkPiece
  /// that are made in table ComponentIntermediateWorkPiece
  /// 
  /// It is necessary to allow the Analyzer service
  /// to update correctly all the Analysis tables.
  /// </summary>
  [Serializable]
  public class ComponentIntermediateWorkPieceUpdate: GlobalModification, IComponentIntermediateWorkPieceUpdate
  {
    #region Members
    IComponent m_component;
    IIntermediateWorkPiece m_intermediateWorkPiece;
    ComponentIntermediateWorkPieceUpdateModificationType m_typeOfModification;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (IComponentIntermediateWorkPieceUpdate).FullName);

    #region constructor
    /// <summary>
    /// Default constructor
    /// </summary>
    protected ComponentIntermediateWorkPieceUpdate ()
    {
    }
    
    /// <summary>
    ///   Constructor
    /// </summary>
    /// <param name="component"></param>
    /// <param name="intermediateworkpiece"></param>
    /// <param name="type"></param>
    internal protected ComponentIntermediateWorkPieceUpdate (IComponent component, IIntermediateWorkPiece intermediateworkpiece, ComponentIntermediateWorkPieceUpdateModificationType type)
    {
      this.m_component = component;
      this.m_intermediateWorkPiece = intermediateworkpiece;
      this.m_typeOfModification = type;
    }
    #endregion

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "ComponentIntermediateWorkPieceUpdate"; }
    }

    /// <summary>
    /// Component
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IComponent Component {
      get { return m_component; }
      set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.ErrorFormat ("Component.set: " +
                           "null value");
          throw new ArgumentNullException ("ComponentIntermediateWorkPieceUpdate.Component");
        }
        m_component = value;
      }
    }
    
    /// <summary>
    /// Component for XML serialization
    /// </summary>
    [XmlElement("Component")]
    public virtual Component XmlSerializationComponent {
      get { return this.Component as Component; }
      set { this.Component = value; }
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
          throw new ArgumentNullException ("ComponentIntermediateWorkPieceUpdate.IntermediateWorkPiece");
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
    /// Modification type
    /// </summary>
    public virtual ComponentIntermediateWorkPieceUpdateModificationType TypeOfModification {
      get { return m_typeOfModification; }
      set { m_typeOfModification = value; }
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
      
      ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceUpdateDAO.Lock (this);
      if (null != this.IntermediateWorkPiece) {
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock (this.IntermediateWorkPiece);
        if (null != this.IntermediateWorkPiece.Operation) {
          ModelDAOHelper.DAOFactory.OperationDAO.Lock (this.IntermediateWorkPiece.Operation);
        }
      }
      
      // 1. Update OperationSlot accordingly
      if ( (null != this.IntermediateWorkPiece)
          && (null != this.IntermediateWorkPiece.Operation)) {
        IList<IOperationSlot> impactedOperationSlots;
        switch (this.TypeOfModification) {
          case ComponentIntermediateWorkPieceUpdateModificationType.NEW:
            // Associate the component to the operation slot
            // in case the component is unique for an operation
            if (GetDataStructureOption (DataStructureConfigKey.UniqueComponentFromOperation)) {
              impactedOperationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
                .FindByOperation (this.IntermediateWorkPiece.Operation);
              foreach (IOperationSlot impactedOperationSlot in impactedOperationSlots) {
                impactedOperationSlot.Component = this.Component;
                // Associate the work order
                // in case the work order is unique for a component
                IWorkOrder workOrder = null;
                // in case the work order is unique for a component
                if ( (null != this.Component) // workOrder is already null by default
                    && GetDataStructureOption (DataStructureConfigKey.UniqueWorkOrderFromProjectOrComponent)) {
                  workOrder =
                    Lemoine.Business.Operation.OperationRelations.GuessUniqueWorkOrderFromComponent (this.Component);
                }
                if (!object.Equals (impactedOperationSlot.WorkOrder, workOrder)) {
                  AddAnalysisLog (LogLevel.INFO,
                                  string.Format ("Update or reset also the work order " +
                                                 "from {0} to {1}",
                                                 impactedOperationSlot.WorkOrder,
                                                 workOrder));
                  impactedOperationSlot.WorkOrder = workOrder;
                }
              }
            }
            break;
          case ComponentIntermediateWorkPieceUpdateModificationType.DELETE:
            impactedOperationSlots = (new OperationSlotDAO ())
              .FindByComponentOperation (this.Component,
                                         this.IntermediateWorkPiece.Operation);
            foreach (IOperationSlot impactedOperationSlot in impactedOperationSlots) {
              impactedOperationSlot.Component = null;
              if ( (null != impactedOperationSlot.WorkOrder)
                  && (GetDataStructureOption (DataStructureConfigKey.WorkOrderFromComponentOnly)
                      || (!Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithOperation (impactedOperationSlot.WorkOrder, impactedOperationSlot.Operation)))) {
                // In the case the work order is determined from the component only,
                // or the old work order is not compatible any more with the operation
                // reset also the work order
                impactedOperationSlot.WorkOrder = null;
                // log it !
                AddAnalysisLog (LogLevel.INFO,
                                String.Format ("Reset the work order between {0} and {1}",
                                               impactedOperationSlot.BeginDateTime,
                                               impactedOperationSlot.EndDateTime));
              }
              
              // Remove the line in case
              // - the line is not compatible any more with the operation
              if (!Lemoine.Business.Operation.OperationRelations.IsLineCompatibleWithMachineOperation (impactedOperationSlot.Line,
                                                                        impactedOperationSlot.Machine,
                                                                        impactedOperationSlot.Operation)) {
                impactedOperationSlot.Line = null;
                AddAnalysisLog (LogLevel.INFO,
                                string.Format ("Reset the line between {0} and {1}",
                                               impactedOperationSlot.BeginDateTime,
                                               impactedOperationSlot.EndDateTime));
              }
            }
            break;
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
      NHibernateHelper.Unproxy<IComponent> (ref m_component);
      NHibernateHelper.Unproxy<IIntermediateWorkPiece> (ref m_intermediateWorkPiece);
    }
  }
}
