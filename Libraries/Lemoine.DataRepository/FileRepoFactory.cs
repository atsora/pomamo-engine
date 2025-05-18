// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.FileRepository;
using Lemoine.Core.Log;
using System.Threading;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// File Repository Factory: builds a DOMDocument from an XML file stored in the File Repository
  /// </summary>
  public class FileRepoFactory : IFactory
  {
    /// <summary>
    /// FileRepo exception
    /// </summary>
    public class FileRepoException : RepositoryException
    {
      /// <summary>
      /// <see cref="RepositoryException"/>
      /// </summary>
      /// <param name="message"></param>
      /// <param name="innerException"></param>
      public FileRepoException (string message, Exception innerException)
        : base (message, innerException)
      {
      }
    }

    static readonly ILog log = LogManager.GetLogger (typeof (FileRepoFactory).FullName);

    /// <summary>
    /// Namespace in the File Repository
    /// </summary>
    public string NameSpace { get; set; }

    /// <summary>
    /// Path in the File Repository
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="nameSpace">Namespace in the file repository</param>
    /// <param name="path">Path in the file repository</param>
    public FileRepoFactory (string nameSpace, string path)
    {
      NameSpace = nameSpace;
      Path = path;
    }

    /// <summary>
    /// Specialized method to build the DOMDocument
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="optional"></param>
    /// <returns>The DOMDocument or null if optional is set and the document was not found</returns>
    public virtual System.Xml.XmlDocument GetData (CancellationToken cancellationToken, bool optional = false)
    {
      System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument ();
      string xmlString;
      try {
        xmlString = FileRepoClient.GetString (NameSpace, Path, optional);
      }
      catch (MissingFileException ex) {
        if (log.IsWarnEnabled) {
          log.Warn ($"GetData: MissingFile returned by GetString of {NameSpace}/{Path}");
        }
        throw new FileRepoException ("FileRepoClient.GetString error", ex);
      }
      catch (Exception ex) {
        log.Error ($"GetData: GetString error of {NameSpace}/{Path}", ex);
        throw new FileRepoException ("FileRepoClient.GetString error", ex);
      }
      cancellationToken.ThrowIfCancellationRequested ();
      if (optional && string.IsNullOrEmpty (xmlString)) {
        if (log.IsInfoEnabled) {
          log.Info ($"GetData: optional and an empty string was returned => return null");
        }
        return null;
      }
      xmlDocument.LoadXml (xmlString);
      return xmlDocument;
    }

    /// <summary>
    /// <see cref="IFactory.CheckSynchronizationOkAction" />
    /// </summary>
    /// <returns></returns>
    public bool CheckSynchronizationOkAction ()
    {
      log.Debug ("CheckSynchronizationOkAction: return false");
      return false;
    }

    /// <summary>
    /// <see cref="IFactory.FlagSynchronizationAsSuccess" />
    /// </summary>
    /// <param name="document"></param>
    public void FlagSynchronizationAsSuccess (System.Xml.XmlDocument document)
    {
      // Do nothing special for the moment
      return;
    }

    /// <summary>
    /// <see cref="IFactory.FlagSynchronizationAsFailure" />
    /// </summary>
    /// <param name="document"></param>
    public void FlagSynchronizationAsFailure (System.Xml.XmlDocument document)
    {
      // Do nothing special for the moment
      return;
    }
  }
}
