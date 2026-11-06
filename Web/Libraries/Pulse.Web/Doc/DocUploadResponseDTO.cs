// Copyright (C) 2026 Atsora Solutions

using System;
using System.Collections.Generic;
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
    /// Tags of the document
    /// </summary>
    public IList<string> Tags { get; set; }

    /// <summary>
    /// Id of the kind of the document, null if it has no kind
    /// </summary>
    public int? KindId { get; set; }

    /// <summary>
    /// Revision of the document after the upload
    ///
    /// If the uploaded content is the same as the most recent revision,
    /// no new revision is created and the most recent one is returned
    /// </summary>
    public int Revision { get; set; }
  }
}
