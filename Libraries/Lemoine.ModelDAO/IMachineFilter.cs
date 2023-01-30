// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.ModelDAO;

namespace Lemoine.Model
{
  /// <summary>
  /// Initial set of machines in the Machine Filter
  /// </summary>
  public enum MachineFilterInitialSet
  {
    /// <summary>
    /// No machine is associated to Machine Filter initially
    /// </summary>
    None = 0,
    /// <summary>
    /// All the machines to the Machine Filter initially
    /// </summary>
    All = 1
  }
  
  /// <summary>
  /// Model for table MachineFilter
  /// </summary>
  public interface IMachineFilter: ISelectionable, IDataWithVersion, ISerializableModel, IDisplayable
  {
    // Note: IMachine does not inherit from IVersionable
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Name of the machine filter
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Initial set of machines that are associated to this machine filter
    /// </summary>
    MachineFilterInitialSet InitialSet { get; set; }

    /// <summary>
    /// List of items that are part of the machine filter or excluded from the machine filter
    /// </summary>
    IList<IMachineFilterItem> Items { get; }
    
    /// <summary>
    /// Check if a machine matches this machine filter
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    bool IsMatch (IMachine machine);
  }
}
