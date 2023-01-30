// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IOperationSourceWorkPiece.
  /// </summary>
  public interface IOperationSourceWorkPieceDAO: IGenericDAO<IOperationSourceWorkPiece, int>
  {
    /// <summary>
    /// Try to get the OperationSourceWorkPiece entities related to an operation
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    IList<IOperationSourceWorkPiece> GetOperationSourceWorkPiece(IOperation operation);
  }
}
