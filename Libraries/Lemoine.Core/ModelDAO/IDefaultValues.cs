// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// 
  /// </summary>
  public interface IDefaultValues
  {
    /// <summary>
    /// Complete this method with all the default values
    /// </summary>
    /// <returns>false if only partially completed</returns>
    bool InsertDefaultValues ();

    /// <summary>
    /// Complete this method with all the default values
    /// </summary>
    /// <returns>false if only partially completed</returns>
    System.Threading.Tasks.Task<bool> InsertDefaultValuesAsync ();
  }
}
