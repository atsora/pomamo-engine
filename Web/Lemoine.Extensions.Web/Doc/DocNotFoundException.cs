// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Extensions.Web.Doc
{
  /// <summary>
  /// Exception that is raised by <see cref="IDocExtension"/>
  /// when a document or one of its versions does not exist
  /// </summary>
  public class DocNotFoundException : Exception
  {
    /// <summary>
    /// Path of the document that was not found
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Version that was not found, if applicable
    /// </summary>
    public int? Version { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path"></param>
    /// <param name="version">nullable</param>
    public DocNotFoundException (string path, int? version = null)
      : base (version.HasValue
              ? $"Version {version.Value} of document {path} was not found"
              : $"Document {path} was not found")
    {
      this.Path = path;
      this.Version = version;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path"></param>
    /// <param name="version">nullable</param>
    /// <param name="innerException"></param>
    public DocNotFoundException (string path, int? version, Exception innerException)
      : base (version.HasValue
              ? $"Version {version.Value} of document {path} was not found"
              : $"Document {path} was not found",
              innerException)
    {
      this.Path = path;
      this.Version = version;
    }
  }
}
