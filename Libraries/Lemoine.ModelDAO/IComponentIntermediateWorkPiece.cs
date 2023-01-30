// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table ComponentIntermediateWorkPiece
  /// 
  /// This lists the different association possibilities
  /// between a Component and an IntermediateWorkPiece.
  /// </summary>
  public interface IComponentIntermediateWorkPiece: IDataWithIdentifiers, IVersionable
  {
    /// <summary>
    /// Reference to the related component
    /// 
    /// Be careful when set is used ! This is part of the key
    /// </summary>
    IComponent Component { get; }
    
    /// <summary>
    /// Reference to the related intermediate work piece
    /// 
    /// Be careful when set is used ! This is part of the key
    /// </summary>
    IIntermediateWorkPiece IntermediateWorkPiece { get; }
    
    /// <summary>
    /// For a given component, code that is associated to an intermediate work piece
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// For a given component, order in which the different intermediate work pieces must be made
    /// </summary>
    int? Order { get; set; }
  }
}
