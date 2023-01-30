// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.IO;
using System.Net;
using Lemoine.Core.Log;
using Lemoine.WebClient;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// FileRepoClient that is using the web service
  /// </summary>
  public class FileRepoClientWeb : Lemoine.WebClient.Query, IFileRepoClient
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (FileRepoClientWeb).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor with a default web service URL
    /// </summary>
    public FileRepoClientWeb ()
      : base (Lemoine.Info.PulseInfo.WebServiceUrl)
    {
    }

#if NETSTANDARD
    /// <summary>
    /// Constructor with a default web service URL
    /// </summary>
    public FileRepoClientWeb (System.Net.Http.HttpClient httpClient)
      : base (httpClient, Lemoine.Info.PulseInfo.WebServiceUrl)
    {
    }
#endif // NETSTANDARD

    /// <summary>
    /// Constructor with a specified specified web service URL
    /// </summary>
    /// <param name="webServiceUrl"></param>
    public FileRepoClientWeb (string webServiceUrl)
      : base (webServiceUrl)
    {
    }

#if NETSTANDARD
    /// <summary>
    /// Constructor with a specified specified web service URL
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="webServiceUrl"></param>
    public FileRepoClientWeb (System.Net.Http.HttpClient httpClient, string webServiceUrl)
      : base (httpClient, webServiceUrl)
    {
    }
#endif // NETSTANDARD
#endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IFileRepoClient implementation
    /// <summary>
    /// <see cref="Lemoine.FileRepository.IFileRepoClient" />
    /// </summary>
    /// <returns></returns>
    public bool Test ()
    {
      try {
        return UniqueResult<bool> (new RequestUrl ("FileRepo/Test"));
      }
      catch (Exception ex) {
        log.Error ("Test: exception", ex);
        return false;
      }
    }

    /// <summary>
    /// <see cref="Lemoine.FileRepository.IFileRepoClient" />
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public System.Collections.Generic.ICollection<string> ListFilesInDirectory (string nspace, string path)
    {
      return List<string> (new RequestUrl ("FileRepo/ListFilesInDirectory")
                           .Add ("NSpace", nspace)
                           .Add ("Path", path));
    }

    /// <summary>
    /// <see cref="Lemoine.FileRepository.IFileRepoClient" />
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public System.Collections.Generic.ICollection<string> ListDirectoriesInDirectory (string nspace, string path)
    {
      return List<string> (new RequestUrl ("FileRepo/ListDirectoriesInDirectory")
                           .Add ("NSpace", nspace)
                           .Add ("Path", path));
    }

    /// <summary>
    /// <see cref="Lemoine.FileRepository.IFileRepoClient" />
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <param name="localPath"></param>
    /// <returns></returns>
    public void GetFile (string nspace, string path, string localPath)
    {
      var content = GetBinary (nspace, path);

      try {
        var localDirectory = Path.GetDirectoryName (localPath);
        if (!Directory.Exists (localDirectory)) {
          Directory.CreateDirectory (localDirectory);
        }
        using (var writer = new BinaryWriter (File.Open (localPath, FileMode.Create))) {
          writer.Write (content);
        }
      }
      catch (WebException ex) {
        if (IsMissingFileException (ex)) {
          if (log.IsDebugEnabled) {
            log.Debug ("GetFile: MissingFileException", ex);
          }
          throw new MissingFileException (nspace, path, ex);
        }
        log.Error ("GetFile: exception", ex);
        throw;
      }
      catch (Exception ex) {
        log.Error ($"GetFile: error while writing into {localPath}", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="Lemoine.FileRepository.IFileRepoClient" />
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public string GetString (string nspace, string path, bool optional = false)
    {
      try {
        var s = StringResult (new RequestUrl ("FileRepo/GetString")
          .Add ("NSpace", nspace)
          .Add ("Path", path)
          .Add ("Optional", optional)
          .Add ("responseType", "text/plain")
          );
        return s;
      }
      catch (WebException ex) {
        if (IsMissingFileException (ex)) {
          if (log.IsDebugEnabled) {
            log.Debug ("GetString: MissingFileException", ex);
          }
          throw new MissingFileException (nspace, path, ex);
        }
        log.Error ("GetString: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="Lemoine.FileRepository.IFileRepoClient" />
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public byte[] GetBinary (string nspace, string path)
    {
      try {
        return BinaryResult (new RequestUrl ("FileRepo/GetBinary")
          .Add ("NSpace", nspace)
          .Add ("Path", path)
          .Add ("responseType", "application/octet-stream")
          );
      }
      catch (WebException ex) {
        if (IsMissingFileException (ex)) {
          if (log.IsDebugEnabled) {
            log.Debug ("GetBinary: MissingFileException", ex);
          }
          throw new MissingFileException (nspace, path, ex);
        }
        log.Error ("GetBinary: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="Lemoine.FileRepository.IFileRepoClient" />
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public DateTime GetLastModifiedDate (string nspace, string path)
    {
      try {
        return UniqueResult<DateTime> (new RequestUrl ("FileRepo/GetLastModifiedDate")
                                       .Add ("NSpace", nspace)
                                       .Add ("Path", path));
      }
      catch (WebException ex) {
        if (IsMissingFileException (ex)) {
          if (log.IsDebugEnabled) {
            log.Debug ("GetLastModifiedDate: MissingFileException", ex);
          }
          throw new MissingFileException (nspace, path, ex);
        }
        log.Error ("GetLastModifiedDate: exception", ex);
        throw;
      }
    }

    bool IsMissingFileException (WebException ex)
    {
      var response = ex.Response as HttpWebResponse;
      if (null != response) {
        if (response.StatusCode.Equals (HttpStatusCode.InternalServerError)) {
          // New web service
          using (var streamReader = new StreamReader (response.GetResponseStream ())) {
            var text = streamReader.ReadToEnd ();
            if (text.Contains (typeof (Lemoine.FileRepository.MissingFileException).Name)) {
              return true;
            }
          }

          // Old web service in :8081
          var statusDescription = response.StatusDescription;
          if (string.Equals (statusDescription,
            typeof (Lemoine.FileRepository.MissingFileException).Name)) {
            return true;
          }
        }
      }

      return false;
    }

    #endregion
  }
}
