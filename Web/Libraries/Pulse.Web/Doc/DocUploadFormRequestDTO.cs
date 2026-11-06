// Copyright (C) 2026 Atsora Solutions

using System;
using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;

namespace Pulse.Web.Doc
{
  /// <summary>
  /// Request DTO for the /Doc/UploadForm service
  ///
  /// The request is a multipart/form-data form, with the file in its first file part,
  /// for example from an HTML input type=file element.
  /// The other parameters are read from the form fields, else from the query
  /// </summary>
  [Api ("Request DTO for /Doc/UploadForm service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Doc/UploadForm", "POST", Summary = "Upload a new revision of a document from a multipart/form-data form", Notes = "The file is the first file of the form. Fields (or query parameters): DocId (new revision of an existing document), Path (the document is created if it does not exist yet) or neither (new document with a path generated from the file name), and optionally Description, Tags (separated by a comma) and KindId")]
  public class DocUploadFormRequestDTO : IReturn<DocUploadResponseDTO>
  {
    /// <summary>
    /// Id of an existing document to upload a new revision of
    /// </summary>
    [ApiMember (Name = "DocId", Description = "Id of an existing document to upload a new revision of", ParameterType = "form", DataType = "int", IsRequired = false)]
    public int? DocId { get; set; }

    /// <summary>
    /// Path of the document. The document is created if it does not exist yet
    ///
    /// If neither DocId nor Path is set, a new document is created with a path
    /// generated from the name of the uploaded file
    /// </summary>
    [ApiMember (Name = "Path", Description = "Path of the document, relative, for example tasks/lubrication.pdf. The document is created if it does not exist yet. If neither DocId nor Path is set, a path {random key}/{file name} is generated", ParameterType = "form", DataType = "string", IsRequired = false)]
    public string Path { get; set; }

    /// <summary>
    /// Description of the new revision
    /// </summary>
    [ApiMember (Name = "Description", Description = "Description of the new revision", ParameterType = "form", DataType = "string", IsRequired = false)]
    public string Description { get; set; }

    /// <summary>
    /// Tags of the document, separated by a comma, that replace the existing ones
    ///
    /// Not set: the tags are left unchanged. Set but empty: the tags are removed
    /// </summary>
    [ApiMember (Name = "Tags", Description = "Tags of the document, separated by a comma (or one Tags field per tag), that replace the existing ones. Not set: unchanged. Empty: no tag", ParameterType = "form", DataType = "string", IsRequired = false)]
    public string Tags { get; set; }

    /// <summary>
    /// Id of the kind of the document
    ///
    /// Not set: the kind is left unchanged. Set but empty: the kind is removed
    /// </summary>
    [ApiMember (Name = "KindId", Description = "Id of the kind of the document (see the GraphQL query docKinds). Not set: unchanged. Empty: no kind", ParameterType = "form", DataType = "int", IsRequired = false)]
    public string KindId { get; set; }
  }
}
