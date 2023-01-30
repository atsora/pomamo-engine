// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IStamp.
  /// </summary>
  public interface IStampDAO: IGenericUpdateDAO<IStamp, int>
  {
    /// <summary>
    /// FindAll stamps for a given IsoFile
    /// </summary>
    /// <returns></returns>
    IList<IStamp> FindAllWithIsoFile (IIsoFile isoFile);
    
    /// <summary>
    /// Get all the stamps which have an associated position
    /// for a given IsoFile
    /// and return the result by ascending position
    /// </summary>
    /// <param name="isoFileId"></param>
    /// <returns></returns>
    IList<IStamp> GetAllWithAscendingPosition (int isoFileId);
    
    /// <summary>
    /// FindAll stamps for a given sequence
    /// </summary>
    /// <param name="sequence"></param>
    /// <returns></returns>
    IList<IStamp> FindAllWithSequence (ISequence sequence);
    
    /// <summary>
    /// Find all the stamps for a specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    IList<IStamp> FindAllWithComponent (IComponent component);
  }
}
