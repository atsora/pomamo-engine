// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace WizardEventLongPeriod
{
  /// <summary>
  /// Description of EventLevelCell.
  /// </summary>
  public partial class EventLevelCell : UserControl
  {
    static readonly ILog log = LogManager.GetLogger(typeof (EventLevelCell).FullName);

    #region Getters / Setters
    /// <summary>
    /// Get / set the time
    /// </summary>
    public TimeSpan Time {
      get { return durationPicker.Duration; }
      set { durationPicker.Duration = value; }
    }
    
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
    public IEventLevel EventLevel { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor (within a transaction)
    /// </summary>
    /// <param name="eventLevel"></param>
    public EventLevelCell(IEventLevel eventLevel)
    {
      EventLevel = eventLevel;
      
      InitializeComponent();
      checkBox.Text = String.Format("{0} (priority {1})", EventLevel.NameOrTranslation, EventLevel.Priority);
    }
    #endregion // Constructors
    
    #region Event reactions
    void CheckBoxCheckedChanged(object sender, EventArgs e)
    {
      durationPicker.Enabled = checkBox.Checked;
    }
    #endregion // Event reactions
  }
}
