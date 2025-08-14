// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IManufacturingOrder.
  /// </summary>
  public interface IManufacturingOrderDAO: IGenericUpdateDAO<IManufacturingOrder, int>
  {
    /// <summary>
    /// Find all the manufacturing orders matching a specific operation
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    IList<IManufacturingOrder> FindAllByOperation (IOperation operation);

    /// <summary>
    /// Find all the manufacturing orders matching a specific component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    IList<IManufacturingOrder> FindAllByComponent (IComponent component);

    /// <summary>
    /// Find all the manufacturing orders matching a specific work order
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    IList<IManufacturingOrder> FindAllByWorkOrder (IWorkOrder workOrder);

    /// <summary>
    /// Get all the next possible manufacturing orders that match the specified machine and operation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    IList<IManufacturingOrder> GetNext (IMachine machine,
                          IOperation operation);
  }
}
