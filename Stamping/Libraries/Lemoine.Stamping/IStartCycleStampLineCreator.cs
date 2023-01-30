// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Stamp line creator using the start cycle variable
  /// </summary>
  public interface IStartCycleStampLineCreator
  {
    /// <summary>
    /// Create a line that contains the stamp
    /// 
    /// Usually used with a variable
    /// </summary>
    /// <param name="stamp"></param>
    /// <returns></returns>
    string CreateStartCycleStampLine (double stamp);
  }
}
