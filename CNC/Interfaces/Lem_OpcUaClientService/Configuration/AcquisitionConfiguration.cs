// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: GPL-2.0

namespace Lemoine.Cnc.OpcUaClientService.Configuration
{
  /// <summary>
  /// Configuration of the connection to the OPC UA server of one machine, and of the data to read on it
  ///
  /// It is what a /xml request carries. Two configurations are equal when they describe the same
  /// connection and the same data: the connection is only initialized again when they differ.
  /// </summary>
  public sealed class AcquisitionConfiguration : IEquatable<AcquisitionConfiguration>
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connectionProperties">properties of the OPC UA client module, not null</param>
    /// <param name="dataItems">data to read, not null</param>
    public AcquisitionConfiguration (IEnumerable<KeyValuePair<string, string>> connectionProperties, IEnumerable<DataItem> dataItems)
    {
      ArgumentNullException.ThrowIfNull (connectionProperties);
      ArgumentNullException.ThrowIfNull (dataItems);

      this.ConnectionProperties = connectionProperties
        .OrderBy (x => x.Key, StringComparer.InvariantCultureIgnoreCase)
        .ToList ();
      this.DataItems = dataItems.ToList ();
    }

    /// <summary>
    /// Properties to set on the OPC UA client module, for example ServerUrl, Username or UseSecurity
    ///
    /// They are ordered by key, so that two configurations that only differ by the order are equal
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, string>> ConnectionProperties { get; }

    /// <summary>
    /// Data the /data and /xml requests return
    /// </summary>
    public IReadOnlyList<DataItem> DataItems { get; }

    /// <summary>
    /// OPC UA parameters the data items read, without any duplicate
    /// </summary>
    public IEnumerable<string> Parameters => this.DataItems
      .Where (x => x.Property is null && !string.IsNullOrEmpty (x.Param))
      .Select (x => x.Param!)
      .Distinct (StringComparer.Ordinal);

    /// <summary>
    /// <see cref="IEquatable{T}.Equals(T)" />
    /// </summary>
    /// <param name="other">nullable</param>
    public bool Equals (AcquisitionConfiguration? other)
    {
      if (other is null) {
        return false;
      }
      if (ReferenceEquals (this, other)) {
        return true;
      }
      return this.ConnectionProperties.SequenceEqual (other.ConnectionProperties)
        && this.DataItems.SequenceEqual (other.DataItems);
    }

    /// <summary>
    /// <see cref="object.Equals(object)" />
    /// </summary>
    /// <param name="obj">nullable</param>
    public override bool Equals (object? obj) => Equals (obj as AcquisitionConfiguration);

    /// <summary>
    /// <see cref="object.GetHashCode" />
    /// </summary>
    public override int GetHashCode ()
    {
      var hashCode = new HashCode ();
      foreach (var connectionProperty in this.ConnectionProperties) {
        hashCode.Add (connectionProperty.Key, StringComparer.InvariantCultureIgnoreCase);
        hashCode.Add (connectionProperty.Value);
      }
      foreach (var dataItem in this.DataItems) {
        hashCode.Add (dataItem);
      }
      return hashCode.ToHashCode ();
    }

    /// <summary>
    /// Description of the connection, for the logs, without any password
    /// </summary>
    public override string ToString ()
    {
      var properties = this.ConnectionProperties
        .Where (x => !x.Key.Contains ("password", StringComparison.InvariantCultureIgnoreCase))
        .Select (x => $"{x.Key}={x.Value}");
      return $"{string.Join (" ", properties)} data={this.DataItems.Count}";
    }
  }
}
