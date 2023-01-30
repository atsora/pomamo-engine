// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IProjectComponentUpdate.
  /// </summary>
  public interface IProjectComponentUpdateDAO: IGenericDAO<IProjectComponentUpdate, long>
  {
    /// <summary>
    /// Find all the ProjectComponentUpdate for a specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    IList<IProjectComponentUpdate> FindAllWithComponent (IComponent component);
  }
}
