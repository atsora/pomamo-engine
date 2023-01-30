// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Collections.Generic;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IOperationTypeDAO.
  /// </summary>
  public interface IOperationTypeDAO: IGenericUpdateDAO<IOperationType, int>
  {
    /// <summary>
    /// Get All OperationType ordered by name
    /// </summary>
    /// <returns></returns>
    IList<IOperationType> FindAllOrderByName();

  }
}
