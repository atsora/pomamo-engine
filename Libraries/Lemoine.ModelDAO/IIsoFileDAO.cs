// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IIsoFile.
  /// </summary>
  public interface IIsoFileDAO : IGenericUpdateDAO<IIsoFile, int>
  {
    /// <summary>
    /// Try to get the IsoFile entity matching the stamping
    /// of a source Iso file into a target directory
    /// several iso files may be returned ...
    /// </summary>
    /// <param name="sourceIsoFileName"></param>
    /// <param name="sourceIsoFileDirectory"></param>
    /// <param name="targetIsoDirectory"></param>
    /// <returns></returns>
    IList<IIsoFile> GetIsoFile(string sourceIsoFileName,
                               string sourceIsoFileDirectory,
                               string targetIsoDirectory);
    
    /// <summary>
    /// Try to get the IsoFile from a program name
    /// </summary>
    /// <param name="programName"></param>
    /// <returns></returns>
    IList<IIsoFile> GetIsoFile(string programName);
  }
}
