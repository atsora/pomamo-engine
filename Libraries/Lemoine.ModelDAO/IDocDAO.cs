// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for <see cref="IDoc"/>
  /// </summary>
  public interface IDocDAO : IGenericUpdateDAO<IDoc, int>
  {
    /// <summary>
    /// Find the unique <see cref="IDoc"/> with the specified path
    /// 
    /// null is returned if no document matches the path
    /// </summary>
    /// <param name="path">not null and not empty</param>
    /// <returns>nullable</returns>
    IDoc FindByPath (string path);

    /// <summary>
    /// Asynchronous version of <see cref="FindByPath(string)"/>
    /// </summary>
    /// <param name="path">not null and not empty</param>
    /// <returns>nullable</returns>
    Task<IDoc> FindByPathAsync (string path);

    /// <summary>
    /// Find the <see cref="IDoc"/> with the specified path,
    /// creating and making persistent a new one if it does not exist yet
    /// </summary>
    /// <param name="path">not null and not empty</param>
    /// <returns>not null</returns>
    IDoc FindOrCreateByPath (string path);
  }
}
