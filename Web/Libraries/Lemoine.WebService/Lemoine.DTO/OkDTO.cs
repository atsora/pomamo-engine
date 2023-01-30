// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.DTO
{
  /// <summary>
  /// Description of OkDTO.
  /// </summary>
  public class OkDTO
  {
    /// <summary>
    /// Id (e.g. of revision)
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Version
    /// </summary>
    public int Version { get; set; }
    
    /// <summary>
    /// Message of OkDTO
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    public OkDTO(int id)
    {
      this.Id = id;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    public OkDTO(long id)
    {
      this.Id = id;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version"></param>
    public OkDTO(int id, int version)
    {
      this.Id = id;
      this.Version = version;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="message"></param>
    public OkDTO(int id, string message)
    {
      this.Id = id;
      this.Message = message;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public OkDTO(string message)
    {
      this.Message = message;
    }
  }
}
