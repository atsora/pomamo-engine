// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Extensions.Web.Doc
{
  /// <summary>
  /// Exception that is raised by <see cref="IDocExtension"/>
  /// when a document or one of its revisions does not exist
  /// </summary>
  public class DocNotFoundException : Exception
  {
    /// <summary>
    /// Path of the document that was not found
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Revision that was not found, if applicable
    /// </summary>
    public int? Revision { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path"></param>
    /// <param name="revision">nullable</param>
    public DocNotFoundException (string path, int? revision = null)
      : base (revision.HasValue
              ? $"Revision {revision.Value} of document {path} was not found"
              : $"Document {path} was not found")
    {
      this.Path = path;
      this.Revision = revision;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path"></param>
    /// <param name="revision">nullable</param>
    /// <param name="innerException"></param>
    public DocNotFoundException (string path, int? revision, Exception innerException)
      : base (revision.HasValue
              ? $"Revision {revision.Value} of document {path} was not found"
              : $"Document {path} was not found",
              innerException)
    {
      this.Path = path;
      this.Revision = revision;
    }
  }
}
