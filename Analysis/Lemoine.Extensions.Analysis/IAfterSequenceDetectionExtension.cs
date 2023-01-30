// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Analysis
{
  /// <summary>
  /// Extension to call a method after a call to ISequenceDetection
  /// </summary>
  public interface IAfterSequenceDetectionExtension: Detection.ISequenceDetection, IExtension
  {
    /// <summary>
    /// Set the machine module
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    bool Initialize (IMachineModule machineModule);
  }
}
