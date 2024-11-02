// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ISequence.
  /// </summary>
  public interface ISequenceDAO: IGenericUpdateDAO<ISequence, int>
  {
    /// <summary>
    /// Find by id for XML serialization
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    ISequence FindByIdForXmlSerialization (int id);

    /// <summary>
    /// FindAll sequences for a given Operation
    /// </summary>
    /// <returns></returns>
    IList<ISequence> FindAllWithOperation (IOperation operation);

    /// <summary>
    /// FindAll sequences for a given Path
    /// </summary>
    /// <returns></returns>
    IList<ISequence> FindAllWithPath (IPath path);
  }
}
