// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Interface for IComparer of ISequence
  /// Lower order sequences returned first,
  /// based on Ids if order is equal
  /// Comparison of sequences is only relevant for two sequences
  /// of the same operation
  /// </summary>
  public interface ISequenceComparer : IComparer<ISequence>
  {
  }
}
