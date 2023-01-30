// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineCategory.
  /// </summary>
  public interface IMachineCategoryDAO: IGenericUpdateDAO<IMachineCategory, int>
  {
    /// <summary>
    /// Get all the items sorted by their Id
    /// </summary>
    /// <returns></returns>
    IList<IMachineCategory> FindAllSortedById ();
  }
}
