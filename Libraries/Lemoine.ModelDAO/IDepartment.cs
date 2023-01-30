// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table Department
  /// </summary>
  public interface IDepartment: IDataWithIdentifiers, IDisplayable, ISelectionable, IDataWithVersion
    , ISerializableModel
    , IMachineFilterItemSet
    , IComparable
  {
    // Note: IDepartment does not inherit from IVersionable
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Name of the department
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Code used in some companies to identify a department
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// Department external code
    /// 
    /// It may help synchronizing our data with an external database
    /// </summary>
    string ExternalCode { get; set; }
    
    /// <summary>
    /// Priority to use to display it in the reports or in the applications
    /// </summary>
    int? DisplayPriority { get; set; }
    
    /// <summary>
    /// Machines that are associated to this department
    /// </summary>
    ICollection<IMachine> Machines { get; }
  }
}
