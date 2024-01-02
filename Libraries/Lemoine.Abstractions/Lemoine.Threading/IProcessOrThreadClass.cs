// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;

namespace Lemoine.Threading
{
  /// <summary>
  /// Interface to a class whose main method is called in a thread or in a process
  /// </summary>
  public interface IProcessOrThreadClass: IProcessClass, IThreadClass
  {
  }
}
