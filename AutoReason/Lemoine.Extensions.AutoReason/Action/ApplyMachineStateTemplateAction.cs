// Copyright (C) 2026 Atsora Solutions

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason.ActionableAutoReasonExtension;
using Lemoine.Model;

namespace Lemoine.Extensions.AutoReason.Action
{
  /// <summary>
  /// Action to apply a machine state template on a specified range
  /// </summary>
  public class ApplyMachineStateTemplateAction : IMachineStateTemplateAction
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplyMachineStateTemplateAction).FullName);

    readonly int m_commitNumber = 0;
    readonly IApplyMachineStateTemplateAutoExtension m_extension;
    readonly IMachine m_machine;
    readonly IMachineStateTemplate m_machineStateTemplate;
    readonly IMachineStateTemplate m_nextMachineStateTemplate;
    readonly UtcDateTimeRange m_range;
    readonly string m_dynamic;
    readonly AssociationOption? m_option;

    /// <summary>
    /// Constructor with the default machine and machine state templates of the extension
    /// </summary>
    /// <param name="extension">not null</param>
    /// <param name="range"></param>
    /// <param name="dynamic">dynamic times (start/end) description: start,end</param>
    /// <param name="option"></param>
    /// <param name="commitNumber"></param>
    public ApplyMachineStateTemplateAction (IApplyMachineStateTemplateAutoExtension extension, UtcDateTimeRange range, string dynamic = "", AssociationOption? option = null, int commitNumber = 0)
      : this (extension, extension.Machine, extension.MachineStateTemplate, extension.NextMachineStateTemplate, range, dynamic, option, commitNumber: commitNumber)
    {
    }

    /// <summary>
    /// Constructor with an alternative machine and alternative machine state templates
    /// </summary>
    /// <param name="extension">not null</param>
    /// <param name="machine">not null</param>
    /// <param name="machineStateTemplate">not null</param>
    /// <param name="nextMachineStateTemplate">nullable</param>
    /// <param name="range"></param>
    /// <param name="dynamic">dynamic times (start/end) description: start,end</param>
    /// <param name="option"></param>
    /// <param name="commitNumber"></param>
    public ApplyMachineStateTemplateAction (IApplyMachineStateTemplateAutoExtension extension, IMachine machine, IMachineStateTemplate machineStateTemplate, IMachineStateTemplate nextMachineStateTemplate, UtcDateTimeRange range, string dynamic = "", AssociationOption? option = null, int commitNumber = 0)
    {
      Debug.Assert (null != extension);
      Debug.Assert (null != machine);
      Debug.Assert (null != machineStateTemplate);

      m_commitNumber = commitNumber;
      m_extension = extension;
      m_machine = machine;
      m_machineStateTemplate = machineStateTemplate;
      m_nextMachineStateTemplate = nextMachineStateTemplate;
      m_range = range;
      m_dynamic = dynamic;
      m_option = option;
    }

    /// <summary>
    /// <see cref="IAutoReasonAction.Name"/>
    /// </summary>
    public string Name => "ApplyMachineStateTemplate";

    /// <summary>
    /// <see cref="IAutoReasonAction.CommitNumber"/>
    /// </summary>
    public int CommitNumber => m_commitNumber;

    /// <summary>
    /// <see cref="IAutoReasonAction.Run"/>
    /// </summary>
    public void Run ()
    {
      m_extension.ApplyMachineStateTemplate (m_machine, m_machineStateTemplate, m_nextMachineStateTemplate, m_range, m_dynamic, m_option);
    }
  }
}
