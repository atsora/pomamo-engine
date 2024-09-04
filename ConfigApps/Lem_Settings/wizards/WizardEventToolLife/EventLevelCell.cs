// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace WizardEventToolLife
{
  /// <summary>
  /// Description of EventLevelCell.
  /// </summary>
  public partial class EventLevelCell : UserControl
  {
    static readonly ILog log = LogManager.GetLogger(typeof (EventLevelCell).FullName);

    #region Getters / Setters
    /// <summary>
    /// Get / set the event level
    /// </summary>
    public EventToolLifeType EventType { get; private set; }
    
    /// <summary>
    /// Get / set the checked state
    /// </summary>
    public bool Checked {
      get { return checkBox.Checked; }
      set { checkBox.Checked = value; }
    }
    
    /// <summary>
    /// Get the event level
    /// </summary>
    public IEventLevel EventLevel {
      get { return combobox.SelectedValue as IEventLevel; }
      set { combobox.SelectedValue = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor (within a transaction)
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="levels"></param>
    public EventLevelCell(EventToolLifeType eventType, IList<IEventLevel> levels)
    {
      EventType = eventType;
      
      InitializeComponent();
      checkBox.Text = eventType.Name();
      
      foreach (var level in levels) {
        combobox.AddItem(level.Display, level, level.Priority);
      }

      if (levels.Count > 0) {
        combobox.SelectedIndex = 0;
      }
    }
    #endregion // Constructors
    
    #region Event reactions
    void CheckBoxCheckedChanged(object sender, EventArgs e)
    {
      combobox.Enabled = checkBox.Checked;
    }
    #endregion // Event reactions
  }
}
