// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;

namespace Lemoine.Extensions.Web.Doc
{
  /// <summary>
  /// Content of a document for a specific revision,
  /// with the data the web application requires to return it to the client
  /// </summary>
  public struct DocContent
  {
    /// <summary>
    /// Effective revision that is returned
    /// 
    /// This is the resolved revision when the latest revision was requested
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// File name to suggest to the client for the download
    /// 
    /// Not null and not empty
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// MIME type of the content, for example application/pdf
    /// 
    /// Possibly empty if it could not be determined
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Content of the document
    /// 
    /// It is up to the caller to dispose it
    /// </summary>
    public Stream Content { get; set; }
  }
}
