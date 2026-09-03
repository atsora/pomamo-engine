// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: GPL-2.0

namespace Lemoine.Cnc.OpcUaClientService.Services
{
  /// <summary>
  /// Exception that is raised when a request targets a method the OPC UA client module does not implement
  /// </summary>
  public sealed class UnknownMethodException : Exception
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="method"></param>
    public UnknownMethodException (string? method)
      : base ($"Method {method} not found")
    { }
  }

  /// <summary>
  /// Exception that is raised when a request targets a property the OPC UA client module does not implement
  /// </summary>
  public sealed class UnknownPropertyException : Exception
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="property"></param>
    public UnknownPropertyException (string? property)
      : base ($"Property {property} not found")
    { }
  }

  /// <summary>
  /// Exception that is raised when a posted configuration is not valid
  /// </summary>
  public sealed class InvalidConfigurationException : Exception
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public InvalidConfigurationException (string message)
      : base (message)
    { }
  }

  /// <summary>
  /// Exception that is raised when an acquisition was not configured yet by a /xml request
  /// </summary>
  public sealed class UnknownAcquisitionException : Exception
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="acquisitionIdentifier">nullable</param>
    public UnknownAcquisitionException (string? acquisitionIdentifier)
      : base ($"Acquisition {acquisitionIdentifier} not found")
    { }
  }

  /// <summary>
  /// Exception that is raised when the connection to the OPC UA server of an acquisition is not available
  /// </summary>
  public sealed class ConnectionNotAvailableException : Exception
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="acquisitionIdentifier"></param>
    public ConnectionNotAvailableException (string acquisitionIdentifier)
      : base ($"Connection of the acquisition {acquisitionIdentifier} is not available")
    { }
  }
}
