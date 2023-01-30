// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Lemoine.Core.Log;

namespace Lemoine.Xml
{
  /// <summary>
  /// Xsl definition with a light XSL string, without the xsl:stylesheet header, beginning with xsl:template
  /// </summary>
  [Serializable]
  public sealed class XslTemplateDefinition: XslDefinition
  {
    #region Members
    string m_xslTemplate;
    string m_xslFull;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (XslTemplateDefinition).FullName);

    #region Getters / Setters
    /// <summary>
    /// Full XSL without the header (begin with xsl:template)
    /// 
    /// The automatic header is:
    /// &lt;xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"&gt;
    /// &lt;xsl:output method="text" omit-xml-declaration="yes" indent="no" /&gt;
    /// </summary>
    [XmlElement]
    public string XslTemplate {
      get { return m_xslTemplate; }
      set
      {
        try {
          m_xslTemplate = value;
          m_xslFull = AddHeaderFooter (value);
          m_xslt.Load (XmlReader.Create (new StringReader (m_xslFull)));
        }
        catch (Exception ex) {
          log.ErrorFormat ("XslTemplate: " +
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
    public XslTemplateDefinition ()
    {
    }
    
    /// <summary>
    /// Constructor with initialization
    /// </summary>
    /// <param name="xslTemplate"></param>
    public XslTemplateDefinition (string xslTemplate)
    {
      this.XslTemplate = xslTemplate;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}
