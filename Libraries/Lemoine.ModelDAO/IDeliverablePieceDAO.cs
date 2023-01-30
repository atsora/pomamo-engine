// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Description of IDeliverablePieceDAO.
  /// </summary>
  public interface IDeliverablePieceDAO
    : IGenericUpdateDAO<IDeliverablePiece, int>
  {
    /// <summary>
    /// Find a Deliverablepiece using its serial number(code) and component
    /// </summary>
    IDeliverablePiece FindByCodeAndComponent (string serialNumber, IComponent component);

    /// <summary>
    /// Find all  Deliverablepieces matching a serial number code
    /// </summary>
    IList<IDeliverablePiece> FindByCode (string serialNumber);

    /// <summary>
    /// Find all the deliverable pieces that are associated to an operation cycle
    /// </summary>
    /// <param name="operationCycle">not null</param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IEnumerable<IDeliverablePiece>> FindByOperationCycleAsync (IOperationCycle operationCycle);
  }
}
