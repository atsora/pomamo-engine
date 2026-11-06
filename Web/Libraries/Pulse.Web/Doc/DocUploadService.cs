// Copyright (C) 2026 Atsora Solutions

using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.Primitives;
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
      DocProperties properties = new DocProperties ();
      if (HttpContext is not null) {
        request = GetRequestFromQuery (HttpContext.Request.Query);
        if (request is null) {
          return new ErrorDTO ("Invalid DocId", ErrorStatus.WrongRequestParameter);
        }
        var query = HttpContext.Request.Query;
        var propertiesError = TryGetProperties (key => GetQueryValues (query, key), out properties);
        if (propertiesError is not null) {
          return propertiesError;
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

      return await UploadAsync (m_body, request.DocId, request.Path, request.FileName, request.Description, properties);
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

      // A form field first, else a query parameter. null if neither is present
      StringValues? GetValues (string key)
      {
        var formField = form
          .FirstOrDefault (x => string.Equals (x.Key, key, StringComparison.InvariantCultureIgnoreCase));
        return formField.Key is not null
          ? formField.Value
          : GetQueryValues (httpRequest.Query, key);
      }
      var propertiesError = TryGetProperties (GetValues, out var properties);
      if (propertiesError is not null) {
        return propertiesError;
      }

      using (var content = file.OpenReadStream ()) {
        return await UploadAsync (content, docId,
          GetValue (nameof (DocUploadFormRequestDTO.Path)),
          file.FileName,
          GetValue (nameof (DocUploadFormRequestDTO.Description)),
          properties);
      }
    }

    /// <summary>
    /// Tags and kind a client asks to set, with the upload
    /// </summary>
    internal struct DocProperties
    {
      /// <summary>
      /// New tags, null to leave them unchanged
      /// </summary>
      public IList<string> Tags { get; set; }

      /// <summary>
      /// Change the kind of the document
      /// </summary>
      public bool SetKind { get; set; }

      /// <summary>
      /// New kind if <see cref="SetKind"/> is set, null to remove it
      /// </summary>
      public int? KindId { get; set; }
    }

    /// <summary>
    /// Read the optional parameters Tags and KindId
    ///
    /// <item>Tags: the tags, separated by a comma, or one tag per parameter.
    /// Not present: unchanged. Present but empty: no tag</item>
    /// <item>KindId: the id of the kind. Not present: unchanged. Present but empty: no kind</item>
    /// </summary>
    /// <param name="getValues">values of a parameter, null if it is not present</param>
    /// <param name="properties"></param>
    /// <returns>null on success, else the error to return</returns>
    internal static ErrorDTO TryGetProperties (Func<string, StringValues?> getValues, out DocProperties properties)
    {
      properties = new DocProperties ();

      var tags = getValues (nameof (DocUploadRequestDTO.Tags));
      if (tags.HasValue) {
        properties.Tags = tags.Value
          .Where (x => x is not null)
          .SelectMany (x => x.Split (','))
          .ToList (); // Trimmed and cleaned up by IDoc.Tags
      }

      var kindId = getValues (nameof (DocUploadRequestDTO.KindId));
      if (kindId.HasValue) {
        properties.SetKind = true;
        var kindIdString = kindId.Value.ToString ().Trim ();
        if (!string.IsNullOrEmpty (kindIdString)) {
          if (int.TryParse (kindIdString, out var id)) {
            properties.KindId = id;
          }
          else {
            log.Error ($"TryGetProperties: invalid KindId {kindIdString}");
            return new ErrorDTO ("Invalid KindId", ErrorStatus.WrongRequestParameter);
          }
        }
      }

      return null;
    }

    /// <summary>
    /// Upload a new revision of a document with <see cref="DocUploader"/>
    /// </summary>
    /// <param name="content">not null</param>
    /// <param name="docId">id of an existing document</param>
    /// <param name="path">path of the document, created if it does not exist yet. If neither docId nor path is set, a path is generated</param>
    /// <param name="fileName">only considered to generate the path</param>
    /// <param name="description">description of the new revision</param>
    /// <param name="properties">tags and kind to set</param>
    /// <returns>a <see cref="DocUploadResponseDTO"/> or an <see cref="ErrorDTO"/></returns>
    async Task<object> UploadAsync (Stream content, int? docId, string path, string fileName, string description, DocProperties properties)
    {
      try {
        var result = await DocUploader.UploadAsync (content, docId, path, fileName, description,
          properties.Tags, properties.SetKind, properties.KindId);
        return new DocUploadResponseDTO {
          DocId = result.DocId,
          Path = result.Path,
          Tags = result.Tags,
          KindId = result.KindId,
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

    /// <summary>
    /// Values of a query parameter
    /// </summary>
    /// <param name="query"></param>
    /// <param name="key">case insensitive</param>
    /// <returns>null if the parameter is not present</returns>
    static StringValues? GetQueryValues (IQueryCollection query, string key)
    {
      var parameter = query
        .FirstOrDefault (x => string.Equals (x.Key, key, StringComparison.InvariantCultureIgnoreCase));
      return parameter.Key is null ? (StringValues?)null : parameter.Value;
    }

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
