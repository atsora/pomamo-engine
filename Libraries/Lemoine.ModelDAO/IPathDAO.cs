// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IPath.
  /// </summary>
  public interface IPathDAO: IGenericUpdateDAO<IPath, int>
  {
    /// <summary>
    /// Find path for a given operation and number
    /// </summary>
    /// <returns></returns>
    IPath FindByOperationAndNumber (IOperation operation, int pathNumber);
    
    /// <summary>
    /// FindAll paths for a given Operation
    /// sorted by Number
    /// </summary>
    /// <returns></returns>
    IList<IPath> FindAllWithOperation (IOperation operation);
    
    /// <summary>
    /// Initialize the associated sequences
    /// </summary>
    /// <param name="path"></param>
    void InitializeSequences (IPath path);
  }
}
