// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// (Tree)Node DTO
  /// </summary>
  public class NodeDTO
  {
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Child Nodes (null for leaf, otherwise at least one element)
    /// </summary>
    List<NodeDTO> Children { get; set; }
  }
}
