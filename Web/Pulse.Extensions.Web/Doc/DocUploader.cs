// Copyright (C) 2026 Atsora Solutions

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Extensions.Web.Graphql;

namespace Pulse.Extensions.Web.Doc
{
  /// <summary>
  /// Result of <see cref="DocUploader.UploadAsync"/>
  /// </summary>
  public struct DocUploadResult
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

  /// <summary>
  /// Upload a new revision of a document, shared by the REST and the GraphQL services
  ///
  /// The document is stored by the <see cref="Lemoine.Extensions.Web.Doc.IDocExtension"/>
  /// implementation with the highest score that manages its path
  /// </summary>
  public static class DocUploader
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DocUploader).FullName);

    /// <summary>
    /// Upload a new revision of a document
    /// </summary>
    /// <param name="content">not null</param>
    /// <param name="docId">id of an existing document</param>
    /// <param name="path">path of the document, created if it does not exist yet. If neither docId nor path is set, a path is generated from fileName</param>
    /// <param name="fileName">only considered to generate the path</param>
    /// <param name="description">description of the new revision</param>
    /// <returns></returns>
    /// <exception cref="DataProcessingException">invalid parameters, missing configuration or upload failure</exception>
    public static async Task<DocUploadResult> UploadAsync (Stream content, int? docId, string path, string fileName, string description)
    {
      if (content is null) {
        log.Fatal ("UploadAsync: content is null");
        throw new ArgumentNullException (nameof (content));
      }

      path = path?.Trim ();
      if (!docId.HasValue && string.IsNullOrEmpty (path)) {
        path = GeneratePath (fileName);
        if (log.IsDebugEnabled) {
          log.Debug ($"UploadAsync: neither DocId nor Path is set => new doc with the generated path {path}");
        }
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Web.Doc.Upload")) {
          IDoc doc = null;
          if (docId.HasValue) {
            doc = await ModelDAOHelper.DAOFactory.DocDAO.FindByIdAsync (docId.Value);
            if (doc is null) {
              log.Error ($"UploadAsync: no doc with id {docId}");
              throw new DataProcessingException ($"No document with id {docId}", ErrorStatus.WrongRequestParameter);
            }
            if (!string.IsNullOrEmpty (path) && !string.Equals (path, doc.Path)) {
              log.Error ($"UploadAsync: path {path} does not match the path {doc.Path} of doc id {doc.Id}");
              throw new DataProcessingException ($"Path {path} does not match the path of document {doc.Id}", ErrorStatus.WrongRequestParameter);
            }
            path = doc.Path;
          }

          // Check an extension manages the path before a new doc is created
          var extension = (await DocExtensions.GetExtensionsAsync (path)).FirstOrDefault ();
          if (extension is null) {
            log.Error ($"UploadAsync: no doc extension manages the path {path}");
            throw new DataProcessingException ($"No document storage is configured for path {path}", ErrorStatus.MissingConfiguration);
          }

          // Note: no ??= here, this library is also compiled in C# 7.3 for net48
          if (doc is null) {
            doc = await ModelDAOHelper.DAOFactory.DocDAO.FindOrCreateByPathAsync (path);
          }

          int revision;
          try {
            revision = await extension.UploadAsync (content, description ?? "");
          }
          catch (Exception ex) {
            log.Error ($"UploadAsync: upload of {path} failed", ex);
            transaction.Rollback ();
            throw new DataProcessingException ($"Upload of document {path} failed", ErrorStatus.UnexpectedError, ex);
          }

          transaction.Commit ();

          if (log.IsInfoEnabled) {
            log.Info ($"UploadAsync: revision {revision} of {path} uploaded, doc id {doc.Id}");
          }
          return new DocUploadResult {
            DocId = doc.Id,
            Path = doc.Path,
            Revision = revision
          };
        }
      }
    }

    /// <summary>
    /// Generate a unique path for a new document: a random key, followed by the file name
    /// if one is given, so that the doc extension may get the file name and the content type from it
    /// </summary>
    /// <param name="fileName">nullable or empty</param>
    /// <returns>not null and not empty</returns>
    static string GeneratePath (string fileName)
    {
      var key = Guid.NewGuid ().ToString ("N");
      // Keep only the last segment of the file name, without the characters that are not valid in a file name
      var name = System.IO.Path.GetFileName ((fileName ?? "").Replace ('\\', '/').Split ('/').Last ().Trim ());
      var invalidChars = System.IO.Path.GetInvalidFileNameChars ();
      name = new string (name.Where (c => !invalidChars.Contains (c)).ToArray ()).Trim ();
      return (string.IsNullOrEmpty (name) || name.Equals (".") || name.Equals (".."))
        ? key
        : $"{key}/{name}";
    }
  }
}
