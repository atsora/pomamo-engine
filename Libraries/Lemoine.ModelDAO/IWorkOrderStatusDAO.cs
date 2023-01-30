// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IWorkOrderStatus.
  /// </summary>
  public interface IWorkOrderStatusDAO: IGenericUpdateDAO<IWorkOrderStatus, int>
  {
    
    /// <summary>
    /// Get List of WorkOrderStatus ordered by name
    /// </summary>
    /// <returns>List of WorkOrderStatus ordered by name</returns>
    IList<IWorkOrderStatus> FindAllOrderByName();

    /// <summary>
    /// Get work order statuses for a list of Ids
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IDictionary<int, IWorkOrderStatus>> GetWorkOrderStatusesByIdAsync (IEnumerable<int> ids, CancellationToken cancellationToken);
  }
}
