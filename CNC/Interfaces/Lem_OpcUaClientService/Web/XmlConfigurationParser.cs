// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: GPL-2.0

using System.Xml.Linq;
using Lemoine.Cnc.OpcUaClientService.Configuration;
using Lemoine.Cnc.OpcUaClientService.Services;

namespace Lemoine.Cnc.OpcUaClientService.Web
{
  /// <summary>
  /// Parse the XML an acquisition posts on /xml
  ///
  /// The expected body is the one the CncCoreXmlPost module builds:
  ///
  /// <code>
  /// &lt;root&gt;
  ///   &lt;moduleref ref="opcua" ServerUrl="opc.tcp://host:4840" UseSecurity="false"&gt;
  ///     &lt;get method="GetDouble" param="/Channel/State/actFeedRateIpo[1]"&gt;RawFeedrate&lt;/get&gt;
  ///     &lt;get property="ConnectionError"&gt;AcquisitionError&lt;/get&gt;
  ///   &lt;/moduleref&gt;
  /// &lt;/root&gt;
  /// </code>
  ///
  /// The attributes of the moduleref element, and the property elements it may contain, are the
  /// properties of the OPC UA client module. The get elements are the data to read.
  ///
  /// The ref attribute is ignored: unlike the generic cnc core service, this service always uses
  /// the OPC UA client module, so there is no module to select.
  /// </summary>
  public static class XmlConfigurationParser
  {
    /// <summary>
    /// Attributes of the moduleref element that are not properties of the OPC UA client module
    /// </summary>
    static readonly string[] RESERVED_ATTRIBUTES = new[] { "ref", "starterror" };

    /// <summary>
    /// Parse a posted configuration
    /// </summary>
    /// <param name="xml">not null or empty</param>
    /// <returns>the configuration of the acquisition</returns>
    /// <exception cref="InvalidConfigurationException">the XML is not valid</exception>
    public static AcquisitionConfiguration Parse (string xml)
    {
      if (string.IsNullOrWhiteSpace (xml)) {
        throw new InvalidConfigurationException ("Empty request body");
      }

      XDocument document;
      try {
        document = XDocument.Parse (xml);
      }
      catch (Exception ex) {
        throw new InvalidConfigurationException ($"Not a valid XML document: {ex.Message}");
      }

      var moduleRefElement = document.Root?.Name.LocalName == "moduleref"
        ? document.Root
        : document.Root?.Elements ().FirstOrDefault (x => x.Name.LocalName == "moduleref");
      if (moduleRefElement is null) {
        throw new InvalidConfigurationException ("No moduleref element in the request body");
      }

      var connectionProperties = new Dictionary<string, string> (StringComparer.InvariantCultureIgnoreCase);
      foreach (var attribute in moduleRefElement.Attributes ()) {
        if (attribute.IsNamespaceDeclaration
          || RESERVED_ATTRIBUTES.Contains (attribute.Name.LocalName, StringComparer.InvariantCultureIgnoreCase)) {
          continue;
        }
        ConnectionProperty.Validate (attribute.Name.LocalName, attribute.Value);
        connectionProperties[attribute.Name.LocalName] = attribute.Value;
      }
      // A property element carries a value an attribute cannot hold, and it survives the acquisition
      // engine, which only forwards the children of the module element
      foreach (var propertyElement in moduleRefElement.Elements ().Where (x => x.Name.LocalName == "property")) {
        var name = propertyElement.Attribute ("name")?.Value;
        if (string.IsNullOrEmpty (name)) {
          throw new InvalidConfigurationException ("A property element has no name attribute");
        }
        var propertyValue = propertyElement.Attribute ("value")?.Value ?? propertyElement.Value;
        ConnectionProperty.Validate (name, propertyValue);
        connectionProperties[name] = propertyValue;
      }

      var dataItems = new List<DataItem> ();
      foreach (var getElement in moduleRefElement.Elements ().Where (x => x.Name.LocalName == "get")) {
        var key = getElement.Value.Trim ();
        if (string.IsNullOrEmpty (key)) {
          throw new InvalidConfigurationException ("A get element has no data key");
        }
        var method = getElement.Attribute ("method")?.Value;
        var property = getElement.Attribute ("property")?.Value;
        var param = getElement.Attribute ("param")?.Value;
        if (string.IsNullOrEmpty (method) && string.IsNullOrEmpty (property)) {
          // Without any method, a node to read is a raw get, and everything else is a property
          if (string.IsNullOrEmpty (param)) {
            property = key;
          }
          else {
            method = "Get";
          }
        }
        dataItems.Add (new DataItem (key, string.IsNullOrEmpty (method) ? null : method,
          string.IsNullOrEmpty (property) ? null : property, param));
      }

      if (!connectionProperties.TryGetValue (ConnectionProperty.ServerUrl, out var serverUrl)
        || string.IsNullOrWhiteSpace (serverUrl)) {
        throw new InvalidConfigurationException ($"No {ConnectionProperty.ServerUrl} in the request: the acquisition must send the url of the OPC UA server of the machine");
      }

      return new AcquisitionConfiguration (connectionProperties, dataItems);
    }
  }
}
