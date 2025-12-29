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
    /// Insert a new row in database with a new machine state template association
    /// </summary>
    long Insert (IMachine machine, UtcDateTimeRange range, IMachineStateTemplate machineStateTemplate);

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

    /// <summary>
    /// Insert a row in database that corresponds to a sub-modification
    /// 
    /// Check first range is not null
    /// </summary>
    /// <param name="association"></param>
    /// <param name="range">not null</param>
    /// <param name="preChange"></param>
    /// <param name="parent">optional: alternative parent</param>
    /// <returns></returns>
    IMachineModification InsertSub (IMachineStateTemplateAssociation association, UtcDateTimeRange range, Action<IMachineStateTemplateAssociation> preChange, IMachineModification parent);
  }
}
