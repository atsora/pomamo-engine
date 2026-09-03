// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: GPL-2.0

namespace Lemoine.Cnc.OpcUaClientService.Web
{
  /// <summary>
  /// Response of a /get or a /set request
  ///
  /// The names of the members are the ones the acquisition modules expect: they must not be changed
  /// </summary>
  public sealed class SingleResponse
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="acquisitionIdentifier">nullable</param>
    /// <param name="moduleref">nullable</param>
    /// <param name="instruction">get or set</param>
    /// <param name="method">nullable</param>
    /// <param name="property">nullable</param>
    /// <param name="param">nullable</param>
    public SingleResponse (string? acquisitionIdentifier, string? moduleref, string instruction, string? method, string? property, string? param)
    {
      this.AcquisitionIdentifier = acquisitionIdentifier;
      this.Moduleref = moduleref ?? "";
      this.Instruction = instruction;
      this.Method = method;
      this.Property = property;
      this.Param = param;
    }

    /// <summary>
    /// Acquisition identifier of the request
    /// </summary>
    public string? AcquisitionIdentifier { get; set; }

    /// <summary>
    /// Module reference of the request
    /// </summary>
    public string Moduleref { get; set; }

    /// <summary>
    /// Instruction: get or set
    /// </summary>
    public string Instruction { get; set; }

    /// <summary>
    /// Method of the request, when the request targets a method
    /// </summary>
    public string? Method { get; set; }

    /// <summary>
    /// Property of the request, when the request targets a property
    /// </summary>
    public string? Property { get; set; }

    /// <summary>
    /// Parameter of the request
    /// </summary>
    public string? Param { get; set; }

    /// <summary>
    /// The request was successful
    /// </summary>
    public bool Success { get; set; } = false;

    /// <summary>
    /// Returned value, when the request was a successful get
    /// </summary>
    public object? Result { get; set; } = null;

    /// <summary>
    /// Error message, when the request failed
    /// </summary>
    public string? Error { get; set; } = null;

    /// <summary>
    /// Flag the request as successful, without any returned value
    /// </summary>
    public SingleResponse SetSuccess ()
    {
      this.Result = null;
      this.Success = true;
      this.Error = null;
      return this;
    }

    /// <summary>
    /// Flag the request as successful and set the returned value
    /// </summary>
    /// <param name="result">nullable</param>
    public SingleResponse SetResult (object? result)
    {
      this.Result = result;
      this.Success = true;
      this.Error = null;
      return this;
    }

    /// <summary>
    /// Flag the request as failed and set the error message
    /// </summary>
    /// <param name="error">not null</param>
    public SingleResponse SetError (string error)
    {
      ArgumentNullException.ThrowIfNull (error);

      this.Result = null;
      this.Success = false;
      this.Error = error;
      return this;
    }
  }
}
