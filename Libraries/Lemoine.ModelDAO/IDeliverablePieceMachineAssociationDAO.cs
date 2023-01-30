// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IDeliverablePieceMachineAssociationDAO.
  /// </summary>
  public interface IDeliverablePieceMachineAssociationDAO
    :  IGenericDAO<IDeliverablePieceMachineAssociation, int>
  {
    /// <summary>
    /// Find all deliverable piece / machine associations
    /// for a given machine and deliverable piece serial number (code)
    /// with begin date less than or equal to endDate,
    /// and with null end date,
    /// ordered on ascending begin date
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="endDate"></param>
    /// <param name="serialNumber"></param>
    /// <returns></returns>
    IDeliverablePieceMachineAssociation FindMatching(IMachine machine,
                                                     DateTime endDate,
                                                     string serialNumber);
  }
}

