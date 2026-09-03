// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: GPL-2.0

namespace Lemoine.Cnc.OpcUaClientService.Configuration
{
  /// <summary>
  /// One value to read, which corresponds to one get instruction of the posted XML
  /// </summary>
  /// <param name="Key">key of the value in the returned dictionary, not null or empty</param>
  /// <param name="Method">get method of the OPC UA client module, nullable when Property is set</param>
  /// <param name="Property">property of the OPC UA client module, nullable when Method is set</param>
  /// <param name="Param">OPC UA node to read, nullable when Property is set</param>
  public sealed record DataItem (string Key, string? Method, string? Property, string? Param);
}
