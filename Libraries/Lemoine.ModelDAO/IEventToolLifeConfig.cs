// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of IEventToolLifeConfig.
  /// </summary>
  public interface IEventToolLifeConfig: IVersionable, ISerializableModel
  {
    /// <summary>
    /// Optional machine filter
    /// </summary>
    IMachineFilter MachineFilter { get; set; }
    
    /// <summary>
    /// Associated machine observation state
    /// 
    /// It may be null (whichever machine observation state)
    /// </summary>
    IMachineObservationState MachineObservationState { get; set; }
    
    /// <summary>
    /// Level to associate to the event (can't be null)
    /// </summary>
    IEventLevel Level { get; set; }
    
    /// <summary>
    /// Type of tool life event
    /// </summary>
    EventToolLifeType Type { get; set; }
  }
}
