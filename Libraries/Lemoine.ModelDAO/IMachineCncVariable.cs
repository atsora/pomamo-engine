// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Returns the definition of a cnc variable on one machine
  /// </summary>
  public interface IMachineCncVariable : IDataWithIdentifiers, IDataWithVersion
  {
    // Note: IMachine does not inherit from IVersionable
    //       else the corresponding properties can't be used in a DataGridView binding

    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Restrict the setting to a set of machines
    /// </summary>
    IMachineFilter MachineFilter { get; set; }

    /// <summary>
    /// Cnc variable key
    /// </summary>
    string CncVariableKey { get; }

    /// <summary>
    /// Cnc variable value
    /// </summary>
    object CncVariableValue { get; }

    /// <summary>
    /// Associated component
    /// </summary>
    IComponent Component { get; set; }

    /// <summary>
    /// Associated operation
    /// </summary>
    IOperation Operation { get; set; }

    /// <summary>
    /// Associated sequence
    /// </summary>
    ISequence Sequence { get; set; }
  }
}
