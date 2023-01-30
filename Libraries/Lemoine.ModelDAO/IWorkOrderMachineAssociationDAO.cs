// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  ///  DAO interface for IWorkOrderMachineAssociation.
  /// </summary>
  public interface IWorkOrderMachineAssociationDAO
    : IGenericByMachineDAO<IWorkOrderMachineAssociation, long>
  {
    /// <summary>
    /// Find the matching work order / machine association
    /// for a given machine, work order and begin date
    /// if it exists
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="workOrder"></param>
    /// <param name="startDate"></param>
    /// <returns></returns>
    IWorkOrderMachineAssociation FindMatching(IMachine machine, IWorkOrder workOrder, DateTime startDate);
  }
}
