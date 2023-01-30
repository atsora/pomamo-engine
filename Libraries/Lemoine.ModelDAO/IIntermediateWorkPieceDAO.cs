// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Collections.Generic;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Description of IIsoFileDAO.
  /// </summary>
  public interface IIntermediateWorkPieceDAO : IGenericUpdateDAO<IIntermediateWorkPiece, int>
    , IMergeDAO<IIntermediateWorkPiece>
  {
    /// <summary>
    /// Get orphans IntermediateWorkPiece, means IntermediateWorkPiece without link to Component
    /// </summary>
    /// <returns></returns>
    IList<IIntermediateWorkPiece> GetOrphans();
    
    /// <summary>
    /// Initialize the possible next operations
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    void InitializePossibleNextOperations (IIntermediateWorkPiece intermediateWorkPiece);
    
    /// <summary>
    /// Find all ipw by operation
    /// </summary>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    IList<IIntermediateWorkPiece> FindByOperation(IOperation operation);

    /// <summary>
    /// Find intermediate work pieces given its component and its order 
    /// </summary>
    /// <param name="component"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    IEnumerable<IIntermediateWorkPiece> FindWithOrderForComponent (IComponent component, int order);
  }
}
