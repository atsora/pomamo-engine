// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of ISerialNumberModification.
  /// </summary>
  public interface ISerialNumberModification : IGlobalModification, IPartitionedByMachine
  {
    /// <summary>
    /// Reference to the SerialNumber
    /// </summary>
    string SerialNumber { get; set; }
    
    /// <summary>
    /// Begin or end date time of related operation cycle
    /// </summary>
    DateTime BeginOrEndDateTime { get; set; }
    
    /// <summary>
    /// Is BeginOrEndDateTime refering to begin or end ?
    /// </summary>
    bool IsBegin { get; set; }
  }
}


