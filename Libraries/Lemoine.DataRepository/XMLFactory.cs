// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Threading;
using System.Xml;

using Lemoine.Core.Log;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// XML Factory: builds a DOMDocument from an XML file or string
  /// 
  /// <example>
  /// Example of XML structure:
  /// <![CDATA[
  /// <root>
  ///   <job name="JOBNAME" hours="2.5">
  ///     <component name="COMPNAME" startdate="2007-07-01 00:00:00" />
  ///   </job>
  /// </root>
  /// ]]>
  /// </example>
  /// </summary>
  public class XMLFactory : IFactory
  {
    #region Members
    XmlSourceType m_source;
    string m_xmlString;
    string m_url;
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (XMLFactory).FullName);

    #region Getters / Setters
    /// <summary>
    /// Source of the XML: raw string or file/url
    /// </summary>
    public XmlSourceType Source
    {
      get { return m_source; }
      set { m_source = value; }
    }

    /// <summary>
    /// XML String in case source is STRING
    /// </summary>
    public string XmlString
    {
      get { return m_xmlString; }
      set { m_xmlString = value; }
    }

    /// <summary>
    /// URL or local file name in case source is FILEORURL
    /// </summary>
    public string Url
    {
      get { return m_url; }
      set { m_url = value; }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="source">Source type of the data</param>
    /// <param name="data">Data: raw string or URL</param>
    public XMLFactory (XmlSourceType source, string data)
    {
      this.m_source = source;
      switch (source) {
        case XmlSourceType.STRING:
          this.m_xmlString = data;
          break;
        case XmlSourceType.URI:
          this.m_url = data;
          break;
      }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Specialized method to build the DOMDocument
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="optional"></param>
    /// <returns>The DOMDocument</returns>
    public System.Xml.XmlDocument GetData (CancellationToken cancellationToken, bool optional = false)
    {
      try {
        System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument ();
        switch (m_source) {
        case XmlSourceType.STRING:
          xmlDocument.LoadXml (this.m_xmlString);
          break;
        case XmlSourceType.URI:
          using (var reader = new XmlTextReader (m_url)) {
            reader.Read ();
            xmlDocument.Load (reader);
          }
          break;
        }
        return xmlDocument;
      }
      catch (FileNotFoundException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetData: FileNotFoundException url={m_url} optional={optional}", ex);
        }
        if (optional) {
          return null;
        }
        else {
          throw;
        }
      }
      catch (DirectoryNotFoundException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetData: DirectoryNotFoundException url={m_url} optional={optional}", ex);
        }
        if (optional) {
          return null;
        }
        else {
          throw;
        }
      }
    }

    /// <summary>
    /// <see cref="IFactory.CheckSynchronizationOkAction" />
    /// </summary>
    /// <returns></returns>
    public bool CheckSynchronizationOkAction ()
    {
      log.Debug ("CheckSynchronizationOkAction: " +
                 "return false");
      return false;
    }

    /// <summary>
    /// <see cref="IFactory.FlagSynchronizationAsSuccess" />
    /// </summary>
    /// <param name="document"></param>
    public void FlagSynchronizationAsSuccess (XmlDocument document)
    {
      // Do nothing special for the moment
      return;
    }

    /// <summary>
    /// <see cref="IFactory.FlagSynchronizationAsFailure" />
    /// </summary>
    /// <param name="document"></param>
    public void FlagSynchronizationAsFailure (XmlDocument document)
    {
      // Do nothing special for the moment
      return;
    }
    #endregion
  }
}
