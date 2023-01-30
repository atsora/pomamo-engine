// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO which give configuration to display WorkInformation.
  /// </summary>
  public class WorkInformationConfigDTO
  {

    /// <summary>
    /// Work piece informations
    /// </summary>
    public bool IsEditable { get; set; }
    
    /// <summary>
    /// Data Missing
    /// </summary>
    public bool OperationFromCnc { get; set; }

    /// <summary>
    /// Data Missing
    /// </summary>
    public bool OnePartPerWorkOrder { get; set; }
    
  }
}
