// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IDisplay.
  /// </summary>
  public interface IDisplayDAO: IGenericUpdateDAO<IDisplay, int>
  {
    /// <summary>
    /// Find the IDisplay object for the given table and the null variant
    /// 
    /// Return null if not found
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="table">not null or empty</param>
    /// <returns></returns>
    IDisplay FindWithTable (string table);
    
    /// <summary>
    /// Find the IDisplay object for the specified table and variant
    /// 
    /// Return null if not found
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="table">not null or empty</param>
    /// <param name="variant">nullable</param>
    /// <returns></returns>
    IDisplay FindWithTableVariant (string table, string variant);

    /// <summary>
    /// Get the pattern to use for the given table and pattern
    /// 
    /// If the specified table was not found,
    /// return an empty string
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    string GetPattern (string table);

    /// <summary>
    /// Get the pattern to use for the given table, pattern and display
    /// 
    /// If the specified table was not found,
    /// return an empty string
    /// </summary>
    /// <param name="table"></param>
    /// <param name="variant"></param>
    /// <returns></returns>
    string GetPatternWithDefault (string table, string variant);
  }
}
