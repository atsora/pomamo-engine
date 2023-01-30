// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Business
{
  /// <summary>
  /// Interface to set new Performance Recorders in plugins
  /// </summary>
  public interface IPerfRecorderExtension
    : Lemoine.Core.Performance.IPerfRecorder
    , Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the extension
    /// </summary>
    /// <returns></returns>
    bool Initialize ();
  }
}
