// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Xml;
using Lemoine.Cnc.Asp;
using Lemoine.CncEngine;
using Lemoine.Core.Log;

namespace Lemoine.CncEngine.Asp
{
  /// <summary>
  /// Set service
  /// </summary>
  public sealed class XmlPostService
  {
    readonly ILog log = LogManager.GetLogger (typeof (XmlPostService).FullName);

    readonly IAcquisitionSet m_acquisitionSet;
    readonly IAcquisitionFinder m_acquisitionFinder;

    /// <summary>
    /// Constructor
    /// </summary>
    public XmlPostService (IAcquisitionSet acquisitionSet, IAcquisitionFinder acquisitionFinder)
    {
      m_acquisitionSet = acquisitionSet;
      m_acquisitionFinder = acquisitionFinder;
    }

    /// <summary>
    /// data request
    /// </summary>
    public IDictionary<string, object> PostXml (CancellationToken cancellationToken, string acquisitionIdentifier, System.IO.Stream body)
    {
      var acquisition = GetAcquisition (cancellationToken, acquisitionIdentifier);

      var xmlDocument = new XmlDocument ();
      xmlDocument.Load (body);

      return PostXml (cancellationToken, acquisition, xmlDocument);
    }

    /// <summary>
    /// data request
    /// </summary>
    public IDictionary<string, object> PostXml (CancellationToken cancellationToken, string acquisitionIdentifier, string xml)
    {
      var acquisition = GetAcquisition (cancellationToken, acquisitionIdentifier);

      var xmlDocument = new XmlDocument ();
      xmlDocument.LoadXml (xml);

      return PostXml (cancellationToken, acquisition, xmlDocument);
    }

    Acquisition GetAcquisition (CancellationToken cancellationToken, string acquisitionIdentifier)
    {
      var acquisition = m_acquisitionSet
        .GetAcquisitions (cancellationToken: cancellationToken)
        .FirstOrDefault (a => m_acquisitionFinder.IsMatch (a, acquisitionIdentifier));
      if (acquisition is null) {
        log.Error ($"GetAcquisition: no acquisition with identifier {acquisitionIdentifier}");
        throw new UnknownAcquisitionException ($"Error: unknown acquisition {acquisitionIdentifier}");
      }
      return acquisition;
    }

    IDictionary<string, object> PostXml (CancellationToken cancellationToken, Acquisition acquisition, XmlDocument xmlDocument)
    {
      var data = new Dictionary<string, object> ();

      foreach (XmlNode node in xmlDocument.DocumentElement.ChildNodes) {
        if (null == node) {
          log.Fatal ($"PostXml: node is null, which is unexpected");
          throw new InvalidOperationException ("Unexpected null node");
        }
        if (!node.NodeType.Equals (XmlNodeType.Element)) {
          continue;
        }
        XmlElement element = node as XmlElement ?? throw new Exception ("Node is expected to be an element");
        acquisition.ProcessModule (element, data, cancellationToken);
        // TODO: get the errors as well
      }

      return data;
    }
  }
}
