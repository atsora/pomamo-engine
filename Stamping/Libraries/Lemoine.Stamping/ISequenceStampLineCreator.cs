// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Stamp line creator that is using the sequence stamp variable
  /// </summary>
  public interface ISequenceStampLineCreator
  {
    /// <summary>
    /// Create a line that contains the stamp
    /// 
    /// Usually used with a variable
    /// </summary>
    /// <param name="stamp"></param>
    /// <returns></returns>
    string CreateSequenceStampLine (double stamp);
  }
}
