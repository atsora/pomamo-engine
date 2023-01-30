// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

namespace ConfiguratorRedirectEvent
{
  /// <summary>
  /// Description of ActionCell.
  /// </summary>
  public partial class ActionCell : UserControl
  {
    #region Members
    readonly Action m_action;
    readonly bool m_preparation;
    #endregion // Members

    #region Events
    /// <summary>
    /// Event triggered when the button "Edit" is clicked
    /// The argument is the action that will be read
    /// </summary>
    public event Action<Action> ReadTriggered;
    
    /// <summary>
    /// Event triggered when the checkbox has been used
    /// The first argument is the action
    /// The second argument is the enable state of the action
    /// </summary>
    public event Action<Action, bool> EnableChanged;
    #endregion
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="action"></param>
    /// <param name="viewMode"></param>
    public ActionCell(Action action, bool viewMode)
    {
      m_action = action;
      InitializeComponent();
      checkEnable.Enabled = !viewMode;
      
      m_preparation = true;
      checkEnable.Checked = action.Activated;
      labelTitle.Text = action.Title;
      labelDescription.Text = action.Description;
      m_preparation = false;
    }
    #endregion // Constructors

    #region Event reactions
    void ButtonOpenFileClick(object sender, EventArgs e)
    {
      if (ReadTriggered != null) {
        ReadTriggered (m_action);
      }
    }
    
    void CheckEnableCheckedChanged(object sender, EventArgs e)
    {
      if (m_preparation) {
        return;
      }

      // Change the activation state of the action
      m_action.Activated = checkEnable.Checked;
      
      if (EnableChanged != null) {
        EnableChanged (m_action, checkEnable.Checked);
      }
    }
    #endregion // Event reactions
  }
}
