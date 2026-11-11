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
  /// Xsl definition with a full XSL string (including the xsl:stylesheet header)
  /// </summary>
  [Serializable]
  public sealed class XslFullDefinition: XslDefinition
  {
    string m_xslFull;

    static readonly ILog log = LogManager.GetLogger(typeof (XslFullDefinition).FullName);

    /// <summary>
    /// Full XSL with the full header (including xsl:stylesheet)
    /// </summary>
    [XmlElement]
    public string XslFull {
      get { return m_xslFull; }
      set
      {
        try {
          m_xslFull = value;
          m_xslt.Load (XmlReader.Create (new StringReader (value)));
        }
        catch (Exception ex) {
          log.Error ($"XslFull: exception {ex}");
          throw;
        }
      }
    }

    #region Constructors
    /// <summary>
    /// Default constructor for XML serialization
    /// </summary>
    public XslFullDefinition ()
    {
    }
    
    /// <summary>
    /// Constructor with initialization
    /// </summary>
    /// <param name="xslFull"></param>
    public XslFullDefinition (string xslFull)
    {
      this.XslFull = xslFull;
    }
    #endregion // Constructors
  }
}
