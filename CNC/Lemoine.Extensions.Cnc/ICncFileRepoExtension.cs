// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml;
using Lemoine.Model;
using System.Collections.Generic;

namespace Lemoine.Extensions.Cnc
{
  /// <summary>
  /// Extension to CncFileRepo
  /// </summary>
  public interface ICncFileRepoExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Order in which the extension points must be processed
    /// </summary>
    double XmlExtensionOrder { get; }

    /// <summary>
    /// Initialize the plugin. Return true if it is applicable for this cnc acquisition, else false
    /// </summary>
    /// <param name="cncAcquisition"></param>
    /// <returns></returns>
    bool Initialize (ICncAcquisition cncAcquisition);

    /// <summary>
    /// Get the Cnc variable keys to read on this specific machine module 
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    IEnumerable<string> GetCncVariableKeys (IMachineModule machineModule);

    /// <summary>
    /// Get the default detection method from a specific machine module
    /// If not applicable, null is returned
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    DetectionMethod? GetDefaultDetectionMethod (IMachineModule machineModule);

    /// <summary>
    /// Get the include path for the specified extension
    /// If not applicable, a null or empty string is returned
    /// </summary>
    /// <param name="extensionName"></param>
    /// <returns></returns>
    string GetIncludePath (string extensionName);

    /// <summary>
    /// Get the path of XML template to include  for the specified extension
    /// If not applicable, a null string is returned
    /// </summary>
    /// <param name="extensionName"></param>
    /// <returns>the path and the replacement strings</returns>
    Tuple<string, Dictionary<string, string>> GetIncludedXmlTemplate (string extensionName);

    /// <summary>
    /// Add in the extension points, an XML string directly
    /// </summary>
    /// <param name="extensionName"></param>
    /// <returns></returns>
    string GetExtensionAsXmlString (string extensionName);
  }
}
