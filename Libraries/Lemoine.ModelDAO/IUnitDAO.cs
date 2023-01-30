// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IUnit.
  /// </summary>
  public interface IUnitDAO: IGenericUpdateDAO<IUnit, int>
  {
    /// <summary>
    /// Find a unit with the enum
    /// </summary>
    /// <param name="unitId"></param>
    /// <returns></returns>
    IUnit FindByUnitId (UnitId unitId);
  }
}
