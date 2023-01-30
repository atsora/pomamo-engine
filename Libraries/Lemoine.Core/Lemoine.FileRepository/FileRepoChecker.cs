// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// Check when the file repository is again available
  /// </summary>
  public class FileRepoChecker : IFileRepoChecker
  {
    readonly ILog log = LogManager.GetLogger (typeof (FileRepoChecker).FullName);

    volatile bool m_initialized = false;
    bool m_fileRepoAvailableAtStart = false;
    object m_lock = new object ();

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public FileRepoChecker ()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Check at start if the file repo is available
    /// </summary>
    /// <returns></returns>
    public void Initialize ()
    {
      if (m_initialized) {
        return;
      }

      bool fileRepoAvailableAtStart;
      try {
        fileRepoAvailableAtStart = FileRepoClient.Test ();
      }
      catch (Exception ex) {
        log.Error ("Initialize: exception, skip it", ex);
        return;
      }

      if (m_initialized) {
        log.Warn ("Initialize: it was initialized in the mean time by another thread");
      }
      else { // !m_initialized
        lock (m_lock) {
          if (!m_initialized) {
            m_fileRepoAvailableAtStart = fileRepoAvailableAtStart;
            m_initialized = true;
          }
        }
      }
    }

    /// <summary>
    /// Return false if the file repo is now available in case it was not before
    /// </summary>
    /// <returns></returns>
    public bool Check ()
    {
      Initialize ();
      return !IsFileRepoNowAvailable ();
    }

    bool IsFileRepoNowAvailable ()
    {
      if (m_fileRepoAvailableAtStart) {
        return false;
      }
      else { // !m_fileRepoAvailableAtStart
        var fileRepoAvailable = FileRepoClient.Test ();
        if (fileRepoAvailable) {
          log.Info ($"IsFileRepoNowAvailable: the file repo is now available (previously it was not) => return true");
          return true;
        }
        else {
          return false;
        }
      }
    }
  }
}
