// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Description of ILineAssociation.
  /// </summary>
  public interface ILineAssociation : IGlobalModification, IPeriodAssociation
  {
    /// <summary>
    /// Reference to the Line
    /// 
    /// It can't be null
    /// </summary>
    ILine Line { get; set; }
  }
}
