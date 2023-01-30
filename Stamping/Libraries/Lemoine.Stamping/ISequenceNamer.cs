// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Interface to deduce a sequence name from various properties
  /// </summary>
  public interface ISequenceNamer
  {
    string GetSequenceName (int sequenceOrder);
  }
}
