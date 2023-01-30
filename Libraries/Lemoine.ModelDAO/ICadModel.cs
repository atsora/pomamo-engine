// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table CADModel
  /// 
  /// This table lists the different CADModel names
  /// that were found in the ISO files and
  /// allows to keep to which component or operation
  /// a CAD Model name should point to.
  /// </summary>
  public interface ICadModel: IVersionable, IDataWithIdentifiers
  {
    /// <summary>
    /// CADModel name
    /// 
    /// Name as found in the ISO file
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Reference to a Component in case the reference is known
    /// </summary>
    IComponent Component { get; set; }
    
    /// <summary>
    /// Reference to an Operation in case the reference is known
    /// </summary>
    IOperation Operation { get; set; }
    
    /// <summary>
    /// Set of sequences that reference this CAD Model
    /// </summary>
    ICollection<ISequence> Sequences { get; }
  }
}
