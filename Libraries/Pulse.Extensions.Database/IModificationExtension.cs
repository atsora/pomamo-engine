// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// Extension to react to modification changes
  /// </summary>
  public interface IModificationExtension: Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Notify a change of analysis status
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="oldStatus"></param>
    /// <param name="newStatus"></param>
    void NotifyAnalysisStatusChange (IModification modification, AnalysisStatus oldStatus, AnalysisStatus newStatus);
  }
}
