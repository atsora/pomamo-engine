// Copyright (C) 2026 Atsora Solutions

using System;
using Lemoine.Extensions.Web.Attributes;

namespace Pulse.Web.Doc
{
  /// <summary>
  /// Response DTO for the /Doc/WorkInfoDocCount service
  /// </summary>
  [Api ("WorkInfoDocCount Response DTO")]
  public class WorkInfoDocCountResponseDTO
  {
    /// <summary>
    /// Number of distinct documents of the operation (its active revision) and of the component,
    /// a document associated to both being counted once
    /// </summary>
    public int DocCount { get; set; }
  }
}
