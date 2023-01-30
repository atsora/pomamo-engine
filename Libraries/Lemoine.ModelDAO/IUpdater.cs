// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table Updater
  /// </summary>
  public interface IUpdater: IVersionable, IDisplayable, ISerializableModel
  {
    /// <summary>
    /// Associated revisions
    /// </summary>
    ICollection<IRevision> Revisions { get; }
    
    /// <summary>
    /// Cast Updater to the underlying class
    /// </summary>
    /// <returns></returns>
    T As<T>() where T: class, IUpdater;
  }
}
