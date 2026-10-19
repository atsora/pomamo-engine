// Copyright (C) 2026 Atsora Solutions

using System;
using System.Collections.Generic;
using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;

namespace Pulse.Web.Doc
{
  /// <summary>
  /// Request DTO for the /Doc/Download service
  /// </summary>
  [Api ("Request DTO for /Doc/Download service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Doc/Download", "GET", Summary = "Download a revision of a document (default is the most recent one)", Notes = "To use with ?DocId=&Revision=&Inline=")]
  [Route ("/Doc/Download/{DocId}", "GET", Summary = "Download a revision of a document (default is the most recent one)", Notes = "To use with ?Revision=&Inline=")]
  public class DocDownloadRequestDTO : IReturn<ICollection<byte[]>>
  {
    /// <summary>
    /// Id of the document
    /// </summary>
    [ApiMember (Name = "DocId", Description = "Id of the document", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int DocId { get; set; }

    /// <summary>
    /// Revision to download. If not set, the most recent revision is returned
    /// </summary>
    [ApiMember (Name = "Revision", Description = "Revision to download. Default is the most recent one", ParameterType = "query", DataType = "int", IsRequired = false)]
    public int? Revision { get; set; }

    /// <summary>
    /// Ask the browser to display the document (Content-Disposition: inline) instead of saving it
    ///
    /// Not considered for the content types that may run a script (HTML, SVG, XML, ...),
    /// that are always returned as an attachment
    /// </summary>
    [ApiMember (Name = "Inline", Description = "Display the document in the browser instead of saving it. Default is false. Not considered for HTML, SVG and XML documents", ParameterType = "query", DataType = "bool", IsRequired = false)]
    public bool Inline { get; set; }
  }
}
