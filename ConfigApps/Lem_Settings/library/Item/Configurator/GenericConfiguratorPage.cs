// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.Settings
{
  /// <summary>
  /// Description of GenericConfiguratorPage.
  /// </summary>
  public class GenericConfiguratorPage : GenericViewPage
  {
    #region Events
    /// <summary>
    /// Event emitted when data has been edited
    /// A revision (which can be null) has to be attached
    /// </summary>
    public event Action<IRevision> DataChangedEvent;
    
    /// <summary>
    /// Event emitted when data has been edited
    /// </summary>
    /// <param name="revision">revision to apply, may be null</param>
    protected void EmitDataChangedEvent(IRevision revision)
    {
      DataChangedEvent(revision);
    }
    
    /// <summary>
    /// Ask the software to show a warning "really quit" when the user press "home"
    /// "true" or "false" has to be sent as an argument
    /// By default the protection is enabled except for the root page
    /// </summary>
    public event Action<bool> ProtectAgainstQuit;
    
    /// <summary>
    /// Ask the software to show a warning "really quit" when the user press "home"
    /// By default the protection is enabled except for the root page
    /// </summary>
    /// <param name="isProtected">state of the protection</param>
    protected void EmitProtectAgainstQuit(bool isProtected)
    {
      // In view mode this event is not connected
      if (ProtectAgainstQuit != null) {
        ProtectAgainstQuit (isProtected);
      }
    }
    
    /// <summary>
    /// Log an action with 3 strings
    /// The first argument is the name of the function that has been used,
    /// the second argument describes the data used by the function,
    /// the third argument is the result of the action.
    /// </summary>
    public event Action<string, string, string> LogAction;
    
    /// <summary>
    /// Log an action
    /// </summary>
    /// <param name="functionName">name of the function that has been used</param>
    /// <param name="dataDescription">the data used by the function</param>
    /// <param name="result">result of the action</param>
    protected void EmitLogAction(string functionName, string dataDescription, string result)
    {
      LogAction(functionName, dataDescription, result);
    }
    #endregion // Events
    
    #region Getters / Setters
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public virtual IList<Type> EditableTypes { get { return null; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public GenericConfiguratorPage() : base() {}
    #endregion // Constructors
  }
}
