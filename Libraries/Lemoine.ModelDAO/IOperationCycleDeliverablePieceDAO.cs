// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IOperationCycleDeliverablePiece.
  /// </summary>
  public interface IOperationCycleDeliverablePieceDAO :
    IGenericByMachineUpdateDAO<IOperationCycleDeliverablePiece, int>
  {
    
    /// <summary>
    /// Find all operation cycle/deliverable piece associations
    /// for a given operation cycle
    /// </summary>
    /// <param name="operationCycle">not null</param>
    /// <returns></returns>
    IList<IOperationCycleDeliverablePiece>
      FindAllWithOperationCycle(IOperationCycle operationCycle);
    
    /// <summary>
    /// Returns unique operation cycle/deliverable piece association
    /// for a given operation cycle/deliverable piece pair
    /// if it exists
    /// </summary>
    /// <param name="operationCycle">not null</param>
    /// <param name="deliverablePiece">not null</param>
    /// <returns></returns>
    IOperationCycleDeliverablePiece
      FindWithOperationCycleDeliverablePiece(IOperationCycle operationCycle, IDeliverablePiece deliverablePiece);
    
    /// <summary>
    /// Find all operation cycle/deliverable piece associations
    /// for a given machine in a datetime range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="utcFrom"></param>
    /// <param name="utcTo"></param>
    /// <returns></returns>
    IList<IOperationCycleDeliverablePiece> FindAllInRangeByMachine(IMonitoredMachine machine, DateTime? utcFrom, DateTime? utcTo);
    
  }
}
