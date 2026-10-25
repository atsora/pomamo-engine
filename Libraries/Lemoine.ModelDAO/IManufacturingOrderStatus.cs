// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Row of the manuforderstatus table: the display and the color of a
  /// <see cref="ManufacturingOrderStatus"/>, whose value is the id
  ///
  /// A manufacturing order carries the enum, not this entity: this one is read-only
  /// reference data
  /// </summary>
  public interface IManufacturingOrderStatus : IVersionable, IDataWithIdentifiers, IDisplayable, IDataWithTranslation, ISerializableModel
  {
    /// <summary>
    /// The status this row describes
    /// </summary>
    ManufacturingOrderStatus Status { get; }

    /// <summary>
    /// Color, in the #RRGGBB form
    /// </summary>
    string Color { get; }
  }
}
