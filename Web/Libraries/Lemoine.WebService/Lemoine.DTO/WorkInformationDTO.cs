// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO for Work Information
  /// </summary>
  public class WorkInformationDTO
  {
    /// <summary>
    /// Kind
    /// </summary>
    public WorkInformationKind Kind { get; set; }
    
    /// <summary>
    /// Value (may be null)
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public WorkInformationDTO ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="kind"></param>
    /// <param name="value"></param>
    public WorkInformationDTO (WorkInformationKind kind, string value)
    {
      this.Kind = kind;
      this.Value = value;
    }
  }
}
