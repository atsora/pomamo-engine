// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Interface to implement to interpret a some data, for example a CAD name or a file name
  /// </summary>
  public interface IDataInterpreter
  {
    /// <summary>
    /// Interpret the data
    /// </summary>
    /// <param name="stampingData"></param>
    /// <returns></returns>
    bool Interpret (StampingData stampingData);
  }
}
