// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for <see cref="IManufacturingOrderStatus"/>, read-only reference data
  /// </summary>
  public interface IManufacturingOrderStatusDAO : IGenericDAO<IManufacturingOrderStatus, int>
  {
  }
}
