// Copyright (c) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;

namespace Lemoine.Threading
{
  /// <summary>
  /// Logged thread class
  /// </summary>
  public interface ILoggedThreadClass: IThreadClass, ILogged
  {
  }
}
