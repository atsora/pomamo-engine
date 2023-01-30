// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineStateTemplateAssociation.
  /// </summary>
  public interface IMachineStateTemplateAssociationDAO: IGenericByMachineDAO<IMachineStateTemplateAssociation, long>
  {
    /// <summary>
    /// Get all MachineStateTemplateAssociation for a specific machine within a period
    /// Valid segments have:
    /// - their beginning strictly inferior to the end of the period, AND
    /// - their end strictly superior to the beginning of the period
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    IList<IMachineStateTemplateAssociation> FindByMachineAndPeriod(IMachine machine, DateTime start, DateTime end);
  }
}
