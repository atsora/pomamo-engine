// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// model of table nonconformancereason
  /// </summary>
  public interface INonConformanceReason : IDataWithVersion,  IDataWithIdentifiers, IDisplayable, ISerializableModel
  {
    // Note: IReason does not inherit from IVersionable and IDataWithTranslation
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Name
    /// 
    /// not null.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Code
    /// </summary>
    string Code { get ; set ; }

    /// <summary>
    /// Description of nonconformance reason
    /// </summary>
    string Description { get ; set ; }

    /// <summary>
    /// Details associated to nonconformance reason is required
    /// </summary>
    bool DetailsRequired { get ; set ; }

  }
}
