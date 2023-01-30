// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Base interface for all the machine association modification tables
  /// </summary>
  public interface IMachineAssociation: IMachineModification, IPeriodAssociation
  {
  }
}
