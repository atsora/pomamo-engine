// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: GPL-2.0

using System.Globalization;
using System.Reflection;
using Lemoine.Cnc.OpcUaClientService.Services;

namespace Lemoine.Cnc.OpcUaClientService.Configuration
{
  /// <summary>
  /// Properties of the OPC UA connection an acquisition may send in its /xml requests
  ///
  /// The set is explicit rather than any settable property of the module, so that a key that is
  /// misspelled in an acquisition configuration is reported at once, instead of silently leaving
  /// the connection half configured.
  /// </summary>
  public static class ConnectionProperty
  {
    /// <summary>
    /// Property that every acquisition must send
    /// </summary>
    public const string ServerUrl = "ServerUrl";

    /// <summary>
    /// Properties an acquisition may send, in the order they are documented
    /// </summary>
    public static readonly IReadOnlyList<string> Supported = new List<string> {
      ServerUrl,
      "UseSecurity",
      "SecurityMode",
      "DefaultNamespace",
      "Username",
      "Password",
      "CertificatePassword",
      "RenewCertificate",
      "TimeoutSeconds",
      "BrowseAndLog",
      "CncAlarmSubscription",
      "CncAlarmNamespace",
      "CncAcquisitionId"
    };

    /// <summary>
    /// Is a property one an acquisition may send ?
    /// </summary>
    /// <param name="name">nullable</param>
    public static bool IsSupported (string? name) =>
      name is not null && Supported.Contains (name, StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Check that a property may be sent, and that its value can be applied to the module
    /// </summary>
    /// <param name="name">nullable</param>
    /// <param name="value">nullable</param>
    /// <exception cref="InvalidConfigurationException">the property or its value is not valid</exception>
    public static void Validate (string? name, string? value)
    {
      if (!IsSupported (name)) {
        throw new InvalidConfigurationException ($"{name} is not a connection property of the OPC UA client. Expected one of: {string.Join (", ", Supported)}");
      }

      var propertyType = GetPropertyType (name!);
      if (propertyType is null) {
        // The module lost a property the service still accepts: this is a coding error, not a
        // configuration one, so it must be visible
        throw new InvalidConfigurationException ($"{name} does not exist any more in the OPC UA client module");
      }
      if (typeof (string) == propertyType) {
        return;
      }

      try {
        Convert.ChangeType (value, propertyType, CultureInfo.InvariantCulture);
      }
      catch (Exception) {
        throw new InvalidConfigurationException ($"{value} is not a valid value for the property {name}, a {propertyType.Name} is expected");
      }
    }

    static Type? GetPropertyType (string name) =>
      typeof (Lemoine.Cnc.OpcUaClient)
        .GetProperty (name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
        ?.PropertyType;
  }
}
