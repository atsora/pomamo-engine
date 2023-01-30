// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Operation revision
  /// </summary>
  public interface IOperationRevision : IVersionable, IDataWithIdentifiers, IDisplayable, IEquatable<IOperationRevision>, ISerializableModel, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Associated operation
    /// </summary>
    IOperation Operation { get; }

    /// <summary>
    /// Date/time of the operation revision
    /// </summary>
    DateTime DateTime { get; }

    /// <summary>
    /// Revision number
    /// </summary>
    int? Number { get; set; }

    /// <summary>
    /// Description of the revision
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// Associated operation models
    /// </summary>
    IEnumerable<IOperationModel> OperationModels { get; }

    // TODO: iso files ?
  }
}
