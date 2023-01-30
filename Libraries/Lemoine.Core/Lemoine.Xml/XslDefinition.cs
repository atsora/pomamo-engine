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
  /// Abstract class
  /// </summary>
  [Serializable]
  [XmlInclude (typeof (XslFullDefinition))]
  [XmlInclude (typeof (XslTemplateDefinition))]
  [XmlInclude (typeof (XslTextDefinition))]
  [XmlInclude (typeof (XslValueOfDefinition))]
  [XmlInclude (typeof (XslFileDefinition))]
  [XmlInclude (typeof (XslHtmlBodyTemplateDefinition))]
  public abstract class XslDefinition
  {
    #region Members
    /// <summary>
    /// Compiled XSL transformation
    /// </summary>
    protected XslCompiledTransform m_xslt = new XslCompiledTransform ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (XslDefinition).FullName);

    #region Getters / Setters
    /// <summary>
    /// XSL compiled transform
    /// </summary>
    [XmlIgnore]
    public XslCompiledTransform Xslt {
      get { return m_xslt; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public XslDefinition ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add the xsl:stylesheet xand xsl:output headers
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    protected virtual string AddHeaderFooter (string s)
    {
      string fullXsl = @"<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"">
<xsl:output method=""text"" omit-xml-declaration=""yes"" indent=""no"" />
";
      fullXsl += s;
      fullXsl += @"
</xsl:stylesheet>";
      return fullXsl;
    }

    /// <summary>
    /// Add the template and text nodes
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    protected string AddTemplateTextNodes (string s)
    {
      string xslTemplate = @"<xsl:template match=""/"">
  <xsl:text>";
      xslTemplate += s;
      xslTemplate += @"</xsl:text>
</xsl:template>";
      return xslTemplate;
    }

    /// <summary>
    /// Add the template and valueOf nodes
    /// </summary>
    /// <param name="xpath"></param>
    /// <returns></returns>
    protected string AddTemplateValueOfNodes (string xpath)
    {
      string xslTemplate =
        string.Format (@"<xsl:template match=""/"">
  <xsl:value-of select=""{0}"" />
</xsl:template>",
                       xpath);
      return xslTemplate;
    }
    #endregion // Methods
  }
}
