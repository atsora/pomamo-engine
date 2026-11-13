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
    /// Id of the item (null when unknown or when there is no item):
    /// <list type="bullet">
    /// <item>Operation: operation id</item>
    /// <item>Component: component id</item>
    /// <item>Part: part id, that is also its component id</item>
    /// <item>WorkOrder / Job / Project: work order / job / project id</item>
    /// </list>
    /// </summary>
    public int? Id { get; set; }

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

    /// <summary>
    /// Constructor with an id
    /// </summary>
    /// <param name="kind"></param>
    /// <param name="value"></param>
    /// <param name="id">nullable</param>
    public WorkInformationDTO (WorkInformationKind kind, string value, int? id)
      : this (kind, value)
    {
      this.Id = id;
    }
  }
}
