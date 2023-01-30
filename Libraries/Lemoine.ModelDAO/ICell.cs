// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Cell kind
  /// </summary>
  public enum CellKind
  {
    /// <summary>
    /// No specific attribute
    /// </summary>
    None = 0,
    /// <summary>
    /// The cell is made of sequential machines
    /// </summary>
    Sequential = 1,
    /// <summary>
    /// The cell is made of concurrent machines
    /// </summary>
    Concurrent = 2,
  }

  /// <summary>
  /// Model for table Cell
  /// </summary>
  public interface ICell: IDataWithIdentifiers, IDisplayable, ISelectionable, IDataWithVersion
    , ISerializableModel
    , IMachineFilterItemSet
    , IComparable
  {
    // Note: ICell does not inherit from IVersionable
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Name of the cell
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Code used in some companies to identify a cell
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// Cell external code
    /// 
    /// It may help synchronizing our data with an external database
    /// </summary>
    string ExternalCode { get; set; }
    
    /// <summary>
    /// Priority to use to display it in the reports or in the applications
    /// </summary>
    int? DisplayPriority { get; set; }
    
    /// <summary>
    /// Cell kind
    /// </summary>
    CellKind Kind { get; set; }

    /// <summary>
    /// Machines that are associated to this cell
    /// </summary>
    ICollection<IMachine> Machines { get; }
  }
}
