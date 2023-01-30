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
  /// Xsl definition with only a xsl:text node content,
  /// without the xsl:stylesheet, the xsl:template and xsl:text nodes
  /// </summary>
  [Serializable]
  public sealed class XslTextDefinition: XslDefinition
  {
    #region Members
    string m_xslText;
    string m_xslFull;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (XslTextDefinition).FullName);

    #region Getters / Setters
    /// <summary>
    /// XSL precising only a xsl:text node
    /// 
    /// The xsl:stylesheet header, the xsl:template and the xsl:text nodes are automatically added
    /// </summary>
    [XmlElement]
    public string XslText {
      get { return m_xslText; }
      set
      {
        try {
          m_xslText = value;
          m_xslFull = AddHeaderFooter (AddTemplateTextNodes (value));
          m_xslt.Load (XmlReader.Create (new StringReader (m_xslFull)));
        }
        catch (Exception ex) {
          log.ErrorFormat ("XslText: " +
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
    public XslTextDefinition ()
    {
    }
    
    /// <summary>
    /// Constructor with initialization
    /// </summary>
    /// <param name="xslText"></param>
    public XslTextDefinition (string xslText)
    {
      this.XslText = xslText;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}
