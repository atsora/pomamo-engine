// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table OperationSourceWorkPiece
  /// 
  /// The associated table lists the different source work pieces
  /// that are necessary to run an operation.
  /// 
  /// From the operations, their source and destination work pieces,
  /// we can rebuild the work process to make a component.
  /// </summary>
  [Serializable]
  public class OperationSourceWorkPiece: IOperationSourceWorkPiece
  {
    #region Members
    IOperation m_operation = null;
    int m_version = 1;
    int m_id = 0;
    IIntermediateWorkPiece m_intermediateWorkPiece = null;
    int m_quantity = 1;
    int m_quantityDenominator = 1;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (OperationSourceWorkPiece).FullName);

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected OperationSourceWorkPiece ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="intermediateWorkPiece"></param>
    public OperationSourceWorkPiece (IOperation operation,
                                     IIntermediateWorkPiece intermediateWorkPiece)
    {
      Operation = operation;
      IntermediateWorkPiece = intermediateWorkPiece;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Reference to the Operation
    /// </summary>
    public virtual IOperation Operation {
      get { return m_operation; }
      protected set {
        if (value == null) {
          log.Error("Operation cannot be null");
          throw new ArgumentNullException("Operation");
        }
        m_operation = value;
      }
    }
    
    /// <summary>
    /// Reference to the intermediate work piece
    /// </summary>
    public virtual IIntermediateWorkPiece IntermediateWorkPiece {
      get { return m_intermediateWorkPiece; }
      protected set {
        if (value == null) {
          log.Error("IntermediateWorkPiece cannot be null");
          throw new ArgumentNullException("IntermediateWorkPiece");
        }
        m_intermediateWorkPiece = value;
      }
    }
    
    /// <summary>
    /// Number of the same work pieces that are needed for the operation
    /// 
    /// Default is 1
    /// </summary>
    public virtual int Quantity {
      get { return m_quantity; }
      set { m_quantity = value; }
    }
    
    /// <summary>
    /// Denominator of the number of intermediate workpieces needed for an operation
    /// 
    /// Default is 1, cannot be 0 or less
    /// </summary>
    public virtual int QuantityDenominator {
      get { return m_quantityDenominator; }
      set { m_quantityDenominator = value; }
    }
    
    /// <summary>
    /// OperationSourceWorkPiece id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// OperationSourceWorkPiece Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }
    #endregion // Getters / Setters
    
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        if (this.Operation != null) {
          hashCode += 1000000007 * this.Operation.GetHashCode();
        }

        if (this.IntermediateWorkPiece != null) {
          hashCode += 1000000009 * this.IntermediateWorkPiece.GetHashCode();
        }
      }
      return hashCode;
    }
    
    /// <summary>
    /// <see cref="Object.Equals(object)" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      IOperationSourceWorkPiece other = obj as OperationSourceWorkPiece;
      if (other == null) {
        return false;
      }

      return object.Equals(this.Operation, other.Operation)
        && object.Equals(this.IntermediateWorkPiece, other.IntermediateWorkPiece);
    }
  }
}
