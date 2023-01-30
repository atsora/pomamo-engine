// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for the ComponentType table
  /// </summary>
  public interface IComponentType: IVersionable, IDataWithIdentifiers, IDisplayable, IDataWithTranslation, IReferenceData, ISerializableModel
  {
    /// <summary>
    /// Component type code
    /// </summary>
    string Code { get; set; }
  }
}
