// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Interface for all the models that have a Version property
  /// to manage the concurrent accesses
  /// </summary>
  public interface IVersionable: Lemoine.Collections.IDataWithId, IDataWithVersion
  {
  }
  
  /// <summary>
  /// Interface for all the models that have a Version property
  /// to manage the concurrent accesses and an ID of a specific type
  /// </summary>
  public interface IVersionable<ID>: Lemoine.Collections.IDataWithId<ID>, IDataWithVersion
  {
  }
}
