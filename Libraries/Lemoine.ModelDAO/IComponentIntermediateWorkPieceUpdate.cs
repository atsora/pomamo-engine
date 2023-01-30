// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Accepted types of modification
  /// </summary>
  public enum ComponentIntermediateWorkPieceUpdateModificationType {
    /// <summary>
    /// New component / intermediate work piece association
    /// </summary>
    NEW = 1,
    /// <summary>
    /// Remove a component / intermediate work piece association
    /// </summary>
    DELETE = 2
  };

  /// <summary>
  /// Model for table ComponentIntermediateWorkPieceUpdate
  /// 
  /// This table tracks the modifications in the relations
  /// between a Component and an IntermediateWorkPiece
  /// that are made in table ComponentIntermediateWorkPiece
  /// 
  /// It is necessary to allow the Analyzer service
  /// to update correctly all the Analysis tables.
  /// </summary>
  public interface IComponentIntermediateWorkPieceUpdate: IGlobalModification
  {
    /// <summary>
    /// Component
    /// 
    /// Not null
    /// </summary>
    IComponent Component { get; set; }
    
    /// <summary>
    /// Intermediate work piece
    /// 
    /// Not null
    /// </summary>
    IIntermediateWorkPiece IntermediateWorkPiece { get; set; }
    
    /// <summary>
    /// Modification type
    /// </summary>
    ComponentIntermediateWorkPieceUpdateModificationType TypeOfModification { get; }
  }
}
