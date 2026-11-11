// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using Lemoine.Core.Log;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// XML builder interface
  ///
  /// The aim of the builder is to build a DOMDocument into an XML file
  /// </summary>
  public class XMLBuilder : IBuilder
  {
    string m_filename;

    static readonly ILog log = LogManager.GetLogger (typeof (XMLBuilder).FullName);

    /// <summary>
    /// Name of the XML file
    /// </summary>
    public string Filename
    {
      get { return m_filename; }
      set { m_filename = value; }
    }

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="filename"></param>
    public XMLBuilder (string filename)
    {
      this.m_filename = filename;
    }
    #endregion

    /// <summary>
    /// Give the possibility to use an asynchronous commit
    /// </summary>
    public void SetAsynchronousCommit ()
    {
      // Do nothing
      return;
    }

    /// <summary>
    /// Build an XML file from a DOMDocument
    /// 
    /// <see cref="IBuilder.Build">IBuilder.Build</see>
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="cancellationToken"></param>
    public void Build (System.Xml.XmlDocument doc, CancellationToken cancellationToken)
    {
      doc.Save (m_filename);
    }
  }
}
