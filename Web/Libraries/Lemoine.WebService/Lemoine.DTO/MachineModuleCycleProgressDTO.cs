// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO for MachineModuleCycleProgress: information about sequence progress in
  /// current cycle on machine module
  /// </summary>
  public class MachineModuleCycleProgressDTO
  {    
    /// <summary>
    /// Id of machine module
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Name of machine module
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Sequence information
    /// </summary>
    public List<SequenceInCycleStateDTO> SeqInCycleStateList { get; set; }
    
    /// <summary>
    /// Next stop information if available (only next one, in current cycle); may be null
    /// </summary>
    public NextStopDTO NextStop { get; set; }

  }
}
