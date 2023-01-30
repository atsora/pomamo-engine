// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Core.StateMachine
{
  /// <summary>
  /// State to a switch to a specific state only at a specific frequency
  /// </summary>
  public class FrequencyState<TContext> : State<TContext>
    where TContext : IContext<TContext>
  {
    readonly ILog log = LogManager.GetLogger (typeof (FrequencyState<TContext>).FullName);

    readonly string m_name;
    readonly TimeSpan m_frequency;
    readonly IState<TContext> m_timedState;
    readonly IState<TContext> m_otherState;

    DateTime m_lastCall = DateTime.MinValue;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="frequency"></param>
    /// <param name="timedState">State to switch to only at the specific frequency</param>
    /// <param name="otherState"></param>
    public FrequencyState (string name, TimeSpan frequency, IState<TContext> timedState, IState<TContext> otherState)
      : base (null)
    {
      m_name = name;
      m_frequency = frequency;
      m_timedState = timedState;
      m_otherState = otherState;
    }

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override string Name => m_name;

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override string PerfName => "";

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override void Handle (CancellationToken cancellationToken)
    {
      try {
        var nextCall = m_lastCall.Add (m_frequency);
        if (log.IsDebugEnabled) {
          log.Debug ($"Handle: last={m_lastCall} next={nextCall}");
        }
        if (nextCall <= DateTime.UtcNow) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Handle: timed state since nextCall={nextCall} is reached");
          }
          var call = DateTime.UtcNow;
          this.Context.SwitchTo (m_timedState);
          m_lastCall = call;
        }
        else {
          this.Context.SwitchTo (m_otherState);
        }
      }
      catch (Exception ex) {
        log.Fatal ("Handle: unexpected error", ex);
        throw;
      }
    }
    #endregion // Constructors

  }
}
