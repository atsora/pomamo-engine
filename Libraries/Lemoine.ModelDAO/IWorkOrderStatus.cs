// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of the WorkOrderStatus table
  /// </summary>
  public interface IWorkOrderStatus: IVersionable, IDataWithIdentifiers, IDisplayable, IDataWithTranslation, IReferenceData, ISerializableModel
  {
  }
}
