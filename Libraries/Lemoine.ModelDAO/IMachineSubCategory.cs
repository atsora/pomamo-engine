// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table MachineSubCategory
  /// </summary>
  public interface IMachineSubCategory
    : IDataWithIdentifiers, IDisplayable, ISelectionable, IDataWithVersion
    , IMachineFilterItemSet
    , IComparable
  {
    // Note: IMachineSubCategory does not inherit from IVersionable
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Name of the machineSubCategory
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Code used in some companies to identify a machineSubCategory
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// MachineSubCategory external code
    /// 
    /// It may help synchronizing our data with an external database
    /// </summary>
    string ExternalCode { get; set; }
    
    /// <summary>
    /// Priority to use to display it in the reports or in the applications
    /// </summary>
    int? DisplayPriority { get; set; }
    
    /// <summary>
    /// Machines that are associated to this subcategory
    /// </summary>
    ICollection<IMachine> Machines { get; }
  }
}
