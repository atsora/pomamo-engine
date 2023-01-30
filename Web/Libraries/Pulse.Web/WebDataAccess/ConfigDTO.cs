// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// DTO for a config
  /// </summary>
  public class ConfigDTO
  {
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool Active { get; set; }
  }
}
