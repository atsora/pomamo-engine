// Copyright (C) 2026 Atsora Solutions

using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Doc;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Responses;
using Lemoine.ModelDAO;
using Lemoine.WebMiddleware.HttpContext;
using Microsoft.AspNetCore.Http;
using Pulse.Extensions.Web.Doc;
using Microsoft.AspNetCore.StaticFiles;

namespace Pulse.Web.Doc
{
  /// <summary>
  /// Download a revision of a document (the most recent one by default)
  ///
  /// The content is returned with the MIME type of the document, determined from the extension
  /// of its file name. The file name is given in the Content-Disposition header (attachment,
  /// or inline with ?Inline=true) and the effective revision in the X-Doc-Revision header.
  /// The content types that may run a script (HTML, SVG, XML, ...) are always returned
  /// as an attachment, in a sandbox.
  /// In case of error, an ErrorDTO is returned in JSON.
  ///
  /// The response is not cached, so that the most recent revision is always returned
  /// </summary>
  public class DocDownloadService
    : IHandler
    , IHttpContextSupport
    , IResponseContentTypeSupport
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DocDownloadService).FullName);

    const string REVISION_HEADER = "X-Doc-Revision";
    const string DEFAULT_CONTENT_TYPE = "application/octet-stream";

    /// <summary>
    /// Content types that may run a script in the browser:
    /// they are never displayed inline (in addition to the *+xml ones)
    /// </summary>
    static readonly string[] ACTIVE_CONTENT_TYPES = {
      "text/html",
      "application/xhtml+xml",
      "image/svg+xml",
      "text/xml",
      "application/xml",
      "text/javascript",
      "application/javascript",
      "application/x-javascript",
      "text/xsl",
      "application/xslt+xml"
    };

    /// <summary>
    /// MIME types from the file extension (about 400 known extensions)
    /// </summary>
    static readonly FileExtensionContentTypeProvider s_contentTypeProvider = new FileExtensionContentTypeProvider ();

    bool m_error = false;
    string m_contentType = DEFAULT_CONTENT_TYPE;

    /// <summary>
    /// <see cref="IHttpContextSupport"/>
    /// </summary>
    public HttpContext HttpContext { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public DocDownloadService ()
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<object> Get (DocDownloadRequestDTO request)
    {
      string path;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var doc = await ModelDAOHelper.DAOFactory.DocDAO.FindByIdAsync (request.DocId);
        if (doc is null) {
          log.Error ($"Get: no doc with id {request.DocId}");
          return Error ($"No document with id {request.DocId}", ErrorStatus.WrongRequestParameter);
        }
        path = doc.Path;
      }

      var extensions = await DocExtensions.GetExtensionsAsync (path);
      if (0 == extensions.Count) {
        log.Error ($"Get: no doc extension manages the path {path}");
        return Error ($"No document storage is configured for path {path}", ErrorStatus.MissingConfiguration);
      }

      foreach (var extension in extensions) {
        DocContent docContent;
        try {
          docContent = await extension.GetAsync (request.Revision);
        }
        catch (DocNotFoundException ex) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: revision {request.Revision} of {path} not found by extension {extension}, try the next one", ex);
          }
          continue;
        }
        catch (Exception ex) {
          log.Error ($"Get: extension {extension} returned an exception for {path}, try the next one", ex);
          continue;
        }

        byte[] bytes;
        using (var content = docContent.Content) {
          if (content is null) {
            log.Error ($"Get: extension {extension} returned no content for {path}, try the next one");
            continue;
          }
          using (var memoryStream = new MemoryStream ()) {
            await content.CopyToAsync (memoryStream);
            bytes = memoryStream.ToArray ();
          }
        }
        m_contentType = GetContentType (docContent);
        SetHeaders (docContent, m_contentType, request.Inline);
        return bytes;
      }

      log.Error ($"Get: revision {request.Revision} of {path} was not found");
      var revisionText = request.Revision.HasValue ? $"Revision {request.Revision} of document" : "Document";
      return Error ($"{revisionText} {request.DocId} was not found", ErrorStatus.WrongRequestParameter);
    }

    ErrorDTO Error (string message, ErrorStatus status)
    {
      m_error = true;
      return new ErrorDTO (message, status);
    }

    /// <summary>
    /// Get the MIME type of the document: from the extension of its file name first,
    /// then the one that was suggested by the extension, else application/octet-stream
    /// </summary>
    /// <param name="docContent"></param>
    /// <returns>not null and not empty</returns>
    static string GetContentType (DocContent docContent)
    {
      if (!string.IsNullOrEmpty (docContent.FileName)
        && s_contentTypeProvider.TryGetContentType (docContent.FileName, out var contentType)) {
        return contentType;
      }
      return string.IsNullOrEmpty (docContent.ContentType)
        ? DEFAULT_CONTENT_TYPE
        : docContent.ContentType;
    }

    /// <summary>
    /// Is the content type one that may run a script in the browser?
    /// </summary>
    /// <param name="contentType"></param>
    /// <returns></returns>
    static bool IsActiveContent (string contentType)
    {
      var mediaType = contentType.Split (';')[0].Trim ();
      return ACTIVE_CONTENT_TYPES.Contains (mediaType, StringComparer.OrdinalIgnoreCase)
        || mediaType.EndsWith ("+xml", StringComparison.OrdinalIgnoreCase);
    }

    void SetHeaders (DocContent docContent, string contentType, bool inline)
    {
      if (HttpContext is null) {
        log.Error ("SetHeaders: no http context, the headers can't be set");
        return;
      }

      var headers = HttpContext.Response.Headers;
      var activeContent = IsActiveContent (contentType);
      if (inline && activeContent && log.IsInfoEnabled) {
        log.Info ($"SetHeaders: {contentType} may run a script => returned as an attachment, not inline");
      }
      var dispositionType = (inline && !activeContent) ? "inline" : "attachment";
      if (string.IsNullOrEmpty (docContent.FileName)) {
        headers["Content-Disposition"] = dispositionType;
      }
      else {
        // FileName is an ASCII fallback (a header value must be ASCII),
        // FileNameStar the RFC 5987 encoded file name that keeps the non-ASCII characters
        var asciiFileName = new string (docContent.FileName
          .Select (c => (c < 32 || 126 < c || c == '"' || c == '\\') ? '_' : c)
          .ToArray ());
        var contentDisposition = new ContentDispositionHeaderValue (dispositionType) {
          FileName = $"\"{asciiFileName}\"",
          FileNameStar = docContent.FileName
        };
        headers["Content-Disposition"] = contentDisposition.ToString ();
      }
      // The browser must not guess another content type, an active one for example
      headers["X-Content-Type-Options"] = "nosniff";
      if (activeContent) {
        // In case it is displayed all the same, no script and no access to the origin of the web service
        headers["Content-Security-Policy"] = "sandbox";
      }
      headers[REVISION_HEADER] = docContent.Revision.ToString ();
      // So that a client from another origin (the web app) can read them
      headers.Append ("Access-Control-Expose-Headers", $"Content-Disposition, {REVISION_HEADER}");
    }

    /// <summary>
    /// <see cref="IResponseContentTypeSupport"/>
    ///
    /// MIME type of the document, or application/json in case of error
    /// </summary>
    /// <returns></returns>
    public string GetResponseContentType () => m_error ? "application/json" : m_contentType;
  }
}
