// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IPart.
  /// </summary>
  public interface IPartDAO: IGenericUpdateDAO<IPart, int>
    , IMergeDAO<IPart>
  {
    /// <summary>
    /// Find a part by its ComponentId
    /// </summary>
    /// <param name="componentId"></param>
    /// <returns></returns>
    IPart FindByComponentId (int componentId);

    /// <summary>
    /// Find a part by its ProjectId
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    IPart FindByProjectId (int projectId);
    
    /// <summary>
    /// Return the first part that matches the specified name
    /// </summary>
    IPart FindByName (string name);

    /// <summary>
    /// Find a part by its code (unique)
    /// </summary>
    IPart FindByCode (string code);
  }
}
