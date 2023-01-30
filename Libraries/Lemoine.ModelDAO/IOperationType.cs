// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of the OperationType table
  /// </summary>
  public interface IOperationType: IVersionable, IDataWithIdentifiers, IDataWithTranslation, IDisplayable, IReferenceData, ISerializableModel
  {
    /// <summary>
    /// Operation type code
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// Operation type priority
    /// </summary>
    int? Priority { get; set; }
  }
}
