// Copyright (C) 2026 Atsora Solutions

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.WebMiddleware.HttpContext;
using Microsoft.AspNetCore.Http;
using Pulse.Extensions.Web.Doc;
using Pulse.Extensions.Web.Graphql;

namespace Pulse.Web.Doc
{
  /// <summary>
  /// Upload a new revision of a document, the content being either the raw body of the POST request
  /// (/Doc/Upload) or the first file of a multipart/form-data form (/Doc/UploadForm)
  ///
  /// The document is stored by the <see cref="Lemoine.Extensions.Web.Doc.IDocExtension"/>
  /// implementation with the highest score that manages its path
  /// </summary>
  public class DocUploadService
    : IHandler
    , IHttpContextSupport
    , IBodySupport
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DocUploadService).FullName);

    Stream m_body;

    /// <summary>
    /// <see cref="IHttpContextSupport"/>
    /// </summary>
    public HttpContext HttpContext { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public DocUploadService ()
    {
    }

    /// <summary>
    /// Post method
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<object> Post (DocUploadRequestDTO request)
    {
      // The raw body is not a JSON request DTO: the middleware may still have deserialized
      // some properties from it (for example when a JSON document is uploaded),
      // so consider only the query parameters
      if (HttpContext is not null) {
        request = GetRequestFromQuery (HttpContext.Request.Query);
        if (request is null) {
          return new ErrorDTO ("Invalid DocId", ErrorStatus.WrongRequestParameter);
        }
      }

      if (m_body is null) {
        log.Error ("Post: no request body");
        return new ErrorDTO ("No document content in the request body", ErrorStatus.WrongRequestParameter);
      }
      if (m_body.CanSeek) {
        m_body.Seek (0, SeekOrigin.Begin);
        if (0 == m_body.Length) {
          log.Error ("Post: empty request body");
          return new ErrorDTO ("Empty document content", ErrorStatus.WrongRequestParameter);
        }
      }

      return await UploadAsync (m_body, request.DocId, request.Path, request.FileName, request.Description);
    }

    /// <summary>
    /// Post method for a multipart/form-data form
    /// </summary>
    /// <param name="request">not considered, the parameters are read from the form, else from the query</param>
    /// <returns></returns>
    public async Task<object> Post (DocUploadFormRequestDTO request)
    {
      if (HttpContext is null) {
        log.Fatal ("Post: no http context, the form can't be read");
        return new ErrorDTO ("Form not available", ErrorStatus.UnexpectedError);
      }

      var httpRequest = HttpContext.Request;
      if (!httpRequest.HasFormContentType) {
        log.Error ($"Post: content type {httpRequest.ContentType} is not a form");
        return new ErrorDTO ("A multipart/form-data request is expected", ErrorStatus.WrongRequestParameter);
      }
      // The middleware may have already read the body to deserialize the request DTO
      if (httpRequest.Body.CanSeek) {
        httpRequest.Body.Seek (0, SeekOrigin.Begin);
      }

      IFormCollection form;
      try {
        form = await httpRequest.ReadFormAsync ();
      }
      catch (Exception ex) {
        log.Error ("Post: the form could not be read", ex);
        return new ErrorDTO ("Invalid form", ErrorStatus.WrongRequestParameter, ex.Message);
      }

      var file = form.Files.FirstOrDefault ();
      if (file is null) {
        log.Error ("Post: no file in the form");
        return new ErrorDTO ("No file in the form", ErrorStatus.WrongRequestParameter);
      }
      if (0 == file.Length) {
        log.Error ($"Post: empty file {file.FileName}");
        return new ErrorDTO ("Empty document content", ErrorStatus.WrongRequestParameter);
      }

      string GetValue (string key)
      {
        var formValue = form
          .FirstOrDefault (x => string.Equals (x.Key, key, StringComparison.InvariantCultureIgnoreCase))
          .Value
          .ToString ();
        return string.IsNullOrEmpty (formValue)
          ? GetQueryValue (httpRequest.Query, key)
          : formValue;
      }

      int? docId = null;
      var docIdString = GetValue (nameof (DocUploadFormRequestDTO.DocId));
      if (!string.IsNullOrEmpty (docIdString)) {
        if (int.TryParse (docIdString, out var id)) {
          docId = id;
        }
        else {
          log.Error ($"Post: invalid DocId {docIdString}");
          return new ErrorDTO ("Invalid DocId", ErrorStatus.WrongRequestParameter);
        }
      }

      using (var content = file.OpenReadStream ()) {
        return await UploadAsync (content, docId,
          GetValue (nameof (DocUploadFormRequestDTO.Path)),
          file.FileName,
          GetValue (nameof (DocUploadFormRequestDTO.Description)));
      }
    }

    /// <summary>
    /// Upload a new revision of a document with <see cref="DocUploader"/>
    /// </summary>
    /// <param name="content">not null</param>
    /// <param name="docId">id of an existing document</param>
    /// <param name="path">path of the document, created if it does not exist yet. If neither docId nor path is set, a path is generated</param>
    /// <param name="fileName">only considered to generate the path</param>
    /// <param name="description">description of the new revision</param>
    /// <returns>a <see cref="DocUploadResponseDTO"/> or an <see cref="ErrorDTO"/></returns>
    async Task<object> UploadAsync (Stream content, int? docId, string path, string fileName, string description)
    {
      try {
        var result = await DocUploader.UploadAsync (content, docId, path, fileName, description);
        return new DocUploadResponseDTO {
          DocId = result.DocId,
          Path = result.Path,
          Revision = result.Revision
        };
      }
      catch (DataProcessingException ex) {
        return new ErrorDTO (ex.Message, ex.ErrorStatus, ex.InnerException?.Message ?? "");
      }
    }
    /// <summary>
    /// Build the request from the query parameters only
    /// </summary>
    /// <param name="query"></param>
    /// <returns>null if a parameter is invalid</returns>
    static DocUploadRequestDTO GetRequestFromQuery (IQueryCollection query)
    {
      var request = new DocUploadRequestDTO ();
      var docId = GetQueryValue (query, nameof (DocUploadRequestDTO.DocId));
      if (!string.IsNullOrEmpty (docId)) {
        if (int.TryParse (docId, out var id)) {
          request.DocId = id;
        }
        else {
          log.Error ($"GetRequestFromQuery: invalid DocId {docId}");
          return null;
        }
      }
      request.Path = GetQueryValue (query, nameof (DocUploadRequestDTO.Path));
      request.FileName = GetQueryValue (query, nameof (DocUploadRequestDTO.FileName));
      request.Description = GetQueryValue (query, nameof (DocUploadRequestDTO.Description));
      return request;
    }

    static string GetQueryValue (IQueryCollection query, string key) =>
      query
        .FirstOrDefault (x => string.Equals (x.Key, key, StringComparison.InvariantCultureIgnoreCase))
        .Value
        .ToString ();

    #region IBodySupport
    /// <summary>
    /// <see cref="IBodySupport"/>
    /// </summary>
    /// <param name="body"></param>
    public void SetBody (Stream body)
    {
      m_body = body;
    }
    #endregion // IBodySupport
  }
}
