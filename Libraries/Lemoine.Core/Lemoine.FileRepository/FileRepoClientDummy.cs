// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// Description of FileRepoClientDummy.
  /// </summary>
  public sealed class FileRepoClientDummy: IFileRepoClient
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FileRepoClientDummy).FullName);

    #region IFileRepoClient implementation
    /// <summary>
    /// <see cref="Lemoine.FileRepository.IFileRepoClient" />
    /// </summary>
    /// <returns></returns>
    public bool Test()
    {
      return true;
    }
    
    /// <summary>
    /// Return an empty list
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public ICollection<string> ListFilesInDirectory(string nspace, string path)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Return an empty list
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public System.Collections.Generic.ICollection<string> ListDirectoriesInDirectory(string nspace, string path)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Raise an exception
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <param name="localPath"></param>
    public void GetFile(string nspace, string path, string localPath)
    {
      throw new NotImplementedException();
    }
    
    /// <summary>
    /// Raise an exception
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <param name="optional"></param>
    /// <returns></returns>
    public string GetString(string nspace, string path, bool optional = false)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Raise an exception
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public byte[] GetBinary(string nspace, string path)
    {
      throw new NotImplementedException();
    }    
    
    /// <summary>
    /// Raise an exception
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public DateTime GetLastModifiedDate(string nspace, string path)
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}
