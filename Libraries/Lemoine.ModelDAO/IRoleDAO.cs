// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IRole.
  /// </summary>
  public interface IRoleDAO: IGenericUpdateDAO<IRole, int>
  {
    /// <summary>
    /// Get a role given its key
    /// </summary>
    /// <param name="roleKey"></param>
    /// <returns></returns>
    Task<IRole> FindByKeyAsync (string roleKey, CancellationToken cancellationToken = default);
  }
}
