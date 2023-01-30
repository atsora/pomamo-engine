// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineSubCategory.
  /// </summary>
  public interface IMachineSubCategoryDAO: IGenericUpdateDAO<IMachineSubCategory, int>
  {
    /// <summary>
    /// Get all the items sorted by their Id
    /// </summary>
    /// <returns></returns>
    IList<IMachineSubCategory> FindAllSortedById ();
  }
}
