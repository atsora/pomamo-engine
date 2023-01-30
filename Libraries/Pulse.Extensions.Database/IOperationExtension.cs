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
  public interface IOperationExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Additional actions to take when two operations are merged
    /// </summary>
    /// <param name="oldOperation">not null</param>
    /// <param name="newOperation">not null</param>
    void Merge (IOperation oldOperation,
                IOperation newOperation);
  }
}
