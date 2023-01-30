// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Response DTO for CycleProgress
  /// 
  /// If not applicable (since no cycle is detected on the machine),
  /// an error DTO is returned instead
  /// </summary>
  [Api ("CycleProgress Response DTO")]
  public class CycleProgressResponseDTO
  {
    /// <summary>
    /// No effective operation is detected on the machine
    /// </summary>
    public bool? NoEffectiveOperation { get; set; }

    /// <summary>
    /// The detected cycle is invalid
    /// </summary>
    public bool? InvalidCycle { get; set; }

    /// <summary>
    /// Time of the data
    /// </summary>
    public string DataTime { get; set; }

    /// <summary>
    /// Current operation
    /// </summary>
    public OperationDTO Operation { get; set; }

    /// <summary>
    /// Data by machine module
    /// 
    /// null if request.Light is true
    /// </summary>
    public List<CycleProgressByMachineModuleDTO> ByMachineModule { get; set; }

    /// <summary>
    /// Coming events (ordered by ascending estimated date/time): next stop, cycle completion, ...
    /// </summary>
    public List<EventDTO> ComingEvents { get; set; }

    /// <summary>
    /// Current active events: active stop, completed cycle, ...
    /// Set by order of importance, more important active event first
    /// </summary>
    public List<EventDTO> ActiveEvents { get; set; }

    /// <summary>
    /// Start date/time of the cycle, if applicable
    /// </summary>
    public string CycleStartDateTime { get; set; }

    /// <summary>
    /// Estimated time of the next cycle end, if known
    /// 
    /// Note: please consider now the ComingEvents property to report when a cycle end is supposed to come
    /// </summary>
    public string EstimatedCycleEndDateTime { get; set; }

    /// <summary>
    /// Completion % (between 0 and 1) of the cycle, only if applicable
    /// </summary>
    public double? Completion { get; set; }

    /// <summary>
    /// Possible errors
    /// </summary>
    public List<string> Errors { get; set; }

    /// <summary>
    /// Possible warnings
    /// </summary>
    public List<string> Warnings { get; set; }

    /// <summary>
    /// Message on the data
    /// </summary>
    public List<string> Notices { get; set; }

    /// <summary>
    /// Current machine activity if the machine is not stopped and there is an estimated next stop or cycle end date/time
    /// </summary>
    public bool? Running { get; set; }
  }

  /// <summary>
  /// Response DTO for CycleProgress by machine module
  /// </summary>
  public class CycleProgressByMachineModuleDTO
  {
    /// <summary>
    /// Reference to the machine module
    /// </summary>
    public MachineModuleDTO MachineModule { get; set; }

    /// <summary>
    /// Current sequence, with its start date/time
    /// </summary>
    public CycleProgressCurrentSequenceDTO Current { get; set; }

    /// <summary>
    /// List of sequences to process for this machine module
    /// </summary>
    public List<CycleProgressSequenceDTO> Sequences { get; set; }
  }

  /// <summary>
  /// Response DTO for the current sequence
  /// </summary>
  public class CycleProgressCurrentSequenceDTO : SequenceDTO
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public CycleProgressCurrentSequenceDTO () { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="sequence"></param>
    internal protected CycleProgressCurrentSequenceDTO (ISequence sequence)
      : base (sequence)
    {
    }

    /// <summary>
    /// Begin date/time of the current sequence
    /// </summary>
    public string Begin { get; set; }

    /// <summary>
    /// Completion % for the current sequence, if a standard duration is set
    /// 
    /// Warning: it may be greater than 1 if the current sequence is late
    /// </summary>
    public double? Completion { get; set; }
  }

  /// <summary>
  /// Sequence DTO for cycle progress
  /// </summary>
  public class CycleProgressSequenceDTO : SequenceDTO
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public CycleProgressSequenceDTO () { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="sequence"></param>
    internal protected CycleProgressSequenceDTO (ISequence sequence)
      : base (sequence)
    {
    }

    /// <summary>
    /// Is this sequence, the current sequence ?
    /// </summary>
    public bool IsCurrent { get; set; }

    /// <summary>
    /// Is the sequence completed for the current cycle
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Begin % in an operation bar or pie
    /// </summary>
    public double? BeginPercent { get; set; }

    /// <summary>
    /// End % in an operation bar or pie
    /// 
    /// Warning: it may be greater than 1.0
    /// </summary>
    public double? EndPercent { get; set; }
  }
}
