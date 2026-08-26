// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Lemoine.Extensions;

namespace Lemoine.Extensions.Web.Doc
{
  /// <summary>
  /// Extension to store and retrieve the documents that are referenced in the doc table
  /// 
  /// A document is only identified by its path: this extension does not depend on the database.
  /// The first implementation is based on git, where a revision is associated to a commit.
  /// 
  /// An extension is initialized for a specific path with <see cref="Initialize (string)"/>.
  /// The other methods apply to this path.
  /// </summary>
  public interface IDocExtension : IExtension
  {
    /// <summary>
    /// Initialize the extension for the specified path.
    /// 
    /// If false is returned, this implementation does not manage this path
    /// and must not be considered
    /// </summary>
    /// <param name="path">not null and not empty</param>
    /// <returns>success</returns>
    bool Initialize (string path);

    /// <summary>
    /// Score of the extension (an extension with a higher score is considered first)
    /// </summary>
    double Score { get; }

    /// <summary>
    /// List the available revisions of the document, from the most recent to the oldest one
    /// 
    /// An empty list is returned if the document does not exist yet
    /// </summary>
    /// <returns>not null</returns>
    Task<IEnumerable<DocRevision>> GetRevisionsAsync ();

    /// <summary>
    /// Get the most recent revision number of the document
    /// 
    /// null is returned if the document does not exist yet
    /// </summary>
    /// <returns>nullable</returns>
    Task<int?> GetLastRevisionAsync ();

    /// <summary>
    /// Make the document available, so that the web application may return it to the client
    /// </summary>
    /// <param name="revision">revision to get. If null, the most recent revision is returned</param>
    /// <returns></returns>
    /// <exception cref="DocNotFoundException">the document or the requested revision does not exist</exception>
    Task<DocContent> GetAsync (int? revision = null);

    /// <summary>
    /// Upload a new revision of the document
    /// 
    /// If the document does not exist yet, it is created
    /// </summary>
    /// <param name="content">not null. It is only read, it is not disposed</param>
    /// <param name="description">description of the new revision. In the git implementation, the commit message</param>
    /// <returns>the new revision number</returns>
    Task<int> UploadAsync (Stream content, string description = "");
  }
}
