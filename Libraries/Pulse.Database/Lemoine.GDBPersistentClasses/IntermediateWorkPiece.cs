// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Collections;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table intermediateworkpiece
  /// 
  /// An intermediate work piece is the result of an operation.
  /// It represents a step in the machining of a component.
  /// </summary>
  [Serializable]
  public class IntermediateWorkPiece : DataWithDisplayFunction, IIntermediateWorkPiece, IMergeable<IIntermediateWorkPiece>, IEquatable<IIntermediateWorkPiece>, Lemoine.Collections.IDataWithId
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    string m_code;
    string m_externalCode;
    string m_documentLink;
    double? m_weight;
    IOperation m_operation;
    int m_operationQuantity = 1;
    ICollection<IOperation> m_possibleNextOperations = new InitialNullIdSet<IOperation, int> ();
    ICollection<IComponentIntermediateWorkPiece> m_componentIntermediateWorkPieces = new List<IComponentIntermediateWorkPiece> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (IntermediateWorkPiece).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id", "ExternalCode", "Code", "Name" }; }
    }

    /// <summary>
    /// IntermediateWorkPiece ID
    /// </summary>
    [XmlAttribute ("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Associated SimpleOperation
    /// </summary>
    [XmlIgnore]
    public virtual ISimpleOperation SimpleOperation
    {
      get { return new SimpleOperation (this); }
    }

    /// <summary>
    /// Full name of the intermediate workpiece as used in the shop
    /// </summary>
    [XmlAttribute ("Name"), MergeAuto]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }

    /// <summary>
    /// Code of the intermediate work piece
    /// </summary>
    [XmlAttribute ("Code"), MergeAuto]
    public virtual string Code
    {
      get { return m_code; }
      set { m_code = value; }
    }

    /// <summary>
    /// External code
    /// 
    /// It may  help synchronizing system data with an external database
    /// </summary>
    [XmlAttribute ("ExternalCode"), MergeAuto]
    public virtual string ExternalCode
    {
      get { return m_externalCode; }
      set { m_externalCode = value; }
    }

    /// <summary>
    /// Link to the documentation in the network
    /// </summary>
    [XmlAttribute ("DocumentLink"), MergeAuto]
    public virtual string DocumentLink
    {
      get { return m_documentLink; }
      set { m_documentLink = value; }
    }

    /// <summary>
    /// Weight of the intermediate work piece when it is done
    /// 
    /// (this may help counting the number of made work pieces)
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual double? Weight
    {
      get { return m_weight; }
      set { m_weight = value; }
    }

    /// <summary>
    /// Reference to the operation that makes this work piece
    /// </summary>
    [MergeParent, XmlIgnore]
    public virtual IOperation Operation
    {
      get { return m_operation; }
      set {
        if (object.Equals (m_operation, value)) {
          // Nothing to do
          return;
        }
        // Remove the intermediate work piece from the previous operation
        if (null != m_operation) {
          (m_operation as Operation)
            .RemoveIntermediateWorkPieceForInternalUse (this);
        }
        m_operation = value;
        if (null != m_operation) {
          // Add the intermediate work piece to the new operation
          (m_operation as Operation)
            .AddIntermediateWorkPieceForInternalUse (this);
        }
      }
    }

    /// <summary>
    /// Reference to the operation that makes this work piece
    /// for Xml Serialization
    /// </summary>
    [XmlElement ("Operation")]
    public virtual Operation XmlSerializationOperation
    {
      get { return this.Operation as Operation; }
      set { this.Operation = value; }
    }

    /// <summary>
    /// Number of intermediate work pieces the operation makes.
    /// </summary>
    [XmlAttribute ("OperationQuantity"), MergeAuto]
    public virtual int OperationQuantity
    {
      get { return m_operationQuantity; }
      set { m_operationQuantity = value; }
    }

    /// <summary>
    /// Possible next operations for this intermediate work piece
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IOperation> PossibleNextOperations
    {
      get {
        return m_possibleNextOperations;
      }
    }

    /// <summary>
    /// Set of components this intermediate work piece is known to be a part of
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IComponentIntermediateWorkPiece> ComponentIntermediateWorkPieces
    {
      get {
        return m_componentIntermediateWorkPieces;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected IntermediateWorkPiece ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operation"></param>
    protected internal IntermediateWorkPiece (IOperation operation)
    {
      m_operation = operation;
      if (null != m_operation) {
        // Add the intermediate work piece to the new operation
        (m_operation as Operation)
          .AddIntermediateWorkPieceForInternalUse (this);
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Check if an intermediate work piece is undefined
    /// 
    /// An intermediate work piece is considered as undefined if it has no name and no code
    /// </summary>
    /// <returns></returns>
    public virtual bool IsUndefined ()
    {
      return ((this.Name == null)
              || (0 == this.Name.Length))
        && ((this.Code == null)
            || (0 == this.Code.Length));
    }
    #endregion // Methods

    #region Add methods
    /// <summary>
    /// Add a possible next operation
    /// </summary>
    /// <param name="operation"></param>
    public virtual void AddPossibleNextOperation (IOperation operation)
    {
      operation.AddSource (this);
    }

    /// <summary>
    /// Remove a possible next operation
    /// </summary>
    /// <param name="operation"></param>
    public virtual void RemovePossibleNextOperation (IOperation operation)
    {
      operation.RemoveSource (this);
    }

    /// <summary>
    /// Add a componentIntermediateWorkPiece locally to the structure
    /// </summary>
    /// <param name="componentIntermediateWorkPiece"></param>
    protected internal virtual void AddComponentForInternalUse (IComponentIntermediateWorkPiece componentIntermediateWorkPiece)
    {
      AddToProxyCollection<IComponentIntermediateWorkPiece> (ComponentIntermediateWorkPieces, componentIntermediateWorkPiece);
    }

    /// <summary>
    /// Remove a componentIntermediateWorkPiece locally from the structure
    /// </summary>
    /// <param name="componentIntermediateWorkPiece"></param>
    protected internal virtual void RemoveComponentForInternalUse (IComponentIntermediateWorkPiece componentIntermediateWorkPiece)
    {
      RemoveFromProxyCollection<IComponentIntermediateWorkPiece> (ComponentIntermediateWorkPieces, componentIntermediateWorkPiece);
    }
    #endregion // Add methods

    #region Methods
    private class MergeComponentIntermediateWorkPiece
    {
      IIntermediateWorkPiece a_t;
      IIntermediateWorkPiece a_other;
      ConflictResolution a_conflictResolution;

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="t"></param>
      /// <param name="other"></param>
      /// <param name="conflictResolution"></param>
      public MergeComponentIntermediateWorkPiece (IIntermediateWorkPiece t,
                                                  IIntermediateWorkPiece other,
                                                  ConflictResolution conflictResolution)
      {
        a_t = t;
        a_other = other;
        a_conflictResolution = conflictResolution;
      }

      /// <summary>
      /// Merge
      /// </summary>
      /// <param name="otherComponentIntermediateWorkPiece"></param>
      public void MergeDelegate (IComponentIntermediateWorkPiece otherComponentIntermediateWorkPiece)
      {
        // Check if other shares a component with this
        foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in a_t.ComponentIntermediateWorkPieces) {
          if (componentIntermediateWorkPiece.Component.Equals (otherComponentIntermediateWorkPiece.Component)) {
            // A component is shared
            // - Check the code first
            string code = componentIntermediateWorkPiece.Code;
            if (!string.IsNullOrEmpty (otherComponentIntermediateWorkPiece.Code)) {
              if (string.IsNullOrEmpty (code)) {
                code = otherComponentIntermediateWorkPiece.Code;
              }
              else if (!otherComponentIntermediateWorkPiece.Code.Equals (code)) { // The code differs
                log.WarnFormat ("Merge: " +
                                "Component Code differs between this {0} and other {1}",
                                code, otherComponentIntermediateWorkPiece.Code);
                if (a_conflictResolution.Equals (ConflictResolution.Keep)) {
                  log.InfoFormat ("Merge: " +
                                  "Conflict resolution, keep the existing code {0}",
                                  code);
                }
                else if (a_conflictResolution.Equals (ConflictResolution.Overwrite)) {
                  log.InfoFormat ("Merge: " +
                                  "Conflict resolution, overwrite the existing code {0} with {1}",
                                  code, otherComponentIntermediateWorkPiece.Code);
                  code = otherComponentIntermediateWorkPiece.Code;
                }
                else if (a_conflictResolution.Equals (ConflictResolution.Exception)) {
                  log.ErrorFormat ("Merge: " +
                                   "Conflict resolution, " +
                                   "component code differs betwen this {0} and other {1} " +
                                   "=> raise ConflictException",
                                   code, otherComponentIntermediateWorkPiece.Code);
                  throw new ConflictException (otherComponentIntermediateWorkPiece.Code, code, "ComponentIntermediateWorkPiece.Code");
                }
              }
            }
            // - Check the order
            int? order = componentIntermediateWorkPiece.Order;
            if (otherComponentIntermediateWorkPiece.Order.HasValue) {
              if (!order.HasValue) {
                log.DebugFormat ("Merge: " +
                                 "take the Component Order {0} of other (order of this is null)",
                                 otherComponentIntermediateWorkPiece.Order.Value);
                order = otherComponentIntermediateWorkPiece.Order;
              }
              else if (!otherComponentIntermediateWorkPiece.Order.Value.Equals (order.Value)) { // The order differs
                log.WarnFormat ("Merge: " +
                                "Component Order differs between this {0} and other {1}",
                                order, otherComponentIntermediateWorkPiece.Order);
                if (a_conflictResolution.Equals (ConflictResolution.Keep)) {
                  log.InfoFormat ("Merge: " +
                                  "Conflict resolution, keep the existing order {0}",
                                  order);
                }
                else if (a_conflictResolution.Equals (ConflictResolution.Overwrite)) {
                  log.InfoFormat ("Merge: " +
                                  "Conflict resolution, overwrite the existing order {0} with {1}",
                                  order, otherComponentIntermediateWorkPiece.Order);
                  order = otherComponentIntermediateWorkPiece.Order;
                }
                else if (a_conflictResolution.Equals (ConflictResolution.Exception)) {
                  log.ErrorFormat ("Merge: " +
                                   "Conflict resolution, " +
                                   "component order differs betwen this {0} and other {1} " +
                                   "=> raise ConflictException",
                                   order, otherComponentIntermediateWorkPiece.Order);
                  throw new ConflictException (otherComponentIntermediateWorkPiece.Order, order, "ComponentIntermediateWorkPiece.Order");
                }
              }
            }
            ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakeTransient (otherComponentIntermediateWorkPiece);
            componentIntermediateWorkPiece.Code = code;
            componentIntermediateWorkPiece.Order = order;
            ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (componentIntermediateWorkPiece);
            return;
          } // if
        } // foreach

        // The two intermediate work pieces do not share any component
        IComponentIntermediateWorkPiece newComponentIntermediateWorkPiece;
        if (!string.IsNullOrEmpty (otherComponentIntermediateWorkPiece.Code)) {
          newComponentIntermediateWorkPiece =
            otherComponentIntermediateWorkPiece.Component.AddIntermediateWorkPiece (a_t,
                                                                                    otherComponentIntermediateWorkPiece.Code);
        }
        else if (otherComponentIntermediateWorkPiece.Order.HasValue) {
          newComponentIntermediateWorkPiece =
            otherComponentIntermediateWorkPiece.Component.AddIntermediateWorkPiece (a_t,
                                                                                    otherComponentIntermediateWorkPiece.Order.Value);
        }
        else {
          newComponentIntermediateWorkPiece =
            otherComponentIntermediateWorkPiece.Component.AddIntermediateWorkPiece (a_t);
        }
        ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (newComponentIntermediateWorkPiece);
        IList<IComponentIntermediateWorkPiece> listciwp = otherComponentIntermediateWorkPiece.Component.RemoveIntermediateWorkPiece (a_other);
        foreach (IComponentIntermediateWorkPiece ciwp in listciwp) {
          Debug.Assert (NHibernateHelper.GetCurrentSession ().Contains (ciwp.IntermediateWorkPiece));
          ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakeTransient (ciwp);
        }
      }
    }

    /// <summary>
    /// <see cref="IMergeable&lt;T&gt;.Merge" />
    /// </summary>
    /// <param name="other"></param>
    /// <param name="conflictResolution"></param>
    public virtual void Merge (IIntermediateWorkPiece other,
                               ConflictResolution conflictResolution)
    {
      { // WorkOrderLineQuantity
        foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in other.ComponentIntermediateWorkPieces) {
          IComponent component = componentIntermediateWorkPiece.Component;
          if (0 != ((Lemoine.Collections.IDataWithId)component).Id) { // Not for transient objects
            IList<ILine> lines = ModelDAOHelper.DAOFactory.LineDAO.FindAllByComponent (componentIntermediateWorkPiece.Component);
            foreach (ILine line in lines) {
              IList<IWorkOrderLine> workOrderLines = ModelDAOHelper.DAOFactory.WorkOrderLineDAO
                .FindAllByLine (line);
              foreach (IWorkOrderLine workOrderLine in workOrderLines) {
                foreach (IWorkOrderLineQuantity quantity in workOrderLine.IntermediateWorkPieceQuantities.Values) {
                  if (object.Equals (quantity.IntermediateWorkPiece, other)) {
                    log.DebugFormat ("Merge: " +
                                     "in WorkOrderLine, move the quantity {0} from {1} to {2}",
                                     quantity.Quantity, other, this);
                    workOrderLine.AddIntermediateWorkPieceQuantity (this,
                                                                    quantity.Quantity);
                    workOrderLine.SetIntermediateWorkPieceQuantity (other,
                                                                    0);
                  }
                }
              }
            }
          }
        }
      }

      Mergeable.MergeAuto (this, other, conflictResolution);
      ModifyItems<IOperation> (other.PossibleNextOperations,
                               new Modifier<IOperation>
                               (delegate (IOperation operation) {
                                 this.AddPossibleNextOperation (operation);
                                 other.RemovePossibleNextOperation (operation);
                               }));
      // TODO: source / quantity
      MergeComponentIntermediateWorkPiece mergeComponentIntermediateWorkPiece =
        new MergeComponentIntermediateWorkPiece (this, other, conflictResolution);
      ModifyItems<IComponentIntermediateWorkPiece>
        (other.ComponentIntermediateWorkPieces,
         new Modifier<IComponentIntermediateWorkPiece> (mergeComponentIntermediateWorkPiece.MergeDelegate));
      // TODO: Restrictions following DataStructureOption
    }
    #endregion // Methods

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IOperation> (ref m_operation);
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[IntermediateWorkPiece {this.Id} Name={this.Name} Code={this.Code}]";
      }
      else {
        return $"[IntermediateWorkPiece {this.Id}]";
      }
    }

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IIntermediateWorkPiece other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      IntermediateWorkPiece other = obj as IntermediateWorkPiece;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode ();
        }
        return hashCode;
      }
      else {
        return base.GetHashCode ();
      }
    }

  }
}
