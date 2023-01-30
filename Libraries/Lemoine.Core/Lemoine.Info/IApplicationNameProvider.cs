// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Info
{
  /// <summary>
  /// Interface to provide an application name
  /// </summary>
  public interface IApplicationNameProvider
  {
    string ApplicationName { get; }
  }
}
