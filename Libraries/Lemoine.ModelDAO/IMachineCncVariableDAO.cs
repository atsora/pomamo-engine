// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineCncVariable.
  /// </summary>
  public interface IMachineCncVariableDAO : IGenericUpdateDAO<IMachineCncVariable, int>
  {
    /// <summary>
    /// Find the variable from the machine / key / value
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    IMachineCncVariable FindByKeyValue (IMachine machine, string key, object value);
  }
}
