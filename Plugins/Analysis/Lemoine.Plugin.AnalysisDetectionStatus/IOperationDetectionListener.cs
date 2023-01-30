// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Lemoine.Plugin.AnalysisDetectionStatus
{
  /// <summary>
  /// 
  /// </summary>
  public interface IOperationDetectionListener
  {
    IMachine Machine { get; }

    void NotifyOperationDetection (IMachine machine, DateTime dateTime);
  }
}
