// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Interface to implement to parse a comment
  /// </summary>
  public interface ICommentParser
  {
    /// <summary>
    /// Parse a comment
    /// </summary>
    /// <param name="comment"></param>
    /// <param name=""></param>
    /// <returns></returns>
    bool ParseComment (string comment, StampingData stampingData);
  }
}
