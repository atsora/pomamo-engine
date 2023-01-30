// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.Business.Extension
{
  /// <summary>
  /// Request class to get extensions (from cache) that depend on a monitored machine
  /// </summary>
  public class MonitoredMachineExtensions<T>
    : ParameterExtensions<T, IMonitoredMachine>
    , IRequest<IEnumerable<T>>
    where T : Lemoine.Extensions.IExtension
  {
    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    public MonitoredMachineExtensions (IMonitoredMachine machine)
      : base (machine, m => m.Id.ToString ())
    {
    }

    /// <summary>
    /// Constructor with a filter lambda function
    /// 
    /// To be used when the Initialize method is available in the extension and returns true
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="filter"></param>
    public MonitoredMachineExtensions (IMonitoredMachine machine, Func<T, IMonitoredMachine, bool> filter)
      : base (machine, m => m.Id.ToString (), filter)
    {
    }
    #endregion // Constructors
  }
}
