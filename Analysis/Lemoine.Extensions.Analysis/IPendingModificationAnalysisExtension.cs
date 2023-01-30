// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.Extensions.Analysis
{
  /// <summary>
  /// Extensions to PendingGlobalModificationAnalysis and PendingMachineModificationAnalysis
  /// </summary>
  public interface IPendingModificationAnalysisExtension: Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// For PendingGlobalModificationAnalysis machine is null.
    /// 
    /// In the case of PendingGlobalMachineModificationAnalysis, Initialize is not called
    /// </summary>
    /// <param name="machine">nullable</param>
    void Initialize (IMachine machine);
    
    /// <summary>
    /// Run just before MakeAnalysis
    /// </summary>
    /// <param name="modification"></param>
    void BeforeMakeAnalysis (IModification modification);
    
    /// <summary>
    /// Run just after MakeAnalysis
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="completed"></param>
    void AfterMakeAnalysis (IModification modification, bool completed);
    
    /// <summary>
    /// MakeAnalysis ended with an exception
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="ex"></param>
    void MakeAnalysisException (IModification modification, Exception ex);

    /// <summary>
    /// Notify a modification with sub-modifications has just been flagged as completed
    /// </summary>
    /// <param name="modification"></param>
    void NotifyAllSubModificationsCompleted (IModification modification);
  }
}
