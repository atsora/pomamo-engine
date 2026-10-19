// Copyright (C) 2026 Atsora Solutions

using System;
using Lemoine.Extensions.Web.Attributes;

namespace Pulse.Web.Doc
{
  /// <summary>
  /// Response DTO for the /Doc/Upload service
  /// </summary>
  [Api ("DocUpload Response DTO")]
  public class DocUploadResponseDTO
  {
    /// <summary>
    /// Id of the document
    /// </summary>
    public int DocId { get; set; }

    /// <summary>
    /// Path of the document
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Revision of the document after the upload
    ///
    /// If the uploaded content is the same as the most recent revision,
    /// no new revision is created and the most recent one is returned
    /// </summary>
    public int Revision { get; set; }
  }
}
