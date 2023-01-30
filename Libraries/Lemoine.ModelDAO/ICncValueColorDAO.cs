// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ICncValueColor.
  /// </summary>
  public interface ICncValueColorDAO
  {
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <param name="extend">Try to extend the slots</param>
    /// <returns></returns>
    ICncValueColor FindAt (IMachineModule machineModule, IField field, DateTime dateTime, bool extend);
    
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <param name="extend">Try to extend the slots</param>
    /// <returns></returns>
    IList<ICncValueColor> FindOverlapsRange (IMachineModule machineModule, IField field, UtcDateTimeRange range, bool extend);
  }
}
