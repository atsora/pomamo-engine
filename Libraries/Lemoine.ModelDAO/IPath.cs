// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table Path.
  /// </summary>
  public interface IPath : IVersionable, IDataWithIdentifiers, IDisplayable, IComparable, Lemoine.Collections.IDataWithId
  {
    
    /// <summary>
    /// Parent operation
    /// </summary>
    IOperation Operation { get; set; }
    
    /// <summary>
    /// Path number in the parent operation (order)
    /// </summary>
    int Number { get; set; }
    
    /// <summary>
    /// Set of sequences that are associated to this path
    /// </summary>
    ICollection<ISequence> Sequences { get; }
    
    /// <summary>
    /// Remove a sequence
    /// </summary>
    /// <param name="sequence"></param>
    void RemoveSequence (ISequence sequence);
  }
}
