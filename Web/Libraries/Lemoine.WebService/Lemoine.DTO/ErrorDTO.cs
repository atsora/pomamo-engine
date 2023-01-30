// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.DTO
{
  
  /// <summary>
  /// DTO for errors (in case of ill-built request or db "problem" occurs when building response)
  /// </summary>
  public class ErrorDTO
  {
    /// <summary>
    /// Error Message of ErrorDTO
    /// </summary>
    public string ErrorMessage { get; set; }
    /*
    /// <summary>
    /// Should the message be display by the client ?
    /// </summary>
    public bool DisplayStatus { get; set; }
    */
   
   /// <summary>
   /// Give status of the error
   /// </summary>
    public ErrorStatus Status { get; set; }
    
    /// <summary>
    /// Default constructor (for XML serialization)
    /// </summary>
    public ErrorDTO() {
      
    }
    
    /// <summary>
    /// Constructor of ErrorDTO
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="status"></param>
    public ErrorDTO(string msg, ErrorStatus status) {
      ErrorMessage = msg;
      Status = status;      
    }
    
  }
  
  /// <summary>
  /// Define different status about Error
  /// </summary>
  public enum ErrorStatus { 
    /// <summary>
    /// Permanent error status due to wrong parameter in request
    /// </summary>
    PERMANENT, 
    /// <summary>
    /// Permanent error status due to wrong configuration
    /// </summary>
    PERMANENT_NO_CONFIG, 
    /// <summary>
    /// Transient error status, retry request with a delay
    /// </summary>
    TRANSIENT_DELAY, 
    /// <summary>
    /// Transient error status, retry immediately request
    /// </summary>
    TRANSIENT, 
    /// <summary>
    /// No data error status
    /// </summary>
    NO_DATA, 
    /// <summary>
    /// Not applicable error status
    /// </summary>
    NOT_APPLICABLE 
  };

  
}
