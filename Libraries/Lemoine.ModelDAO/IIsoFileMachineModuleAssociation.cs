// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of IIsoFileMachineModuleAssociation.
  /// </summary>
  public interface IIsoFileMachineModuleAssociation: IMachineModuleAssociation
  {
    /// <summary>
    /// Reference to the IsoFile persistent class
    /// </summary>
    IIsoFile IsoFile { get; set; }
  }
}
