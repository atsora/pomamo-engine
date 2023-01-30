// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// It associates a given machining resource to a list of operation types it is able to do.
  /// 
  /// A preference value may be added. For example, if the preference is 1,
  /// this is the main operation type this machine was designed to do.
  /// If the preference is 2, this machine is able to do such an operation type,
  /// but there may be some machines that have a better performance for this type of operation.
  /// </summary>
  public interface IMachineOperationType
  {
    /// <summary>
    /// Machine to associate to an operation type
    /// </summary>
    IMachine Machine { get; }
    
    /// <summary>
    /// Operation type to associate to a machine
    /// </summary>
    IOperationType OperationType { get; }
    
    /// <summary>
    /// Operation type preference
    /// 
    /// If the preference is 1, this is the main operation type
    /// this machine was designed to do.
    /// If the preference is 2, this machine is able to do
    /// such an operation type, but there may be some machines
    /// that have a better performance for this type of operation.
    /// </summary>
    int Preference { get; set; }
  }
}
