// Copyright (C) 2026 Atsora Solutions

using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.Extensions.AutoReason.ActionableAutoReasonExtension
{
  /// <summary>
  /// Auto machine state template extension that is able to apply a machine state template
  /// </summary>
  public interface IApplyMachineStateTemplateAutoExtension
    : IAutoMachineStateTemplateExtension
  {
    /// <summary>
    /// Apply the specified machine state template to the specified range with the dynamic times
    /// 
    /// It needs to be run in a transaction
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineStateTemplate">not null</param>
    /// <param name="nextMachineStateTemplate">machine state template to apply once the dynamic end is reached. Nullable</param>
    /// <param name="range"></param>
    /// <param name="dynamic">dynamic times (start/end) description: start,end</param>
    /// <param name="option"></param>
    void ApplyMachineStateTemplate (IMachine machine, IMachineStateTemplate machineStateTemplate, IMachineStateTemplate nextMachineStateTemplate, UtcDateTimeRange range, string dynamic, AssociationOption? option);
  }
}
