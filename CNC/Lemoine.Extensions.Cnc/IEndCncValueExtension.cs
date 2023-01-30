// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.Extensions.Cnc
{
  /// <summary>
  /// Extension to overwrite the default behavior whether a cnc value should be ended or not
  /// </summary>
  public interface IEndCncValueExtension: Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="machineModule"></param>
    bool Initialize (IMachineModule machineModule);
    
    /// <summary>
    /// Should a cnc value be ended ?
    /// 
    /// If null is returned, leave the default behavior
    /// </summary>
    /// <param name="fieldCode"></param>
    /// <param name="previousDateTime"></param>
    /// <param name="newDateTime"></param>
    /// <param name="previousStopped">Previous stopped status</param>
    /// <returns></returns>
    bool? IsEndCncValueRequested (string fieldCode,
                                  DateTime previousDateTime,
                                  DateTime newDateTime,
                                  bool previousStopped);
  }
}
