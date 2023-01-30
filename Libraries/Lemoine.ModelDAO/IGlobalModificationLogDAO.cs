// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IGlobalModificationLog.
  /// </summary>
  public interface IGlobalModificationLogDAO: IGenericDAO<IGlobalModificationLog, int>
  {
    /// <summary>
    /// Add a new analysis log for a specified modification, level and message
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="level"></param>
    /// <param name="message"></param>
    void Add (IGlobalModification modification,
              LogLevel level,
              string message);
  }
}
