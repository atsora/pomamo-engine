// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IReason.
  /// </summary>
  public interface IReasonGroupDAO: IGenericUpdateDAO<IReasonGroup, int>
  {
    /// <summary>
    /// Get all the items sorted by their Id
    /// </summary>
    /// <returns></returns>
    IList<IReasonGroup> FindAllSortedById ();

    /// <summary>
    /// FindAll implementation
    /// with an eager fetch of the corresponding reasons
    /// </summary>
    /// <returns></returns>
    IList<IReasonGroup> FindAllWithReasons ();
  }
}

