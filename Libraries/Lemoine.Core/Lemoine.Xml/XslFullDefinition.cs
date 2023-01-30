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
    #region Members
    string m_xslFull;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (XslFullDefinition).FullName);

    #region Getters / Setters
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
          log.ErrorFormat ("XslFull: " +
                           "exception {0}",
                           ex);
          throw;
        }
      }
    }
    #endregion // Getters / Setters

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

    #region Methods
    #endregion // Methods
  }
}
