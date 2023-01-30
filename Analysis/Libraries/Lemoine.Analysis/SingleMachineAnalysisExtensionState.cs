// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Core.StateMachine;
using Lemoine.Extensions.Analysis;
using Lemoine.Extensions.Analysis.StateMachine;

namespace Lemoine.Analysis
{
  /// <summary>
  /// SingleMachineAnalysisExtensionState
  /// 
  /// State where no transition to the next state is set
  /// To be used in a MultiState
  /// </summary>
  public class SingleMachineAnalysisExtensionState<T>
    : State<T>
    where T: IMachineActivityAnalysis, IContext<T>
  {
    readonly ILog log = LogManager.GetLogger (typeof (SingleMachineAnalysisExtensionState<T>).FullName);

    readonly ISingleMachineAnalysisExtension m_extension;
    readonly string m_pluginName;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="extension">not null</param>
    public SingleMachineAnalysisExtensionState (ISingleMachineAnalysisExtension extension)
      : base (null)
    {
      Debug.Assert (null != extension);

      m_extension = extension;
      m_pluginName = extension.GetType ().AssemblyQualifiedName.Replace ("Lemoine.Plugin.", "");
    }

    public override string Name => m_pluginName;

    public override string PerfName => m_pluginName;

    public override void Handle (CancellationToken cancellationToken)
    {
      m_extension.RunOnce (cancellationToken);
    }
    #endregion // Constructors

  }
}
