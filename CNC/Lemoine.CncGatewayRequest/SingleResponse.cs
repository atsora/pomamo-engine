// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace Lemoine.CncGatewayRequest
{
  /// <summary>
  /// Type for a single response request (get/set) in the Cnc Core service
  /// </summary>
  public class SingleResponse
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public SingleResponse ()
    {
    }

    /// <summary>
    /// Acquisition identifier
    /// 
    /// Nullable
    /// </summary>
    public string AcquisitionIdentifier { get; set; }

    /// <summary>
    /// Module reference
    /// 
    /// Not nullable
    /// </summary>
    public string Moduleref { get; set; }

    /// <summary>
    /// Instruction
    /// 
    /// Not nullable
    /// </summary>
    public string Instruction { get; set; }

    /// <summary>
    /// Method
    /// 
    /// Nullable
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// Property
    /// 
    /// Nullable
    /// </summary>
    public string Property { get; set; }

    /// <summary>
    /// Parameter
    /// 
    /// Nullable
    /// </summary>
    public string Param { get; set; }

    /// <summary>
    /// Success
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Result
    /// 
    /// Nullable
    /// </summary>
    public object Result { get; set; }

    /// <summary>
    /// Error string
    /// 
    /// Nullable
    /// </summary>
    public string Error { get; set; }
  }

}
