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
  /// MissingFileException
  /// </summary>
  public class MissingFileException: Exception
  {
    readonly ILog log = LogManager.GetLogger (typeof (MissingFileException).FullName);

    static readonly string MESSAGE = "Missing file in FileRepository";

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="nspace">Namespace</param>
    /// <param name="path">File path</param>
    public MissingFileException (string nspace, string path)
      : base (MESSAGE)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"MissingFileException: {nspace}/{path}");
      }
      this.Data["NSpace"] = nspace;
      this.Data["Path"] = path;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="nspace">Namespace</param>
    /// <param name="path">File path</param>
    /// <param name="innerException">inner exception</param>
    public MissingFileException (string nspace, string path, Exception innerException)
      : base (MESSAGE, innerException)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"MissingFileException: {nspace}/{path}, inner={innerException}");
      }
      this.Data["NSpace"] = nspace;
      this.Data["Path"] = path;
    }
    #endregion // Constructors

    /// <summary>
    /// Add nspace/path to the message
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return this.Data["NSpace"].ToString () + "/" + this.Data["Path"].ToString () + ": " + base.ToString (); 
    }

  }
}
