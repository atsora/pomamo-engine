// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.StateMachine;
using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Analysis.StateMachine
{
  /// <summary>
  /// 
  /// </summary>
  public interface IGlobalAnalysisStateMachineExtension
    : IExtension
  {
    /// <summary>
    /// Initialize the plugin
    /// 
    /// If false is returned, this extension is not considered
    /// </summary>
    /// <param name="context">not null</param>
    /// <returns></returns>
    bool Initialize (IGlobalAnalysis context);

    /// <summary>
    /// Priority
    /// 
    /// Only the extension with the hightest priority is considered
    /// </summary>
    double Priority { get; }

    /// <summary>
    /// Initial state
    /// </summary>
    IState<IGlobalAnalysis> InitialState { get; }
  }
}
