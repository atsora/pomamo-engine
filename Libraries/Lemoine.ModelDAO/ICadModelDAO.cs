// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ICadModel.
  /// </summary>
  public interface ICadModelDAO: IGenericUpdateDAO<ICadModel, int>
  {
    /// <summary>
    /// Find the Cad models for a specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    IList<ICadModel> FindAllWithComponent (IComponent component);
  }
}
