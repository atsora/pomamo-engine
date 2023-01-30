// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ICompany.
  /// </summary>
  public interface ICompanyDAO: IGenericUpdateDAO<ICompany, int>
  {
    /// <summary>
    /// Get all the items sorted by their Id
    /// </summary>
    /// <returns></returns>
    IList<ICompany> FindAllSortedById ();
  }
}
