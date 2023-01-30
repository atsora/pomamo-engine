// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// 
  /// </summary>
  public interface IWorkOrderExtension: Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Additional actions to take when two work orders are merged
    /// </summary>
    /// <param name="oldWorkOrder"></param>
    /// <param name="newWorkOrder"></param>
    void Merge (IWorkOrder oldWorkOrder,
                IWorkOrder newWorkOrder);
  }
}
