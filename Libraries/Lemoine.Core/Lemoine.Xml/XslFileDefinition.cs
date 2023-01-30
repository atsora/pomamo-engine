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
  /// Xsl definition with a path to a XSL file
  /// </summary>
  [Serializable]
  public sealed class XslFileDefinition: XslDefinition
  {
    #region Members
    string m_xslFile;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (XslFileDefinition).FullName);

    #region Getters / Setters
    /// <summary>
    /// Path to the XSL file
    /// </summary>
    [XmlElement]
    public string XslFile {
      get { return m_xslFile; }
      set
      {
        try {
          m_xslFile = value;
          m_xslt.Load (m_xslFile);
        }
        catch (Exception ex) {
          log.ErrorFormat ("XslFile: " +
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
    public XslFileDefinition ()
    {
    }
    
    /// <summary>
    /// Constructor with initialization
    /// </summary>
    /// <param name="xslFile"></param>
    public XslFileDefinition (string xslFile)
    {
      this.XslFile = xslFile;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}
