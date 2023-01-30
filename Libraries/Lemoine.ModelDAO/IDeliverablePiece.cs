// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;


namespace Lemoine.Model
{
  /// <summary>
  /// Model of table DeliverablePiece
  /// </summary>
  public interface IDeliverablePiece : IVersionable, IDataWithIdentifiers, IDisplayable, ISerializableModel // , IEquatable<IDeliverablePiece>
  {
    /// <summary>
    /// Code (serial number) (nullable)
    /// </summary>
    string Code { get ; set ; }

    /// <summary>
    /// Component (not null)
    /// </summary>
    IComponent Component { get ; set ; }

    /// <summary>
    /// Work order (nullable)
    /// </summary>
    IWorkOrder WorkOrder { get; set; }
  }
}
