// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IOperation.
  /// </summary>
  public interface IOperationDAO: IGenericUpdateDAO<IOperation, int>
    , IMergeDAO <IOperation>
  {
    /// <summary>
    /// Find all the operations from a given operation code
    /// </summary>
    /// <param name="code">not null</param>
    /// <returns></returns>
    IList<IOperation> FindByCode (string code);

    /// <summary>
    /// Return the completion of an operation when the specified sequence starts
    /// </summary>
    /// <param name="sequence">not null</param>
    /// <returns></returns>
    double? GetCompletion (ISequence sequence);

    /// <summary>
    /// Find the operation with the specified id with an eager fetch of its sequences
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IOperation FindByIdWithSequences (int id);

    /// <summary>
    /// Initialize the associated intermediate workpieces
    /// </summary>
    /// <param name="operation"></param>
    void InitializeIntermediateWorkPieces (IOperation operation);

    /// <summary>
    /// Find the items whose name, code or external code contains the text, whatever the case,
    /// the archived ones last, then in the order of their ids
    /// </summary>
    /// <param name="search">not null or empty</param>
    /// <param name="maxResults">maximum number of items to return</param>
    /// <returns>not null</returns>
    System.Threading.Tasks.Task<IList<IOperation>> FindBySearchAsync (string search, int maxResults);
  }
}
