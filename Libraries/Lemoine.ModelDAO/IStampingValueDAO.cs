// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IStampingValue.
  /// </summary>
  public interface IStampingValueDAO: IGenericUpdateDAO<IStampingValue, int>
  {
    /// <summary>
    /// Find the stamping value that matches the sequence and the field
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    IStampingValue Find (ISequence sequence, IField field);
  }
}
