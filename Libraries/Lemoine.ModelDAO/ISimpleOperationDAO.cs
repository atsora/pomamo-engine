// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ISimpleOperation.
  /// </summary>
  public interface ISimpleOperationDAO: IGenericUpdateDAO<ISimpleOperation, int>
    , IMergeDAO<ISimpleOperation>
  {
    /// <summary>
    /// Find a simple operation by its OperationId
    /// </summary>
    /// <param name="operationId"></param>
    /// <returns></returns>
    ISimpleOperation FindByOperationId (int operationId);

    /// <summary>
    /// Find a simple operation by its OperationId asynchronously
    /// </summary>
    /// <param name="operationId"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<ISimpleOperation> FindByOperationIdAsync (int operationId);

    /// <summary>
    /// Find a simple operation by its IntermediateWorkPieceId
    /// </summary>
    /// <param name="intermediateWorkPieceId"></param>
    /// <returns></returns>
    ISimpleOperation FindByIntermediateWorkPieceId (int intermediateWorkPieceId);

    /// <summary>
    /// Find a simple operation by its IntermediateWorkPieceId asynchronously
    /// </summary>
    /// <param name="intermediateWorkPieceId"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<ISimpleOperation> FindByIntermediateWorkPieceIdAsync (int intermediateWorkPieceId);

    /// <summary>
    /// Find a simple operation given its component and the order
    /// </summary>
    /// <param name="component">not null</param>
    /// <param name="order"></param>
    /// <returns></returns>
    IEnumerable<ISimpleOperation> FindWithOrderForComponent (IComponent component, int order);
  }
}
