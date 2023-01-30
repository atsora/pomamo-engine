// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  ///  DTO for ReasonSlot
  /// this version represents DateTime in ISO format
  /// </summary>
  public class ReasonSlotV2DTO
  {
    /// <summary>
    /// Id of slot
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Begin of reason slot in ISO format
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// (Opt.) End of reason slot in ISO format
    /// </summary>
    public string End { get; set; }
    
    /// <summary>
    /// Reason 
    /// </summary>
    public ReasonDTO Reason { get; set; }
    
    /// <summary>
    /// Is an overwrite required for this reason (slot) ?
    /// </summary>
    public bool OverwriteRequired { get; set; }
  }
}
