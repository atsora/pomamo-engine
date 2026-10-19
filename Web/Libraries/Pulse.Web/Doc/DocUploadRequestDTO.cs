// Copyright (C) 2026 Atsora Solutions

using System;
using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;

namespace Pulse.Web.Doc
{
  /// <summary>
  /// Request DTO for the /Doc/Upload service
  ///
  /// The content of the document is the raw body of the POST request
  /// </summary>
  [Api ("Request DTO for /Doc/Upload service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Doc/Upload", "POST", Summary = "Upload a new revision of a document, the content being the raw request body", Notes = "To use with ?DocId= (new revision of an existing document), ?Path= (the document is created if it does not exist yet) or neither (new document with a generated path, ending with &FileName= if set), and optionally &Description=")]
  public class DocUploadRequestDTO : IReturn<DocUploadResponseDTO>
  {
    /// <summary>
    /// Id of an existing document to upload a new revision of
    /// </summary>
    [ApiMember (Name = "DocId", Description = "Id of an existing document to upload a new revision of", ParameterType = "query", DataType = "int", IsRequired = false)]
    public int? DocId { get; set; }

    /// <summary>
    /// Path of the document. The document is created if it does not exist yet
    ///
    /// If neither DocId nor Path is set, a new document is created with a generated path
    /// </summary>
    [ApiMember (Name = "Path", Description = "Path of the document, relative, for example tasks/lubrication.pdf. The document is created if it does not exist yet. If neither DocId nor Path is set, a path is generated", ParameterType = "query", DataType = "string", IsRequired = false)]
    public string Path { get; set; }

    /// <summary>
    /// File name of a new document, only considered when the path is generated
    ///
    /// The generated path is {random key}/{FileName}, so that the file name and the content type
    /// are kept at the download
    /// </summary>
    [ApiMember (Name = "FileName", Description = "File name of a new document, for example lubrication.pdf, only considered when neither DocId nor Path is set. The generated path is {random key}/{FileName}", ParameterType = "query", DataType = "string", IsRequired = false)]
    public string FileName { get; set; }

    /// <summary>
    /// Description of the new revision
    /// </summary>
    [ApiMember (Name = "Description", Description = "Description of the new revision", ParameterType = "query", DataType = "string", IsRequired = false)]
    public string Description { get; set; }
  }
}
