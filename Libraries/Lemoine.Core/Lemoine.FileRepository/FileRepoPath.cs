// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// FileRepoPath
  /// 
  /// Utility functions for a Path with a standard separator '/'
  /// </summary>
  public static class FileRepoPath
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FileRepoPath).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    /// <summary>
    /// Combine file repo paths
    /// </summary>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static string Combine (params string[] paths)
    {
      return string.Join ("/", paths);
    }

    /// <summary>
    /// Convert a file repo path to an of native path
    /// </summary>
    /// <param name="fileRepoPath"></param>
    /// <returns></returns>
    public static string ConvertToOsPath (string fileRepoPath)
    {
      if (Path.DirectorySeparatorChar.Equals ('/')) {
        return fileRepoPath;
      }
      else {
        return Path.Combine (fileRepoPath.Split (new char[] { '/' }));
      }
    }
  }
}
