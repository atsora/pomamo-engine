// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml;
using System.Xml.XPath;

using Lemoine.Core.Log;
using Lemoine.Extensions.Alert;

namespace Lemoine.Alert
{
  /// <summary>
  /// Trigger an action only if the node that is returned by the XPath exists and is not null
  /// </summary>
  [Serializable]
  public class XPathTrigger: ITrigger
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (XPathTrigger).FullName);

    #region Getters / Setters
    /// <summary>
    /// XPath to evaluate
    /// </summary>
    public string XPath { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor (for XML serialization)
    /// </summary>
    public XPathTrigger ()
    {
    }
    
    /// <summary>
    /// Additional constructor with initialization
    /// </summary>
    /// <param name="xpath"></param>
    public XPathTrigger (string xpath)
    {
      this.XPath = xpath;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Implements <see cref="ITrigger" />
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool Eval (XmlElement data)
    {
      XPathNavigator navigator = data.CreateNavigator ();

      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(data.OwnerDocument.NameTable);
      namespaceManager.AddNamespace ("xsi", "http://www.w3.org/2001/XMLSchema-instance");

      XPathNavigator node = navigator.SelectSingleNode (this.XPath, namespaceManager);
      if (log.IsDebugEnabled) {
        log.Debug ($"Eval: XPath {this.XPath} of {data} returned {node}");
      }
      return (null != node);
    }
    #endregion // Methods
  }
}
