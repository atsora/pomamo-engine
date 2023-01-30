// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.Settings
{
  /// <summary>
  /// Description of IConfiguratorPage.
  /// </summary>
  public interface IConfiguratorPage : IViewPage
  {
    /// <summary>
    /// Event emitted when data has been edited
    /// A revision (which can be null) has to be attached
    /// </summary>
    event Action<IRevision> DataChangedEvent;
    
    /// <summary>
    /// Ask the software to show a warning "really quit" when the user press "home"
    /// "true" or "false" has to be sent as an argument
    /// By default the protection is enabled except for the root page
    /// </summary>
    event Action<bool> ProtectAgainstQuit;
    
    /// <summary>
    /// Log an action with 3 strings
    /// The first argument is the name of the function that has been used,
    /// the second argument describes the data used by the function,
    /// the third argument is the result of the action.
    /// </summary>
    event Action<string, string, string> LogAction;
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    IList<Type> EditableTypes { get; }
  }
}
