// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;

using Lemoine.Core.Log;

namespace Lemoine.Xml
{
  /// <summary>
  /// Xsl definition with only a XPath definition of xsl:value-of node,
  /// without the xsl:stylesheet, the xsl:template and xsl:value-of nodes
  /// </summary>
  [Serializable]
  public sealed class XslValueOfDefinition: XslDefinition
  {
    #region Members
    string m_xpath;
    string m_xslFull;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (XslValueOfDefinition).FullName);

    #region Getters / Setters
    /// <summary>
    /// XPath definition of the xsl:valueOf node
    /// 
    /// The xsl:stylesheet header, the xsl:template and the xsl:value-of nodes are automatically added
    /// </summary>
    [XmlElement]
    public string XPath {
      get { return m_xpath; }
      set
      {
        try {
          m_xpath = value;
          m_xslFull = AddHeaderFooter (AddTemplateValueOfNodes (value));
          m_xslt.Load (XmlReader.Create (new StringReader (m_xslFull)));
        }
        catch (Exception ex) {
          log.ErrorFormat ("XPath: " +
                           "exception {0}",
                           ex);
          throw;
        }
      }
    }

    /// <summary>
    /// Full XSL with the full header (including xsl:stylesheet)
    /// </summary>
    [XmlIgnore]
    public string XslFull {
      get { return m_xslFull; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor for XML serialization
    /// </summary>
    public XslValueOfDefinition ()
    {
    }
    
    /// <summary>
    /// Constructor with initialization
    /// </summary>
    /// <param name="xpath"></param>
    public XslValueOfDefinition (string xpath)
    {
      this.XPath = xpath;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}
