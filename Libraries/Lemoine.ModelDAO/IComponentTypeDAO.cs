// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Collections.Generic;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO for IComponentTypeDAO.
  /// </summary>
  public interface IComponentTypeDAO: IGenericUpdateDAO<IComponentType, int>
  {
    /// <summary>
    /// Get all ComponentType ordered by name
    /// </summary>
    /// <returns></returns>
    IList<IComponentType> FindAllOrderByName();

    /// <summary>
    /// Get the unique component type with the specified name
    /// </summary>
    /// <returns>not null or empty</returns>
    IComponentType FindByName (string name);

    /// <summary>
    /// Get the unique component type with the specified code
    /// </summary>
    /// <returns>not null or empty</returns>
    IComponentType FindByCode (string code);
  }
}
