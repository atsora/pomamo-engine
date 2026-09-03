// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: GPL-2.0

namespace Lemoine.Cnc.OpcUaClientService.Configuration
{
  /// <summary>
  /// Options of the OPC UA client service
  ///
  /// They only hold what does not depend on a machine: the configuration of the machines
  /// themselves comes from the acquisitions, through their /xml requests
  /// </summary>
  public sealed class OpcUaClientServiceOptions
  {
    /// <summary>
    /// Name of the configuration section
    /// </summary>
    public const string SectionName = "OpcUaClientService";

    /// <summary>
    /// Name of the configuration file
    ///
    /// Every service of the product is published in the same directory, so appsettings.json would
    /// be shared by all of them: the configuration of this service is in a file that carries its
    /// name instead
    /// </summary>
    public const string FileName = "Lem_OpcUaClientService.json";

    /// <summary>
    /// Endpoint the service listens on
    ///
    /// It stays on the loopback interface by default: the service has no authentication, and the
    /// acquisition that requests it runs on the same machine
    /// </summary>
    public string Url { get; set; } = "http://127.0.0.1:4841";

    /// <summary>
    /// Duration in ms during which the values that were read from an OPC UA server
    /// are considered valid, and are returned without any new read request
    /// </summary>
    public int CacheDurationMs { get; set; } = 500;

    /// <summary>
    /// Delay in ms during which the parameters that the /get requests newly ask for are gathered,
    /// before the query of an acquisition is prepared again
    ///
    /// It does not apply to the parameters a /xml request carries, since they are all known at once
    /// </summary>
    public int RegistrationDelayMs { get; set; } = 2000;
  }
}
