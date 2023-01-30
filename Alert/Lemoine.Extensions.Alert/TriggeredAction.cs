// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Alert
{
  /// <summary>
  /// Description of TriggeredAction.
  /// </summary>
  [Serializable]
  public class TriggeredAction
  {
    #region Members
    ITrigger m_trigger;
    IList<IAction> m_actions = new List<IAction> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (TriggeredAction).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated trigger
    /// </summary>
    public ITrigger Trigger
    {
      get { return m_trigger; }
      set { m_trigger = value; }
    }
    
    /// <summary>
    /// Associated actions
    /// </summary>
    public IList<IAction> Actions
    {
      get { return m_actions; }
      set { m_actions = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TriggeredAction ()
    {
    }
    #endregion // Constructors
  }
}
