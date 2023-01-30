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
  /// Request class to get extensions (from cache) that depend on a machine module
  /// </summary>
  public class MachineModuleExtensions<T>
    : ParameterExtensions<T, IMachineModule>
    , IRequest<IEnumerable<T>>
    where T : Lemoine.Extensions.IExtension
  {
    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule"></param>
    public MachineModuleExtensions (IMachineModule machineModule)
      : base (machineModule, m => m.Id.ToString ())
    {
    }

    /// <summary>
    /// Constructor with a filter lambda function
    /// 
    /// To be used when the Initialize method is available in the extension and returns true
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="filter"></param>
    public MachineModuleExtensions (IMachineModule machineModule, Func<T, IMachineModule, bool> filter)
      : base (machineModule, m => m.Id.ToString (), filter)
    {
    }
    #endregion // Constructors
  }
}
